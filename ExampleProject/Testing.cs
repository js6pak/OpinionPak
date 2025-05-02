// SPDX-License-Identifier: LGPL-3.0-only
// SPDX-FileCopyrightText: 2024 js6pak

using System.Diagnostics.CodeAnalysis;

namespace ExampleProject;

[SuppressMessage("Maintainability", "CA1515:Consider making public types internal")]
[SuppressMessage("Style", "IDE0059:Unnecessary assignment of a value")]
[SuppressMessage("StyleCop.CSharp.MaintainabilityRules", "SA1404:Code analysis suppression should have justification")]
#pragma warning disable CS0168, CS0219
public static class Testing
{
    public static void X()
    {
        Console.WriteLine("a");

        Console.WriteLine("b");
    }

    public static int X(int b, int y, int z)
    {
        return 5 + (y * b / 6 % z) - 2;
    }

    public static bool X(bool x, bool y, bool z, bool a, bool b)
    {
        return x || (y && z && a) || b;
    }
}
