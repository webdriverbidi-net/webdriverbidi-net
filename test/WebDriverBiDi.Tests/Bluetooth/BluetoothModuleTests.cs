namespace WebDriverBiDi.Bluetooth;

using TestUtilities;

[TestFixture]
public class BluetoothModuleTests
{
    [Test]
    public async Task TestDisableSimulation()
    {
        TestWebSocketConnection connection = new();
        connection.DataSendComplete += async (sender, e) =>
        {
            string responseJson = $$"""
                                  {
                                    "type": "success",
                                    "id": {{e.SentCommandId}},
                                    "result": {}
                                  }
                                  """;
            await connection.RaiseDataReceivedEventAsync(responseJson);
        };

        BiDiDriver driver = new(TimeSpan.FromMilliseconds(500), new(connection));
        await driver.StartAsync("ws:localhost");
        BluetoothModule module = new(driver);

        Task<DisableSimulationCommandResult> task = module.DisableSimulationAsync(new DisableSimulationCommandParameters("myContextId"));
        task.Wait(TimeSpan.FromSeconds(1));
        DisableSimulationCommandResult result = task.Result;
        
        Assert.That(result, Is.Not.Null);
    }

    [Test]
    public async Task TestHandleRequestDevicePromptCommandAcceptingPrompt()
    {
        TestWebSocketConnection connection = new();
        connection.DataSendComplete += async (sender, e) =>
        {
            string responseJson = $$"""
                                  {
                                    "type": "success",
                                    "id": {{e.SentCommandId}},
                                    "result": {}
                                  }
                                  """;
            await connection.RaiseDataReceivedEventAsync(responseJson);
        };

        BiDiDriver driver = new(TimeSpan.FromMilliseconds(500), new(connection));
        await driver.StartAsync("ws:localhost");
        BluetoothModule module = new(driver);

        Task<HandleRequestDevicePromptCommandResult> task = module.HandleRequestDevicePromptAsync(new HandleRequestDevicePromptAcceptCommandParameters("myContextId", "myPromptId", "myDeviceId"));
        task.Wait(TimeSpan.FromSeconds(1));
        EmptyResult result = task.Result;
        
        Assert.That(result, Is.Not.Null);
    }

    [Test]
    public async Task TestHandleRequestDevicePromptCommandCancelingPrompt()
    {
        TestWebSocketConnection connection = new();
        connection.DataSendComplete += async (sender, e) =>
        {
            string responseJson = $$"""
                                  {
                                    "type": "success",
                                    "id": {{e.SentCommandId}},
                                    "result": {}
                                  }
                                  """;
            await connection.RaiseDataReceivedEventAsync(responseJson);
        };

        BiDiDriver driver = new(TimeSpan.FromMilliseconds(500), new(connection));
        await driver.StartAsync("ws:localhost");
        BluetoothModule module = new(driver);

        Task<HandleRequestDevicePromptCommandResult> task = module.HandleRequestDevicePromptAsync(new HandleRequestDevicePromptCancelCommandParameters("myContextId", "myPromptId"));
        task.Wait(TimeSpan.FromSeconds(1));
        HandleRequestDevicePromptCommandResult result = task.Result;
        
        Assert.That(result, Is.Not.Null);
    }

    [Test]
    public async Task TestSimulateAdapterCommand()
    {
        TestWebSocketConnection connection = new();
        connection.DataSendComplete += async (sender, e) =>
        {
            string responseJson = $$"""
                                  {
                                    "type": "success",
                                    "id": {{e.SentCommandId}},
                                    "result": {}
                                  }
                                  """;
            await connection.RaiseDataReceivedEventAsync(responseJson);
        };

        BiDiDriver driver = new(TimeSpan.FromMilliseconds(500), new(connection));
        await driver.StartAsync("ws:localhost");
        BluetoothModule module = new(driver);

        Task<SimulateAdapterCommandResult> task = module.SimulateAdapterAsync(new SimulateAdapterCommandParameters("myContextId", AdapterState.PoweredOn));
        task.Wait(TimeSpan.FromSeconds(1));
        SimulateAdapterCommandResult result = task.Result;        
        Assert.That(result, Is.Not.Null);
    }

