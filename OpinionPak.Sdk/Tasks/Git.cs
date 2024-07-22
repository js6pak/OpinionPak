// SPDX-License-Identifier: LGPL-3.0-only
// SPDX-FileCopyrightText: 2024 js6pak

using System.ComponentModel;
using System.Diagnostics;

namespace OpinionPak.Sdk.Tasks;

internal static class Git
{
    public static async Task<bool> IsInsideWorkTree(string workingDirectory)
    {
        return await RevParseBoolean(workingDirectory, "--is-inside-work-tree");
    }

    public static async Task<bool> IsShallowRepository(string workingDirectory)
    {
        return await RevParseBoolean(workingDirectory, "--is-shallow-repository");
    }

    private static async Task<bool> RevParseBoolean(string workingDirectory, string flag)
    {
        var (exitCode, standardOutput, _) = await Run(workingDirectory, "rev-parse", flag);
        return exitCode == 0 && bool.Parse(standardOutput);
    }

    public static async Task<string?> DescribeTag(string workingDirectory, string match, bool currentOnly)
    {
        var arguments = new[] { "describe", "--abbrev=0", "--tags", "--match", match };

        if (currentOnly)
        {
            arguments = [.. arguments, "--contains", "HEAD"];
        }

        var (exitCode, standardOutput, standardError) = await Run(workingDirectory, arguments);

        if (exitCode != 0)
        {
            if (
                standardError == "fatal: No names found, cannot describe anything." ||
                standardError.StartsWith("fatal: cannot describe", StringComparison.Ordinal) ||
                standardError.StartsWith("fatal: No tags can describe", StringComparison.Ordinal)
            )
            {
                return null;
            }

            throw new GitException(standardError);
        }

        if (standardOutput.EndsWith("^0", StringComparison.Ordinal))
        {
            return standardOutput[..^2];
        }

        return standardOutput;
    }

    private const int ErrorFileNotFound = 0x2;

    private static async Task<(int ExitCode, string StandardOutput, string StandardError)> Run(string workingDirectory, params string[] arguments)
    {
        using var process = new Process();

        process.StartInfo = new ProcessStartInfo
        {
            FileName = "git",
            WorkingDirectory = workingDirectory,
            UseShellExecute = false,
            CreateNoWindow = true,
            RedirectStandardOutput = true,
            RedirectStandardError = true,
        };

#if NET5_0_OR_GREATER
        foreach (var argument in arguments)
        {
            process.StartInfo.ArgumentList.Add(argument);
        }
#else
        process.StartInfo.Arguments = string.Join(" ", arguments);
#endif

        try
        {
            process.Start();
        }
        catch (Win32Exception e) when (e.NativeErrorCode == ErrorFileNotFound)
        {
            throw new GitNotFoundException();
        }

#if NET5_0_OR_GREATER
        await process.WaitForExitAsync();
#else
        process.WaitForExit();
#endif

        var standardOutput = await process.StandardOutput.ReadToEndAsync();
        var standardError = await process.StandardError.ReadToEndAsync();

        return (process.ExitCode, standardOutput.TrimEnd(), standardError.TrimEnd());
    }
}

public sealed class GitNotFoundException : Exception
{
    public GitNotFoundException()
    {
    }

    public GitNotFoundException(string message) : base(message)
    {
    }

    public GitNotFoundException(string message, Exception innerException) : base(message, innerException)
    {
    }
}

public sealed class GitException : Exception
{
    public GitException()
    {
    }

    public GitException(string message) : base(message)
    {
    }

    public GitException(string message, Exception innerException) : base(message, innerException)
    {
    }
}
