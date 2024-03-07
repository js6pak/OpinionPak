// SPDX-License-Identifier: LGPL-3.0-only
// SPDX-FileCopyrightText: 2024 js6pak

// Based on https://github.com/dotnet/roslyn/blob/026c96327b02c5ce4d3208f821e02d2ffa825312/src/Workspaces/Core/Portable/Options/EditorConfig/EditorConfigFileGenerator.cs

using System.Collections.Immutable;
using System.Diagnostics.CodeAnalysis;
using System.Text;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Diagnostics;
using Microsoft.CodeAnalysis.Options;

namespace EditorConfigGenerator;

internal static class StyleEditorConfigFileGenerator
{
    public static string Generate(
        ImmutableArray<(string Feature, ImmutableArray<IOption2> Options)> groupedOptions,
        IOptionsReader configOptions,
        string language
    )
    {
        var editorconfig = new StringBuilder();

        editorconfig.AppendLine("is_global = true");
        editorconfig.AppendLine("global_level = -50");
        editorconfig.AppendLine();

        foreach (var (feature, options) in groupedOptions)
        {
            editorconfig.AppendLine($"#### {feature} ####");

            foreach (var optionGrouping in options.GroupBy(o => o.Definition.Group).OrderBy(g => g.Key.Priority))
            {
                editorconfig.AppendLine();
                editorconfig.AppendLine($"# {optionGrouping.Key.Description}");

                foreach (var option in optionGrouping)
                {
                    var optionKey = new OptionKey2(option, option.IsPerLanguage ? language : null);
                    var value = configOptions.TryGetOption<object?>(optionKey, out var existingValue)
                        ? existingValue
                        : option.DefaultValue;

                    if (!Equals(value, option.DefaultValue))
                    {
                        editorconfig.AppendLine($"; default: {option.Definition.Serializer.Serialize(option.DefaultValue)}");
                    }

                    editorconfig.AppendLine($"{option.Definition.ConfigName} = {option.Definition.Serializer.Serialize(value)}");
                }
            }
        }

        return editorconfig.ToString();
    }
}

internal sealed class AnalyzerConfigSetOptions(AnalyzerConfigSet analyzerConfigSet) : AnalyzerConfigOptions
{
    public override bool TryGetValue(string key, [NotNullWhen(true)] out string? value)
    {
        return analyzerConfigSet.GlobalConfigOptions.AnalyzerOptions.TryGetValue(key, out value);
    }
}
