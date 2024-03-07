// SPDX-License-Identifier: LGPL-3.0-only
// SPDX-FileCopyrightText: 2024 js6pak

using Microsoft.Build.Framework;
using NuGet.Packaging.Core;
using NuGet.Versioning;

namespace OpinionPak.Sdk.Tasks;

public sealed class FilterByPackageIdentity : MSBuildTask
{
    [Required]
    public required ITaskItem[] Input { get; init; }

    [Required]
    public required ITaskItem[] Packages { get; init; }

    public required bool IgnoreVersion { get; init; }

    [Output]
    public ITaskItem[] Output { get; private set; } = null!;

    public override bool Execute()
    {
        var packageIdentityComparer = IgnoreVersion
            ? new PackageIdentityComparer(new IgnoreVersionComparer())
            : PackageIdentityComparer.Default;

        var packages = new HashSet<PackageIdentity>(packageIdentityComparer);

        foreach (var item in Packages)
        {
            var name = item.ItemSpec;
            var version = item.GetMetadata("Version");

            packages.Add(
                new PackageIdentity(
                    name,
                    string.IsNullOrEmpty(version) ? null : NuGetVersion.Parse(version)
                )
            );
        }

        var output = new List<ITaskItem>();

        foreach (var item in Input)
        {
            var packageIdentity = GetPackageIdentity(item);
            if (packageIdentity == null)
            {
                continue;
            }

            if (packages.Contains(packageIdentity))
            {
                output.Add(item);
            }
        }

        Output = output.ToArray();

        return true;
    }

    private static PackageIdentity? GetPackageIdentity(ITaskItem item)
    {
        var packageName = item.GetMetadata("NuGetPackageId");
        var packageVersion = item.GetMetadata("NuGetPackageVersion");

        if (string.IsNullOrEmpty(packageName))
        {
            return null;
        }

        return new PackageIdentity(
            packageName,
            string.IsNullOrEmpty(packageVersion) ? null : NuGetVersion.Parse(packageVersion)
        );
    }

    private sealed class IgnoreVersionComparer : IVersionComparer
    {
        public bool Equals(SemanticVersion? x, SemanticVersion? y)
        {
            return true;
        }

        public int GetHashCode(SemanticVersion obj)
        {
            return 0;
        }

        public int Compare(SemanticVersion? x, SemanticVersion? y)
        {
            return 0;
        }
    }
}
