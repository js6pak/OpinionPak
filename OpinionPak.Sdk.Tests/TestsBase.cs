// SPDX-License-Identifier: LGPL-3.0-only
// SPDX-FileCopyrightText: 2024 js6pak

using System.Collections;
using System.Xml.Linq;
using Microsoft.Build.Framework;
using Microsoft.Build.Utilities.ProjectCreation;
using Xunit;
using Xunit.Abstractions;

namespace OpinionPak.Sdk.Tests;

public abstract partial class TestsBase<T> : MSBuildSdkTestBase
    where T : ITestsMethods
{
    private readonly ITestOutputHelper _output;

    protected TestsBase(ITestOutputHelper output) : base(T.ModifyNuGetConfig)
    {
        _output = output;

        foreach (DictionaryEntry variable in Environment.GetEnvironmentVariables())
        {
            var name = (string) variable.Key;
            if (name.StartsWith("GITHUB_", StringComparison.OrdinalIgnoreCase))
            {
                Environment.SetEnvironmentVariable(name, null);
            }
        }
    }

    private ProjectCreator CreateProject(
        string sdk = ProjectCreatorConstants.SdkCsprojDefaultSdk,
        bool disableFileHeader = true,
        bool disableNanoVer = true,
        bool disableRootEditorConfig = true
    )
    {
        var projectCreator = T.CreateProject(GetTempFileWithExtension(".csproj"), sdk);

        projectCreator.Property("SolutionDir", TestRootPath);

        if (disableFileHeader)
        {
            projectCreator.Property("GenerateFileHeaderEditorConfig", "false");
        }

        if (disableNanoVer)
        {
            projectCreator.Property("NanoVer", "false");
        }

        if (disableRootEditorConfig)
        {
            projectCreator.Property("GenerateRootEditorConfig", "false");
        }

        return projectCreator;
    }

    private async Task<(string StandardOutput, string StandardError)> ReadAsync(string name, string args)
    {
        return await SimpleExec.Command.ReadAsync(name, args, TestRootPath);
    }

    private async Task RunAsync(string name, string args)
    {
        await ReadAsync(name, args);
    }

    [Fact]
    public void UsingOpinionPakValueSet()
    {
        CreateProject()
            .TryGetPropertyValue("UsingOpinionPak", out var propertyValue);

        Assert.Equal("true", propertyValue);
    }

    private BuildOutput Build(ProjectCreator projectCreator, string target = "Build", bool shouldSucceed = true)
    {
        projectCreator.TryBuild(restore: true, target, out var result, out var buildOutput);

        if (shouldSucceed) Assert.True(result, buildOutput.GetConsoleLog());
        else Assert.False(result, buildOutput.GetConsoleLog());

        _output.WriteLine(buildOutput.GetConsoleLog());

        return buildOutput;
    }

    [Fact]
    public void SimpleBuild()
    {
        var projectCreator = CreateProject()
            .CustomAction(
                creator =>
                {
                    creator.Target("TakeAction", afterTargets: "Build")
                        .TaskMessage("86F00AF59170450E9D687652D74A6394", MessageImportance.High);
                }
            )
            .Save();

        using var buildOutput = Build(projectCreator);

        buildOutput.AssertNoWarnings();

        Assert.Contains("86F00AF59170450E9D687652D74A6394", buildOutput.Messages.High);
    }
}

// Workaround to avoid xunit trying to load msbuild classes before we call the MSBuildTestBase constructor
public interface ITestsMethods
{
    static virtual void ModifyNuGetConfig(string testRootPath, XDocument nuGetConfig)
    {
    }

    static abstract ProjectCreator CreateProject(string path, string sdk);
}
