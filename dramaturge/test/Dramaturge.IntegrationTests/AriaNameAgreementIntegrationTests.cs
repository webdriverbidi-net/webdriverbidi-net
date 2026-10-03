// <copyright file="AriaNameAgreementIntegrationTests.cs" company="WebDriverBiDi.NET Committers">
// Copyright (c) WebDriverBiDi.NET Committers. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.
// </copyright>

namespace Dramaturge;

using Dramaturge.Browsers;
using Dramaturge.TestUtilities;

/// <summary>
/// Accessibility snapshots compute names in the page, while GetByRole asks the browser, so the two can disagree. For
/// each named node of a snapshot of a page of common patterns, these tests check that GetByRole with the node's role
/// and name finds the same element. The differences known for each browser are listed, so a new one fails, and so
/// does one that a browser fixes, until its entry is removed. The documentation lists the same differences.
/// </summary>
public class AriaNameAgreementIntegrationTests
{
    private static readonly Dictionary<BrowserKind, string[]> KnownDifferences = new()
    {
        [BrowserKind.Chrome] =
        [
            // Chrome does not name a figure from its figcaption.
            "figure \"Sales figures\"",

            // Browsers do not name a table row from its cells.
            "row \"Item Cost\"",
            "row \"Tea 2\"",
        ],
        [BrowserKind.Firefox] =
        [
            // Firefox names a submit button with no value "Submit Query", as the specification says; Chrome says "Submit".
            "button \"Submit\"",

            // Firefox does not give an svg element without a role the role image.
            "img \"Titled svg\"",

            // Browsers do not name a table row from its cells.
            "row \"Item Cost\"",
            "row \"Tea 2\"",
        ],
    };

    [Theory]
    [MemberData(nameof(TestBrowsers.All), MemberType = typeof(TestBrowsers))]
    public async Task SnapshotNamesFindTheSameElementsByRole(BrowserKind browserKind)
    {
        await using TestPageServer server = await TestPageServer.StartAsync();
        await using BrowserGroup group = await TestBrowsers.LaunchAsync(browserKind);
        Page page = await group.DefaultBrowser.NewPageAsync(cancellationToken: TestContext.Current.CancellationToken);
        await page.NavigateAsync(server.UrlFor("aria-names.html"), cancellationToken: TestContext.Current.CancellationToken);
        AriaSnapshot snapshot = await page.AriaSnapshotAsync(cancellationToken: TestContext.Current.CancellationToken);

        List<string> differences = [];
        foreach (AriaNode node in Named(snapshot.Root))
        {
            ElementLocator byRole = page.GetByRole(node.Role, node.Name);
            if (await byRole.And(snapshot.Locator(node.Ref!)).CountAsync(TestContext.Current.CancellationToken) == 0)
            {
                differences.Add($"{node.Role} \"{node.Name}\"");
            }
        }

        Assert.Equal(KnownDifferences[browserKind], differences);
    }

    // The nodes with a name, in document order.
    private static IEnumerable<AriaNode> Named(AriaNode node)
    {
        foreach (AriaNode child in node.Children)
        {
            if (child.Name.Length > 0)
            {
                yield return child;
            }

            foreach (AriaNode descendant in Named(child))
            {
                yield return descendant;
            }
        }
    }
}
