// SPDX-License-Identifier: LGPL-3.0-only
// SPDX-FileCopyrightText: 2024 js6pak

// Based on https://github.com/dotnet/sdk/blob/v8.0.200/src/Tasks/Microsoft.NET.Build.Tasks/FilterResolvedFiles.cs

using Microsoft.Build.Framework;
using Microsoft.Build.Utilities;
using NuGet.Common;
using NuGet.Packaging.Core;
using NuGet.ProjectModel;

namespace OpinionPak.Sdk.Tasks;

public sealed class GetTransitiveDependencies : MSBuildTask
{
    [Required]
    public required string AssetsFilePath { get; init; }

    [Required]
    public required ITaskItem[] Packages { get; init; }

    public required bool IgnoreVersion { get; init; }

    [Required]
    public required string TargetFramework { get; init; }

    public required string RuntimeIdentifier { get; init; }

    [Output]
    public ITaskItem[] TransitivePackages { get; private set; } = null!;

    public override bool Execute()
    {
        var lockFile = LockFileUtilities.GetLockFile(AssetsFilePath, NullLogger.Instance);
        var lockFileTarget = lockFile.GetTarget(TargetFramework, RuntimeIdentifier);

        var runtimeLibraries = lockFileTarget.Libraries;
        var libraryLookup = runtimeLibraries.ToDictionary(e => e.Name!, StringComparer.OrdinalIgnoreCase);

        var transitivePackages = new HashSet<PackageIdentity>();

        foreach (var packageItem in Packages)
        {
            var packageName = packageItem.ItemSpec;
            if (!string.IsNullOrEmpty(packageName))
            {
                var package = lockFileTarget.GetTargetLibrary(packageName);
                if (package == null)
                {
                    Log.LogError($"Package {packageName} not found");
                    return false;
                }

                transitivePackages.UnionWith(GetTransitivePackagesList(package, libraryLookup, IgnoreVersion));
            }
        }

        var transitivePackageItems = new List<ITaskItem>();

        foreach (var transitivePackage in transitivePackages)
        {
            var item = new TaskItem(transitivePackage.Id);
            item.SetMetadata("Version", transitivePackage.Version.ToString());
            transitivePackageItems.Add(item);
        }

        TransitivePackages = transitivePackageItems.ToArray();

        return true;
    }

    private static HashSet<PackageIdentity> GetTransitivePackagesList(LockFileTargetLibrary package, IDictionary<string, LockFileTargetLibrary> libraryLookup, bool ignoreVersion = false)
    {
        var exclusionList = new HashSet<PackageIdentity>
        {
            new(package.Name, package.Version),
        };

        CollectDependencies(libraryLookup, package.Dependencies, exclusionList, ignoreVersion);

        return exclusionList;
    }

    private static void CollectDependencies(IDictionary<string, LockFileTargetLibrary> libraryLookup, IEnumerable<PackageDependency> dependencies, HashSet<PackageIdentity> exclusionList, bool ignoreVersion = false)
    {
        foreach (var dependency in dependencies)
        {
            if (!libraryLookup.TryGetValue(dependency.Id, out var library))
            {
                continue;
            }

            if (library.Version!.Equals(dependency.VersionRange.MinVersion) || ignoreVersion)
            {
                if (exclusionList.Add(new PackageIdentity(library.Name, library.Version)))
                {
                    CollectDependencies(libraryLookup, library.Dependencies, exclusionList, ignoreVersion);
                }
            }
        }
    }
}
