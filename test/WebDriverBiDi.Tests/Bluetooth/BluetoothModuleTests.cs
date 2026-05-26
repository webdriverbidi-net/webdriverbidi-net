namespace WebDriverBiDi.Bluetooth;

using TestUtilities;

public class BluetoothModuleTests
{
    [Fact]
    public async Task TestDisableSimulation()
    {
        TestWebSocketConnection connection = new();
        connection.OnDataSendComplete.AddObserver(async e =>
        {
            string responseJson = $$"""
                                  {
                                    "type": "success",
                                    "id": {{e.SentCommandId}},
                                    "result": {}
                                  }
                                  """;
            await connection.RaiseDataReceivedEventAsync(responseJson);
        });

        await using BiDiDriver driver = new(TimeSpan.FromMilliseconds(500), new(connection));
        await driver.StartAsync("ws:localhost", TestContext.Current.CancellationToken);
        BluetoothModule module = driver.Bluetooth;

        Task<DisableSimulationCommandResult> task = module.DisableSimulationAsync(new DisableSimulationCommandParameters("myContextId"), cancellationToken: TestContext.Current.CancellationToken);
        DisableSimulationCommandResult result = await task;

        Assert.NotNull(result);
    }

    [Fact]
    public async Task TestHandleRequestDevicePromptCommandAcceptingPrompt()
    {
        TestWebSocketConnection connection = new();
        connection.OnDataSendComplete.AddObserver(async e =>
        {
            string responseJson = $$"""
                                  {
                                    "type": "success",
                                    "id": {{e.SentCommandId}},
                                    "result": {}
                                  }
                                  """;
            await connection.RaiseDataReceivedEventAsync(responseJson);
        });

        await using BiDiDriver driver = new(TimeSpan.FromMilliseconds(500), new(connection));
        await driver.StartAsync("ws:localhost", TestContext.Current.CancellationToken);
        BluetoothModule module = driver.Bluetooth;

        Task<HandleRequestDevicePromptCommandResult> task = module.HandleRequestDevicePromptAsync(new HandleRequestDevicePromptAcceptCommandParameters("myContextId", "myPromptId", "myDeviceId"), cancellationToken: TestContext.Current.CancellationToken);
        HandleRequestDevicePromptCommandResult result = await task.WaitAsync(TimeSpan.FromSeconds(1), TestContext.Current.CancellationToken);

        Assert.NotNull(result);
    }

    [Fact]
    public async Task TestHandleRequestDevicePromptCommandCancelingPrompt()
    {
        TestWebSocketConnection connection = new();
        connection.OnDataSendComplete.AddObserver(async e =>
        {
            string responseJson = $$"""
                                  {
                                    "type": "success",
                                    "id": {{e.SentCommandId}},
                                    "result": {}
                                  }
                                  """;
            await connection.RaiseDataReceivedEventAsync(responseJson);
        });

        await using BiDiDriver driver = new(TimeSpan.FromMilliseconds(500), new(connection));
        await driver.StartAsync("ws:localhost", TestContext.Current.CancellationToken);
        BluetoothModule module = driver.Bluetooth;

        Task<HandleRequestDevicePromptCommandResult> task = module.HandleRequestDevicePromptAsync(new HandleRequestDevicePromptCancelCommandParameters("myContextId", "myPromptId"), cancellationToken: TestContext.Current.CancellationToken);
        HandleRequestDevicePromptCommandResult result = await task.WaitAsync(TimeSpan.FromSeconds(1), TestContext.Current.CancellationToken); ;

        Assert.NotNull(result);
    }

    [Fact]
    public async Task TestSimulateAdapterCommand()
    {
        TestWebSocketConnection connection = new();
        connection.OnDataSendComplete.AddObserver(async e =>
        {
            string responseJson = $$"""
                                  {
                                    "type": "success",
                                    "id": {{e.SentCommandId}},
                                    "result": {}
                                  }
                                  """;
            await connection.RaiseDataReceivedEventAsync(responseJson);
        });

        await using BiDiDriver driver = new(TimeSpan.FromMilliseconds(500), new(connection));
        await driver.StartAsync("ws:localhost", TestContext.Current.CancellationToken);
        BluetoothModule module = driver.Bluetooth;

        Task<SimulateAdapterCommandResult> task = module.SimulateAdapterAsync(new SimulateAdapterCommandParameters("myContextId", AdapterState.PoweredOn), cancellationToken: TestContext.Current.CancellationToken);
        SimulateAdapterCommandResult result = await task.WaitAsync(TimeSpan.FromSeconds(1), TestContext.Current.CancellationToken); ;
        Assert.NotNull(result);
    }

