// SPDX-License-Identifier: LGPL-3.0-only
// SPDX-FileCopyrightText: 2024 js6pak

namespace OpinionPak.Sdk.Tests;

public abstract class SdkTestsBase : IDisposable
{
    protected string TestRootPath { get; } = Path.Combine(Path.GetTempPath(), Path.GetRandomFileName());

    protected SdkTestsBase()
    {
        Directory.CreateDirectory(TestRootPath);
    }

    public void Dispose()
    {
        Dispose(true);
        GC.SuppressFinalize(this);
    }

    protected virtual void Dispose(bool disposing)
    {
        if (!disposing) return;

        try
        {
            Directory.Delete(TestRootPath, recursive: true);
        }
        catch (DirectoryNotFoundException)
        {
            // ignored
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
                // ignored
            }
        }
    }

    protected string WriteFile(string fileName, string contents)
    {
        var path = Path.Combine(TestRootPath, fileName);
        File.WriteAllText(path, contents);
        return path;
    }
}