    [Test]
    public async Task TestSimulateAdvertisementCommand()
    {
        TestWebSocketConnection connection = new();
        connection.DataSendComplete += async (sender, e) =>
        {
            string responseJson = $$"""
                                  {
                                    "type": "success",
                                    "id": {{e.SentCommandId}},
                                    "result": {}
                                  }
                                  """;
            await connection.RaiseDataReceivedEventAsync(responseJson);
        };

        BiDiDriver driver = new(TimeSpan.FromMilliseconds(500), new(connection));
        await driver.StartAsync("ws:localhost");
        BluetoothModule module = new(driver);

        Task<SimulateAdvertisementCommandResult> task = module.SimulateAdvertisementAsync(new SimulateAdvertisementCommandParameters("myContextId", new SimulateAdvertisementScanEntry("08:08:08:08:08", -10, new ScanRecord())));
        task.Wait(TimeSpan.FromSeconds(1));
        SimulateAdvertisementCommandResult result = task.Result;
        
        Assert.That(result, Is.Not.Null);
    }

    [Test]
    public async Task TestSimulateCharacteristic()
    {
        TestWebSocketConnection connection = new();
        connection.DataSendComplete += async (sender, e) =>
        {
            string responseJson = $$"""
                                  {
                                    "type": "success",
                                    "id": {{e.SentCommandId}},
                                    "result": {}
                                  }
                                  """;
            await connection.RaiseDataReceivedEventAsync(responseJson);
        };

        BiDiDriver driver = new(TimeSpan.FromMilliseconds(500), new(connection));
        await driver.StartAsync("ws:localhost");
        BluetoothModule module = new(driver);

        Task<SimulateCharacteristicCommandResult> task = module.SimulateCharacteristicAsync(new SimulateCharacteristicCommandParameters("myContextId", "myAddress", "my-service-uuid", "my-characteristic-uuid", SimulateCharacteristicType.Add));
        task.Wait(TimeSpan.FromSeconds(1));
        SimulateCharacteristicCommandResult result = task.Result;
        
        Assert.That(result, Is.Not.Null);
    }

    [Test]
    public async Task TestSimulateCharacteristicResponse()
    {
        TestWebSocketConnection connection = new();
        connection.DataSendComplete += async (sender, e) =>
        {
            string responseJson = $$"""
                                  {
                                    "type": "success",
                                    "id": {{e.SentCommandId}},
                                    "result": {}
                                  }
                                  """;
            await connection.RaiseDataReceivedEventAsync(responseJson);
        };

        BiDiDriver driver = new(TimeSpan.FromMilliseconds(500), new(connection));
        await driver.StartAsync("ws:localhost");
        BluetoothModule module = new(driver);

        Task<SimulateCharacteristicResponseCommandResult> task = module.SimulateCharacteristicResponseAsync(new SimulateCharacteristicResponseCommandParameters("myContextId", "myAddress", "my-service-uuid", "my-characteristic-uuid", SimulateCharacteristicResponseType.Read, 0));
        task.Wait(TimeSpan.FromSeconds(1));
        SimulateCharacteristicResponseCommandResult result = task.Result;
        
        Assert.That(result, Is.Not.Null);
    }

    [Test]
    public async Task TestSimulateDescriptor()
    {
        TestWebSocketConnection connection = new();
        connection.DataSendComplete += async (sender, e) =>
        {
            string responseJson = $$"""
                                  {
                                    "type": "success",
                                    "id": {{e.SentCommandId}},
                                    "result": {}
                                  }
                                  """;
            await connection.RaiseDataReceivedEventAsync(responseJson);
        };

        BiDiDriver driver = new(TimeSpan.FromMilliseconds(500), new(connection));
        await driver.StartAsync("ws:localhost");
        BluetoothModule module = new(driver);

        Task<SimulateDescriptorCommandResult> task = module.SimulateDescriptorAsync(new SimulateDescriptorCommandParameters("myContextId", "myAddress", "my-service-uuid", "my-characteristic-uuid", "my-descriptor-uuid", SimulateDescriptorType.Add));
        task.Wait(TimeSpan.FromSeconds(1));
        SimulateDescriptorCommandResult result = task.Result;
        
        Assert.That(result, Is.Not.Null);
    }

    [Test]
    public async Task TestSimulateDescriptorResponse()
    {
        TestWebSocketConnection connection = new();
        connection.DataSendComplete += async (sender, e) =>
        {
            string responseJson = $$"""
                                  {
                                    "type": "success",
                                    "id": {{e.SentCommandId}},
                                    "result": {}
                                  }
                                  """;
            await connection.RaiseDataReceivedEventAsync(responseJson);
        };

        BiDiDriver driver = new(TimeSpan.FromMilliseconds(500), new(connection));
        await driver.StartAsync("ws:localhost");
        BluetoothModule module = new(driver);

        Task<SimulateDescriptorResponseCommandResult> task = module.SimulateDescriptorResponseAsync(new SimulateDescriptorResponseCommandParameters("myContextId", "myAddress", "my-service-uuid", "my-characteristic-uuid", "my-descriptor-uuid", SimulateDescriptorResponseType.Read, 0));
        task.Wait(TimeSpan.FromSeconds(1));
        SimulateDescriptorResponseCommandResult result = task.Result;
        
        Assert.That(result, Is.Not.Null);
    }

