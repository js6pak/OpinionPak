// SPDX-License-Identifier: LGPL-3.0-only
// SPDX-FileCopyrightText: 2024 js6pak

using OpinionPak.Sdk.Tests.Utilities;

namespace OpinionPak.Sdk.Tests;

public partial class SdkTests
{
    private const string FileHeaderMismatch = "IDE0073";

    [Test]
    [Arguments(
        true,
        /* lang=csharp */
        """
        // SPDX-License-Identifier: LGPL-3.0-only
        // SPDX-FileCopyrightText: 2024 js6pak

        Console.WriteLine("Hello, World!");
        """
    )]
    [Arguments(
        false,
        /* lang=csharp */
        """
        Console.WriteLine("Hello, World!");
        """
    )]
    public async Task FileHeader(bool success, string programText)
    {
        await File.WriteAllTextAsync(
            Path.Combine(TestRootPath, "Program.cs"),
            programText + Environment.NewLine
        );

        var result = await DotnetBuildCommand.ExecuteAsync(
            project: CreateProject(disableFileHeader: false),
            properties: new Dictionary<string, string>
            {
                ["OutputType"] = "Exe",
                ["Copyright"] = "2024 js6pak",
                ["PackageLicenseExpression"] = "LGPL-3.0-only",
            },
            validateNoWarnings: false
        );

        var warnings = result.Warnings.ToArray();
        var warningCodes = warnings.Select(e => e.Code);

        if (success) await Assert.That(warningCodes).DoesNotContain(FileHeaderMismatch);
        else await Assert.That(warningCodes).Contains(FileHeaderMismatch);

        await Assert.That(warnings.Where(e => e.Code != FileHeaderMismatch)).IsEmpty();
    }
}