    [Fact]
    public async Task TestSimulateAdvertisementCommand()
    {
        TestWebSocketConnection connection = new();
        connection.OnDataSendComplete.AddObserver(async e =>
        {
            string responseJson = $$"""
                                  {
                                    "type": "success",
                                    "id": {{e.SentCommandId}},
                                    "result": {}
                                  }
                                  """;
            await connection.RaiseDataReceivedEventAsync(responseJson);
        });

        await using BiDiDriver driver = new(TimeSpan.FromMilliseconds(500), new(connection));
        await driver.StartAsync("ws:localhost", TestContext.Current.CancellationToken);
        BluetoothModule module = driver.Bluetooth;

        Task<SimulateAdvertisementCommandResult> task = module.SimulateAdvertisementAsync(new SimulateAdvertisementCommandParameters("myContextId", new SimulateAdvertisementScanEntry("08:08:08:08:08", -10, new ScanRecord())), cancellationToken: TestContext.Current.CancellationToken);
        SimulateAdvertisementCommandResult result = await task.WaitAsync(TimeSpan.FromSeconds(1), TestContext.Current.CancellationToken); ;

        Assert.NotNull(result);
    }

    [Fact]
    public async Task TestSimulateCharacteristic()
    {
        TestWebSocketConnection connection = new();
        connection.OnDataSendComplete.AddObserver(async e =>
        {
            string responseJson = $$"""
                                  {
                                    "type": "success",
                                    "id": {{e.SentCommandId}},
                                    "result": {}
                                  }
                                  """;
            await connection.RaiseDataReceivedEventAsync(responseJson);
        });

        await using BiDiDriver driver = new(TimeSpan.FromMilliseconds(500), new(connection));
        await driver.StartAsync("ws:localhost", TestContext.Current.CancellationToken);
        BluetoothModule module = driver.Bluetooth;

        Task<SimulateCharacteristicCommandResult> task = module.SimulateCharacteristicAsync(new SimulateCharacteristicCommandParameters("myContextId", "myAddress", "my-service-uuid", "my-characteristic-uuid", SimulateCharacteristicType.Add), cancellationToken: TestContext.Current.CancellationToken);
        SimulateCharacteristicCommandResult result = await task.WaitAsync(TimeSpan.FromSeconds(1), TestContext.Current.CancellationToken); ;

        Assert.NotNull(result);
    }

    [Fact]
    public async Task TestSimulateCharacteristicResponse()
    {
        TestWebSocketConnection connection = new();
        connection.OnDataSendComplete.AddObserver(async e =>
        {
            string responseJson = $$"""
                                  {
                                    "type": "success",
                                    "id": {{e.SentCommandId}},
                                    "result": {}
                                  }
                                  """;
            await connection.RaiseDataReceivedEventAsync(responseJson);
        });

        await using BiDiDriver driver = new(TimeSpan.FromMilliseconds(500), new(connection));
        await driver.StartAsync("ws:localhost", TestContext.Current.CancellationToken);
        BluetoothModule module = driver.Bluetooth;

        Task<SimulateCharacteristicResponseCommandResult> task = module.SimulateCharacteristicResponseAsync(new SimulateCharacteristicResponseCommandParameters("myContextId", "myAddress", "my-service-uuid", "my-characteristic-uuid", SimulateCharacteristicResponseType.Read, 0), cancellationToken: TestContext.Current.CancellationToken);
        SimulateCharacteristicResponseCommandResult result = await task.WaitAsync(TimeSpan.FromSeconds(1), TestContext.Current.CancellationToken); ;

        Assert.NotNull(result);
    }

