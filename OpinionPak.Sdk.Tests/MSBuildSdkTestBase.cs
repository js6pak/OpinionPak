// SPDX-License-Identifier: LGPL-3.0-only
// SPDX-FileCopyrightText: 2024 js6pak

// Based on https://github.com/microsoft/MSBuildSdks/blob/48931d2669fa40de04889422def7c5e7500a6fa6/src/TestShared/MSBuildSdkTestBase.cs

using System.Reflection;
using System.Xml.Linq;
using Microsoft.Build.Utilities.ProjectCreation;

namespace OpinionPak.Sdk.Tests;

public abstract class MSBuildSdkTestBase : IDisposable
{
    private readonly string _testRootPath = Path.Combine(Path.GetTempPath(), Path.GetRandomFileName());

    static MSBuildSdkTestBase()
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

    protected MSBuildSdkTestBase(Action<string, XDocument> modifyNuGetConfig)
    {
        ArgumentNullException.ThrowIfNull(modifyNuGetConfig);

        var nuGetConfig = new XDocument(
            new XElement(
                "configuration",
                new XElement(
                    "packageSources",
                    new XElement("clear"),
                    new XElement("add", new XAttribute("key", "NuGet.org"), new XAttribute("value", "https://api.nuget.org/v3/index.json"))
                )
            )
        );

        modifyNuGetConfig(TestRootPath, nuGetConfig);

        File.WriteAllText(Path.Combine(TestRootPath, "NuGet.config"), nuGetConfig.ToString());
    }

    public string TestRootPath
    {
        get
        {
            Directory.CreateDirectory(_testRootPath);
            return _testRootPath;
        }
    }

    public void Dispose()
    {
        Dispose(true);
        GC.SuppressFinalize(this);
    }

    protected virtual void Dispose(bool disposing)
    {
        if (!disposing) return;
        if (!Directory.Exists(TestRootPath)) return;

        try
        {
            Directory.Delete(TestRootPath, recursive: true);
        }
        catch (Exception)
        {
            try
            {
                Thread.Sleep(500);

                Directory.Delete(TestRootPath, recursive: true);
            }
            catch (Exception)
            {
                // Ignored
            }
        }
    }

    protected string GetTempFileWithExtension(string? extension = null)
    {
        return Path.Combine(TestRootPath, $"{Path.GetRandomFileName()}{extension ?? string.Empty}");
    }
}
