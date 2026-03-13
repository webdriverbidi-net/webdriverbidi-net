// <copyright file="BluetoothModuleSamples.cs" company="WebDriverBiDi.NET Committers">
// Copyright (c) WebDriverBiDi.NET Committers. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for license information.
// </copyright>
// Code snippets for docs/articles/modules/bluetooth.md

#pragma warning disable CS8600, CS8602

namespace WebDriverBiDi.Docs.Code.Modules;

using System.Collections.Generic;
using WebDriverBiDi;
using WebDriverBiDi.Bluetooth;
using WebDriverBiDi.BrowsingContext;
using WebDriverBiDi.Script;
using WebDriverBiDi.Session;

/// <summary>
/// Snippets for Bluetooth module documentation. Compiled at build time to prevent API drift.
/// </summary>
public static class BluetoothModuleSamples
{
    /// <summary>
    /// Accessing the module.
    /// </summary>
    public static void AccessingModule(BiDiDriver driver)
    {
#region AccessingModule
        BluetoothModule bluetooth = driver.Bluetooth;
#endregion
    }

    /// <summary>
    /// Basic device simulation using preconnected peripheral.
    /// </summary>
    public static async Task BasicDeviceSimulation(BiDiDriver driver, string contextId)
    {
#region BasicDeviceSimulation
        SimulatePreconnectedPeripheralCommandParameters parameters =
            new SimulatePreconnectedPeripheralCommandParameters(
                contextId,
                "00:00:00:00:00:00",
                "Test Device");
        parameters.ManufacturerData.Add(new BluetoothManufacturerData(
            0x004C,
            Convert.ToBase64String(new byte[] { 0x01, 0x02, 0x03 })));

        await driver.Bluetooth.SimulatePreconnectedPeripheralAsync(parameters);
        Console.WriteLine("Bluetooth device simulated");
#endregion
    }

    /// <summary>
    /// Device with services.
    /// </summary>
    public static async Task DeviceWithServices(BiDiDriver driver, string contextId)
    {
#region DevicewithServices
        SimulatePreconnectedPeripheralCommandParameters @params =
            new SimulatePreconnectedPeripheralCommandParameters(
                contextId,
                "AA:BB:CC:DD:EE:FF",
                "Heart Rate Monitor");
        @params.KnownServiceUUIDs.Add("heart_rate");

        await driver.Bluetooth.SimulatePreconnectedPeripheralAsync(@params);
        Console.WriteLine("Bluetooth heart rate monitor simulated");
#endregion
    }

    /// <summary>
    /// Basic advertisement.
    /// </summary>
    public static async Task BasicAdvertisement(BiDiDriver driver, string contextId)
    {
#region BasicAdvertisement
        ScanRecord scanRecord = new ScanRecord();
        SimulateAdvertisementScanEntry scanEntry = new SimulateAdvertisementScanEntry(
            "00:00:00:00:00:00",
            -50,
            scanRecord);

        SimulateAdvertisementCommandParameters @params =
            new SimulateAdvertisementCommandParameters(contextId, scanEntry);

        await driver.Bluetooth.SimulateAdvertisementAsync(@params);
        Console.WriteLine("Bluetooth advertisement simulated");
#endregion
    }

    /// <summary>
    /// Advertisement with signal strength (weak and strong).
    /// </summary>
    public static async Task AdvertisementWithSignalStrength(BiDiDriver driver, string contextId)
    {
#region AdvertisementwithSignalStrength
        // Simulate weak signal
        ScanRecord scanRecord = new ScanRecord();
        SimulateAdvertisementScanEntry weakSignal = new SimulateAdvertisementScanEntry(
            "00:00:00:00:00:00",
            -80,
            scanRecord);
        await driver.Bluetooth.SimulateAdvertisementAsync(
            new SimulateAdvertisementCommandParameters(contextId, weakSignal));

        // Simulate strong signal
        SimulateAdvertisementScanEntry strongSignal = new SimulateAdvertisementScanEntry(
            "00:00:00:00:00:00",
            -30,
            scanRecord);
        await driver.Bluetooth.SimulateAdvertisementAsync(
            new SimulateAdvertisementCommandParameters(contextId, strongSignal));
#endregion
    }

