// SPDX-License-Identifier: LGPL-3.0-only
// SPDX-FileCopyrightText: 2024 js6pak

namespace OpinionPak.Sdk.Tests;

#pragma warning disable CA1822
public sealed class FailingTest
{
    [Test]
    public void Fail()
    {
        Assert.Fail("yes");
    }
}
