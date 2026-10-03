// <copyright file="LoginTests.cs" company="WebDriverBiDi.NET Committers">
// Copyright (c) WebDriverBiDi.NET Committers. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.
// </copyright>

namespace Dramaturge.Examples;

using System.Text.RegularExpressions;

/// <summary>
/// Signing in, with the shop's API answered by the test.
/// </summary>
public class LoginTests : ShopTest
{
    [Fact]
    public async Task SignedInUserIsWelcomed()
    {
        await this.Page.RouteAsync(Shop.Url + "api/login", route => route.FulfillAsync(200, """{"name":"Ada"}"""));
        await this.Page.NavigateAsync(Shop.Url + "login");

        await this.Page.GetByLabel("User name").FillAsync("ada");
        await this.Page.GetByLabel("Password").FillAsync("correct horse battery staple");
        await this.Page.GetByRole("button", "Sign in").ClickAsync();

        await Expect(this.Page).ToHaveUrlAsync(new Regex("/account$"));
        await Expect(this.Page).ToHaveTitleAsync("Your account");
        await Expect(this.Page.GetByRole("heading")).ToHaveTextAsync("Welcome, Ada");
    }

    [Fact]
    public async Task FailedSignInShowsAnError()
    {
        await this.Page.RouteAsync(Shop.Url + "api/login", route => route.FulfillAsync(503, "Service unavailable"));
        await this.Page.NavigateAsync(Shop.Url + "login");

        await this.Page.GetByLabel("User name").FillAsync("ada");
        await this.Page.GetByRole("button", "Sign in").ClickAsync();

        await Expect(this.Page.GetByRole("alert")).ToHaveTextAsync("Sign-in failed. Try again later.");
        await Expect(this.Page).ToHaveUrlAsync(Shop.Url + "login");
    }
}
