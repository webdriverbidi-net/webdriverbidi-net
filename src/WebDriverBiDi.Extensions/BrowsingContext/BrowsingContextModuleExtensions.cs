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
    extension(BrowsingContextModule module)
    {
        /// <summary>
        /// Closes a browsing context.
        /// </summary>
        /// <param name="browsingContextId">The ID of the browsing context.</param>
        /// <param name="timeoutOverride">The timeout override to use for the command. If omitted, the value of <see cref="BiDiDriver.DefaultCommandTimeout"/> is used.</param>
        /// <param name="cancellationToken">A token that cancels waiting for the command's response.</param>
        /// <returns>A task representing the asynchronous operation.</returns>
        public async Task CloseAsync(string browsingContextId, TimeSpan? timeoutOverride = null, CancellationToken cancellationToken = default)
        {
            await module.CloseAsync(new CloseCommandParameters(browsingContextId), timeoutOverride, cancellationToken).ConfigureAwait(false);
        }

        /// <summary>
        /// Gets the top-level browsing contexts (tabs and windows), without their child frames.
        /// </summary>
        /// <param name="timeoutOverride">The timeout override to use for the command. If omitted, the value of <see cref="BiDiDriver.DefaultCommandTimeout"/> is used.</param>
        /// <param name="cancellationToken">A token that cancels waiting for the command's response.</param>
        /// <returns>The top-level browsing contexts.</returns>
        public async Task<IReadOnlyList<BrowsingContextInfo>> GetTopLevelBrowsingContextsAsync(TimeSpan? timeoutOverride = null, CancellationToken cancellationToken = default)
        {
            GetTreeCommandResult result = await module.GetTreeAsync(new GetTreeCommandParameters() { MaxDepth = 0 }, timeoutOverride, cancellationToken).ConfigureAwait(false);
            return [.. result.ContextTree];
        }

        /// <summary>
        /// Navigates a browsing context to a URL.
        /// </summary>
        /// <param name="browsingContextId">The ID of the browsing context.</param>
        /// <param name="url">The URL.</param>
        /// <param name="wait">How far the new document must have loaded before the command completes, or <see langword="null"/> for the remote end's default.</param>
        /// <param name="timeoutOverride">The timeout override to use for the command. If omitted, the value of <see cref="BiDiDriver.DefaultCommandTimeout"/> is used.</param>
        /// <param name="cancellationToken">A token that cancels waiting for the command's response.</param>
        /// <returns>The URL navigated to.</returns>
        public async Task<string> NavigateAsync(string browsingContextId, string url, ReadinessState? wait = null, TimeSpan? timeoutOverride = null, CancellationToken cancellationToken = default)
        {
            NavigateCommandParameters parameters = new(browsingContextId, url)
            {
                Wait = wait,
            };
            NavigateCommandResult result = await module.NavigateAsync(parameters, timeoutOverride, cancellationToken).ConfigureAwait(false);
            return result.Url;
        }

        /// <summary>
        /// Captures a PNG screenshot of a browsing context.
        /// </summary>
        /// <param name="browsingContextId">The ID of the browsing context.</param>
        /// <param name="origin">Whether to capture the viewport or the whole document.</param>
        /// <param name="timeoutOverride">The timeout override to use for the command. If omitted, the value of <see cref="BiDiDriver.DefaultCommandTimeout"/> is used.</param>
        /// <param name="cancellationToken">A token that cancels waiting for the command's response.</param>
        /// <returns>The PNG image.</returns>
        public async Task<byte[]> CaptureScreenshotAsync(string browsingContextId, ScreenshotOrigin origin = ScreenshotOrigin.Viewport, TimeSpan? timeoutOverride = null, CancellationToken cancellationToken = default)
        {
            CaptureScreenshotCommandParameters parameters = new(browsingContextId)
            {
                Origin = origin,
            };
            CaptureScreenshotCommandResult result = await module.CaptureScreenshotAsync(parameters, timeoutOverride, cancellationToken).ConfigureAwait(false);
            return Convert.FromBase64String(result.Data);
        }

        /// <summary>
        /// Finds the nodes matching a CSS selector.
        /// </summary>
        /// <param name="browsingContextId">The ID of the browsing context.</param>
        /// <param name="cssSelector">The CSS selector.</param>
        /// <param name="parentNodes">The nodes within which to search, or <see langword="null"/> for the whole document.</param>
        /// <param name="timeoutOverride">The timeout override to use for the command. If omitted, the value of <see cref="BiDiDriver.DefaultCommandTimeout"/> is used.</param>
        /// <param name="cancellationToken">A token that cancels waiting for the command's response.</param>
        /// <returns>References to the matching nodes.</returns>
        public Task<IReadOnlyList<SharedReference>> LocateNodesByCssSelectorAsync(string browsingContextId, string cssSelector, IEnumerable<SharedReference>? parentNodes = null, TimeSpan? timeoutOverride = null, CancellationToken cancellationToken = default)
        {
            return LocateNodesAsync(module, browsingContextId, new CssLocator(cssSelector), parentNodes, timeoutOverride, cancellationToken);
        }

        /// <summary>
        /// Finds the nodes matching an XPath expression.
        /// </summary>
        /// <param name="browsingContextId">The ID of the browsing context.</param>
        /// <param name="xpath">The XPath expression.</param>
        /// <param name="parentNodes">The nodes within which to search, or <see langword="null"/> for the whole document.</param>
        /// <param name="timeoutOverride">The timeout override to use for the command. If omitted, the value of <see cref="BiDiDriver.DefaultCommandTimeout"/> is used.</param>
        /// <param name="cancellationToken">A token that cancels waiting for the command's response.</param>
        /// <returns>References to the matching nodes.</returns>
        public Task<IReadOnlyList<SharedReference>> LocateNodesByXPathAsync(string browsingContextId, string xpath, IEnumerable<SharedReference>? parentNodes = null, TimeSpan? timeoutOverride = null, CancellationToken cancellationToken = default)
        {
            return LocateNodesAsync(module, browsingContextId, new XPathLocator(xpath), parentNodes, timeoutOverride, cancellationToken);
        }

        /// <summary>
        /// Finds the nodes with an accessible role and, optionally, an accessible name.
        /// </summary>
        /// <param name="browsingContextId">The ID of the browsing context.</param>
        /// <param name="accessibleRole">The accessible role, such as "button".</param>
        /// <param name="accessibleName">The accessible name, or <see langword="null"/> to match any.</param>
        /// <param name="parentNodes">The nodes within which to search, or <see langword="null"/> for the whole document.</param>
        /// <param name="timeoutOverride">The timeout override to use for the command. If omitted, the value of <see cref="BiDiDriver.DefaultCommandTimeout"/> is used.</param>
        /// <param name="cancellationToken">A token that cancels waiting for the command's response.</param>
        /// <returns>References to the matching nodes.</returns>
        public Task<IReadOnlyList<SharedReference>> LocateNodesByAccessibleRoleAsync(string browsingContextId, string accessibleRole, string? accessibleName = null, IEnumerable<SharedReference>? parentNodes = null, TimeSpan? timeoutOverride = null, CancellationToken cancellationToken = default)
        {
            AccessibilityLocator locator = new()
            {
                Role = accessibleRole,
                Name = accessibleName,
            };
            return LocateNodesAsync(module, browsingContextId, locator, parentNodes, timeoutOverride, cancellationToken);
        }

        /// <summary>
        /// Finds the nodes whose rendered text matches.
        /// </summary>
        /// <param name="browsingContextId">The ID of the browsing context.</param>
        /// <param name="text">The text.</param>
        /// <param name="isPartialMatch"><see langword="true"/> to match nodes whose text contains <paramref name="text"/>; <see langword="false"/> to match only nodes whose text is exactly it.</param>
        /// <param name="matchCase"><see langword="true"/> to match case.</param>
        /// <param name="parentNodes">The nodes within which to search, or <see langword="null"/> for the whole document.</param>
        /// <param name="timeoutOverride">The timeout override to use for the command. If omitted, the value of <see cref="BiDiDriver.DefaultCommandTimeout"/> is used.</param>
        /// <param name="cancellationToken">A token that cancels waiting for the command's response.</param>
        /// <returns>References to the matching nodes.</returns>
        public Task<IReadOnlyList<SharedReference>> LocateNodesByVisibleTextAsync(string browsingContextId, string text, bool isPartialMatch = true, bool matchCase = false, IEnumerable<SharedReference>? parentNodes = null, TimeSpan? timeoutOverride = null, CancellationToken cancellationToken = default)
        {
            InnerTextLocator locator = new(text)
            {
                IgnoreCase = !matchCase,
                MatchType = isPartialMatch ? InnerTextMatchType.Partial : InnerTextMatchType.Full,
            };
            return LocateNodesAsync(module, browsingContextId, locator, parentNodes, timeoutOverride, cancellationToken);
        }

        private async Task<IReadOnlyList<SharedReference>> LocateNodesAsync(string browsingContextId, Locator locator, IEnumerable<SharedReference>? parentNodes, TimeSpan? timeoutOverride, CancellationToken cancellationToken)
        {
            LocateNodesCommandParameters parameters = new(browsingContextId, locator);
            if (parentNodes is not null)
            {
                parameters.StartNodes.AddRange(parentNodes);
            }

            LocateNodesCommandResult result = await module.LocateNodesAsync(parameters, timeoutOverride, cancellationToken).ConfigureAwait(false);
            return [.. result.Nodes.Select(node => node.ToSharedReference())];
        }
    }
}