    /// <summary>
    /// Testing Bluetooth scanner.
    /// </summary>
    public static async Task TestingBluetoothScanner(BiDiDriver driver, string contextId)
    {
#region TestingBluetoothScanner
        // Simulate a Bluetooth device
        SimulatePreconnectedPeripheralCommandParameters deviceParams =
            new SimulatePreconnectedPeripheralCommandParameters(
                contextId,
                "AA:BB:CC:DD:EE:FF",
                "Heart Rate Monitor");
        deviceParams.KnownServiceUUIDs.Add("heart_rate");

        await driver.Bluetooth.SimulatePreconnectedPeripheralAsync(deviceParams);

        // Trigger Bluetooth scan in page
        await driver.BrowsingContext.NavigateAsync(
            new NavigateCommandParameters(contextId, "https://example.com")
            { Wait = ReadinessState.Complete });

        // Navigate to page with Bluetooth functionality
        EvaluateResult result = await driver.Script.EvaluateAsync(
            new EvaluateCommandParameters(
                @"navigator.bluetooth.requestDevice({
                    filters: [{ services: ['heart_rate'] }]
                }).then(device => device.name)",
                new ContextTarget(contextId),
                true));

        if (result is EvaluateResultSuccess success)
        {
            string deviceName = success.Result.ValueAs<string>();
            Console.WriteLine($"Found device: {deviceName}");
        }
#endregion
    }

    /// <summary>
    /// Testing multiple devices.
    /// </summary>
    public static async Task TestingMultipleDevices(BiDiDriver driver, string contextId)
    {
#region TestingMultipleDevices
        List<string> deviceAddresses = new List<string>
        {
            "AA:BB:CC:DD:EE:01",
            "AA:BB:CC:DD:EE:02",
            "AA:BB:CC:DD:EE:03"
        };

        foreach (string address in deviceAddresses)
        {
            SimulatePreconnectedPeripheralCommandParameters @params =
                new SimulatePreconnectedPeripheralCommandParameters(
                    contextId,
                    address,
                    $"Device {address[^2..]}");
            @params.KnownServiceUUIDs.Add("battery_service");

            await driver.Bluetooth.SimulatePreconnectedPeripheralAsync(@params);
        }

        // Test device discovery
        EvaluateResult result = await driver.Script.EvaluateAsync(
            new EvaluateCommandParameters(
                @"navigator.bluetooth.requestDevice({
                    filters: [{ services: ['battery_service'] }],
                    acceptAllDevices: false
                }).then(device => device.id)",
                new ContextTarget(contextId),
                true));
#endregion
    }

    /// <summary>
    /// Testing device connection.
    /// </summary>
    public static async Task TestingDeviceConnection(BiDiDriver driver, string contextId)
    {
#region TestingDeviceConnection
        // Simulate device
        SimulatePreconnectedPeripheralCommandParameters deviceParams =
            new SimulatePreconnectedPeripheralCommandParameters(
                contextId,
                "11:22:33:44:55:66",
                "Temperature Sensor");
        deviceParams.KnownServiceUUIDs.Add("environmental_sensing");

        await driver.Bluetooth.SimulatePreconnectedPeripheralAsync(deviceParams);

        // Advertise device
        ScanRecord scanRecord = new ScanRecord { UUIDs = new List<string> { "environmental_sensing" } };
        SimulateAdvertisementScanEntry adScanEntry = new SimulateAdvertisementScanEntry(
            "11:22:33:44:55:66",
            -45,
            scanRecord);
        await driver.Bluetooth.SimulateAdvertisementAsync(
            new SimulateAdvertisementCommandParameters(contextId, adScanEntry));

        // Test connection
        EvaluateResult result = await driver.Script.EvaluateAsync(
            new EvaluateCommandParameters(
                @"navigator.bluetooth.requestDevice({
                    filters: [{ services: ['environmental_sensing'] }]
                })
                .then(device => device.gatt.connect())
                .then(server => 'connected')
                .catch(err => `error: ${err.message}`)",
                new ContextTarget(contextId),
                true));

        if (result is EvaluateResultSuccess success)
        {
            string connectionStatus = success.Result.ValueAs<string>();
            Console.WriteLine($"Connection status: {connectionStatus}");
        }
