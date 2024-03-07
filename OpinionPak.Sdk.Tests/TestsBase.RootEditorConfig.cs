// SPDX-License-Identifier: LGPL-3.0-only
// SPDX-FileCopyrightText: 2024 js6pak

using Xunit;

namespace OpinionPak.Sdk.Tests;

public abstract partial class TestsBase<T>
{
    [Fact]
    public void RootEditorConfig()
    {
        var projectCreator = CreateProject(disableRootEditorConfig: false)
            .Save();

        using var buildOutput = Build(projectCreator);

        buildOutput.AssertNoWarnings();

        Assert.True(File.Exists(Path.Combine(TestRootPath, ".editorconfig")));
    }
}
