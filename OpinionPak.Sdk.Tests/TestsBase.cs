// SPDX-License-Identifier: LGPL-3.0-only
// SPDX-FileCopyrightText: 2024 js6pak

using System.Collections;
using Microsoft.Build.Framework;
using Microsoft.Build.Utilities.ProjectCreation;

namespace OpinionPak.Sdk.Tests;

public abstract partial class TestsBase : MSBuildSdkTestBase
{
    [Before(TestSession)]
    public static void ClearEnvironment()
    {
        var allowed = new[] { "HOME", "PATH" };

        foreach (DictionaryEntry variable in Environment.GetEnvironmentVariables())
        {
            var name = (string) variable.Key;

            if (!allowed.Contains(name))
            {
                Environment.SetEnvironmentVariable(name, null);
            }
        }
    }

    protected abstract ProjectCreator CreateProject(string path, string sdk);

    private ProjectCreator CreateProject(
        string sdk = ProjectCreatorConstants.SdkCsprojDefaultSdk,
        bool disableFileHeader = true,
        bool disableNanoVer = true,
        bool disableRootEditorConfig = true
    )
    {
        var projectCreator = CreateProject(GetTempFileWithExtension(".csproj"), sdk);

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

    [Test]
    public async Task UsingOpinionPakValueSet()
    {
        CreateProject()
            .TryGetPropertyValue("UsingOpinionPak", out var propertyValue);

        await Assert.That(propertyValue).IsEqualTo("true");
    }

    private static async Task<BuildOutput> BuildAsync(ProjectCreator projectCreator, string target = "Build", bool shouldSucceed = true)
    {
        projectCreator.TryBuild(restore: true, target, out var result, out var buildOutput);

        await Assert.That(result).IsEqualTo(shouldSucceed).Because(buildOutput.GetConsoleLog());

        await TestContext.Current!.OutputWriter.WriteAsync(buildOutput.GetConsoleLog());

        return buildOutput;
    }

    [Test]
    public async Task SimpleBuild()
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

        using var buildOutput = await BuildAsync(projectCreator);

        await buildOutput.AssertNoWarnings();

        await Assert.That(buildOutput.Messages.High).Contains("86F00AF59170450E9D687652D74A6394");
    }
}
