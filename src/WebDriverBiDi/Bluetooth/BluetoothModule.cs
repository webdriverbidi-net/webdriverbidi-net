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

    private const string RequestDevicePromptUpdatedEventName = $"{BluetoothModuleName}.requestDevicePromptUpdated";
    private const string GattConnectionAttemptedEventName = $"{BluetoothModuleName}.gattConnectionAttempted";
    private const string CharacteristicEventGeneratedEventName = $"{BluetoothModuleName}.characteristicEventGenerated";
    private const string DescriptorEventGeneratedEventName = $"{BluetoothModuleName}.descriptorEventGenerated";

    /// <summary>
    /// Initializes a new instance of the <see cref="BluetoothModule"/> class.
    /// </summary>
    /// <param name="driver">The <see cref="BiDiDriver"/> used in the module commands and events.</param>
    public BluetoothModule(BiDiDriver driver)
        : base(driver)
    {
        this.RegisterAsyncEventInvoker<RequestDevicePromptUpdatedEventArgs>(RequestDevicePromptUpdatedEventName, this.OnRequestDevicePromptUpdatedAsync);
        this.RegisterAsyncEventInvoker<GattConnectionAttemptedEventArgs>(GattConnectionAttemptedEventName, this.OnGattConnectionAttemptedAsync);
        this.RegisterAsyncEventInvoker<CharacteristicEventGeneratedEventArgs>(CharacteristicEventGeneratedEventName, this.OnCharacteristicEventGeneratedAsync);
        this.RegisterAsyncEventInvoker<DescriptorEventGeneratedEventArgs>(DescriptorEventGeneratedEventName, this.OnDescriptorGeneratedEventAsync);
    }

    /// <summary>
    /// Gets an observable event that notifies when a Bluetooth device generates a characteristic event.
    /// </summary>
    public ObservableEvent<CharacteristicEventGeneratedEventArgs> OnCharacteristicGeneratedEvent { get; } = new(CharacteristicEventGeneratedEventName);

    /// <summary>
    /// Gets an observable event that notifies when a Bluetooth device generates a descriptor event.
    /// </summary>
    public ObservableEvent<DescriptorEventGeneratedEventArgs> OnDescriptorGeneratedEvent { get; } = new(DescriptorEventGeneratedEventName);

    /// <summary>
    /// Gets an observable event that notifies when a Bluetooth device attempts a GATT connection.
    /// </summary>
    public ObservableEvent<GattConnectionAttemptedEventArgs> OnGattConnectionAttempted { get; } = new(GattConnectionAttemptedEventName);

    /// <summary>
    /// Gets an observable event that notifies when a Bluetooth device prompt is updated.
    /// </summary>
    public ObservableEvent<RequestDevicePromptUpdatedEventArgs> OnRequestDevicePromptUpdated { get; } = new(RequestDevicePromptUpdatedEventName);

    /// <summary>
    /// Gets the module name.
    /// </summary>
    public override string ModuleName => BluetoothModuleName;

    /// <summary>
    /// Disables simulation of Bluetooth devices.
    /// </summary>
    /// <param name="commandProperties">The parameters for the command.</param>
    /// <returns>An empty command result.</returns>
    public async Task<DisableSimulationCommandResult> DisableSimulationAsync(DisableSimulationCommandParameters commandProperties)
    {
        return await this.Driver.ExecuteCommandAsync<DisableSimulationCommandResult>(commandProperties).ConfigureAwait(false);
    }

    /// <summary>
    /// Handles the prompt requesting connection to Bluetooth devices.
    /// </summary>
    /// <param name="commandProperties">The parameters for the command.</param>
    /// <returns>An empty command result.</returns>
    public async Task<HandleRequestDevicePromptCommandResult> HandleRequestDevicePromptAsync(HandleRequestDevicePromptCommandParameters commandProperties)
    {
        return await this.Driver.ExecuteCommandAsync<HandleRequestDevicePromptCommandResult>(commandProperties).ConfigureAwait(false);
    }

    /// <summary>
    /// Simulates the presence or absence of the Bluetooth adapter, along with its power settings.
    /// </summary>
    /// <param name="commandProperties">The parameters for the command.</param>
    /// <returns>An empty command result.</returns>
    public async Task<SimulateAdapterCommandResult> SimulateAdapterAsync(SimulateAdapterCommandParameters commandProperties)
    {
        return await this.Driver.ExecuteCommandAsync<SimulateAdapterCommandResult>(commandProperties).ConfigureAwait(false);
    }

    /// <summary>
    /// Simulates the advertisement of availability of Bluetooth peripherals.
    /// </summary>
    /// <param name="commandProperties">The parameters for the command.</param>
    /// <returns>An empty command result.</returns>
    public async Task<SimulateAdvertisementCommandResult> SimulateAdvertisementAsync(SimulateAdvertisementCommandParameters commandProperties)
    {
        return await this.Driver.ExecuteCommandAsync<SimulateAdvertisementCommandResult>(commandProperties).ConfigureAwait(false);
    }

    /// <summary>
    /// Simulates a characteristic for a Bluetooth device.
    /// </summary>
    /// <param name="commandProperties">The parameters for the command.</param>
    /// <returns>An empty command result.</returns>
    public async Task<SimulateCharacteristicCommandResult> SimulateCharacteristicAsync(SimulateCharacteristicCommandParameters commandProperties)
    {
        return await this.Driver.ExecuteCommandAsync<SimulateCharacteristicCommandResult>(commandProperties).ConfigureAwait(false);
    }

    /// <summary>
    /// Simulates a characteristic response for a Bluetooth device.
    /// </summary>
    /// <param name="commandProperties">The parameters for the command.</param>
    /// <returns>An empty command result.</returns>
    public async Task<SimulateCharacteristicResponseCommandResult> SimulateCharacteristicResponseAsync(SimulateCharacteristicResponseCommandParameters commandProperties)
    {
        return await this.Driver.ExecuteCommandAsync<SimulateCharacteristicResponseCommandResult>(commandProperties).ConfigureAwait(false);
    }

    /// <summary>
    /// Simulates a descriptor response for a Bluetooth device.
    /// </summary>
    /// <param name="commandProperties">The parameters for the command.</param>
    /// <returns>An empty command result.</returns>
    public async Task<SimulateDescriptorCommandResult> SimulateDescriptorAsync(SimulateDescriptorCommandParameters commandProperties)
    {
        return await this.Driver.ExecuteCommandAsync<SimulateDescriptorCommandResult>(commandProperties).ConfigureAwait(false);
    }

    /// <summary>
    /// Simulates a descriptor response for a Bluetooth device.
    /// </summary>
    /// <param name="commandProperties">The parameters for the command.</param>
    /// <returns>An empty command result.</returns>
    public async Task<SimulateDescriptorResponseCommandResult> SimulateDescriptorResponseAsync(SimulateDescriptorResponseCommandParameters commandProperties)
    {
        return await this.Driver.ExecuteCommandAsync<SimulateDescriptorResponseCommandResult>(commandProperties).ConfigureAwait(false);
    }

    /// <summary>
    /// Simulates a GATT connection response for a Bluetooth device.
    /// </summary>
    /// <param name="commandProperties">The parameters for the command.</param>
    /// <returns>An empty command result.</returns>
    public async Task<SimulateGattConnectionResponseCommandResult> SimulateGattConnectionResponseAsync(SimulateGattConnectionResponseCommandParameters commandProperties)
    {
        return await this.Driver.ExecuteCommandAsync<SimulateGattConnectionResponseCommandResult>(commandProperties).ConfigureAwait(false);
    }

    /// <summary>
    /// Simulates a GATT disconnection response for a Bluetooth device.
    /// </summary>
    /// <param name="commandProperties">The parameters for the command.</param>
    /// <returns>An empty command result.</returns>
    public async Task<SimulateGattDisconnectionCommandResult> SimulateGattDisconnectionAsync(SimulateGattDisconnectionCommandParameters commandProperties)
    {
        return await this.Driver.ExecuteCommandAsync<SimulateGattDisconnectionCommandResult>(commandProperties).ConfigureAwait(false);
    }

    /// <summary>
    /// Simulates the presence of a Bluetooth peripheral already connected to the page.
    /// </summary>
    /// <param name="commandProperties">The parameters for the command.</param>
    /// <returns>An empty command result.</returns>
    public async Task<SimulatePreconnectedPeripheralCommandResult> SimulatePreconnectedPeripheralAsync(SimulatePreconnectedPeripheralCommandParameters commandProperties)
    {
        return await this.Driver.ExecuteCommandAsync<SimulatePreconnectedPeripheralCommandResult>(commandProperties).ConfigureAwait(false);
    }

    /// <summary>
    /// Simulates a service for a Bluetooth device.
    /// </summary>
    /// <param name="commandProperties">The parameters for the command.</param>
    /// <returns>An empty command result.</returns>
    public async Task<SimulateServiceCommandResult> SimulateServiceAsync(SimulateServiceCommandParameters commandProperties)
    {
        return await this.Driver.ExecuteCommandAsync<SimulateServiceCommandResult>(commandProperties).ConfigureAwait(false);
    }

    private async Task OnCharacteristicEventGeneratedAsync(EventInfo<CharacteristicEventGeneratedEventArgs> eventData)
    {
        CharacteristicEventGeneratedEventArgs eventArgs = eventData.ToEventArgs<CharacteristicEventGeneratedEventArgs>();
        await this.OnCharacteristicGeneratedEvent.NotifyObserversAsync(eventArgs);
    }

    private async Task OnDescriptorGeneratedEventAsync(EventInfo<DescriptorEventGeneratedEventArgs> eventData)
    {
        DescriptorEventGeneratedEventArgs eventArgs = eventData.ToEventArgs<DescriptorEventGeneratedEventArgs>();
        await this.OnDescriptorGeneratedEvent.NotifyObserversAsync(eventArgs);
    }

    private async Task OnGattConnectionAttemptedAsync(EventInfo<GattConnectionAttemptedEventArgs> eventData)
    {
        GattConnectionAttemptedEventArgs eventArgs = eventData.ToEventArgs<GattConnectionAttemptedEventArgs>();
        await this.OnGattConnectionAttempted.NotifyObserversAsync(eventArgs);
    }

    private async Task OnRequestDevicePromptUpdatedAsync(EventInfo<RequestDevicePromptUpdatedEventArgs> eventData)
    {
        RequestDevicePromptUpdatedEventArgs eventArgs = eventData.ToEventArgs<RequestDevicePromptUpdatedEventArgs>();
        await this.OnRequestDevicePromptUpdated.NotifyObserversAsync(eventArgs);
    }
}