    [Fact]
    public async Task TestSimulateDescriptor()
    {
        TestWebSocketConnection connection = new();
        connection.OnDataSendComplete.AddObserver(async e =>
        {
            string responseJson = $$"""
                                  {
                                    "type": "success",
                                    "id": {{e.SentCommandId}},
                                    "result": {}
                                  }
                                  """;
            await connection.RaiseDataReceivedEventAsync(responseJson);
        });

        await using BiDiDriver driver = new(TimeSpan.FromMilliseconds(500), new(connection));
        await driver.StartAsync("ws:localhost", TestContext.Current.CancellationToken);
        BluetoothModule module = driver.Bluetooth;

        Task<SimulateDescriptorCommandResult> task = module.SimulateDescriptorAsync(new SimulateDescriptorCommandParameters("myContextId", "myAddress", "my-service-uuid", "my-characteristic-uuid", "my-descriptor-uuid", SimulateDescriptorType.Add), cancellationToken: TestContext.Current.CancellationToken);
        SimulateDescriptorCommandResult result = await task.WaitAsync(TimeSpan.FromSeconds(1), TestContext.Current.CancellationToken); ;

        Assert.NotNull(result);
    }

    [Fact]
    public async Task TestSimulateDescriptorResponse()
    {
        TestWebSocketConnection connection = new();
        connection.OnDataSendComplete.AddObserver(async e =>
        {
            string responseJson = $$"""
                                  {
                                    "type": "success",
                                    "id": {{e.SentCommandId}},
                                    "result": {}
                                  }
                                  """;
            await connection.RaiseDataReceivedEventAsync(responseJson);
        });

        await using BiDiDriver driver = new(TimeSpan.FromMilliseconds(500), new(connection));
        await driver.StartAsync("ws:localhost", TestContext.Current.CancellationToken);
        BluetoothModule module = driver.Bluetooth;

        Task<SimulateDescriptorResponseCommandResult> task = module.SimulateDescriptorResponseAsync(new SimulateDescriptorResponseCommandParameters("myContextId", "myAddress", "my-service-uuid", "my-characteristic-uuid", "my-descriptor-uuid", SimulateDescriptorResponseType.Read, 0), cancellationToken: TestContext.Current.CancellationToken);
        SimulateDescriptorResponseCommandResult result = await task.WaitAsync(TimeSpan.FromSeconds(1), TestContext.Current.CancellationToken); ;

        Assert.NotNull(result);
    }

    [Fact]
    public async Task TestSimulateGattConnectionResponse()
    {
        TestWebSocketConnection connection = new();
        connection.OnDataSendComplete.AddObserver(async e =>
        {
            string responseJson = $$"""
                                  {
                                    "type": "success",
                                    "id": {{e.SentCommandId}},
                                    "result": {}
                                  }
                                  """;
            await connection.RaiseDataReceivedEventAsync(responseJson);
        });

        await using BiDiDriver driver = new(TimeSpan.FromMilliseconds(500), new(connection));
        await driver.StartAsync("ws:localhost", TestContext.Current.CancellationToken);
        BluetoothModule module = driver.Bluetooth;

        Task<SimulateGattConnectionResponseCommandResult> task = module.SimulateGattConnectionResponseAsync(new SimulateGattConnectionResponseCommandParameters("myContextId", "myAddress", 0), cancellationToken: TestContext.Current.CancellationToken);
        SimulateGattConnectionResponseCommandResult result = await task.WaitAsync(TimeSpan.FromSeconds(1), TestContext.Current.CancellationToken); ;

        Assert.NotNull(result);
    }

    [Fact]
    public async Task TestSimulateGattDisconnection()
    {
        TestWebSocketConnection connection = new();
        connection.OnDataSendComplete.AddObserver(async e =>
        {
            string responseJson = $$"""
                                  {
                                    "type": "success",
                                    "id": {{e.SentCommandId}},
                                    "result": {}
                                  }
                                  """;
            await connection.RaiseDataReceivedEventAsync(responseJson);
        });

        await using BiDiDriver driver = new(TimeSpan.FromMilliseconds(500), new(connection));
        await driver.StartAsync("ws:localhost", TestContext.Current.CancellationToken);
        BluetoothModule module = driver.Bluetooth;

        Task<SimulateGattDisconnectionCommandResult> task = module.SimulateGattDisconnectionAsync(new SimulateGattDisconnectionCommandParameters("myContextId", "myAddress"), cancellationToken: TestContext.Current.CancellationToken);
        SimulateGattDisconnectionCommandResult result = await task.WaitAsync(TimeSpan.FromSeconds(1), TestContext.Current.CancellationToken); ;

        Assert.NotNull(result);
    }

