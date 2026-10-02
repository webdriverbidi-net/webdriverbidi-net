// <copyright file="LocatorsSamples.cs" company="WebDriverBiDi.NET Committers">
// Copyright (c) WebDriverBiDi.NET Committers. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.
// </copyright>
// Code snippets for docs/articles/locators.md

namespace Dramaturge.Docs.Code;

using WebDriverBiDi.BrowsingContext;

/// <summary>
/// Snippets for the locators guide. Compiled at build time to prevent API drift.
/// </summary>
public static class LocatorsSamples
{
    /// <summary>
    /// Finding elements the way a user sees them.
    /// </summary>
    /// <param name="page">A page.</param>
    /// <returns>A task representing the asynchronous operation.</returns>
    public static async Task UserFacing(Page page)
    {
        #region UserFacing
        await page.GetByRole("button", "Sign in").ClickAsync();
        await page.GetByLabel("Email address").FillAsync("ada@example.com");
        await page.GetByPlaceholder("Search").FillAsync("lovelace");
        await page.GetByText("Forgot your password?").ClickAsync();
        await page.GetByAltText("Company logo").HoverAsync();
        await page.GetByTitle("Close").ClickAsync();
        await page.GetByTestId("checkout").ClickAsync();
        #endregion
    }

    /// <summary>
    /// Finding elements by role and state.
    /// </summary>
    /// <param name="page">A page.</param>
    /// <returns>A task representing the asynchronous operation.</returns>
    public static async Task RoleStatesSample(Page page)
    {
        #region RoleStates
        await page.GetByRole("checkbox", "Remember me", new RoleStates() { Checked = ToggleState.Off }).CheckAsync();
        await page.GetByRole("heading", states: new RoleStates() { Level = 2 }).First().ClickAsync();
        await page.GetByRole("button", "Menu", new RoleStates() { Expanded = false }).ClickAsync();
        #endregion
    }

    /// <summary>
    /// Finding elements with the protocol's own locators.
    /// </summary>
    /// <param name="page">A page.</param>
    /// <returns>A task representing the asynchronous operation.</returns>
    public static async Task ProtocolLocators(Page page)
    {
        #region ProtocolLocators
        await page.Locate(new CssLocator("form#login button[type=submit]")).ClickAsync();
        await page.Locate(new XPathLocator("//table/tbody/tr[1]/td[2]")).ClickAsync();
        #endregion
    }

    /// <summary>
    /// Chaining and narrowing locators.
    /// </summary>
    /// <param name="page">A page.</param>
    /// <returns>A task representing the asynchronous operation.</returns>
    public static async Task Chaining(Page page)
    {
        #region Chaining
        // Within the dialog, the button named Save.
        ElementLocator dialog = page.GetByRole("dialog");
        await dialog.GetByRole("button", "Save").ClickAsync();

        // The row that mentions Ada and has a Delete button, then that button.
        ElementLocator row = page.GetByRole("row").Filter(hasText: "Ada", has: page.GetByRole("button", "Delete"));
        await row.GetByRole("button", "Delete").ClickAsync();

        // Elements both locators find, and elements either finds.
        ElementLocator enabledSubmit = page.GetByRole("button").And(page.Locate(new CssLocator("[type=submit]:enabled")));
        ElementLocator signInOrUp = page.GetByRole("link", "Sign in").Or(page.GetByRole("link", "Sign up"));
        #endregion
    }

    /// <summary>
    /// Choosing one of several matches.
    /// </summary>
    /// <param name="page">A page.</param>
    /// <returns>A task representing the asynchronous operation.</returns>
    public static async Task Choosing(Page page)
    {
        #region Choosing
        ElementLocator items = page.GetByRole("listitem");
        int count = await items.CountAsync();
        await items.First().ClickAsync();
        await items.Nth(2).ClickAsync();
        await items.Last().ClickAsync();
        #endregion
    }

    /// <summary>
    /// Reaching into shadow roots and frames.
    /// </summary>
    /// <param name="page">A page.</param>
    /// <returns>A task representing the asynchronous operation.</returns>
    public static async Task ShadowAndFrames(Page page)
    {
        #region ShadowAndFrames
        // A closed shadow root is reached from its host.
        await page.Locate(new CssLocator("date-picker")).ShadowRoot().GetByRole("button", "Next month").ClickAsync();

        // A frame is found from the element that holds it.
        Frame payment = await page.Locate(new CssLocator("iframe[name=payment]")).ContentFrameAsync();
        await payment.GetByLabel("Card number").FillAsync("4242 4242 4242 4242");
        #endregion
    }

    /// <summary>
    /// Waiting for an element's state.
    /// </summary>
    /// <param name="page">A page.</param>
    /// <returns>A task representing the asynchronous operation.</returns>
    public static async Task Waiting(Page page)
    {
        #region Waiting
        await page.GetByRole("progressbar").WaitForAsync(ElementState.Detached);
        #endregion
    }
}
