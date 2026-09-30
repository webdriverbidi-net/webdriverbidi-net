// <copyright file="RouteRegistration.cs" company="WebDriverBiDi.NET Committers">
// Copyright (c) WebDriverBiDi.NET Committers. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.
// </copyright>

namespace WebDriverBiDi.Automation;

using WebDriverBiDi.Network;

/// <summary>
/// A route a page added, which can be removed.
/// </summary>
public sealed class RouteRegistration
{
    private readonly Page page;
    private readonly Func<RequestData, bool> matches;

    /// <summary>
    /// Initializes a new instance of the <see cref="RouteRegistration"/> class.
    /// </summary>
    /// <param name="page">The page the route belongs to.</param>
    /// <param name="matches">Whether a request is the route's.</param>
    /// <param name="description">What the route matches, for messages.</param>
    /// <param name="handler">The route's handler.</param>
    /// <param name="interceptId">The ID of the route's network intercept.</param>
    internal RouteRegistration(Page page, Func<RequestData, bool> matches, string description, Func<Route, Task> handler, string interceptId)
    {
        this.page = page;
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
    public Task RemoveAsync(CancellationToken cancellationToken = default)
    {
        this.page.RemoveRoute(this);
        this.page.Browser.Group.UntrackIntercept(this.InterceptId);
        return this.page.Browser.Group.Driver.Network.RemoveInterceptAsync(new RemoveInterceptCommandParameters(this.InterceptId), cancellationToken: cancellationToken);
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