    [Fact]
    public async Task TestSimulatePreconnectedPeripheralCommand()
    {
        TestWebSocketConnection connection = new();
        connection.OnDataSendComplete.AddObserver(async e =>
        {
            string responseJson = $$"""
                                  {
                                    "type": "success",
                                    "id": {{e.SentCommandId}},
                                    "result": {}
                                  }
                                  """;
            await connection.RaiseDataReceivedEventAsync(responseJson);
        });

        await using BiDiDriver driver = new(TimeSpan.FromMilliseconds(500), new(connection));
        await driver.StartAsync("ws:localhost", TestContext.Current.CancellationToken);
        BluetoothModule module = driver.Bluetooth;

        Task<SimulatePreconnectedPeripheralCommandResult> task = module.SimulatePreconnectedPeripheralAsync(new SimulatePreconnectedPeripheralCommandParameters("myContextId", "08:08:08:08:08", "myDeviceName"), cancellationToken: TestContext.Current.CancellationToken);
        SimulatePreconnectedPeripheralCommandResult result = await task.WaitAsync(TimeSpan.FromSeconds(1), TestContext.Current.CancellationToken); ;

        Assert.NotNull(result);
    }

    [Fact]
    public async Task TestSimulateService()
    {
        TestWebSocketConnection connection = new();
        connection.OnDataSendComplete.AddObserver(async e =>
        {
            string responseJson = $$"""
                                  {
                                    "type": "success",
                                    "id": {{e.SentCommandId}},
                                    "result": {}
                                  }
                                  """;
            await connection.RaiseDataReceivedEventAsync(responseJson);
        });

        await using BiDiDriver driver = new(TimeSpan.FromMilliseconds(500), new(connection));
        await driver.StartAsync("ws:localhost", TestContext.Current.CancellationToken);
        BluetoothModule module = driver.Bluetooth;

        Task<SimulateServiceCommandResult> task = module.SimulateServiceAsync(new SimulateServiceCommandParameters("myContextId", "myAddress", "my-service-uuid", SimulateServiceType.Add), cancellationToken: TestContext.Current.CancellationToken);
        SimulateServiceCommandResult result = await task.WaitAsync(TimeSpan.FromSeconds(1), TestContext.Current.CancellationToken); ;

        Assert.NotNull(result);
    }

    [Fact]
    public async Task TestCanReceiveCharacteristicEventGeneratedEvent()
    {
        TestWebSocketConnection connection = new();
        await using BiDiDriver driver = new(TimeSpan.FromMilliseconds(500), new(connection));
        await driver.StartAsync("ws:localhost", TestContext.Current.CancellationToken);
        BluetoothModule module = driver.Bluetooth;

        TaskCompletionSource taskCompletionSource = new(TaskCreationOptions.RunContinuationsAsynchronously);
        module.OnCharacteristicGeneratedEvent.AddObserver(e =>
        {
            Assert.Equal("myContext", e.BrowsingContextId);
            Assert.Equal("myAddress", e.Address);
            Assert.Equal("my-service-uuid", e.ServiceUuid);
            Assert.Equal("my-characteristic-uuid", e.CharacteristicUuid);
            Assert.Equal(CharacteristicEventGeneratedType.Read, e.Type);
            Assert.Null(e.Data);

            taskCompletionSource.TrySetResult();
        });

        string eventJson = """
                           {
                             "type": "event",
                             "method": "bluetooth.characteristicEventGenerated",
                             "params": {
                               "context": "myContext",
                               "address": "myAddress",
                               "serviceUuid": "my-service-uuid",
                               "characteristicUuid": "my-characteristic-uuid",
                               "type": "read"
                             }
                           }
                           """;
        await connection.RaiseDataReceivedEventAsync(eventJson);
        await taskCompletionSource.Task.WaitAsync(TimeSpan.FromSeconds(5), TestContext.Current.CancellationToken);
    }