    [Test]
    public async Task TestSimulateGattConnectionResponse()
    {
        TestWebSocketConnection connection = new();
        connection.DataSendComplete += async (sender, e) =>
        {
            string responseJson = $$"""
                                  {
                                    "type": "success",
                                    "id": {{e.SentCommandId}},
                                    "result": {}
                                  }
                                  """;
            await connection.RaiseDataReceivedEventAsync(responseJson);
        };

        BiDiDriver driver = new(TimeSpan.FromMilliseconds(500), new(connection));
        await driver.StartAsync("ws:localhost");
        BluetoothModule module = new(driver);

        Task<SimulateGattConnectionResponseCommandResult> task = module.SimulateGattConnectionResponseAsync(new SimulateGattConnectionResponseCommandParameters("myContextId", "myAddress", 0));
        task.Wait(TimeSpan.FromSeconds(1));
        SimulateGattConnectionResponseCommandResult result = task.Result;
        
        Assert.That(result, Is.Not.Null);
    }

    [Test]
    public async Task TestSimulateGattDisconnection()
    {
        TestWebSocketConnection connection = new();
        connection.DataSendComplete += async (sender, e) =>
        {
            string responseJson = $$"""
                                  {
                                    "type": "success",
                                    "id": {{e.SentCommandId}},
                                    "result": {}
                                  }
                                  """;
            await connection.RaiseDataReceivedEventAsync(responseJson);
        };

        BiDiDriver driver = new(TimeSpan.FromMilliseconds(500), new(connection));
        await driver.StartAsync("ws:localhost");
        BluetoothModule module = new(driver);

        Task<SimulateGattDisconnectionCommandResult> task = module.SimulateGattDisconnectionAsync(new SimulateGattDisconnectionCommandParameters("myContextId", "myAddress"));
        task.Wait(TimeSpan.FromSeconds(1));
        SimulateGattDisconnectionCommandResult result = task.Result;
        
        Assert.That(result, Is.Not.Null);
    }

    [Test]
    public async Task TestSimulatePreconnectedPeripheralCommand()
    {
        TestWebSocketConnection connection = new();
        connection.DataSendComplete += async (sender, e) =>
        {
            string responseJson = $$"""
                                  {
                                    "type": "success",
                                    "id": {{e.SentCommandId}},
                                    "result": {}
                                  }
                                  """;
            await connection.RaiseDataReceivedEventAsync(responseJson);
        };

        BiDiDriver driver = new(TimeSpan.FromMilliseconds(500), new(connection));
        await driver.StartAsync("ws:localhost");
        BluetoothModule module = new(driver);

        Task<SimulatePreconnectedPeripheralCommandResult> task = module.SimulatePreconnectedPeripheralAsync(new SimulatePreconnectedPeripheralCommandParameters("myContextId", "08:08:08:08:08", "myDeviceName"));
        task.Wait(TimeSpan.FromSeconds(1));
        SimulatePreconnectedPeripheralCommandResult result = task.Result;
        
        Assert.That(result, Is.Not.Null);
    }

    [Test]
    public async Task TestSimulateService()
    {
        TestWebSocketConnection connection = new();
        connection.DataSendComplete += async (sender, e) =>
        {
            string responseJson = $$"""
                                  {
                                    "type": "success",
                                    "id": {{e.SentCommandId}},
                                    "result": {}
                                  }
                                  """;
            await connection.RaiseDataReceivedEventAsync(responseJson);
        };

        BiDiDriver driver = new(TimeSpan.FromMilliseconds(500), new(connection));
        await driver.StartAsync("ws:localhost");
        BluetoothModule module = new(driver);

        Task<SimulateServiceCommandResult> task = module.SimulateServiceAsync(new SimulateServiceCommandParameters("myContextId", "myAddress", "my-service-uuid", SimulateServiceType.Add));
        task.Wait(TimeSpan.FromSeconds(1));
        SimulateServiceCommandResult result = task.Result;
        
        Assert.That(result, Is.Not.Null);
    }

