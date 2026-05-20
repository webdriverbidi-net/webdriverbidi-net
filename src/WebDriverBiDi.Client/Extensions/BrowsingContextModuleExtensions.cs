// <copyright file="BrowsingContextModuleExtensions.cs" company="WebDriverBiDi.NET Committers">
// Copyright (c) WebDriverBiDi.NET Committers. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.
// </copyright>

namespace WebDriverBiDi.BrowsingContext;

using WebDriverBiDi.Script;

/// <summary>
/// Provides extension methods for the BrowsingContext module.
/// </summary>
public static class BrowsingContextModuleExtensions
{
    /// <summary>
    /// Closes the browsing context with the specified context ID.
    /// </summary>
    /// <param name="module">The <see cref="BrowsingContextModule"/> to extend.</param>
    /// <param name="browsingContextId">The ID of the browsing context to close.</param>
    /// <param name="timeoutOverride">The timeout override to use for the command. If omitted, the value of <see cref="BiDiDriver.DefaultCommandTimeout"/> is used.</param>
    /// <param name="cancellationToken">A cancellation token used to propagate notification that the operation should be canceled. Omitting this argument is the equivalent of using <see cref="CancellationToken.None"/>.</param>
    /// <returns>An Task representing the asynchronous operation.</returns>
    public static async Task CloseAsync(this BrowsingContextModule module, string browsingContextId, TimeSpan? timeoutOverride = null, CancellationToken cancellationToken = default)
    {
        await module.CloseAsync(new CloseCommandParameters(browsingContextId), timeoutOverride, cancellationToken).ConfigureAwait(false);
    }

    /// <summary>
    /// Gets a list of the top-level browsing contexts for the driver. Usually corresponds to a browser tab or window.
    /// </summary>
    /// <param name="module">The <see cref="BrowsingContextModule"/> to extend.</param>
    /// <param name="timeoutOverride">The timeout override to use for the command. If omitted, the value of <see cref="BiDiDriver.DefaultCommandTimeout"/> is used.</param>
    /// <param name="cancellationToken">A cancellation token used to propagate notification that the operation should be canceled. Omitting this argument is the equivalent of using <see cref="CancellationToken.None"/>.</param>
    /// <returns>The list of <see cref="BrowsingContextInfo"/> for the top-level browsing contexts.</returns>
    public static async Task<List<BrowsingContextInfo>> GetTopLevelBrowsingContextsAsync(this BrowsingContextModule module, TimeSpan? timeoutOverride = null, CancellationToken cancellationToken = default)
    {
        GetTreeCommandResult result = await module.GetTreeAsync(new GetTreeCommandParameters() { MaxDepth = 1 }, timeoutOverride, cancellationToken).ConfigureAwait(false);
        return [.. result.ContextTree];
    }

    /// <summary>
    /// Navigates a browsing context to a specifed URL, optionally waiting for a given readiness state.
    /// </summary>
    /// <param name="module">The <see cref="BrowsingContextModule"/> to extend.</param>
    /// <param name="browsingContextId">The ID of the browsing context to navigate.</param>
    /// <param name="url">The URL to which to navigate the browsing context.</param>
    /// <param name="wait">An optional <see cref="ReadinessState"/> parameter describing the navigation event for which to wait.</param>
    /// <param name="timeoutOverride">The timeout override to use for the command. If omitted, the value of <see cref="BiDiDriver.DefaultCommandTimeout"/> is used.</param>
    /// <param name="cancellationToken">A cancellation token used to propagate notification that the operation should be canceled. Omitting this argument is the equivalent of using <see cref="CancellationToken.None"/>.</param>
    /// <returns>The URL navigated to after redirects.</returns>
    public static async Task<string> NavigateAsync(this BrowsingContextModule module, string browsingContextId, string url, ReadinessState? wait, TimeSpan? timeoutOverride = null, CancellationToken cancellationToken = default)
    {
        NavigateCommandParameters parameters = new(browsingContextId, url)
        {
            Wait = wait,
        };
        NavigateCommandResult result = await module.NavigateAsync(parameters, timeoutOverride, cancellationToken).ConfigureAwait(false);
        return result.Url;
    }

