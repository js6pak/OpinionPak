// SPDX-License-Identifier: LGPL-3.0-only
// SPDX-FileCopyrightText: 2024 js6pak

using OpinionPak.Sdk.Tests.Utilities;

namespace OpinionPak.Sdk.Tests;

public partial class SdkTests
{
    [Test]
    public async Task RootEditorConfig()
    {
        await DotnetBuildCommand.ExecuteAsync(
            project: CreateProject(disableRootEditorConfig: false)
        );

        await Assert.That(File.Exists(Path.Combine(TestRootPath, ".editorconfig"))).IsTrue();
    }
}
