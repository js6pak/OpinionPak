// SPDX-License-Identifier: LGPL-3.0-only
// SPDX-FileCopyrightText: 2024 js6pak

using System.Collections;

namespace OpinionPak.Sdk.Tests;

public static class GlobalHooks
{
    [Before(TestSession)]
    public static void ClearEnvironment()
    {
        ReadOnlySpan<string> allowed = ["HOME", "PATH"];

        foreach (DictionaryEntry variable in Environment.GetEnvironmentVariables())
        {
            var name = (string) variable.Key;

            if (!allowed.Contains(name))
            {
                Environment.SetEnvironmentVariable(name, null);
            }
        }

        Environment.SetEnvironmentVariable("DOTNET_CLI_TELEMETRY_OPTOUT", "true");
        Environment.SetEnvironmentVariable("DOTNET_NOLOGO", "true");
    }
}
