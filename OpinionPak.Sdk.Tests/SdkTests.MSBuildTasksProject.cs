// SPDX-License-Identifier: LGPL-3.0-only
// SPDX-FileCopyrightText: 2024 js6pak

using OpinionPak.Sdk.Tests.Utilities;

namespace OpinionPak.Sdk.Tests;

public partial class SdkTests
{
    [Test]
    public async Task MSBuildTasksProject()
    {
        // Import the sdk via Directory.Build.props so we test for https://github.com/js6pak/OpinionPak/issues/1
        WriteFile(
            "Directory.Build.props",
            // lang=xml
            $"""
             <Project>
                 <Sdk Name="OpinionPak.Sdk" Version="{Constants.Version}" />

                <PropertyGroup>
                    <NuGetAudit>false</NuGetAudit>
                    <GenerateFileHeaderEditorConfig>false</GenerateFileHeaderEditorConfig>
                    <NanoVer>false</NanoVer>
                    <GenerateRootEditorConfig>false</GenerateRootEditorConfig>
                </PropertyGroup>
             </Project>
             """
        );

        var extra = SdkVersion.StartsWith("8.")
            ? """
              <Import Project="$(BeforeMicrosoftNETSdkTargets)" Condition="'$(BeforeMicrosoftNETSdkTargets)' != ''" />
              """
            : string.Empty;

        var project = WriteFile(
            "Example.csproj",
            // lang=xml
            $"""
             <Project Sdk="Microsoft.NET.Sdk">
                 <PropertyGroup>
                     <IsMSBuildTasksProject>true</IsMSBuildTasksProject>
                     <TargetNetSdk>8.0.1xx</TargetNetSdk>
                     <IsPackable>true</IsPackable>
                 </PropertyGroup>

                 {extra}
             </Project>
             """
        );

        WriteFile(
            "ExampleTask.cs",
            // lang=cs
            """
            namespace Example;

            public sealed class ExampleTask : MSBuildTask
            {
                public override bool Execute()
                {
                    Log.LogWarning("Hello!");
                    return true;
                }
            }
            """
        );

        await DotnetBuildCommand.ExecuteAsync(project);
    }
}
