// SPDX-License-Identifier: LGPL-3.0-only
// SPDX-FileCopyrightText: 2024 js6pak

using System.Reflection;
using Microsoft.Build.Utilities.ProjectCreation;

namespace OpinionPak.Sdk.Tests;

public static class MSBuildSetupHook
{
    [Before(TestDiscovery)]
    public static void BeforeTestDiscovery()
    {
        // TODO upstream
        var dotNetSdksPathLazyField = typeof(MSBuildAssemblyResolver).GetField("DotNetSdksPathLazy", BindingFlags.NonPublic | BindingFlags.Static)!;
        var dotNetSdksPathLazy = (Lazy<string?>) dotNetSdksPathLazyField.GetValue(null)!;

        SetValue(dotNetSdksPathLazy, Constants.NetSdkPath);

        MSBuildAssemblyResolver.Register();

        static void SetValue<T>(Lazy<T> lazy, T value)
        {
            var stateField = typeof(Lazy<T>).GetField("_state", BindingFlags.NonPublic | BindingFlags.Instance)!;
            var valueField = typeof(Lazy<T>).GetField("_value", BindingFlags.NonPublic | BindingFlags.Instance)!;

            stateField.SetValue(lazy, null);
            valueField.SetValue(lazy, value);
        }
    }
}
