namespace WebDriverBiDi.Session;

using TestUtilities;

[TestFixture]
public class SessionModuleTests
{
    [Test]
    public async Task TestExecuteStatusCommand()
    {
        TestWebSocketConnection connection = new();
        connection.DataSendComplete += async (sender, e) =>
        {
            string responseJson = $$"""
                                  {
                                    "type": "success",
                                    "id": {{e.SentCommandId}},
                                    "result": {
                                      "ready": true,
                                      "message": "ready for connection"
                                    }
                                  }
                                  """;
            await connection.RaiseDataReceivedEventAsync(responseJson);
        };

        BiDiDriver driver = new(TimeSpan.FromMilliseconds(500), new(connection));
        SessionModule module = new(driver);
        await driver.StartAsync("ws:localhost");

        Task<StatusCommandResult> task = module.StatusAsync(new StatusCommandParameters());
        task.Wait(TimeSpan.FromSeconds(1));
        StatusCommandResult result = task.Result;

        Assert.That(result, Is.Not.Null);
        using (Assert.EnterMultipleScope())
        {
            Assert.That(result.IsReady, Is.True);
            Assert.That(result.Message, Is.EqualTo("ready for connection"));
        }
    }

    [Test]
    public async Task TestExecuteStatusCommandWithNoArgument()
    {
        TestWebSocketConnection connection = new();
        connection.DataSendComplete += async (sender, e) =>
        {
            string responseJson = $$"""
                                  {
                                    "type": "success",
                                    "id": {{e.SentCommandId}},
                                    "result": {
                                      "ready": true,
                                      "message": "ready for connection"
                                    }
                                  }
                                  """;
            await connection.RaiseDataReceivedEventAsync(responseJson);
        };

        BiDiDriver driver = new(TimeSpan.FromMilliseconds(500), new(connection));
        SessionModule module = new(driver);
        await driver.StartAsync("ws:localhost");

        Task<StatusCommandResult> task = module.StatusAsync();
        task.Wait(TimeSpan.FromSeconds(1));
        StatusCommandResult result = task.Result;

        Assert.That(result, Is.Not.Null);
        using (Assert.EnterMultipleScope())
        {
            Assert.That(result.IsReady, Is.True);
            Assert.That(result.Message, Is.EqualTo("ready for connection"));
        }
    }

    [Test]
    public async Task TestExecuteSubscribeCommand()
    {
        TestWebSocketConnection connection = new();
        connection.DataSendComplete += async (sender, e) =>
        {
            string responseJson = $$"""
                                  {
                                    "type": "success",
                                    "id": {{e.SentCommandId}},
                                    "result": {
                                      "subscription": "mySubscriptionId"
                                    }
                                  }
                                  """;
            await connection.RaiseDataReceivedEventAsync(responseJson);
        };

        BiDiDriver driver = new(TimeSpan.FromMilliseconds(500), new(connection));
        SessionModule module = new(driver);
        await driver.StartAsync("ws:localhost");

        SubscribeCommandParameters subscribeParameters = new();
        subscribeParameters.Events.Add("log.entryAdded");
        Task<SubscribeCommandResult> task = module.SubscribeAsync(subscribeParameters);
        task.Wait(TimeSpan.FromSeconds(1));
        SubscribeCommandResult result = task.Result;
        
        Assert.That(result, Is.Not.Null);
        Assert.That(result.SubscriptionId, Is.EqualTo("mySubscriptionId"));
    }

    [Test]
    public async Task TestExecuteUnsubscribeByAttributesCommand()
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
        SessionModule module = new(driver);
        await driver.StartAsync("ws:localhost");

        UnsubscribeByAttributesCommandParameters unsubscribeParameters = new();
        unsubscribeParameters.Events.Add("log.entryAdded");
        Task<UnsubscribeCommandResult> task = module.UnsubscribeAsync(unsubscribeParameters);
        task.Wait(TimeSpan.FromSeconds(1));
        UnsubscribeCommandResult result = task.Result;
        
