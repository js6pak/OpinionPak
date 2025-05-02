// SPDX-License-Identifier: LGPL-3.0-only
// SPDX-FileCopyrightText: 2024 js6pak

using Microsoft.Build.Framework;

namespace OpinionPak.Sdk.Tasks;

/// <summary>
/// GetDirectoryNameOfFileAbove but with glob support.
/// </summary>
public sealed class GetDirectoryNameOfFilePatternAbove : MSBuildTask
{
    [Required]
    public required string StartingDirectory { get; init; }

    [Required]
    public required string[] SearchPatterns { get; init; }

    [Output]
    public string? DirectoryName { get; private set; }

    public override bool Execute()
    {
        var lookInDirectory = Path.GetFullPath(StartingDirectory);

        do
        {
            if (SearchPatterns.Any(searchPattern => Directory.GetFiles(lookInDirectory, searchPattern).Length != 0))
            {
                DirectoryName = lookInDirectory;
                return true;
            }

            lookInDirectory = Path.GetDirectoryName(lookInDirectory);
        } while (lookInDirectory != null);

        DirectoryName = string.Empty;
        return true;
    }
}
