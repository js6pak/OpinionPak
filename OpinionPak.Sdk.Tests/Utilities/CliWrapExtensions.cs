// SPDX-License-Identifier: LGPL-3.0-only
// SPDX-FileCopyrightText: 2024 js6pak

using CliWrap;

namespace OpinionPak.Sdk.Tests.Utilities;

internal static class CliWrapExtensions
{
    public static Command WithConsolePipe(this Command command)
    {
        return command
            .WithStandardOutputPipe(PipeTarget.ToDelegate(Console.Out.WriteLine))
            .WithStandardErrorPipe(PipeTarget.ToDelegate(Console.Error.WriteLine));
    }

    public static Command LogExecute(this Command command)
    {
        Console.WriteLine($"> {command.TargetFilePath} {command.Arguments}");
        return command;
    }
}
