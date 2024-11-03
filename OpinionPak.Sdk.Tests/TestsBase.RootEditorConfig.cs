// SPDX-License-Identifier: LGPL-3.0-only
// SPDX-FileCopyrightText: 2024 js6pak

namespace OpinionPak.Sdk.Tests;

public abstract partial class TestsBase
{
    [Test]
    public async Task RootEditorConfig()
    {
        var projectCreator = CreateProject(disableRootEditorConfig: false)
            .Save();

        using var buildOutput = await BuildAsync(projectCreator);

        await buildOutput.AssertNoWarnings();

        await Assert.That(File.Exists(Path.Combine(TestRootPath, ".editorconfig"))).IsTrue();
    }
}
