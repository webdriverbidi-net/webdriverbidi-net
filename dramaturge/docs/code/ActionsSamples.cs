// <copyright file="ActionsSamples.cs" company="WebDriverBiDi.NET Committers">
// Copyright (c) WebDriverBiDi.NET Committers. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.
// </copyright>
// Code snippets for docs/articles/actions.md

namespace Dramaturge.Docs.Code;

using WebDriverBiDi.Input;
using WebDriverBiDi.Script;

/// <summary>
/// Snippets for the actions guide. Compiled at build time to prevent API drift.
/// </summary>
public static class ActionsSamples
{
    /// <summary>
    /// Pointer actions.
    /// </summary>
    /// <param name="page">A page.</param>
    /// <returns>A task representing the asynchronous operation.</returns>
    public static async Task Pointer(Page page)
    {
        #region Pointer
        await page.GetByRole("button", "Save").ClickAsync();
        await page.GetByText("README.md").DblClickAsync();
        await page.GetByRole("link", "Docs").ClickAsync(new ClickOptions() { Modifiers = KeyModifiers.Control });
        await page.GetByRole("row").First().ClickAsync(new ClickOptions() { Button = PointerButton.Right });
        await page.GetByRole("menuitem", "File").HoverAsync();
        await page.GetByRole("slider").ClickAsync(new ClickOptions() { Offset = new PointerOffset(40, 0) });
        await page.GetByRole("button", "Like").TapAsync();
        await page.GetByText("Card 1").DragToAsync(page.GetByRole("region", "Done"));
        #endregion
    }

    /// <summary>
    /// Text entry.
    /// </summary>
    /// <param name="page">A page.</param>
    /// <returns>A task representing the asynchronous operation.</returns>
    public static async Task Text(Page page)
    {
        #region Text
        ElementLocator search = page.GetByRole("searchbox");
        await search.FillAsync("webdriver bidi");
        await search.PressAsync(Keys.Enter);

        // Types each character as a key press, for pages that react to each key, such as an autocomplete.
        await search.ClearAsync();
        await search.PressSequentiallyAsync("drama", new PressSequentiallyOptions() { Delay = TimeSpan.FromMilliseconds(50) });

        await search.PressAsync("a", new KeyActionOptions() { Modifiers = KeyModifiers.Control });
        #endregion
    }

    /// <summary>
    /// Form controls.
    /// </summary>
    /// <param name="page">A page.</param>
    /// <returns>A task representing the asynchronous operation.</returns>
    public static async Task FormControls(Page page)
    {
        #region FormControls
        await page.GetByLabel("I agree to the terms").CheckAsync();
        await page.GetByLabel("Send me news").SetCheckedAsync(false);
        await page.GetByLabel("Express delivery").CheckAsync();

        IReadOnlyList<string> selected = await page.GetByLabel("Country").SelectOptionAsync([SelectOption.ByLabel("Norway")]);
        await page.GetByLabel("Toppings").SelectOptionAsync([SelectOption.ByValue("cheese"), SelectOption.ByIndex(3)]);

        await page.GetByLabel("Attachment").SetInputFilesAsync(["/home/ada/report.pdf"]);
        #endregion
    }

    /// <summary>
    /// Acting without waiting.
    /// </summary>
    /// <param name="page">A page.</param>
    /// <returns>A task representing the asynchronous operation.</returns>
    public static async Task Force(Page page)
    {
        #region Force
        // Clicks even though another element covers it, such as a translucent overlay the page leaves in place.
        await page.GetByRole("button", "Close").ClickAsync(new ClickOptions() { Force = true });
        #endregion
    }

    /// <summary>
    /// Other element actions.
    /// </summary>
    /// <param name="page">A page.</param>
    /// <returns>A task representing the asynchronous operation.</returns>
    public static async Task Other(Page page)
    {
        #region Other
        ElementLocator field = page.GetByLabel("Name");
        await field.FocusAsync();
        await field.BlurAsync();
        await page.GetByText("Terms").ScrollIntoViewIfNeededAsync();

        // Dispatches a synthetic event, which the page sees but which no user action caused.
        bool notCanceled = await page.GetByRole("button", "Save").DispatchEventAsync(
            "click",
            new Dictionary<string, LocalValue>() { ["detail"] = LocalValue.Number(1) });
        #endregion
    }

    /// <summary>
    /// The page's own mouse and keyboard.
    /// </summary>
    /// <param name="page">A page.</param>
    /// <returns>A task representing the asynchronous operation.</returns>
    public static async Task MouseAndKeyboard(Page page)
    {
        #region MouseAndKeyboard
        // Draws a line on a canvas, in CSS pixels of the page's viewport.
        await page.Mouse.MoveAsync(100, 100);
        await page.Mouse.DownAsync();
        await page.Mouse.MoveAsync(300, 200, steps: 10);
        await page.Mouse.UpAsync();

        await page.Keyboard.DownAsync(Keys.Shift);
        await page.Keyboard.PressAsync(Keys.ArrowRight);
        await page.Keyboard.UpAsync(Keys.Shift);
        await page.Keyboard.TypeAsync("Hello");
        #endregion
    }
}
