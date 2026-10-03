// <copyright file="AriaSnapshotSamples.cs" company="WebDriverBiDi.NET Committers">
// Copyright (c) WebDriverBiDi.NET Committers. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.
// </copyright>
// Code snippets for docs/articles/accessibility-snapshots.md

namespace Dramaturge.Docs.Code;

using WebDriverBiDi.BrowsingContext;
using static Dramaturge.Assertions;

/// <summary>
/// Snippets for the accessibility snapshots guide. Compiled at build time to prevent API drift.
/// </summary>
public static class AriaSnapshotSamples
{
    /// <summary>
    /// Taking a snapshot of a page.
    /// </summary>
    /// <param name="page">A page.</param>
    /// <returns>A task representing the asynchronous operation.</returns>
    public static async Task PageSnapshot(Page page)
    {
        #region PageSnapshot
        AriaSnapshot snapshot = await page.AriaSnapshotAsync();
        Console.WriteLine(snapshot);
        #endregion
    }

    /// <summary>
    /// Acting on the element a ref refers to.
    /// </summary>
    /// <param name="page">A page.</param>
    /// <returns>A task representing the asynchronous operation.</returns>
    public static async Task ActOnRef(Page page)
    {
        #region ActOnRef
        AriaSnapshot snapshot = await page.AriaSnapshotAsync();

        // The line '- button "Accept" [ref=f1e2]' describes a button in the page's first frame.
        await snapshot.Locator("f1e2").ClickAsync();
        #endregion
    }

    /// <summary>
    /// Taking a snapshot of part of a page, without refs or frames.
    /// </summary>
    /// <param name="page">A page.</param>
    /// <returns>A task representing the asynchronous operation.</returns>
    public static async Task PartOfAPage(Page page)
    {
        #region PartOfAPage
        AriaSnapshot form = await page.GetByRole("form", "Sign in").AriaSnapshotAsync(
            new AriaSnapshotOptions() { IncludeRefs = false, IncludeFrames = false });
        #endregion
    }

    /// <summary>
    /// Reading the snapshot's tree.
    /// </summary>
    /// <param name="page">A page.</param>
    /// <returns>A task representing the asynchronous operation.</returns>
    public static async Task Tree(Page page)
    {
        #region Tree
        AriaSnapshot snapshot = await page.AriaSnapshotAsync();
        foreach (AriaNode node in snapshot.Root.Children)
        {
            if (node.Role == "checkbox" && node.Checked == ToggleState.On)
            {
                Console.WriteLine($"{node.Name} is checked; its ref is {node.Ref}");
            }
        }
        #endregion
    }

    /// <summary>
    /// Asserting that part of a page matches a template.
    /// </summary>
    /// <param name="page">A page.</param>
    /// <returns>A task representing the asynchronous operation.</returns>
    public static async Task MatchSnapshot(Page page)
    {
        #region MatchSnapshot
        await Expect(page.GetByRole("navigation", "Main")).ToMatchAriaSnapshotAsync("""
            - navigation "Main":
              - link "Home"
              - link /Orders \(\d+\)/
            """);

        await Expect(page.Locate(new CssLocator("body"))).ToMatchAriaSnapshotAsync("""
            - heading "Sign in" [level=1]
            """);
        #endregion
    }
}
