// SPDX-License-Identifier: LGPL-3.0-only
// SPDX-FileCopyrightText: 2024 js6pak

// Based on https://github.com/microsoft/MSBuildSdks/blob/48931d2669fa40de04889422def7c5e7500a6fa6/src/TestShared/MSBuildSdkTestBase.cs

using System.Xml.Linq;

namespace OpinionPak.Sdk.Tests;

public abstract class MSBuildSdkTestBase : IDisposable
{
    private readonly string _testRootPath = Path.Combine(Path.GetTempPath(), Path.GetRandomFileName());

    protected virtual void ModifyNuGetConfig(string testRootPath, XDocument nuGetConfig)
    {
    }

    [Before(Test)]
    public void SetupNuGet()
    {
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

        ModifyNuGetConfig(TestRootPath, nuGetConfig);

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
