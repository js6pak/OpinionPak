// SPDX-License-Identifier: LGPL-3.0-only
// SPDX-FileCopyrightText: 2024 js6pak

using System.Diagnostics;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Text.RegularExpressions;
using CliWrap;
using CliWrap.Buffered;

namespace OpinionPak.Sdk.Tests.Utilities;

internal static partial class DotnetBuildCommand
{
    public static async Task<Result> ExecuteAsync(
        string? project = null,
        string? workingDirectory = null,
        IReadOnlyCollection<string>? targets = null,
        IDictionary<string, string>? properties = null,
        IReadOnlyCollection<string>? getProperty = null,
        IReadOnlyCollection<string>? getItem = null,
        IReadOnlyCollection<string>? getTargetResult = null,
        bool validateSuccess = true,
        bool validateNoWarnings = true
    )
    {
        workingDirectory ??= project != null
            ? Path.GetDirectoryName(project)!
            : Directory.GetCurrentDirectory();

        string? getResultOutputFile = null;

        if (getProperty?.Count > 0 || getItem?.Count > 0 || getTargetResult?.Count > 0)
        {
            getResultOutputFile = Path.Combine(Path.GetTempPath(), Path.GetRandomFileName());
        }

        try
        {
            var commandResult = await Cli.Wrap("dotnet")
                .WithArguments(args =>
                {
                    args.Add("build");
                    AddArgument("terminalLogger", "off");
                    AddArgument("consoleLoggerParameters", "NoSummary");

                    if (targets != null) AddListArgument("target", targets);
                    if (properties != null) AddListArgument("property", properties.Select(p => $"{p.Key}={p.Value}"));

                    if (getResultOutputFile != null) AddArgument("getResultOutputFile", getResultOutputFile);
                    if (getProperty != null) AddListArgument("getProperty", getProperty);
                    if (getItem != null) AddListArgument("getItem", getItem);
                    if (getTargetResult != null) AddListArgument("getTargetResult", getTargetResult);

                    if (project != null) args.Add(project);

                    void AddArgument(string name, string value)
                    {
                        args.Add($"/{name}:{value}");
                    }

                    void AddListArgument(string name, IEnumerable<string> list)
                    {
                        AddArgument(name, string.Join(';', list));
                    }
                })
                .WithWorkingDirectory(workingDirectory)
                .WithValidation(CommandResultValidation.None)
                .WithConsolePipe()
                .LogExecute()
                .ExecuteBufferedAsync();

            GetResultOutput? getResultOutput = null;

            if (getResultOutputFile != null)
            {
                if (getProperty?.Count == 1 && (getItem == null || getItem.Count == 0))
                {
                    var propertyName = getProperty.Single();

                    getResultOutput = new GetResultOutput(
                        new Dictionary<string, string>
                        {
                            [propertyName] = (await File.ReadAllTextAsync(getResultOutputFile)).TrimEnd(),
                        },
                        null,
                        null
                    );
                }
                else
                {
                    await using var stream = File.OpenRead(getResultOutputFile);
                    getResultOutput = await JsonSerializer.DeserializeAsync<GetResultOutput>(stream);
                }
            }

            if (validateSuccess && !commandResult.IsSuccess)
            {
                throw new InvalidOperationException($"dotnet build failed with exit code {commandResult.ExitCode}");
            }

            var result = new Result(
                commandResult.ExitCode,
                commandResult.StandardOutput,
                commandResult.StandardError,
                getResultOutput
            );

            if (validateNoWarnings)
            {
                await result.AssertNoWarningsAsync();
            }

            return result;
        }
        finally
        {
            if (getResultOutputFile != null)
            {
                File.Delete(getResultOutputFile);
            }
        }
    }

    public static async Task<(Result Result, string? PropertyValue)> GetPropertyAsync(
        string propertyName,
        string? project = null,
        string? workingDirectory = null,
        IReadOnlyCollection<string>? targets = null,
        IDictionary<string, string>? properties = null,
        bool validateSuccess = true,
        bool validateNoWarnings = true
    )
    {
        var result = await ExecuteAsync(
            project: project,
            workingDirectory: workingDirectory,
            targets: targets,
            properties: properties,
            getProperty: [propertyName],
            validateSuccess: validateSuccess,
            validateNoWarnings: validateNoWarnings
        );

        return (result, result.GetResultOutput?.Properties?[propertyName]);
    }

    [GeneratedRegex(
        @"^(?<file>[^\s].*)(?:(?:\((?<line>\d+)(?:,\d+|,\d+,\d+)?\))| ):\s+(?<severity>error|warning|message)\s+(?<code>[a-zA-Z]+(?<!MSB)\d+)?:\s*(?<message>.*?)\s+\[(?<projectFile>.*?)\]$",
        RegexOptions.Multiline
    )]
    private static partial Regex MessageRegex { get; }

    public sealed record Result(
        int ExitCode,
        string StandardOutput,
        string StandardError,
        GetResultOutput? GetResultOutput
    )
    {
        public bool IsSuccess => ExitCode == 0;

        public BuildEvent[] Events { get; } = MessageRegex.Matches(StandardOutput).Select(m => new BuildEvent(
            m.Groups["file"].Value,
            m.Groups["severity"].Value switch
            {
                "error" => BuildEventSeverity.Error,
                "warning" => BuildEventSeverity.Warning,
                "message" => BuildEventSeverity.Message,
                _ => throw new UnreachableException(),
            },
            m.Groups["code"].Value,
            m.Groups["message"].Value,
            m.Groups["projectFile"].Value
        )).ToArray();

        public IEnumerable<BuildEvent> Errors => Events.Where(m => m.Severity == BuildEventSeverity.Error);
        public IEnumerable<BuildEvent> Warnings => Events.Where(m => m.Severity == BuildEventSeverity.Warning);

        public async Task AssertNoWarningsAsync()
        {
            await Assert.That(Warnings).IsEmpty();
        }
    }

    public sealed record BuildEvent(string File, BuildEventSeverity Severity, string Code, string Message, string ProjectFile);

    public enum BuildEventSeverity
    {
        Error,
        Warning,
        Message,
    }

    public sealed record GetResultOutput(
        [property: JsonPropertyName("Properties")]
        Dictionary<string, string>? Properties,
        [property: JsonPropertyName("Items")]
        Dictionary<string, Dictionary<string, string>>? Items,
        [property: JsonPropertyName("TargetResults")]
        Dictionary<string, GetResultOutput.TargetResult>? TargetResults
    )
    {
        public sealed record TargetResult(
            [property: JsonPropertyName("Result")]
            string Result,
            [property: JsonPropertyName("Items")]
            Dictionary<string, string> Items
        );
    }
}
