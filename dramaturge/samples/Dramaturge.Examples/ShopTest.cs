// <copyright file="ShopTest.cs" company="WebDriverBiDi.NET Committers">
// Copyright (c) WebDriverBiDi.NET Committers. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.
// </copyright>

namespace Dramaturge.Examples;

/// <summary>
/// The base of a test class: each test gets a page, in a browser of its own, with the shop served.
/// </summary>
public abstract class ShopTest : PageTest
{
    /// <inheritdoc/>
    public override async ValueTask InitializeAsync()
    {
        await base.InitializeAsync();
        await Shop.ServeAsync(this.Page);
    }
}
