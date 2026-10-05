// <copyright file="Route.cs" company="WebDriverBiDi.NET Committers">
// Copyright (c) WebDriverBiDi.NET Committers. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.
// </copyright>

namespace Dramaturge;

using System.Globalization;
using WebDriverBiDi.Network;

/// <summary>
/// A request a route stopped before it was sent, to be answered with a response, continued, or aborted.
/// A route handler that does none of these passes the request to the next route that matches it; a request no
/// route decides continues as it was.
/// </summary>
public sealed class Route
{
    private int isHandled;

    /// <summary>
    /// Initializes a new instance of the <see cref="Route"/> class.
    /// </summary>
    /// <param name="browser">The browser that made the request.</param>
    /// <param name="page">The page that made the request, if a page the group tracks made it.</param>
    /// <param name="frame">The frame that made the request, if the group tracks it.</param>
    /// <param name="request">The request.</param>
    internal Route(Browser browser, Page? page, Frame? frame, RequestData request)
    {
        this.Browser = browser;
        this.Page = page;
        this.Frame = frame;
        this.Request = request;
    }

    /// <summary>
    /// Gets the browser that made the request.
    /// </summary>
    public Browser Browser { get; }

    /// <summary>
    /// Gets the page that made the request, or <see langword="null"/> if no page the group tracks made it, as for a
    /// worker's request.
    /// </summary>
    public Page? Page { get; }

    /// <summary>
    /// Gets the frame that made the request, or <see langword="null"/> if it is not a frame the group tracks.
    /// </summary>
    public Frame? Frame { get; }

    /// <summary>
    /// Gets the request.
    /// </summary>
    public RequestData Request { get; }

    /// <summary>
    /// Gets a value indicating whether the request has been answered, continued, or aborted.
    /// </summary>
    internal bool IsHandled => Volatile.Read(ref this.isHandled) == 1;

    /// <summary>
    /// Answers the request with a response, without sending it.
    /// </summary>
    /// <param name="statusCode">The response's status code.</param>
    /// <param name="body">The response's body, or <see langword="null"/> for none.</param>
    /// <param name="headers">The response's headers, or <see langword="null"/> for none.</param>
    /// <param name="cancellationToken">A token that cancels the command.</param>
    /// <returns>A task that completes when the response has been given.</returns>
    /// <exception cref="InvalidOperationException">Thrown when the request has been handled already.</exception>
    public Task FulfillAsync(ulong statusCode = 200, string? body = null, IReadOnlyDictionary<string, string>? headers = null, CancellationToken cancellationToken = default)
    {
        return this.TraceAsync(Call("Fulfill", "{status}", "fulfill", ("status", statusCode.ToString(CultureInfo.InvariantCulture))), cancellationToken, budget => this.FulfillAsync(statusCode, body is null ? null : BytesValue.FromString(body), headers, budget.CancellationToken));
    }

    /// <summary>
    /// Answers the request with a response whose body is binary, without sending it.
    /// </summary>
    /// <param name="statusCode">The response's status code.</param>
    /// <param name="body">The response's body.</param>
    /// <param name="headers">The response's headers, or <see langword="null"/> for none.</param>
    /// <param name="cancellationToken">A token that cancels the command.</param>
    /// <returns>A task that completes when the response has been given.</returns>
    /// <exception cref="InvalidOperationException">Thrown when the request has been handled already.</exception>
    public Task FulfillAsync(ulong statusCode, byte[] body, IReadOnlyDictionary<string, string>? headers = null, CancellationToken cancellationToken = default)
    {
        return this.TraceAsync(Call("Fulfill", "{status}", "fulfill", ("status", statusCode.ToString(CultureInfo.InvariantCulture))), cancellationToken, budget => this.FulfillAsync(statusCode, BytesValue.FromByteArray(body), headers, budget.CancellationToken));
    }