    [Fact]
    public async Task TestCanReceiveDescriptorEventGeneratedEvent()
    {
        TestWebSocketConnection connection = new();
        await using BiDiDriver driver = new(TimeSpan.FromMilliseconds(500), new(connection));
        await driver.StartAsync("ws:localhost", TestContext.Current.CancellationToken);
        BluetoothModule module = driver.Bluetooth;

        TaskCompletionSource taskCompletionSource = new(TaskCreationOptions.RunContinuationsAsynchronously);
        module.OnDescriptorGeneratedEvent.AddObserver(e =>
        {
            Assert.Equal("myContext", e.BrowsingContextId);
            Assert.Equal("myAddress", e.Address);
            Assert.Equal("my-service-uuid", e.ServiceUuid);
            Assert.Equal("my-characteristic-uuid", e.CharacteristicUuid);
            Assert.Equal("my-descriptor-uuid", e.DescriptorUuid);
            Assert.Equal(DescriptorEventGeneratedType.Read, e.Type);
            Assert.Null(e.Data);

            taskCompletionSource.TrySetResult();
        });

        string eventJson = """
                           {
                             "type": "event",
                             "method": "bluetooth.descriptorEventGenerated",
                             "params": {
                               "context": "myContext",
                               "address": "myAddress",
                               "serviceUuid": "my-service-uuid",
                               "characteristicUuid": "my-characteristic-uuid",
                               "descriptorUuid": "my-descriptor-uuid",
                               "type": "read"
                             }
                           }
                           """;
        await connection.RaiseDataReceivedEventAsync(eventJson);
        await taskCompletionSource.Task.WaitAsync(TimeSpan.FromSeconds(5), TestContext.Current.CancellationToken);
    }

    [Fact]
    public async Task TestGattConnectionAttemptedEvent()
    {
        TestWebSocketConnection connection = new();
        await using BiDiDriver driver = new(TimeSpan.FromMilliseconds(500), new(connection));
        await driver.StartAsync("ws:localhost", TestContext.Current.CancellationToken);
        BluetoothModule module = driver.Bluetooth;

        TaskCompletionSource taskCompletionSource = new(TaskCreationOptions.RunContinuationsAsynchronously);
        module.OnGattConnectionAttempted.AddObserver(e =>
        {
            Assert.Equal("myContext", e.BrowsingContextId);
            Assert.Equal("myAddress", e.Address);

            taskCompletionSource.TrySetResult();
        });

        string eventJson = """
                           {
                             "type": "event",
                             "method": "bluetooth.gattConnectionAttempted",
                             "params": {
                               "context": "myContext",
                               "address": "myAddress"
                             }
                           }
                           """;
        await connection.RaiseDataReceivedEventAsync(eventJson);
        await taskCompletionSource.Task.WaitAsync(TimeSpan.FromSeconds(5), TestContext.Current.CancellationToken);
    }

    [Fact]
    public async Task TestCanReceiveRequestDevicePromptUpdatedEvent()
    {
        TestWebSocketConnection connection = new();
        await using BiDiDriver driver = new(TimeSpan.FromMilliseconds(500), new(connection));
        await driver.StartAsync("ws:localhost", TestContext.Current.CancellationToken);
        BluetoothModule module = driver.Bluetooth;

        TaskCompletionSource taskCompletionSource = new(TaskCreationOptions.RunContinuationsAsynchronously);
        module.OnRequestDevicePromptUpdated.AddObserver(e =>
        {
            Assert.Equal("myContext", e.BrowsingContextId);
            Assert.Equal("myPrompt", e.Prompt);
            Assert.Single(e.Devices);
            Assert.Equal("myDeviceId", e.Devices[0].DeviceId);
            Assert.Equal("myDeviceName", e.Devices[0].DeviceName);

            taskCompletionSource.TrySetResult();
        });

        string eventJson = """
                           {
                             "type": "event",
                             "method": "bluetooth.requestDevicePromptUpdated",
                             "params": {
                               "context": "myContext",
                               "prompt": "myPrompt",
                               "devices": [
                                 {
                                   "id": "myDeviceId",
                                   "name": "myDeviceName"
                                 }
                               ]
                             }
                           }
                           """;
        await connection.RaiseDataReceivedEventAsync(eventJson);
        await taskCompletionSource.Task.WaitAsync(TimeSpan.FromSeconds(5), TestContext.Current.CancellationToken);
    }
}