    [Test]
    public async Task TestCanReceiveCharacteristicEventGeneratedEvent()
    {
        TestWebSocketConnection connection = new();
        BiDiDriver driver = new(TimeSpan.FromMilliseconds(500), new(connection));
        await driver.StartAsync("ws:localhost");
        BluetoothModule module = new(driver);

        ManualResetEvent syncEvent = new(false);
        module.OnCharacteristicGeneratedEvent.AddObserver((CharacteristicEventGeneratedEventArgs e) => {
            using (Assert.EnterMultipleScope())
            {
                Assert.That(e.BrowsingContextId, Is.EqualTo("myContext"));
                Assert.That(e.Address, Is.EqualTo("myAddress"));
                Assert.That(e.ServiceUuid, Is.EqualTo("my-service-uuid"));
                Assert.That(e.CharacteristicUuid, Is.EqualTo("my-characteristic-uuid"));
                Assert.That(e.Type, Is.EqualTo(CharacteristicEventGeneratedType.Read));
                Assert.That(e.Data, Is.Null);
           }
            syncEvent.Set();
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
        bool eventRaised = syncEvent.WaitOne(TimeSpan.FromMilliseconds(250));
        Assert.That(eventRaised, Is.True);
    }

    [Test]
    public async Task TestCanReceiveDescriptorEventGeneratedEvent()
    {
        TestWebSocketConnection connection = new();
        BiDiDriver driver = new(TimeSpan.FromMilliseconds(500), new(connection));
        await driver.StartAsync("ws:localhost");
        BluetoothModule module = new(driver);

        ManualResetEvent syncEvent = new(false);
        module.OnDescriptorGeneratedEvent.AddObserver((DescriptorEventGeneratedEventArgs e) => {
            using (Assert.EnterMultipleScope())
            {
                Assert.That(e.BrowsingContextId, Is.EqualTo("myContext"));
                Assert.That(e.Address, Is.EqualTo("myAddress"));
                Assert.That(e.ServiceUuid, Is.EqualTo("my-service-uuid"));
                Assert.That(e.CharacteristicUuid, Is.EqualTo("my-characteristic-uuid"));
                Assert.That(e.DescriptorUuid, Is.EqualTo("my-descriptor-uuid"));
                Assert.That(e.Type, Is.EqualTo(DescriptorEventGeneratedType.Read));
                Assert.That(e.Data, Is.Null);
           }
            syncEvent.Set();
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
        bool eventRaised = syncEvent.WaitOne(TimeSpan.FromMilliseconds(250));
        Assert.That(eventRaised, Is.True);
    }

    [Test]
    public async Task TestGattConnectionAttemptedEvent()
    {
        TestWebSocketConnection connection = new();
        BiDiDriver driver = new(TimeSpan.FromMilliseconds(500), new(connection));
        await driver.StartAsync("ws:localhost");
        BluetoothModule module = new(driver);

        ManualResetEvent syncEvent = new(false);
        module.OnGattConnectionAttempted.AddObserver((GattConnectionAttemptedEventArgs e) => {
            using (Assert.EnterMultipleScope())
            {
                Assert.That(e.BrowsingContextId, Is.EqualTo("myContext"));
                Assert.That(e.Address, Is.EqualTo("myAddress"));
           }
            syncEvent.Set();
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
        bool eventRaised = syncEvent.WaitOne(TimeSpan.FromMilliseconds(250));
        Assert.That(eventRaised, Is.True);
    }

    [Test]
    public async Task TestCanReceiveRequestDevicePromptUpdatedEvent()
    {
        TestWebSocketConnection connection = new();
        BiDiDriver driver = new(TimeSpan.FromMilliseconds(500), new(connection));
        await driver.StartAsync("ws:localhost");
        BluetoothModule module = new(driver);

        ManualResetEvent syncEvent = new(false);
        module.OnRequestDevicePromptUpdated.AddObserver((RequestDevicePromptUpdatedEventArgs e) => {
            using (Assert.EnterMultipleScope())
            {
                Assert.That(e.BrowsingContextId, Is.EqualTo("myContext"));
                Assert.That(e.Prompt, Is.EqualTo("myPrompt"));
                Assert.That(e.Devices, Has.Count.EqualTo(1));
                Assert.That(e.Devices[0].DeviceId, Is.EqualTo("myDeviceId"));
                Assert.That(e.Devices[0].DeviceName, Is.EqualTo("myDeviceName"));
            }
            syncEvent.Set();
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
        bool eventRaised = syncEvent.WaitOne(TimeSpan.FromMilliseconds(250));
        Assert.That(eventRaised, Is.True);
    }
}
