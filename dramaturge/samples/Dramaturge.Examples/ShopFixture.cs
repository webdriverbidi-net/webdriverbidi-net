// <copyright file="ShopFixture.cs" company="WebDriverBiDi.NET Committers">
// Copyright (c) WebDriverBiDi.NET Committers. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.
// </copyright>

[assembly: AssemblyFixture(typeof(Dramaturge.Examples.ShopFixture))]

namespace Dramaturge.Examples;

/// <summary>
/// The browser every test shares, launched once for the run, with settings for every test. The environment chooses
/// the browser: DRAMATURGE_BROWSER, and CHROME_EXECUTABLE or FIREFOX_EXECUTABLE for an installed one.
/// </summary>
public sealed class ShopFixture : DramaturgeAssemblyFixture
{
    /// <inheritdoc/>
    protected override DramaturgeOptions? GroupOptions => new() { ActionTimeout = TimeSpan.FromSeconds(10), TestIdAttribute = "data-test" };
}
