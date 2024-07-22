// SPDX-License-Identifier: LGPL-3.0-only
// SPDX-FileCopyrightText: 2024 js6pak

using Microsoft.Build.Framework;

namespace OpinionPak.Sdk.Tasks;

public sealed class ReplaceInFile : MSBuildTask
{
    [Required]
    public required string InputFile { get; init; }

    [Required]
    public required string? OutputFile { get; init; }

    [Required]
    public required ITaskItem[] Patterns { get; init; }

    public override bool Execute()
    {
        var outputFile = string.IsNullOrEmpty(OutputFile) ? InputFile : OutputFile;

        var text = File.ReadAllText(InputFile);

        foreach (var pattern in Patterns)
        {
            var replacement = pattern.GetMetadata("Replacement") ?? string.Empty;

#if NET
            text = text.Replace(pattern.ItemSpec, replacement, StringComparison.Ordinal);
#else
            text = text.Replace(pattern.ItemSpec, replacement);
#endif
        }

        Directory.CreateDirectory(Path.GetDirectoryName(outputFile)!);
        File.WriteAllText(outputFile, text);

        return true;
    }
}