#endregion
    }

    /// <summary>
    /// Simulate Bluetooth adapter state.
    /// </summary>
    public static async Task SimulateAdapter(BiDiDriver driver, string contextId)
    {
#region SimulateAdapter
        // Simulate adapter powered on
        SimulateAdapterCommandParameters parameters =
            new SimulateAdapterCommandParameters(contextId, AdapterState.PoweredOn);
        await driver.Bluetooth.SimulateAdapterAsync(parameters);

        // Or simulate adapter absent or powered off
        await driver.Bluetooth.SimulateAdapterAsync(
            new SimulateAdapterCommandParameters(contextId, AdapterState.Absent));
#endregion
    }

    /// <summary>
    /// Disable Bluetooth simulation.
    /// </summary>
    public static async Task DisableSimulation(BiDiDriver driver, string contextId)
    {
#region DisableSimulation
        DisableSimulationCommandParameters parameters =
            new DisableSimulationCommandParameters(contextId);
        await driver.Bluetooth.DisableSimulationAsync(parameters);
#endregion
    }

    /// <summary>
    /// Handle request device prompt - accept or cancel.
    /// </summary>
    public static async Task HandleRequestDevicePrompt(BiDiDriver driver)
    {
#region HandleRequestDevicePrompt
        // Accept the prompt and select a device
        HandleRequestDevicePromptAcceptCommandParameters acceptParams =
            new HandleRequestDevicePromptAcceptCommandParameters(
                "contextId",
                "promptId",
                "deviceId");
        await driver.Bluetooth.HandleRequestDevicePromptAsync(acceptParams);

        // Or cancel the prompt
        HandleRequestDevicePromptCancelCommandParameters cancelParams =
            new HandleRequestDevicePromptCancelCommandParameters("contextId", "promptId");
        await driver.Bluetooth.HandleRequestDevicePromptAsync(cancelParams);
#endregion
    }

    /// <summary>
    /// Simulate GATT service.
    /// </summary>
    public static async Task SimulateService(BiDiDriver driver, string contextId)
    {
#region SimulateService
        SimulateServiceCommandParameters addParams =
            new SimulateServiceCommandParameters(
                contextId,
                "AA:BB:CC:DD:EE:FF",
                "heart_rate",
                SimulateServiceType.Add);
        await driver.Bluetooth.SimulateServiceAsync(addParams);

        // Remove service
        SimulateServiceCommandParameters removeParams =
            new SimulateServiceCommandParameters(
                contextId,
                "AA:BB:CC:DD:EE:FF",
                "heart_rate",
                SimulateServiceType.Remove);
        await driver.Bluetooth.SimulateServiceAsync(removeParams);
#endregion
    }

    /// <summary>
    /// Simulate GATT characteristic.
    /// </summary>
    public static async Task SimulateCharacteristic(BiDiDriver driver, string contextId)
    {
#region SimulateCharacteristic
        CharacteristicProperties properties = new CharacteristicProperties
        {
            IsRead = true,
            IsNotify = true,
        };

        SimulateCharacteristicCommandParameters parameters =
            new SimulateCharacteristicCommandParameters(
                contextId,
                "AA:BB:CC:DD:EE:FF",
                "heart_rate",
                "heart_rate_measurement",
                SimulateCharacteristicType.Add)
        {
            CharacteristicProperties = properties,
        };
        await driver.Bluetooth.SimulateCharacteristicAsync(parameters);
#endregion
    }

    /// <summary>
    /// Simulate characteristic response.
    /// </summary>
    public static async Task SimulateCharacteristicResponse(BiDiDriver driver, string contextId)
    {
#region SimulateCharacteristicResponse
        // Simulate successful read response (code 0 = success)
        SimulateCharacteristicResponseCommandParameters readParams =
            new SimulateCharacteristicResponseCommandParameters(
                contextId,
                "AA:BB:CC:DD:EE:FF",
                "heart_rate",
                "heart_rate_measurement",
                SimulateCharacteristicResponseType.Read,
                0)
        {
            Data = new List<uint> { 0x64 },  // Heart rate value 100
        };
        await driver.Bluetooth.SimulateCharacteristicResponseAsync(readParams);
#endregion
    }

    /// <summary>
    /// Simulate GATT descriptor.
    /// </summary>
    public static async Task SimulateDescriptor(BiDiDriver driver, string contextId)
    {
#region SimulateDescriptor
        SimulateDescriptorCommandParameters parameters =
            new SimulateDescriptorCommandParameters(
                contextId,
                "AA:BB:CC:DD:EE:FF",
                "heart_rate",
                "heart_rate_measurement",
                "gatt.characteristic_user_description",
                SimulateDescriptorType.Add);
        await driver.Bluetooth.SimulateDescriptorAsync(parameters);
#endregion
    }

    /// <summary>
    /// Simulate descriptor response.
    /// </summary>
    public static async Task SimulateDescriptorResponse(BiDiDriver driver, string contextId)
    {
#region SimulateDescriptorResponse
        SimulateDescriptorResponseCommandParameters parameters =
            new SimulateDescriptorResponseCommandParameters(
                contextId,
                "AA:BB:CC:DD:EE:FF",
                "heart_rate",
                "heart_rate_measurement",
                "gatt.characteristic_user_description",
                SimulateDescriptorResponseType.Read,
                0)
        {
            Data = new List<uint> { 0x48, 0x65, 0x61, 0x72, 0x74, 0x20, 0x52, 0x61, 0x74, 0x65 },
        };
        await driver.Bluetooth.SimulateDescriptorResponseAsync(parameters);
#endregion
    }

    /// <summary>
    /// Simulate GATT connection response.
    /// </summary>
    public static async Task SimulateGattConnectionResponse(BiDiDriver driver, string contextId)
    {
#region SimulateGattConnectionResponse
        // Code 0 = success
        SimulateGattConnectionResponseCommandParameters successParams =
            new SimulateGattConnectionResponseCommandParameters(
                contextId,
                "AA:BB:CC:DD:EE:FF",
                0);
        await driver.Bluetooth.SimulateGattConnectionResponseAsync(successParams);
#endregion
    }

    /// <summary>
    /// Simulate GATT disconnection.
    /// </summary>
    public static async Task SimulateGattDisconnection(BiDiDriver driver, string contextId)
    {
#region SimulateGattDisconnection
        SimulateGattDisconnectionCommandParameters parameters =
            new SimulateGattDisconnectionCommandParameters(contextId, "AA:BB:CC:DD:EE:FF");
        await driver.Bluetooth.SimulateGattDisconnectionAsync(parameters);
#endregion
    }

    /// <summary>
    /// Request device prompt updated event.
    /// </summary>
    public static async Task RequestDevicePromptUpdated(BiDiDriver driver)
    {
#region RequestDevicePromptUpdated
        driver.Bluetooth.OnRequestDevicePromptUpdated.AddObserver((RequestDevicePromptUpdatedEventArgs e) =>
        {
            Console.WriteLine($"Prompt {e.Prompt} in context {e.BrowsingContextId}");
            foreach (RequestDeviceInfo device in e.Devices)
            {
                Console.WriteLine($"  Device: {device.DeviceName} ({device.DeviceId})");
            }
        });

        SubscribeCommandParameters subscribe =
            new SubscribeCommandParameters(driver.Bluetooth.OnRequestDevicePromptUpdated.EventName);
        await driver.Session.SubscribeAsync(subscribe);
#endregion
    }

    /// <summary>
    /// GATT connection attempted event.
    /// </summary>
    public static async Task GattConnectionAttempted(BiDiDriver driver)
    {
#region GattConnectionAttempted
        driver.Bluetooth.OnGattConnectionAttempted.AddObserver((GattConnectionAttemptedEventArgs e) =>
        {
            Console.WriteLine($"GATT connection attempted: {e.Address} in context {e.BrowsingContextId}");
        });

        SubscribeCommandParameters subscribe =
            new SubscribeCommandParameters(driver.Bluetooth.OnGattConnectionAttempted.EventName);
        await driver.Session.SubscribeAsync(subscribe);
#endregion
    }

    /// <summary>
    /// Characteristic event generated.
    /// </summary>
    public static async Task CharacteristicEventGenerated(BiDiDriver driver)
    {
#region CharacteristicEventGenerated
        driver.Bluetooth.OnCharacteristicGeneratedEvent.AddObserver((CharacteristicEventGeneratedEventArgs e) =>
        {
            Console.WriteLine($"Characteristic {e.CharacteristicUuid} event: {e.Type}");
        });

        SubscribeCommandParameters subscribe =
            new SubscribeCommandParameters(driver.Bluetooth.OnCharacteristicGeneratedEvent.EventName);
        await driver.Session.SubscribeAsync(subscribe);
#endregion
    }

    /// <summary>
    /// Descriptor event generated.
    /// </summary>
    public static async Task DescriptorEventGenerated(BiDiDriver driver)
    {
#region DescriptorEventGenerated
        driver.Bluetooth.OnDescriptorGeneratedEvent.AddObserver((DescriptorEventGeneratedEventArgs e) =>
        {
            Console.WriteLine($"Descriptor {e.DescriptorUuid} event: {e.Type}");
        });

        SubscribeCommandParameters subscribe =
            new SubscribeCommandParameters(driver.Bluetooth.OnDescriptorGeneratedEvent.EventName);
        await driver.Session.SubscribeAsync(subscribe);
#endregion
    }
}
