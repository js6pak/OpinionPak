// SPDX-License-Identifier: LGPL-3.0-only
// SPDX-FileCopyrightText: 2024 js6pak

using System.Collections.Immutable;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CodeStyle;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.CodeStyle;
using Microsoft.CodeAnalysis.CSharp.Formatting;
using Microsoft.CodeAnalysis.Editing;
using Microsoft.CodeAnalysis.Options;

namespace EditorConfigGenerator;

internal sealed class Program
{
    public static int Main(string[] args)
    {
        var verifyNoChanges = args.Contains("--verify-no-changes");

        if (!GenerateStyleEditorConfigFile(verifyNoChanges))
        {
            return 1;
        }

        return 0;
    }

    private static bool GenerateStyleEditorConfigFile(bool verifyNoChanges)
    {
        const string StyleEditorConfigFilePath = "../OpinionPak.Sdk/Configuration/Style.globalconfig";

        var analyzerConfigSet = ReadAnalyzerConfigs(StyleEditorConfigFilePath);

        var optionsReader = new AnalyzerConfigOptionsReader(new AnalyzerConfigSetOptions(analyzerConfigSet));

        ImmutableArray<(string Feature, ImmutableArray<IOption2> Options)> groupOptions =
        [
            (WorkspacesResources.dot_NET_Coding_Conventions, [
                .. GenerationOptions.AllOptions,
                .. CodeStyleOptions2.AllOptions.Remove(CodeStyleOptions2.FileHeaderTemplate),
            ]),
            (CSharpWorkspaceResources.CSharp_Coding_Conventions, CSharpCodeStyleOptions.AllOptions),
            (CSharpWorkspaceResources.CSharp_Formatting_Rules, CSharpFormattingOptions2.AllOptions),
        ];

        var contents = StyleEditorConfigFileGenerator.Generate(groupOptions, optionsReader, LanguageNames.CSharp);

        if (verifyNoChanges)
        {
            if (File.ReadAllText(StyleEditorConfigFilePath) != contents)
            {
                Console.Error.WriteLine("Style.globalconfig was changed but EditorConfigGenerator wasn't rerun");
                return false;
            }
        }
        else
        {
            File.WriteAllText(StyleEditorConfigFilePath, contents);
        }

        return true;
    }

    private static AnalyzerConfigSet ReadAnalyzerConfigs(params string[] paths)
    {
        var analyzerConfigs = paths
            .Select(path => AnalyzerConfig.Parse(File.ReadAllText(path), "/" + Path.GetFileName(path)))
            .ToImmutableArray();

        return AnalyzerConfigSet.Create(analyzerConfigs);
    }
}
