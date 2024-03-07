// SPDX-License-Identifier: LGPL-3.0-only
// SPDX-FileCopyrightText: 2024 js6pak

using Xunit;

namespace OpinionPak.Sdk.Tests;

public abstract partial class TestsBase<T>
{
    private const string FileHeaderMismatch = "IDE0073";

    [Theory]
    [InlineData(
        true,
        /* lang=csharp */
        """
        // SPDX-License-Identifier: LGPL-3.0-only
        // SPDX-FileCopyrightText: 2024 js6pak

        Console.WriteLine("Hello, World!");
        """
    )]
    [InlineData(
        false,
        /* lang=csharp */
        """
        Console.WriteLine("Hello, World!");
        """
    )]
    public void FileHeader(bool success, string programText)
    {
        var projectCreator = CreateProject(disableFileHeader: false)
            .Property("OutputType", "Exe")
            .Property("Copyright", "2024 js6pak")
            .Property("PackageLicenseExpression", "LGPL-3.0-only")
            .Save();

        File.WriteAllText(
            Path.Combine(TestRootPath, "Program.cs"),
            programText + Environment.NewLine
        );

        using var buildOutput = Build(projectCreator);

        var warnings = buildOutput.GetFilteredWarningEvents().ToArray();

        if (success) Assert.DoesNotContain(warnings, e => e.Code == FileHeaderMismatch);
        else Assert.Contains(warnings, e => e.Code == FileHeaderMismatch);

        Assert.Empty(warnings.Where(e => e.Code != FileHeaderMismatch).Select(e => e.Message));
    }
}
