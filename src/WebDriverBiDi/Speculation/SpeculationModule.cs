// <copyright file="SpeculationModule.cs" company="WebDriverBiDi.NET Committers">
// Copyright (c) WebDriverBiDi.NET Committers. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.
// </copyright>

namespace WebDriverBiDi.Speculation;

/// <summary>
/// The Speculation module contains commands for for managing the remote end behavior
/// for prefetches, prerenders, and speculation rules.
/// </summary>
public sealed class SpeculationModule : Module
{
    /// <summary>
    /// The name of the bluetooth module.
    /// </summary>
    public const string SpeculationModuleName = "speculation";

    private const string PrefetchStatusUpdatedEventName = $"{SpeculationModuleName}.prefetchStatusUpdated";

    private readonly ObservableEvent<PrefetchStatusUpdatedEventArgs> onPrefetchStatusUpdatedEvent = new(PrefetchStatusUpdatedEventName);

    /// <summary>
    /// Initializes a new instance of the <see cref="SpeculationModule"/> class.
    /// </summary>
    /// <param name="driver">The <see cref="BiDiDriver"/> used in the module commands and events.</param>
    public SpeculationModule(BiDiDriver driver)
        : base(driver)
    {
        this.RegisterAsyncEventInvoker<PrefetchStatusUpdatedEventArgs>(PrefetchStatusUpdatedEventName, this.OnPrefetchStatusUpdatedAsync);
    }

    /// <summary>
    /// Gets an observable event that notifies when the prefetch status of a resource is updated.
    /// </summary>
    public ObservableEvent<PrefetchStatusUpdatedEventArgs> OnPrefetchStatusUpdated => this.onPrefetchStatusUpdatedEvent;

    /// <summary>
    /// Gets the module name.
    /// </summary>
    public override string ModuleName => SpeculationModuleName;

    private async Task OnPrefetchStatusUpdatedAsync(EventInfo<PrefetchStatusUpdatedEventArgs> eventData)
    {
        PrefetchStatusUpdatedEventArgs eventArgs = eventData.ToEventArgs<PrefetchStatusUpdatedEventArgs>();
        await this.onPrefetchStatusUpdatedEvent.NotifyObserversAsync(eventArgs);
    }
}
