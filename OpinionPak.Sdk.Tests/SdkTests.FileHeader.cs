// SPDX-License-Identifier: LGPL-3.0-only
// SPDX-FileCopyrightText: 2024 js6pak

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
        var projectCreator = CreateProject(disableFileHeader: false)
            .Property("OutputType", "Exe")
            .Property("Copyright", "2024 js6pak")
            .Property("PackageLicenseExpression", "LGPL-3.0-only")
            .Save();

        await File.WriteAllTextAsync(
            Path.Combine(TestRootPath, "Program.cs"),
            programText + Environment.NewLine
        );

        using var buildOutput = await BuildAsync(projectCreator);

        var warnings = buildOutput.GetFilteredWarningEvents().ToArray();
        var warningCodes = warnings.Select(e => e.Code);

        if (success) await Assert.That(warningCodes).DoesNotContain(FileHeaderMismatch);
        else await Assert.That(warningCodes).Contains(FileHeaderMismatch);

        await Assert.That(warnings.Where(e => e.Code != FileHeaderMismatch).Select(e => e.Message)).IsEmpty();
    }
}
