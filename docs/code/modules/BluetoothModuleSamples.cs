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
}