    /// <summary>
    /// Captures a screenshot of the specified browsing context.
    /// </summary>
    /// <param name="module">The <see cref="BrowsingContextModule"/> to extend.</param>
    /// <param name="browsingContextId">The ID of the browsing context for which to capture the screenshot.</param>
    /// <param name="origin">An optional <see cref="ScreenshotOrigin"/> parameter describing origin of the screenshot. If omitted, takes a screenshot of the browsing context's viewport onlhy.</param>
    /// <param name="timeoutOverride">The timeout override to use for the command. If omitted, the value of <see cref="BiDiDriver.DefaultCommandTimeout"/> is used.</param>
    /// <param name="cancellationToken">A cancellation token used to propagate notification that the operation should be canceled. Omitting this argument is the equivalent of using <see cref="CancellationToken.None"/>.</param>
    /// <returns>A base64-encoded string representing the screenshot in PNG format.</returns>
    public static async Task<string> CaptureScreenshotAsync(this BrowsingContextModule module, string browsingContextId, ScreenshotOrigin? origin = ScreenshotOrigin.Viewport, TimeSpan? timeoutOverride = null, CancellationToken cancellationToken = default)
    {
        CaptureScreenshotCommandParameters parameters = new(browsingContextId)
        {
            Origin = origin,
        };
        CaptureScreenshotCommandResult result = await module.CaptureScreenshotAsync(parameters, timeoutOverride, cancellationToken).ConfigureAwait(false);
        return result.Data;
    }

    /// <summary>
    /// Locates a node in a specified browsing context using CSS selectors.
    /// </summary>
    /// <param name="module">The <see cref="BrowsingContextModule"/> to extend.</param>
    /// <param name="browsingContextId">The ID of the browsing context in which to find the nodes.</param>
    /// <param name="cssSelector">The CSS selector to use to find the nodes.</param>
    /// <param name="parentNodes">An optional list of nodes to use as parents of the nodes to find. If <see langword="null"/> or omitted, locates from the top-level document.</param>
    /// <param name="timeoutOverride">The timeout override to use for the command. If omitted, the value of <see cref="BiDiDriver.DefaultCommandTimeout"/> is used.</param>
    /// <param name="cancellationToken">A cancellation token used to propagate notification that the operation should be canceled. Omitting this argument is the equivalent of using <see cref="CancellationToken.None"/>.</param>
    /// <returns>A list of <see cref="SharedReference"/> objects representing the located nodes.</returns>
    public static async Task<IList<SharedReference>> LocateNodesByCssSelectorAsync(this BrowsingContextModule module, string browsingContextId, string cssSelector, IList<SharedReference>? parentNodes = null, TimeSpan? timeoutOverride = null, CancellationToken cancellationToken = default)
    {
        CssLocator locator = new(cssSelector);
        return await LocateNodesInternalAsync(module, browsingContextId, locator, parentNodes, timeoutOverride, cancellationToken).ConfigureAwait(false);
    }

    /// <summary>
    /// Locates a node in a specified browsing context using XPath.
    /// </summary>
    /// <param name="module">The <see cref="BrowsingContextModule"/> to extend.</param>
    /// <param name="browsingContextId">The ID of the browsing context in which to find the nodes.</param>
    /// <param name="xpath">The XPath to use to find the nodes.</param>
    /// <param name="parentNodes">An optional list of nodes to use as parents of the nodes to find. If <see langword="null"/> or omitted, locates from the top-level document.</param>
    /// <param name="timeoutOverride">The timeout override to use for the command. If omitted, the value of <see cref="BiDiDriver.DefaultCommandTimeout"/> is used.</param>
    /// <param name="cancellationToken">A cancellation token used to propagate notification that the operation should be canceled. Omitting this argument is the equivalent of using <see cref="CancellationToken.None"/>.</param>
    /// <returns>A list of <see cref="SharedReference"/> objects representing the located nodes.</returns>
    public static async Task<IList<SharedReference>> LocateNodesByXPathAsync(this BrowsingContextModule module, string browsingContextId, string xpath, IList<SharedReference>? parentNodes = null, TimeSpan? timeoutOverride = null, CancellationToken cancellationToken = default)
    {
        XPathLocator locator = new(xpath);
        return await LocateNodesInternalAsync(module, browsingContextId, locator, parentNodes, timeoutOverride, cancellationToken).ConfigureAwait(false);
    }

