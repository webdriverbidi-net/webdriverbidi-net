// <copyright file="NetworkModule.cs" company="WebDriverBidi.NET Committers">
// Copyright (c) WebDriverBidi.NET Committers. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.
// </copyright>

namespace WebDriverBidi.Network;

/// <summary>
/// The Network module contains commands and events relating to network traffic.
/// </summary>
public sealed class NetworkModule : Module
{
    /// <summary>
    /// The name of the log module.
    /// </summary>
    public const string NetworkModuleName = "network";

    /// <summary>
    /// Initializes a new instance of the <see cref="NetworkModule"/> class.
    /// </summary>
    /// <param name="driver">The <see cref="Driver"/> used in the module commands and events.</param>
    public NetworkModule(Driver driver)
        : base(driver)
    {
        this.RegisterEventInvoker<BeforeRequestSentEventArgs>("network.beforeRequestSent", this.OnBeforeRequestSent);
        this.RegisterEventInvoker<FetchErrorEventArgs>("network.fetchError", this.OnFetchError);
        this.RegisterEventInvoker<ResponseStartedEventArgs>("network.responseStarted", this.OnResponseStarted);
        this.RegisterEventInvoker<ResponseCompletedEventArgs>("network.responseCompleted", this.OnResponseCompleted);
    }

    /// <summary>
    /// Occurs before a network request is sent.
    /// </summary>
    public event EventHandler<BeforeRequestSentEventArgs>? BeforeRequestSent;

    /// <summary>
    /// Occurs when an error is encountered fetching data.
    /// </summary>
    public event EventHandler<FetchErrorEventArgs>? FetchError;

    /// <summary>
    /// Occurs when network response has started.
    /// </summary>
    public event EventHandler<ResponseStartedEventArgs>? ResponseStarted;

    /// <summary>
    /// Occurs when network response has completed.
    /// </summary>
    public event EventHandler<ResponseCompletedEventArgs>? ResponseCompleted;

    /// <summary>
    /// Gets the module name.
    /// </summary>
    public override string ModuleName => NetworkModuleName;

    private void OnBeforeRequestSent(EventInfo<BeforeRequestSentEventArgs> eventData)
    {
        if (this.BeforeRequestSent is not null)
        {
            BeforeRequestSentEventArgs eventArgs = eventData.ToEventArgs<BeforeRequestSentEventArgs>();
            this.BeforeRequestSent(this, eventArgs);
        }
    }

    private void OnFetchError(EventInfo<FetchErrorEventArgs> eventData)
    {
        if (this.FetchError is not null)
        {
            FetchErrorEventArgs eventArgs = eventData.ToEventArgs<FetchErrorEventArgs>();
            this.FetchError(this, eventArgs);
        }
    }

    private void OnResponseStarted(EventInfo<ResponseStartedEventArgs> eventData)
    {
        if (this.ResponseStarted is not null)
        {
            ResponseStartedEventArgs eventArgs = eventData.ToEventArgs<ResponseStartedEventArgs>();
            this.ResponseStarted(this, eventArgs);
        }
    }

    private void OnResponseCompleted(EventInfo<ResponseCompletedEventArgs> eventData)
    {
        if (this.ResponseCompleted is not null)
        {
            ResponseCompletedEventArgs eventArgs = eventData.ToEventArgs<ResponseCompletedEventArgs>();
            this.ResponseCompleted(this, eventArgs);
        }
    }
}
