// SPDX-License-Identifier: LGPL-3.0-only
// SPDX-FileCopyrightText: 2024 js6pak

namespace OpinionPak.Sdk.Tests;

public partial class SdkTests
{
    [Test]
    public async Task NanoVer()
    {
        var projectCreator = CreateProject(disableNanoVer: false)
            .Save();

        await AssertFail();

        await RunAsync("git", "init");
        await RunAsync("git", "config --local user.email \"<>\"");
        await RunAsync("git", "config --local user.name \"OpinionPak.Sdk.Tests\"");
        await RunAsync("git", "config --local commit.gpgsign false");
        await RunAsync("git", "config --local tag.gpgsign false");
        await RunAsync("git", "commit --allow-empty -m Initial");

        await AssertFail(true);
        await AssertVersion("0.1.0-dev", ignoreWarnings: true);

        await RunAsync("git", "tag 1.2.3 -m \"\"");

        await AssertVersion("1.2.3", true);
        await AssertVersion("1.2.4-dev");

        await RunAsync("git", "commit --allow-empty -m Second");

        await AssertFail(true);
        await AssertVersion("1.2.4-dev");

        async Task AssertVersion(string expected, bool publicRelease = false, bool ignoreWarnings = false)
        {
            var projectInstance = projectCreator.ProjectInstance;

            projectInstance.SetProperty("PublicRelease", publicRelease.ToString());

            using var buildOutput = await BuildAsync(projectCreator, "NanoVer");

            if (!ignoreWarnings)
            {
                await buildOutput.AssertNoWarnings();
            }

            await Assert.That(projectInstance.GetPropertyValue("Version")).IsEqualTo(expected);
        }

        async Task AssertFail(bool publicRelease = false)
        {
            var projectInstance = projectCreator.ProjectInstance;

            projectInstance.SetProperty("PublicRelease", publicRelease.ToString());

            using var buildOutput = await BuildAsync(projectCreator, "NanoVer", shouldSucceed: false);
        }
    }
}
