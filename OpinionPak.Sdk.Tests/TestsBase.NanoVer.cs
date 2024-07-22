// SPDX-License-Identifier: LGPL-3.0-only
// SPDX-FileCopyrightText: 2024 js6pak

using Xunit;

namespace OpinionPak.Sdk.Tests;

public abstract partial class TestsBase<T>
{
    [Fact]
    public async Task NanoVer()
    {
        var projectCreator = CreateProject(disableNanoVer: false)
            .Save();

        AssertFail();

        await RunAsync("git", "init");
        await RunAsync("git", "config --local user.email \"<>\"");
        await RunAsync("git", "config --local user.name \"OpinionPak.Sdk.Tests\"");
        await RunAsync("git", "config --local commit.gpgsign false");
        await RunAsync("git", "config --local tag.gpgsign false");
        await RunAsync("git", "commit --allow-empty -m Initial");

        AssertFail(true);
        AssertVersion("0.1.0-dev", ignoreWarnings: true);

        await RunAsync("git", "tag 1.2.3 -m \"\"");

        AssertVersion("1.2.3", true);
        AssertVersion("1.2.4-dev");

        await RunAsync("git", "commit --allow-empty -m Second");

        AssertFail(true);
        AssertVersion("1.2.4-dev");

        void AssertVersion(string expected, bool publicRelease = false, bool ignoreWarnings = false)
        {
            var projectInstance = projectCreator.ProjectInstance;

            projectInstance.SetProperty("PublicRelease", publicRelease.ToString());

            using var buildOutput = Build(projectCreator, "NanoVer");

            if (!ignoreWarnings)
            {
                buildOutput.AssertNoWarnings();
            }

            Assert.Equal(expected, projectInstance.GetPropertyValue("Version"));
        }

        void AssertFail(bool publicRelease = false)
        {
            var projectInstance = projectCreator.ProjectInstance;

            projectInstance.SetProperty("PublicRelease", publicRelease.ToString());

            using var buildOutput = Build(projectCreator, "NanoVer", shouldSucceed: false);
        }
    }
}