    /// <summary>
    /// Locates a node in a specified browsing context using accessible role.
    /// </summary>
    /// <param name="module">The <see cref="BrowsingContextModule"/> to extend.</param>
    /// <param name="browsingContextId">The ID of the browsing context in which to find the nodes.</param>
    /// <param name="accessibleRole">The accessible role to use to find the nodes.</param>
    /// <param name="accessibleName">An optional accessible name to use to find the nodes. This will filter the list of nodes for accessible name matches within those with the specified accessible role.</param>
    /// <param name="parentNodes">An optional list of nodes to use as parents of the nodes to find. If <see langword="null"/> or omitted, locates from the top-level document.</param>
    /// <param name="timeoutOverride">The timeout override to use for the command. If omitted, the value of <see cref="BiDiDriver.DefaultCommandTimeout"/> is used.</param>
    /// <param name="cancellationToken">A cancellation token used to propagate notification that the operation should be canceled. Omitting this argument is the equivalent of using <see cref="CancellationToken.None"/>.</param>
    /// <returns>A list of <see cref="SharedReference"/> objects representing the located nodes.</returns>
    public static async Task<IList<SharedReference>> LocateNodesByAccessibleRoleAsync(this BrowsingContextModule module, string browsingContextId, string accessibleRole, string? accessibleName = null, IList<SharedReference>? parentNodes = null, TimeSpan? timeoutOverride = null, CancellationToken cancellationToken = default)
    {
        AccessibilityLocator locator = new()
        {
            Role = accessibleRole,
        };
        if (accessibleName is not null)
        {
            locator.Name = accessibleName;
        }

        return await LocateNodesInternalAsync(module, browsingContextId, locator, parentNodes, timeoutOverride, cancellationToken).ConfigureAwait(false);
    }

    /// <summary>
    /// Locates a node in a specified browsing context using visible text.
    /// </summary>
    /// <param name="module">The <see cref="BrowsingContextModule"/> to extend.</param>
    /// <param name="browsingContextId">The ID of the browsing context in which to find the nodes.</param>
    /// <param name="text">The text to use to find the nodes.</param>
    /// <param name="isPartialMatch"><see langword="true"/> to perform a match for a substring of the node text; otherwise <see langword="false"/>. Defaults to true.</param>
    /// <param name="matchCase"><see langword="true"/> to perform a case-sensitive match for the node text; otherwise <see langword="false"/>. Defaults to false.</param>
    /// <param name="parentNodes">An optional list of nodes to use as parents of the nodes to find. If <see langword="null"/> or omitted, locates from the top-level document.</param>
    /// <param name="timeoutOverride">The timeout override to use for the command. If omitted, the value of <see cref="BiDiDriver.DefaultCommandTimeout"/> is used.</param>
    /// <param name="cancellationToken">A cancellation token used to propagate notification that the operation should be canceled. Omitting this argument is the equivalent of using <see cref="CancellationToken.None"/>.</param>
    /// <returns>A list of <see cref="SharedReference"/> objects representing the located nodes.</returns>
    /// <remarks>
    /// Uses the innerText property of nodes to locate the nodes. This property takes CSS styling, visibility, and tranforms
    /// into account.
    /// </remarks>
    public static async Task<IList<SharedReference>> LocateNodesByVisibleTextAsync(this BrowsingContextModule module, string browsingContextId, string text, bool isPartialMatch = true, bool matchCase = false, IList<SharedReference>? parentNodes = null, TimeSpan? timeoutOverride = null, CancellationToken cancellationToken = default)
    {
        InnerTextLocator locator = new(text)
        {
            IgnoreCase = !matchCase,
            MatchType = isPartialMatch ? InnerTextMatchType.Partial : InnerTextMatchType.Full,
        };

        return await LocateNodesInternalAsync(module, browsingContextId, locator, parentNodes, timeoutOverride, cancellationToken).ConfigureAwait(false);
    }

    private static async Task<IList<SharedReference>> LocateNodesInternalAsync(BrowsingContextModule module, string browsingContextId, Locator locator, IList<SharedReference>? parentNodes = null, TimeSpan? timeoutOverride = null, CancellationToken cancellationToken = default)
    {
        LocateNodesCommandParameters parameters = new(browsingContextId, locator);
        if (parentNodes is not null)
        {
            parameters.StartNodes.AddRange(parentNodes);
        }

        LocateNodesCommandResult result = await module.LocateNodesAsync(parameters, timeoutOverride, cancellationToken).ConfigureAwait(false);
        List<SharedReference> nodes = [];
        foreach (NodeRemoteValue node in result.Nodes)
        {
            nodes.Add(node.ToSharedReference());
        }

        return nodes;
    }
}
