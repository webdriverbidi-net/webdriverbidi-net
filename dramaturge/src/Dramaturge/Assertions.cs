// <copyright file="Assertions.cs" company="WebDriverBiDi.NET Committers">
// Copyright (c) WebDriverBiDi.NET Committers. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.
// </copyright>

namespace Dramaturge;

/// <summary>
/// Expectations that are checked again until they are met or their time runs out, for use with any test framework.
/// Import it with <c>using static Dramaturge.Assertions;</c> to write <c>Expect(locator)</c> or <c>Expect(page)</c>.
/// </summary>
public static class Assertions
{
    /// <summary>
    /// Creates the expectations for the elements a locator finds.
    /// </summary>
    /// <param name="locator">The locator.</param>
    /// <returns>The expectations.</returns>
    public static LocatorAssertions Expect(ElementLocator locator)
    {
        return new LocatorAssertions(locator, false);
    }

    /// <summary>
    /// Creates the expectations for a page.
    /// </summary>
    /// <param name="page">The page.</param>
    /// <returns>The expectations.</returns>
    public static PageAssertions Expect(Page page)
    {
        return new PageAssertions(page, false);
    }
}
