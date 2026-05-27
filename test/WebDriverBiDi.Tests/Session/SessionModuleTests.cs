namespace WebDriverBiDi.Session;

using TestUtilities;

public class SessionModuleTests
{
    [Fact]
    public async Task TestExecuteStatusCommand()
    {
        TestWebSocketConnection connection = new();
        connection.OnDataSendComplete.AddObserver(async e =>
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
        });

        await using BiDiDriver driver = new(TimeSpan.FromSeconds(5), new(connection));
        SessionModule module = driver.Session;
        await driver.StartAsync("ws:localhost", TestContext.Current.CancellationToken);

        StatusCommandResult result = await module.StatusAsync(new StatusCommandParameters(), cancellationToken: TestContext.Current.CancellationToken);

        Assert.NotNull(result);

        Assert.True(result.IsReady);
        Assert.Equal("ready for connection", result.Message);
    }

    [Fact]
    public async Task TestExecuteStatusCommandWithNoArgument()
    {
        TestWebSocketConnection connection = new();
        connection.OnDataSendComplete.AddObserver(async e =>
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
        });

        await using BiDiDriver driver = new(TimeSpan.FromSeconds(5), new(connection));
        SessionModule module = driver.Session;
        await driver.StartAsync("ws:localhost", TestContext.Current.CancellationToken);

        StatusCommandResult result = await module.StatusAsync(cancellationToken: TestContext.Current.CancellationToken);

        Assert.NotNull(result);

        Assert.True(result.IsReady);
        Assert.Equal("ready for connection", result.Message);
    }

    [Fact]
    public async Task TestExecuteSubscribeCommand()
    {
        TestWebSocketConnection connection = new();
        connection.OnDataSendComplete.AddObserver(async e =>
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
        });

        await using BiDiDriver driver = new(TimeSpan.FromSeconds(5), new(connection));
        SessionModule module = driver.Session;
        await driver.StartAsync("ws:localhost", TestContext.Current.CancellationToken);

        SubscribeCommandParameters subscribeParameters = new(["log.entryAdded"]);
        SubscribeCommandResult result = await module.SubscribeAsync(subscribeParameters, cancellationToken: TestContext.Current.CancellationToken);

        Assert.NotNull(result);
        Assert.Equal("mySubscriptionId", result.SubscriptionId);
    }

    [Fact]
    public async Task TestExecuteUnsubscribeByAttributesCommand()
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

        await using BiDiDriver driver = new(TimeSpan.FromSeconds(5), new(connection));
        SessionModule module = driver.Session;
        await driver.StartAsync("ws:localhost", TestContext.Current.CancellationToken);

        UnsubscribeByAttributesCommandParameters unsubscribeParameters = new();
        unsubscribeParameters.Events.Add("log.entryAdded");
        UnsubscribeCommandResult result = await module.UnsubscribeAsync(unsubscribeParameters, cancellationToken: TestContext.Current.CancellationToken);

        Assert.NotNull(result);
    }

    [Fact]
    public async Task TestExecuteUnsubscribeBySubscriptionIdsCommand()
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

        await using BiDiDriver driver = new(TimeSpan.FromSeconds(5), new(connection));
        SessionModule module = driver.Session;
        await driver.StartAsync("ws:localhost", TestContext.Current.CancellationToken);

        UnsubscribeByIdsCommandParameters unsubscribeParameters = new();
        unsubscribeParameters.SubscriptionIds.Add("mySubscriptionId");
        UnsubscribeCommandResult result = await module.UnsubscribeAsync(unsubscribeParameters, cancellationToken: TestContext.Current.CancellationToken);

        Assert.NotNull(result);
    }

    [Fact]
    public async Task TestExecuteNewCommand()
    {
        TestWebSocketConnection connection = new();
        connection.OnDataSendComplete.AddObserver(async e =>
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
        });

        await using BiDiDriver driver = new(TimeSpan.FromSeconds(5), new(connection));
        SessionModule module = driver.Session;
        await driver.StartAsync("ws:localhost", TestContext.Current.CancellationToken);

        NewCommandParameters newCommandParameters = new();
        NewCommandResult result = await module.NewSessionAsync(newCommandParameters, cancellationToken: TestContext.Current.CancellationToken);

        Assert.NotNull(result);

        Assert.Equal("mySession", result.SessionId);
        Assert.Equal("greatBrowser", result.Capabilities.BrowserName);
        Assert.Equal("101.5b", result.Capabilities.BrowserVersion);
        Assert.Equal("otherOS", result.Capabilities.PlatformName);
        Assert.Equal("WebDriverBidi.NET/1.0", result.Capabilities.UserAgent);
        Assert.True(result.Capabilities.AcceptInsecureCertificates);
        Assert.True(result.Capabilities.SetWindowRect);
        Assert.NotNull(result.Capabilities.Proxy);
        Assert.Single(result.Capabilities.AdditionalCapabilities);
        Assert.True(result.Capabilities.AdditionalCapabilities.ContainsKey("additionalCapName"));
        object? additionalCapValue = result.Capabilities.AdditionalCapabilities["additionalCapName"];
        Assert.NotNull(additionalCapValue);
        Assert.IsType<string>(additionalCapValue);
        Assert.Equal("additionalCapValue", additionalCapValue.ToString());
    }

    [Fact]
    public async Task TestExecuteEndCommand()
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

        await using BiDiDriver driver = new(TimeSpan.FromSeconds(5), new(connection));
        SessionModule module = driver.Session;
        await driver.StartAsync("ws:localhost", TestContext.Current.CancellationToken);

        EndCommandParameters endParameters = new();
        EndCommandResult result = await module.EndAsync(endParameters, cancellationToken: TestContext.Current.CancellationToken);
        Assert.NotNull(result);
    }

    [Fact]
    public async Task TestExecuteEndCommandWithNoArgument()
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

        await using BiDiDriver driver = new(TimeSpan.FromSeconds(5), new(connection));
        SessionModule module = driver.Session;
        await driver.StartAsync("ws:localhost", TestContext.Current.CancellationToken);

        EndCommandResult result = await module.EndAsync(cancellationToken: TestContext.Current.CancellationToken);

        Assert.NotNull(result);
    }
}
