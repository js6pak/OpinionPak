// SPDX-License-Identifier: LGPL-3.0-only
// SPDX-FileCopyrightText: 2024 js6pak

using System.Xml.Linq;
using CliWrap;
using CliWrap.Buffered;
using OpinionPak.Sdk.Tests.Utilities;

namespace OpinionPak.Sdk.Tests;

[Arguments("10_0_1xx-rc")]
[Arguments("9_0_3xx")]
[Arguments("9_0_1xx")]
[Arguments("8_0_4xx")]
public sealed partial class SdkTests(string SdkVersion) : SdkTestsBase
{
    private string SdkVersion { get; } = SdkVersion.Replace('_', '.'); // a little hack to avoid dots breaking tests explorer TODO fix upstream?

    [Before(Test)]
    public void SetupDotnetHome()
    {
        Environment.SetEnvironmentVariable("DOTNET_CLI_HOME", Path.Combine(TestRootPath, ".dotnet"));
    }

    [Before(Test)]
    public void SetupGlobalJson()
    {
        WriteFile(
            "global.json",
            // lang=json
            $$"""
              {
                "sdk": {
                  "version": "{{SdkVersion.Replace('x', '0')}}",
                  "rollForward": "latestPatch",
                  "allowPrerelease": true
                }
              }
              """
        );
    }

    [Before(Test)]
    public void SetupNuGetConfig()
    {
        var opinionPakSdkPackageDirectory = Path.GetDirectoryName(Constants.OpinionPakSdkPackagePath)!;
        var packagesPath = Path.Combine(TestRootPath, ".packages");

        WriteFile(
            "NuGet.config",
            // lang=xml
            $"""
             <configuration>
               <packageSources>
                 <clear />
                 <add key="local" value="{opinionPakSdkPackageDirectory}" />
                 <add key="NuGet.org" value="https://api.nuget.org/v3/index.json" />
               </packageSources>
               <config>
                 <add key="globalPackagesFolder" value="{packagesPath}" />
               </config>
             </configuration>
             """
        );
    }

    private string CreateProject(
        string sdk = $"OpinionPak.Sdk/{Constants.Version}",
        string targetFramework = "net$(NETCoreAppMaximumVersion)",
        bool disableFileHeader = true,
        bool disableNanoVer = true,
        bool disableRootEditorConfig = true
    )
    {
        var document = new XDocument(
            new XElement(
                "Project",
                new XAttribute("Sdk", sdk),
                new XElement(
                    "PropertyGroup",
                    new XElement("TargetFramework", targetFramework),
                    new XElement("SolutionDir", TestRootPath),
                    new XElement("NuGetAudit", "false"),
                    disableFileHeader ? new XElement("GenerateFileHeaderEditorConfig", "false") : null,
                    disableNanoVer ? new XElement("NanoVer", "false") : null,
                    disableRootEditorConfig ? new XElement("GenerateRootEditorConfig", "false") : null
                )
            )
        );

        return WriteFile($"{Path.GetRandomFileName()}.csproj", document.ToString());
    }

    private async Task<BufferedCommandResult> RunAsync(string targetFilePath, string arguments)
    {
        return await Cli.Wrap(targetFilePath)
            .WithArguments(arguments)
            .WithWorkingDirectory(TestRootPath)
            .WithConsolePipe()
            .LogExecute()
            .ExecuteBufferedAsync();
    }

    [Test]
    public async Task VerifyDotnetSdkVersion()
    {
        var result = await RunAsync("dotnet", "--version");

        var pattern = SdkVersion
            .Replace(".", @"\.")
            .Replace("x", @"\d");

        await Assert.That(result.StandardOutput.TrimEnd()).Matches('^' + pattern);
    }

    [Test]
    public async Task SimpleBuild()
    {
        await DotnetBuildCommand.ExecuteAsync(
            project: CreateProject()
        );
    }

    [Test]
    public async Task UsingOpinionPakValueSet()
    {
        var (_, value) = await DotnetBuildCommand.GetPropertyAsync(
            project: CreateProject(),
            propertyName: "UsingOpinionPak"
        );

        await Assert.That(value).IsEqualTo("true");
    }
}
