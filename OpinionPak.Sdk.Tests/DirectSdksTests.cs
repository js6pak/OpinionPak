// SPDX-License-Identifier: LGPL-3.0-only
// SPDX-FileCopyrightText: 2024 js6pak

using Microsoft.Build.Utilities.ProjectCreation;
using Xunit.Abstractions;

namespace OpinionPak.Sdk.Tests;

public sealed class DirectSdksTests : TestsBase<DirectSdksTests.Methods>
{
    public DirectSdksTests(ITestOutputHelper output) : base(output)
    {
    }

    public sealed class Methods : ITestsMethods
    {
        public static ProjectCreator CreateProject(string path, string sdk)
        {
            return ProjectCreator.Create(path, sdk: sdk)
                .Import(Constants.DirectorySdkPropsPath)
                .Property("TargetFramework", ProjectCreatorConstants.SdkCsprojDefaultTargetFramework);
        }
    }
}
