// <copyright file="BluetoothModule.cs" company="WebDriverBiDi.NET Committers">
// Copyright (c) WebDriverBiDi.NET Committers. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.
// </copyright>

namespace WebDriverBiDi.Bluetooth;

/// <summary>
/// The Bluetooth module contains commands for simulating communication with Bluetooth devices.
/// </summary>
public sealed class BluetoothModule : Module
{
    /// <summary>
    /// The name of the bluetooth module.
    /// </summary>
    public const string BluetoothModuleName = "bluetooth";

    private readonly ObservableEvent<RequestDevicePromptUpdatedEventArgs> onRequestDevicePromptUpdatedEvent = new();

    /// <summary>
    /// Initializes a new instance of the <see cref="BluetoothModule"/> class.
    /// </summary>
    /// <param name="driver">The <see cref="BiDiDriver"/> used in the module commands and events.</param>
    public BluetoothModule(BiDiDriver driver)
        : base(driver)
    {
        this.RegisterAsyncEventInvoker<RequestDevicePromptUpdatedEventArgs>("bluetooth.requestDevicePromptUpdated", this.OnRequestDevicePromptUpdatedAsync);
    }

    /// <summary>
    /// Gets an observable event that notifies when a Bluetooth device prompt is updated.
    /// </summary>
    public ObservableEvent<RequestDevicePromptUpdatedEventArgs> OnRequestDevicePromptUpdated => this.onRequestDevicePromptUpdatedEvent;

    /// <summary>
    /// Gets the module name.
    /// </summary>
    public override string ModuleName => BluetoothModuleName;

    /// <summary>
    /// Handles the prompt requesting connection to Bluetooth devices.
    /// </summary>
    /// <param name="commandProperties">The parameters for the command.</param>
    /// <returns>An empty command result.</returns>
    public async Task<EmptyResult> HandleRequestDevicePrompt(HandleRequestDevicePromptCommandParameters commandProperties)
    {
        return await this.Driver.ExecuteCommandAsync<EmptyResult>(commandProperties).ConfigureAwait(false);
    }

    /// <summary>
    /// Simulates the presence or absence of the Bluetooth adapter, along with its power settings.
    /// </summary>
    /// <param name="commandProperties">The parameters for the command.</param>
    /// <returns>An empty command result.</returns>
    public async Task<EmptyResult> SimulateAdapter(SimulateAdapterCommandParameters commandProperties)
    {
        return await this.Driver.ExecuteCommandAsync<EmptyResult>(commandProperties).ConfigureAwait(false);
    }

    /// <summary>
    /// Simulates the advertisement of availability of Bluetooth peripherals.
    /// </summary>
    /// <param name="commandProperties">The parameters for the command.</param>
    /// <returns>An empty command result.</returns>
    public async Task<EmptyResult> SimulateAdvertisement(SimulateAdvertisementCommandParameters commandProperties)
    {
        return await this.Driver.ExecuteCommandAsync<EmptyResult>(commandProperties).ConfigureAwait(false);
    }

    /// <summary>
    /// Simulates the presence of a Bluetooth peripheral already connected to the page.
    /// </summary>
    /// <param name="commandProperties">The parameters for the command.</param>
    /// <returns>An empty command result.</returns>
    public async Task<EmptyResult> SimulatePreconnectedPeripheral(SimulatePreconnectedPeripheralCommandParameters commandProperties)
    {
        return await this.Driver.ExecuteCommandAsync<EmptyResult>(commandProperties).ConfigureAwait(false);
    }

    private async Task OnRequestDevicePromptUpdatedAsync(EventInfo<RequestDevicePromptUpdatedEventArgs> eventData)
    {
        RequestDevicePromptUpdatedEventArgs eventArgs = eventData.ToEventArgs<RequestDevicePromptUpdatedEventArgs>();
        await this.onRequestDevicePromptUpdatedEvent.NotifyObserversAsync(eventArgs);
    }
}