    /// <summary>
    /// Sends the request, with any changes.
    /// </summary>
    /// <param name="overrides">The changes, or <see langword="null"/> to send it as it is.</param>
    /// <param name="cancellationToken">A token that cancels the command.</param>
    /// <returns>A task that completes when the request continues.</returns>
    /// <exception cref="InvalidOperationException">Thrown when the request has been handled already.</exception>
    public Task ContinueAsync(RouteOverrides? overrides = null, CancellationToken cancellationToken = default)
    {
        this.MarkHandled();
        ContinueRequestCommandParameters parameters = new(this.Request.RequestId)
        {
            Url = overrides?.Url,
            Method = overrides?.Method,
            Headers = ToHeaders(overrides?.Headers),
            Body = overrides?.Body is string body ? BytesValue.FromString(body) : null,
        };
        return this.TraceAsync(Call("Continue", null, "continue"), cancellationToken, budget => this.Browser.Group.Driver.Network.ContinueRequestAsync(parameters, cancellationToken: budget.CancellationToken));
    }

    /// <summary>
    /// Fails the request, as a network error would.
    /// </summary>
    /// <param name="cancellationToken">A token that cancels the command.</param>
    /// <returns>A task that completes when the request has failed.</returns>
    /// <exception cref="InvalidOperationException">Thrown when the request has been handled already.</exception>
    public Task AbortAsync(CancellationToken cancellationToken = default)
    {
        this.MarkHandled();
        return this.TraceAsync(Call("Abort", null, "abort"), cancellationToken, budget => this.Browser.Group.Driver.Network.FailRequestAsync(new FailRequestCommandParameters(this.Request.RequestId), cancellationToken: budget.CancellationToken));
    }

    /// <summary>
    /// Answers the request with a response whose headers may repeat, without sending it.
    /// </summary>
    /// <param name="statusCode">The response's status code.</param>
    /// <param name="reasonPhrase">The response's reason phrase, or an empty string for the browser's own.</param>
    /// <param name="body">The response's body.</param>
    /// <param name="headers">The response's headers.</param>
    /// <returns>A task that completes when the response has been given.</returns>
    /// <exception cref="InvalidOperationException">Thrown when the request has been handled already.</exception>
    internal Task FulfillAsync(ulong statusCode, string reasonPhrase, BytesValue body, List<Header> headers)
    {
        this.MarkHandled();
        ProvideResponseCommandParameters parameters = new(this.Request.RequestId) { StatusCode = statusCode, ReasonPhrase = reasonPhrase.Length == 0 ? null : reasonPhrase, Body = body, Headers = headers };
        return this.Browser.Group.Driver.Network.ProvideResponseAsync(parameters);
    }

    private static TracedCall Call(string title, string? subtitle, string method, params (string Name, object Value)[] parameters)
    {
        return TraceRecording.Call("Route", title, subtitle, method, parameters);
    }

    private static List<Header>? ToHeaders(IReadOnlyDictionary<string, string>? headers)
    {
        return headers is null ? null : [.. headers.Select(header => new Header(header.Key, header.Value))];
    }

    // Recorded without the page: its scripts cannot run while it waits on the request or the dialog, so neither can a snapshot's.
    private Task TraceAsync(TracedCall call, CancellationToken cancellationToken, Func<TimeBudget, Task> action)
    {
        TimeBudget budget = new(this.Browser.Group.Options.ActionTimeout, this.Browser.Group.Options.TimeProvider, cancellationToken);
        return TraceRecording.RunAsync(this.Browser, null, budget, call, action);
    }

    private Task FulfillAsync(ulong statusCode, BytesValue? body, IReadOnlyDictionary<string, string>? headers, CancellationToken cancellationToken)
    {
        this.MarkHandled();
        ProvideResponseCommandParameters parameters = new(this.Request.RequestId) { StatusCode = statusCode, Body = body, Headers = ToHeaders(headers) };
        return this.Browser.Group.Driver.Network.ProvideResponseAsync(parameters, cancellationToken: cancellationToken);
    }

    private void MarkHandled()
    {
        if (Interlocked.Exchange(ref this.isHandled, 1) == 1)
        {
            throw new InvalidOperationException($"The request for {this.Request.Url} has been handled already.");
        }
    }
}
