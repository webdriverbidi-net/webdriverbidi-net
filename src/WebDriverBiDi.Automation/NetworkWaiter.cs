// <copyright file="NetworkWaiter.cs" company="WebDriverBiDi.NET Committers">
// Copyright (c) WebDriverBiDi.NET Committers. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.
// </copyright>

namespace WebDriverBiDi.Automation;

using WebDriverBiDi.Network;

/// <summary>
/// A wait for the first network event about a request that matches.
/// </summary>
/// <typeparam name="T">The event's type.</typeparam>
internal sealed class NetworkWaiter<T>
{
    private readonly Func<RequestData, bool> matches;

    /// <summary>
    /// Initializes a new instance of the <see cref="NetworkWaiter{T}"/> class.
    /// </summary>
    /// <param name="matches">Whether a request is the one awaited.</param>
    public NetworkWaiter(Func<RequestData, bool> matches)
    {
        this.matches = matches;
    }

    /// <summary>
    /// Gets the task that completes with the first matching event, or faults if the condition throws.
    /// </summary>
    public TaskCompletionSource<T> Found { get; } = new(TaskCreationOptions.RunContinuationsAsynchronously);

    /// <summary>
    /// Offers an event to the wait.
    /// </summary>
    /// <param name="args">The event.</param>
    /// <param name="request">The request it is about.</param>
    public void Offer(T args, RequestData request)
    {
        try
        {
            if (this.matches(request))
            {
                this.Found.TrySetResult(args);
            }
        }
        catch (Exception ex)
        {
            this.Found.TrySetException(ex);
        }
    }
}
