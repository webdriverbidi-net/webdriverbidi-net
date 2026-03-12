// <copyright file="SpeculationModule.cs" company="WebDriverBiDi.NET Committers">
// Copyright (c) WebDriverBiDi.NET Committers. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.
// </copyright>

namespace WebDriverBiDi.Speculation;

/// <summary>
/// The Speculation module provides the <c>speculation.prefetchStatusUpdated</c> event for
/// monitoring prefetch status. Defined in the Prefetch spec's Automated testing section.
/// </summary>
public sealed class SpeculationModule : Module
{
    /// <summary>
    /// The name of the speculation module.
    /// </summary>
    public const string SpeculationModuleName = "speculation";

    private const string PrefetchStatusUpdatedEventName = $"{SpeculationModuleName}.prefetchStatusUpdated";

    /// <summary>
    /// Initializes a new instance of the <see cref="SpeculationModule"/> class.
    /// </summary>
    /// <param name="driver">The <see cref="IBiDiCommandExecutor"/> used in the module commands and events.</param>
    public SpeculationModule(IBiDiCommandExecutor driver)
        : base(driver)
    {
        this.RegisterObservableEvent(this.OnPrefetchStatusUpdated);
    }

    /// <summary>
    /// Gets an observable event that notifies when the prefetch status of a resource is updated.
    /// </summary>
    [ObservableEventName(PrefetchStatusUpdatedEventName)]
    public ObservableEvent<PrefetchStatusUpdatedEventArgs> OnPrefetchStatusUpdated { get; } = new(PrefetchStatusUpdatedEventName);

    /// <summary>
    /// Gets the module name.
    /// </summary>
    public override string ModuleName => SpeculationModuleName;
}
