// SPDX-License-Identifier: LGPL-3.0-only
// SPDX-FileCopyrightText: 2024 js6pak

using OpinionPak.Sdk.Tests.Utilities;

namespace OpinionPak.Sdk.Tests;

public partial class SdkTests
{
    [Test]
    public async Task NanoVer()
    {
        var project = CreateProject(disableNanoVer: false);

        await AssertFail("The git repository was not found");

        await RunAsync("git", "init");
        await RunAsync("git", "config --local user.email \"<>\"");
        await RunAsync("git", "config --local user.name \"OpinionPak.Sdk.Tests\"");
        await RunAsync("git", "config --local commit.gpgsign false");
        await RunAsync("git", "config --local tag.gpgsign false");
        await RunAsync("git", "commit --allow-empty -m Initial");

        await AssertFail("No tag found on the current commit", true);
        await AssertVersion("0.1.0-dev", false, warnings: ["No tags found"]);

        await RunAsync("git", "tag 1.2.3 -m \"\"");

        await AssertVersion("1.2.3", true);
        await AssertVersion("1.2.4-dev", false);

        await RunAsync("git", "commit --allow-empty -m Second");

        await AssertFail("No tag found on the current commit", true);
        await AssertVersion("1.2.4-dev", false);

        async Task AssertVersion(string expected, bool publicRelease, string[]? warnings = null)
        {
            var (result, version) = await DotnetBuildCommand.GetPropertyAsync(
                project: project,
                targets: ["NanoVer"],
                properties: new Dictionary<string, string>
                {
                    ["PublicRelease"] = publicRelease.ToString(),
                },
                propertyName: "Version",
                validateNoWarnings: warnings == null
            );

            await Assert.That(version).IsEqualTo(expected);

            if (warnings != null)
            {
                await Assert.That(result.Warnings.Select(w => w.Message)).IsEquivalentTo(warnings);
            }
        }

        async Task AssertFail(string errorMessage, bool publicRelease = false)
        {
            var result = await DotnetBuildCommand.ExecuteAsync(
                project: project,
                targets: ["NanoVer"],
                properties: new Dictionary<string, string>
                {
                    ["PublicRelease"] = publicRelease.ToString(),
                },
                validateSuccess: false
            );

            await Assert.That(result.IsSuccess).IsFalse();
            await Assert.That(result.Errors).ContainsOnly(e => e.Message == errorMessage);
        }
    }
}
