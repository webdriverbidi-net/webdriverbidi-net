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

    private readonly ObservableEvent<CharacteristicEventGeneratedEventArgs> onCharacteristicEventGeneratedEvent = new(CharacteristicEventGeneratedEventName);
    private readonly ObservableEvent<DescriptorEventGeneratedEventArgs> onDescriptorEventGeneratedEvent = new(DescriptorEventGeneratedEventName);
    private readonly ObservableEvent<GattConnectionAttemptedEventArgs> onGattConnectionAttemptedEvent = new(GattConnectionAttemptedEventName);
    private readonly ObservableEvent<RequestDevicePromptUpdatedEventArgs> onRequestDevicePromptUpdatedEvent = new(RequestDevicePromptUpdatedEventName);

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
    public ObservableEvent<CharacteristicEventGeneratedEventArgs> OnCharacteristicGeneratedEvent => this.onCharacteristicEventGeneratedEvent;

    /// <summary>
    /// Gets an observable event that notifies when a Bluetooth device generates a descriptor event.
    /// </summary>
    public ObservableEvent<DescriptorEventGeneratedEventArgs> OnDescriptorGeneratedEvent => this.onDescriptorEventGeneratedEvent;

    /// <summary>
    /// Gets an observable event that notifies when a Bluetooth device attempts a GATT connection.
    /// </summary>
    public ObservableEvent<GattConnectionAttemptedEventArgs> OnGattConnectionAttempted => this.onGattConnectionAttemptedEvent;

    /// <summary>
    /// Gets an observable event that notifies when a Bluetooth device prompt is updated.
    /// </summary>
    public ObservableEvent<RequestDevicePromptUpdatedEventArgs> OnRequestDevicePromptUpdated => this.onRequestDevicePromptUpdatedEvent;

    /// <summary>
    /// Gets the module name.
    /// </summary>
    public override string ModuleName => BluetoothModuleName;

    /// <summary>
    /// Disables simulation of Bluetooth devices.
    /// </summary>
    /// <param name="commandProperties">The parameters for the command.</param>
    /// <returns>An empty command result.</returns>
    public async Task<EmptyResult> DisableSimulationAsync(DisableSimulationCommandParameters commandProperties)
    {
        return await this.Driver.ExecuteCommandAsync<EmptyResult>(commandProperties).ConfigureAwait(false);
    }

    /// <summary>
    /// Handles the prompt requesting connection to Bluetooth devices.
    /// </summary>
    /// <param name="commandProperties">The parameters for the command.</param>
    /// <returns>An empty command result.</returns>
    public async Task<EmptyResult> HandleRequestDevicePromptAsync(HandleRequestDevicePromptCommandParameters commandProperties)
    {
        return await this.Driver.ExecuteCommandAsync<EmptyResult>(commandProperties).ConfigureAwait(false);
    }

    /// <summary>
    /// Simulates the presence or absence of the Bluetooth adapter, along with its power settings.
    /// </summary>
    /// <param name="commandProperties">The parameters for the command.</param>
    /// <returns>An empty command result.</returns>
    public async Task<EmptyResult> SimulateAdapterAsync(SimulateAdapterCommandParameters commandProperties)
    {
        return await this.Driver.ExecuteCommandAsync<EmptyResult>(commandProperties).ConfigureAwait(false);
    }

    /// <summary>
    /// Simulates the advertisement of availability of Bluetooth peripherals.
    /// </summary>
    /// <param name="commandProperties">The parameters for the command.</param>
    /// <returns>An empty command result.</returns>
    public async Task<EmptyResult> SimulateAdvertisementAsync(SimulateAdvertisementCommandParameters commandProperties)
    {
        return await this.Driver.ExecuteCommandAsync<EmptyResult>(commandProperties).ConfigureAwait(false);
    }

    /// <summary>
    /// Simulates a characteristic for a Bluetooth device.
    /// </summary>
    /// <param name="commandProperties">The parameters for the command.</param>
    /// <returns>An empty command result.</returns>
    public async Task<EmptyResult> SimulateCharacteristicAsync(SimulateCharacteristicCommandParameters commandProperties)
    {
        return await this.Driver.ExecuteCommandAsync<EmptyResult>(commandProperties).ConfigureAwait(false);
    }

    /// <summary>
    /// Simulates a characteristic response for a Bluetooth device.
    /// </summary>
    /// <param name="commandProperties">The parameters for the command.</param>
    /// <returns>An empty command result.</returns>
    public async Task<EmptyResult> SimulateCharacteristicResponseAsync(SimulateCharacteristicResponseCommandParameters commandProperties)
    {
        return await this.Driver.ExecuteCommandAsync<EmptyResult>(commandProperties).ConfigureAwait(false);
    }

    /// <summary>
    /// Simulates a descriptor response for a Bluetooth device.
    /// </summary>
    /// <param name="commandProperties">The parameters for the command.</param>
    /// <returns>An empty command result.</returns>
    public async Task<EmptyResult> SimulateDescriptorAsync(SimulateDescriptorCommandParameters commandProperties)
    {
        return await this.Driver.ExecuteCommandAsync<EmptyResult>(commandProperties).ConfigureAwait(false);
    }

    /// <summary>
    /// Simulates a descriptor response for a Bluetooth device.
    /// </summary>
    /// <param name="commandProperties">The parameters for the command.</param>
    /// <returns>An empty command result.</returns>
    public async Task<EmptyResult> SimulateDescriptorResponseAsync(SimulateDescriptorResponseCommandParameters commandProperties)
    {
        return await this.Driver.ExecuteCommandAsync<EmptyResult>(commandProperties).ConfigureAwait(false);
    }

    /// <summary>
    /// Simulates a GATT connection response for a Bluetooth device.
    /// </summary>
    /// <param name="commandProperties">The parameters for the command.</param>
    /// <returns>An empty command result.</returns>
    public async Task<EmptyResult> SimulateGattConnectionResponseAsync(SimulateGattConnectionResponseCommandParameters commandProperties)
    {
        return await this.Driver.ExecuteCommandAsync<EmptyResult>(commandProperties).ConfigureAwait(false);
    }

    /// <summary>
    /// Simulates a GATT disconnection response for a Bluetooth device.
    /// </summary>
    /// <param name="commandProperties">The parameters for the command.</param>
    /// <returns>An empty command result.</returns>
    public async Task<EmptyResult> SimulateGattDisconnectionResponseAsync(SimulateGattDisconnectionCommandParameters commandProperties)
    {
        return await this.Driver.ExecuteCommandAsync<EmptyResult>(commandProperties).ConfigureAwait(false);
    }

    /// <summary>
    /// Simulates the presence of a Bluetooth peripheral already connected to the page.
    /// </summary>
    /// <param name="commandProperties">The parameters for the command.</param>
    /// <returns>An empty command result.</returns>
    public async Task<EmptyResult> SimulatePreconnectedPeripheralAsync(SimulatePreconnectedPeripheralCommandParameters commandProperties)
    {
        return await this.Driver.ExecuteCommandAsync<EmptyResult>(commandProperties).ConfigureAwait(false);
    }

    /// <summary>
    /// Simulates a service for a Bluetooth device.
    /// </summary>
    /// <param name="commandProperties">The parameters for the command.</param>
    /// <returns>An empty command result.</returns>
    public async Task<EmptyResult> SimulateServiceAsync(SimulateServiceCommandParameters commandProperties)
    {
        return await this.Driver.ExecuteCommandAsync<EmptyResult>(commandProperties).ConfigureAwait(false);
    }

    private async Task OnCharacteristicEventGeneratedAsync(EventInfo<CharacteristicEventGeneratedEventArgs> eventData)
    {
        CharacteristicEventGeneratedEventArgs eventArgs = eventData.ToEventArgs<CharacteristicEventGeneratedEventArgs>();
        await this.onCharacteristicEventGeneratedEvent.NotifyObserversAsync(eventArgs);
    }

    private async Task OnDescriptorGeneratedEventAsync(EventInfo<DescriptorEventGeneratedEventArgs> eventData)
    {
        DescriptorEventGeneratedEventArgs eventArgs = eventData.ToEventArgs<DescriptorEventGeneratedEventArgs>();
        await this.onDescriptorEventGeneratedEvent.NotifyObserversAsync(eventArgs);
    }

    private async Task OnGattConnectionAttemptedAsync(EventInfo<GattConnectionAttemptedEventArgs> eventData)
    {
        GattConnectionAttemptedEventArgs eventArgs = eventData.ToEventArgs<GattConnectionAttemptedEventArgs>();
        await this.onGattConnectionAttemptedEvent.NotifyObserversAsync(eventArgs);
    }

    private async Task OnRequestDevicePromptUpdatedAsync(EventInfo<RequestDevicePromptUpdatedEventArgs> eventData)
    {
        RequestDevicePromptUpdatedEventArgs eventArgs = eventData.ToEventArgs<RequestDevicePromptUpdatedEventArgs>();
        await this.onRequestDevicePromptUpdatedEvent.NotifyObserversAsync(eventArgs);
    }
}