        Assert.That(result, Is.Not.Null);
    }

    [Test]
    public async Task TestExecuteUnsubscribeBySubscriptionIdsCommand()
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
        SessionModule module = new(driver);
        await driver.StartAsync("ws:localhost");

        UnsubscribeByIdsCommandParameters unsubscribeParameters = new();
        unsubscribeParameters.SubscriptionIds.Add("mySubscriptionId");
        Task<UnsubscribeCommandResult> task = module.UnsubscribeAsync(unsubscribeParameters);
        task.Wait(TimeSpan.FromSeconds(1));
        UnsubscribeCommandResult result = task.Result;
        
        Assert.That(result, Is.Not.Null);
    }

    [Test]
    public async Task TestExecuteNewCommand()
    {
        TestWebSocketConnection connection = new();
        connection.DataSendComplete += async (sender, e) =>
        {
            string responseJson = $$"""
                                  {
                                    "type": "success",
                                    "id": {{e.SentCommandId}},
                                    "result": {
                                      "sessionId": "mySession",
                                      "capabilities": {
                                        "browserName": "greatBrowser",
                                        "browserVersion": "101.5b",
                                        "platformName": "otherOS",
                                        "userAgent": "WebDriverBidi.NET/1.0",
                                        "acceptInsecureCerts": true,
                                        "proxy": {
                                          "proxyType": "system" 
                                        },
                                        "setWindowRect": true,
                                        "additionalCapName": "additionalCapValue"
                                      }
                                    }
                                  }
                                  """;
            await connection.RaiseDataReceivedEventAsync(responseJson);
        };

        BiDiDriver driver = new(TimeSpan.FromMilliseconds(500), new(connection));
        SessionModule module = new(driver);
        await driver.StartAsync("ws:localhost");

        NewCommandParameters newCommandParameters = new();
        Task<NewCommandResult> task = module.NewSessionAsync(newCommandParameters);
        task.Wait(TimeSpan.FromSeconds(1));
        NewCommandResult result = task.Result;
        
        Assert.That(result, Is.Not.Null);
        using (Assert.EnterMultipleScope())
        {
            Assert.That(result.SessionId, Is.EqualTo("mySession"));
            Assert.That(result.Capabilities.BrowserName, Is.EqualTo("greatBrowser"));
            Assert.That(result.Capabilities.BrowserVersion, Is.EqualTo("101.5b"));
            Assert.That(result.Capabilities.PlatformName, Is.EqualTo("otherOS"));
            Assert.That(result.Capabilities.UserAgent, Is.EqualTo("WebDriverBidi.NET/1.0"));
            Assert.That(result.Capabilities.AcceptInsecureCertificates, Is.True);
            Assert.That(result.Capabilities.SetWindowRect, Is.True);
            Assert.That(result.Capabilities.Proxy, Is.Not.Null);
            Assert.That(result.Capabilities.AdditionalCapabilities, Has.Count.EqualTo(1));
            Assert.That(result.Capabilities.AdditionalCapabilities, Contains.Key("additionalCapName"));
            Assert.That(result.Capabilities.AdditionalCapabilities["additionalCapName"], Is.Not.Null);
            Assert.That(result.Capabilities.AdditionalCapabilities["additionalCapName"], Is.TypeOf<string>());
            Assert.That(result.Capabilities.AdditionalCapabilities["additionalCapName"]!.ToString(), Is.EqualTo("additionalCapValue"));
        }
    }

    [Test]
    public async Task TestExecuteEndCommand()
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
        SessionModule module = new(driver);
        await driver.StartAsync("ws:localhost");

        EndCommandParameters endParameters = new();
        Task<EndCommandResult> task = module.EndAsync(endParameters);
        task.Wait(TimeSpan.FromSeconds(1));
        EndCommandResult result = task.Result;
        Assert.That(result, Is.Not.Null);
    }

    [Test]
    public async Task TestExecuteEndCommandWithNoArgument()
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
        SessionModule module = new(driver);
        await driver.StartAsync("ws:localhost");

        Task<EndCommandResult> task = module.EndAsync();
        task.Wait(TimeSpan.FromSeconds(1));
        EndCommandResult result = task.Result;

        Assert.That(result, Is.Not.Null);
    }
}
