// SPDX-License-Identifier: LGPL-3.0-only
// SPDX-FileCopyrightText: 2024 js6pak

using Microsoft.Build.Framework;
using Microsoft.Build.Utilities.ProjectCreation;

namespace OpinionPak.Sdk.Tests;

internal static class Extensions
{
    public static IEnumerable<BuildWarningEventArgs> GetFilteredWarningEvents(this BuildOutput buildOutput)
    {
        return buildOutput.WarningEvents.Where(e => !e.ProjectFile.EndsWith("OpinionPak.Sdk.csproj", StringComparison.Ordinal));
    }

    public static async Task AssertNoWarnings(this BuildOutput buildOutput)
    {
        await Assert.That(buildOutput.GetFilteredWarningEvents().Select(e => e.Message)).IsEmpty();
    }
}
