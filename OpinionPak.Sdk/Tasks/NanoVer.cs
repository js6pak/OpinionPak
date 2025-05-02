// SPDX-License-Identifier: LGPL-3.0-only
// SPDX-FileCopyrightText: 2024 js6pak

using System.Globalization;
using System.Text.RegularExpressions;
using Microsoft.Build.Framework;
using NuGet.Versioning;

namespace OpinionPak.Sdk.Tasks;

public sealed class NanoVer : MSBuildTask
{
    private const string SimpleVersionGlob = "[0-9]*.[0-9]*.[0-9]*";

    [Required]
    public required string WorkingDirectory { get; init; }

    public required bool PublicRelease { get; init; }

    [Required]
    public required string MinimumMajorMinor { get; init; }

    [Required]
    public required string VersionSuffix { get; init; }

    [Output]
    public string? Version { get; private set; }

    public string TagPrefix { get; init; } = string.Empty;

    public async Task<bool> ExecuteAsync()
    {
        if (!TryParseMinimumMajorMinor(MinimumMajorMinor, out var minimumMinor, out var minimumMajor))
        {
            Log.LogError("Failed to parse NanoVerMinimumMajorMinor");
            return false;
        }

        try
        {
            if (!await Git.IsInsideWorkTreeAsync(WorkingDirectory))
            {
                Log.LogError("The git repository was not found");
                return false;
            }

            if (await Git.IsShallowRepositoryAsync(WorkingDirectory))
            {
                Log.LogError("The git repository can't be shallow");
                return false;
            }

            var tag = await Git.DescribeTagAsync(WorkingDirectory, $"{TagPrefix}{SimpleVersionGlob}", PublicRelease);
            SemanticVersion version;

            if (tag != null)
            {
                version = SemanticVersion.Parse(tag[TagPrefix.Length..]);

                if (!PublicRelease && !version.IsPrerelease)
                {
                    version = new SemanticVersion(version.Major, version.Minor, version.Patch + 1, version.Release);
                }
            }
            else
            {
                if (PublicRelease)
                {
                    Log.LogError("No tag found on the current commit");
                    return false;
                }

                Log.LogWarning("No tags found");
                version = new SemanticVersion(0, 0, 0);
            }

            if (version.Major < minimumMajor || (version.Major == minimumMajor && version.Minor < minimumMinor))
            {
                if (PublicRelease)
                {
                    Log.LogError("Tagged version can't be lower than MinimumMajorMinor");
                    return false;
                }

                version = new SemanticVersion(minimumMajor, minimumMinor, version.Patch, version.Release);
            }

            if (!PublicRelease)
            {
                version = new SemanticVersion(version.Major, version.Minor, version.Patch, VersionSuffix);
            }

            Version = version.ToNormalizedString();
        }
        catch (GitNotFoundException)
        {
            Log.LogError("Git command not found");
            return false;
        }

        return true;
    }

    private static bool TryParseMinimumMajorMinor(string value, out int minimumMinor, out int minimumMajor)
    {
        var minimumMajorMinorRegex = new Regex(@"^(?<major>0|[1-9]\d*)\.(?<minor>0|[1-9]\d*)$", RegexOptions.None, TimeSpan.FromSeconds(1));

        var match = minimumMajorMinorRegex.Match(value);
        if (
            match.Success
            && int.TryParse(match.Groups["major"].Value, NumberStyles.None, CultureInfo.InvariantCulture, out minimumMajor)
            && int.TryParse(match.Groups["minor"].Value, NumberStyles.None, CultureInfo.InvariantCulture, out minimumMinor)
        )
        {
            return true;
        }

        minimumMinor = default;
        minimumMajor = default;
        return false;
    }

    public override bool Execute()
    {
        return ExecuteAsync().GetAwaiter().GetResult();
    }
}
