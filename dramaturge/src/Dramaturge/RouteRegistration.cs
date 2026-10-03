// <copyright file="RouteRegistration.cs" company="WebDriverBiDi.NET Committers">
// Copyright (c) WebDriverBiDi.NET Committers. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.
// </copyright>

namespace Dramaturge;

using WebDriverBiDi.Network;

/// <summary>
/// A route a page or a browser added, which can be removed.
/// </summary>
public sealed class RouteRegistration
{
    private readonly BrowserGroup group;
    private readonly Action<RouteRegistration> forget;
    private readonly Func<RequestData, bool> matches;
    private readonly Func<CancellationToken, Task>? removing;

    /// <summary>
    /// Initializes a new instance of the <see cref="RouteRegistration"/> class.
    /// </summary>
    /// <param name="group">The group whose driver added the route's intercept.</param>
    /// <param name="forget">Removes the route from the page or browser it belongs to.</param>
    /// <param name="matches">Whether a request is the route's.</param>
    /// <param name="description">What the route matches, for messages.</param>
    /// <param name="handler">The route's handler.</param>
    /// <param name="interceptId">The ID of the route's network intercept.</param>
    /// <param name="removing">Removes what else the route added to the browser, or <see langword="null"/> if it added nothing else.</param>
    internal RouteRegistration(BrowserGroup group, Action<RouteRegistration> forget, Func<RequestData, bool> matches, string description, Func<Route, Task> handler, string interceptId, Func<CancellationToken, Task>? removing = null)
    {
        this.removing = removing;
        this.group = group;
        this.forget = forget;
        this.matches = matches;
        this.Description = description;
        this.Handler = handler;
        this.InterceptId = interceptId;
    }

    /// <summary>
    /// Gets what the route matches, such as a URL.
    /// </summary>
    public string Description { get; }

    /// <summary>
    /// Gets the route's handler.
    /// </summary>
    internal Func<Route, Task> Handler { get; }

    /// <summary>
    /// Gets the ID of the route's network intercept.
    /// </summary>
    internal string InterceptId { get; }

    /// <summary>
    /// Removes the route. A request already stopped is still handled.
    /// </summary>
    /// <param name="cancellationToken">A token that cancels the command.</param>
    /// <returns>A task that completes when the route is removed.</returns>
    public async Task RemoveAsync(CancellationToken cancellationToken = default)
    {
        this.forget(this);
        this.group.UntrackIntercept(this.InterceptId);
        await this.group.Driver.Network.RemoveInterceptAsync(new RemoveInterceptCommandParameters(this.InterceptId), cancellationToken: cancellationToken).ConfigureAwait(false);
        if (this.removing is not null)
        {
            await this.removing(cancellationToken).ConfigureAwait(false);
        }
    }

    /// <summary>
    /// Gets a value indicating whether a request is the route's.
    /// </summary>
    /// <param name="request">The request.</param>
    /// <returns><see langword="true"/> if the route matches the request; otherwise, <see langword="false"/>.</returns>
    internal bool Matches(RequestData request)
    {
        return this.matches(request);
    }
}
