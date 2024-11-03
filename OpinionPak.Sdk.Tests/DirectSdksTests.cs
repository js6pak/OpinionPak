// SPDX-License-Identifier: LGPL-3.0-only
// SPDX-FileCopyrightText: 2024 js6pak

using Microsoft.Build.Utilities.ProjectCreation;

namespace OpinionPak.Sdk.Tests;

[InheritsTests]
public sealed class DirectSdksTests : TestsBase
{
    protected override ProjectCreator CreateProject(string path, string sdk)
    {
        return ProjectCreator.Create(path, sdk: sdk)
            .Import(Constants.DirectorySdkPropsPath)
            .Property("TargetFramework", ProjectCreatorConstants.SdkCsprojDefaultTargetFramework);
    }
}
