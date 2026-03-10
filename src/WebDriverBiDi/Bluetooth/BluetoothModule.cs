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
    /// <param name="driver">The <see cref="IBiDiCommandExecutor"/> used in the module commands and events.</param>
    public BluetoothModule(IBiDiCommandExecutor driver)
        : base(driver)
    {
        this.RegisterObservableEvent(this.OnRequestDevicePromptUpdated);
        this.RegisterObservableEvent(this.OnGattConnectionAttempted);
        this.RegisterObservableEvent(this.OnCharacteristicGeneratedEvent);
        this.RegisterObservableEvent(this.OnDescriptorGeneratedEvent);
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
    /// <param name="commandParameters">The parameters for the command.</param>
    /// <param name="timeoutOverride">The timeout override to use for the command. If omitted, the value of <see cref="BiDiDriver.DefaultCommandTimeout"/> is used.</param>
    /// <param name="cancellationToken">A cancellation token used to propagate notification that the operation should be canceled. Omitting this argument is the equivalent of using <see cref="CancellationToken.None"/>.</param>
    /// <returns>An empty command result.</returns>
    public Task<DisableSimulationCommandResult> DisableSimulationAsync(DisableSimulationCommandParameters commandParameters, TimeSpan? timeoutOverride = null, CancellationToken cancellationToken = default)
    {
        return this.Driver.ExecuteCommandAsync(commandParameters, timeoutOverride, cancellationToken);
    }

    /// <summary>
    /// Handles the prompt requesting connection to Bluetooth devices.
    /// </summary>
    /// <param name="commandParameters">The parameters for the command.</param>
    /// <param name="timeoutOverride">The timeout override to use for the command. If omitted, the value of <see cref="BiDiDriver.DefaultCommandTimeout"/> is used.</param>
    /// <param name="cancellationToken">A cancellation token used to propagate notification that the operation should be canceled. Omitting this argument is the equivalent of using <see cref="CancellationToken.None"/>.</param>
    /// <returns>An empty command result.</returns>
    public Task<HandleRequestDevicePromptCommandResult> HandleRequestDevicePromptAsync(HandleRequestDevicePromptCommandParameters commandParameters, TimeSpan? timeoutOverride = null, CancellationToken cancellationToken = default)
    {
        return this.Driver.ExecuteCommandAsync(commandParameters, timeoutOverride, cancellationToken);
    }

    /// <summary>
    /// Simulates the presence or absence of the Bluetooth adapter, along with its power settings.
    /// </summary>
    /// <param name="commandParameters">The parameters for the command.</param>
    /// <param name="timeoutOverride">The timeout override to use for the command. If omitted, the value of <see cref="BiDiDriver.DefaultCommandTimeout"/> is used.</param>
    /// <param name="cancellationToken">A cancellation token used to propagate notification that the operation should be canceled. Omitting this argument is the equivalent of using <see cref="CancellationToken.None"/>.</param>
    /// <returns>An empty command result.</returns>
    public Task<SimulateAdapterCommandResult> SimulateAdapterAsync(SimulateAdapterCommandParameters commandParameters, TimeSpan? timeoutOverride = null, CancellationToken cancellationToken = default)
    {
        return this.Driver.ExecuteCommandAsync(commandParameters, timeoutOverride, cancellationToken);
    }

    /// <summary>
    /// Simulates the advertisement of availability of Bluetooth peripherals.
    /// </summary>
    /// <param name="commandParameters">The parameters for the command.</param>
    /// <param name="timeoutOverride">The timeout override to use for the command. If omitted, the value of <see cref="BiDiDriver.DefaultCommandTimeout"/> is used.</param>
    /// <param name="cancellationToken">A cancellation token used to propagate notification that the operation should be canceled. Omitting this argument is the equivalent of using <see cref="CancellationToken.None"/>.</param>
    /// <returns>An empty command result.</returns>
    public Task<SimulateAdvertisementCommandResult> SimulateAdvertisementAsync(SimulateAdvertisementCommandParameters commandParameters, TimeSpan? timeoutOverride = null, CancellationToken cancellationToken = default)
    {
        return this.Driver.ExecuteCommandAsync(commandParameters, timeoutOverride, cancellationToken);
    }

    /// <summary>
    /// Simulates a characteristic for a Bluetooth device.
    /// </summary>
    /// <param name="commandParameters">The parameters for the command.</param>
    /// <param name="timeoutOverride">The timeout override to use for the command. If omitted, the value of <see cref="BiDiDriver.DefaultCommandTimeout"/> is used.</param>
    /// <param name="cancellationToken">A cancellation token used to propagate notification that the operation should be canceled. Omitting this argument is the equivalent of using <see cref="CancellationToken.None"/>.</param>
    /// <returns>An empty command result.</returns>
    public Task<SimulateCharacteristicCommandResult> SimulateCharacteristicAsync(SimulateCharacteristicCommandParameters commandParameters, TimeSpan? timeoutOverride = null, CancellationToken cancellationToken = default)
    {
        return this.Driver.ExecuteCommandAsync(commandParameters, timeoutOverride, cancellationToken);
    }

    /// <summary>
    /// Simulates a characteristic response for a Bluetooth device.
    /// </summary>
    /// <param name="commandParameters">The parameters for the command.</param>
    /// <param name="timeoutOverride">The timeout override to use for the command. If omitted, the value of <see cref="BiDiDriver.DefaultCommandTimeout"/> is used.</param>
    /// <param name="cancellationToken">A cancellation token used to propagate notification that the operation should be canceled. Omitting this argument is the equivalent of using <see cref="CancellationToken.None"/>.</param>
    /// <returns>An empty command result.</returns>
    public Task<SimulateCharacteristicResponseCommandResult> SimulateCharacteristicResponseAsync(SimulateCharacteristicResponseCommandParameters commandParameters, TimeSpan? timeoutOverride = null, CancellationToken cancellationToken = default)
    {
        return this.Driver.ExecuteCommandAsync(commandParameters, timeoutOverride, cancellationToken);
    }

    /// <summary>
    /// Simulates a descriptor for a Bluetooth device.
    /// </summary>
    /// <param name="commandParameters">The parameters for the command.</param>
    /// <param name="timeoutOverride">The timeout override to use for the command. If omitted, the value of <see cref="BiDiDriver.DefaultCommandTimeout"/> is used.</param>
    /// <param name="cancellationToken">A cancellation token used to propagate notification that the operation should be canceled. Omitting this argument is the equivalent of using <see cref="CancellationToken.None"/>.</param>
    /// <returns>An empty command result.</returns>
    public Task<SimulateDescriptorCommandResult> SimulateDescriptorAsync(SimulateDescriptorCommandParameters commandParameters, TimeSpan? timeoutOverride = null, CancellationToken cancellationToken = default)
    {
        return this.Driver.ExecuteCommandAsync(commandParameters, timeoutOverride, cancellationToken);
    }

    /// <summary>
    /// Simulates a descriptor response for a Bluetooth device.
    /// </summary>
    /// <param name="commandParameters">The parameters for the command.</param>
    /// <param name="timeoutOverride">The timeout override to use for the command. If omitted, the value of <see cref="BiDiDriver.DefaultCommandTimeout"/> is used.</param>
    /// <param name="cancellationToken">A cancellation token used to propagate notification that the operation should be canceled. Omitting this argument is the equivalent of using <see cref="CancellationToken.None"/>.</param>
    /// <returns>An empty command result.</returns>
    public Task<SimulateDescriptorResponseCommandResult> SimulateDescriptorResponseAsync(SimulateDescriptorResponseCommandParameters commandParameters, TimeSpan? timeoutOverride = null, CancellationToken cancellationToken = default)
    {
        return this.Driver.ExecuteCommandAsync(commandParameters, timeoutOverride, cancellationToken);
    }

    /// <summary>
    /// Simulates a GATT connection response for a Bluetooth device.
    /// </summary>
    /// <param name="commandParameters">The parameters for the command.</param>
    /// <param name="timeoutOverride">The timeout override to use for the command. If omitted, the value of <see cref="BiDiDriver.DefaultCommandTimeout"/> is used.</param>
    /// <param name="cancellationToken">A cancellation token used to propagate notification that the operation should be canceled. Omitting this argument is the equivalent of using <see cref="CancellationToken.None"/>.</param>
    /// <returns>An empty command result.</returns>
    public Task<SimulateGattConnectionResponseCommandResult> SimulateGattConnectionResponseAsync(SimulateGattConnectionResponseCommandParameters commandParameters, TimeSpan? timeoutOverride = null, CancellationToken cancellationToken = default)
    {
        return this.Driver.ExecuteCommandAsync(commandParameters, timeoutOverride, cancellationToken);
    }

    /// <summary>
    /// Simulates a GATT disconnection response for a Bluetooth device.
    /// </summary>
    /// <param name="commandParameters">The parameters for the command.</param>
    /// <param name="timeoutOverride">The timeout override to use for the command. If omitted, the value of <see cref="BiDiDriver.DefaultCommandTimeout"/> is used.</param>
    /// <param name="cancellationToken">A cancellation token used to propagate notification that the operation should be canceled. Omitting this argument is the equivalent of using <see cref="CancellationToken.None"/>.</param>
    /// <returns>An empty command result.</returns>
    public Task<SimulateGattDisconnectionCommandResult> SimulateGattDisconnectionAsync(SimulateGattDisconnectionCommandParameters commandParameters, TimeSpan? timeoutOverride = null, CancellationToken cancellationToken = default)
    {
        return this.Driver.ExecuteCommandAsync(commandParameters, timeoutOverride, cancellationToken);
    }

    /// <summary>
    /// Simulates the presence of a Bluetooth peripheral already connected to the page.
    /// </summary>
    /// <param name="commandParameters">The parameters for the command.</param>
    /// <param name="timeoutOverride">The timeout override to use for the command. If omitted, the value of <see cref="BiDiDriver.DefaultCommandTimeout"/> is used.</param>
    /// <param name="cancellationToken">A cancellation token used to propagate notification that the operation should be canceled. Omitting this argument is the equivalent of using <see cref="CancellationToken.None"/>.</param>
    /// <returns>An empty command result.</returns>
    public Task<SimulatePreconnectedPeripheralCommandResult> SimulatePreconnectedPeripheralAsync(SimulatePreconnectedPeripheralCommandParameters commandParameters, TimeSpan? timeoutOverride = null, CancellationToken cancellationToken = default)
    {
        return this.Driver.ExecuteCommandAsync(commandParameters, timeoutOverride, cancellationToken);
    }

    /// <summary>
    /// Simulates a service for a Bluetooth device.
    /// </summary>
    /// <param name="commandParameters">The parameters for the command.</param>
    /// <param name="timeoutOverride">The timeout override to use for the command. If omitted, the value of <see cref="BiDiDriver.DefaultCommandTimeout"/> is used.</param>
    /// <param name="cancellationToken">A cancellation token used to propagate notification that the operation should be canceled. Omitting this argument is the equivalent of using <see cref="CancellationToken.None"/>.</param>
    /// <returns>An empty command result.</returns>
    public Task<SimulateServiceCommandResult> SimulateServiceAsync(SimulateServiceCommandParameters commandParameters, TimeSpan? timeoutOverride = null, CancellationToken cancellationToken = default)
    {
        return this.Driver.ExecuteCommandAsync(commandParameters, timeoutOverride, cancellationToken);
    }
}
