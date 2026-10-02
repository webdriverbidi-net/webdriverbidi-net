// <copyright file="AssertionsSamples.cs" company="WebDriverBiDi.NET Committers">
// Copyright (c) WebDriverBiDi.NET Committers. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.
// </copyright>
// Code snippets for docs/articles/assertions.md

namespace Dramaturge.Docs.Code;

using System.Text.RegularExpressions;
using static Dramaturge.Assertions;

/// <summary>
/// Snippets for the assertions guide. Compiled at build time to prevent API drift.
/// </summary>
public static class AssertionsSamples
{
    /// <summary>
    /// Expectations about elements.
    /// </summary>
    /// <param name="page">A page.</param>
    /// <returns>A task representing the asynchronous operation.</returns>
    public static async Task ElementExpectations(Page page)
    {
        #region ElementExpectations
        await Expect(page.GetByRole("alert")).ToBeVisibleAsync();
        await Expect(page.GetByRole("button", "Submit")).ToBeEnabledAsync();
        await Expect(page.GetByLabel("Email")).ToHaveValueAsync("ada@example.com");
        await Expect(page.GetByRole("status")).ToHaveTextAsync("Saved");
        await Expect(page.GetByRole("listitem")).ToHaveCountAsync(3);
        await Expect(page.GetByTestId("total")).ToHaveTextAsync(new Regex(@"^\$\d+\.\d{2}$"));
        #endregion
    }

    /// <summary>
    /// Negated expectations.
    /// </summary>
    /// <param name="page">A page.</param>
    /// <returns>A task representing the asynchronous operation.</returns>
    public static async Task Negation(Page page)
    {
        #region Negation
        // Waits until the spinner is gone, and fails if it is still showing after the timeout.
        await Expect(page.GetByRole("progressbar")).Not.ToBeVisibleAsync();
        await Expect(page.GetByRole("status")).Not.ToHaveTextAsync("Saving");
        #endregion
    }

    /// <summary>
    /// Expectations about text.
    /// </summary>
    /// <param name="page">A page.</param>
    /// <returns>A task representing the asynchronous operation.</returns>
    public static async Task TextExpectations(Page page)
    {
        #region TextExpectations
        // White space is collapsed and trimmed, on the page and in the expected text.
        await Expect(page.GetByRole("heading")).ToHaveTextAsync("Order  summary");
        await Expect(page.GetByRole("heading")).ToContainTextAsync("summary", ignoreCase: true);

        // The rendered text, without text hidden by CSS.
        await Expect(page.GetByTestId("price")).ToHaveTextAsync("$10.00", useInnerText: true);

        // Every matching element, in order.
        await Expect(page.GetByRole("listitem")).ToHaveTextAsync(["Apples", "Bread", "Cheese"]);
        await Expect(page.GetByRole("listitem")).ToContainTextAsync(["Apples", "Cheese"]);
        #endregion
    }

    /// <summary>
    /// Expectations about a page.
    /// </summary>
    /// <param name="page">A page.</param>
    /// <returns>A task representing the asynchronous operation.</returns>
    public static async Task PageExpectations(Page page)
    {
        #region PageExpectations
        await Expect(page).ToHaveUrlAsync(new Regex("/orders/\\d+$"));
        await Expect(page).ToHaveTitleAsync("Order confirmed");
        #endregion
    }

    /// <summary>
    /// A failed expectation.
    /// </summary>
    /// <param name="page">A page.</param>
    /// <returns>A task representing the asynchronous operation.</returns>
    public static async Task Failure(Page page)
    {
        #region Failure
        try
        {
            await Expect(page.GetByRole("status")).ToHaveTextAsync("Saved", timeout: TimeSpan.FromSeconds(2));
        }
        catch (ExpectationFailedException ex)
        {
            // Expected getByRole "status" to have text "Saved"; received "Saving…" after 2 seconds.
            Console.WriteLine(ex.Message);
            Console.WriteLine($"{ex.Expected} / {ex.Actual} / {ex.Timeout}");
        }
        #endregion
    }
}
