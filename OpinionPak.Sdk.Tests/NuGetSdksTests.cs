// SPDX-License-Identifier: LGPL-3.0-only
// SPDX-FileCopyrightText: 2024 js6pak

using System.Xml.Linq;
using Microsoft.Build.Utilities.ProjectCreation;
using Xunit.Abstractions;

namespace OpinionPak.Sdk.Tests;

public sealed class NuGetSdksTests : TestsBase<NuGetSdksTests.Methods>
{
    public NuGetSdksTests(ITestOutputHelper output) : base(output)
    {
    }

    public sealed class Methods : ITestsMethods
    {
        public static void ModifyNuGetConfig(string testRootPath, XDocument nuGetConfig)
        {
            var packagesPath = Path.Combine(testRootPath, "packages");

            nuGetConfig.Root!.Add(
                new XElement(
                    "config",
                    new XElement("add", new XAttribute("key", "globalPackagesFolder"), new XAttribute("value", packagesPath))
                )
            );

            var opinionPakSdkPackageDirectory = Path.GetDirectoryName(Constants.OpinionPakSdkPackagePath)!;

            nuGetConfig.Root.Element("packageSources")!
                .Element("clear")!
                .AddAfterSelf(new XElement("add", new XAttribute("key", "local"), new XAttribute("value", opinionPakSdkPackageDirectory)));
        }

        public static ProjectCreator CreateProject(string path, string sdk)
        {
            return ProjectCreator.Create(path, sdk: sdk)
                .Sdk("OpinionPak.Sdk", Constants.Version)
                .Property("TargetFramework", ProjectCreatorConstants.SdkCsprojDefaultTargetFramework);
        }
    }
}
