namespace WebDriverBiDi.Emulation;

using TestUtilities;

public class EmulationModuleTests
{
    [Fact]
    public async Task TestSetGeolocationOverrideCommandWithCoordinates()
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

        await using BiDiDriver driver = new(TimeSpan.FromMilliseconds(500), new(connection));
        await driver.StartAsync("ws:localhost", TestContext.Current.CancellationToken);
        EmulationModule module = driver.Emulation;

        Task<SetGeolocationOverrideCommandResult> task = module.SetGeolocationOverrideAsync(new SetGeolocationOverrideCoordinatesCommandParameters(), cancellationToken: TestContext.Current.CancellationToken);
        SetGeolocationOverrideCommandResult result = await task.WaitAsync(TimeSpan.FromSeconds(1), TestContext.Current.CancellationToken);

        Assert.NotNull(result);
    }

    [Fact]
    public async Task TestSetLocaleOverrideCommandWithCoordinates()
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

        await using BiDiDriver driver = new(TimeSpan.FromMilliseconds(500), new(connection));
        await driver.StartAsync("ws:localhost", TestContext.Current.CancellationToken);
        EmulationModule module = driver.Emulation;

        Task<SetLocaleOverrideCommandResult> task = module.SetLocaleOverrideAsync(new SetLocaleOverrideCommandParameters(), cancellationToken: TestContext.Current.CancellationToken);
        SetLocaleOverrideCommandResult result = await task.WaitAsync(TimeSpan.FromSeconds(1), TestContext.Current.CancellationToken);

        Assert.NotNull(result);
    }

    [Fact]
    public async Task TestSetForcedColorsModeThemeOverrideCommandWithCoordinates()
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

        await using BiDiDriver driver = new(TimeSpan.FromMilliseconds(500), new(connection));
        await driver.StartAsync("ws:localhost", TestContext.Current.CancellationToken);
        EmulationModule module = driver.Emulation;

        Task<SetForcedColorsModeThemeOverrideCommandResult> task = module.SetForcedColorsModeThemeOverrideAsync(new SetForcedColorsModeThemeOverrideCommandParameters(), cancellationToken: TestContext.Current.CancellationToken);
        SetForcedColorsModeThemeOverrideCommandResult result = await task.WaitAsync(TimeSpan.FromSeconds(1), TestContext.Current.CancellationToken);

        Assert.NotNull(result);
    }

    [Fact]
    public async Task TestSetScreenOrientationOverrideCommand()
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

        await using BiDiDriver driver = new(TimeSpan.FromMilliseconds(500), new(connection));
        await driver.StartAsync("ws:localhost", TestContext.Current.CancellationToken);
        EmulationModule module = driver.Emulation;

        Task<SetScreenOrientationOverrideCommandResult> task = module.SetScreenOrientationOverrideAsync(new SetScreenOrientationOverrideCommandParameters(), cancellationToken: TestContext.Current.CancellationToken);
        SetScreenOrientationOverrideCommandResult result = await task.WaitAsync(TimeSpan.FromSeconds(1), TestContext.Current.CancellationToken);

        Assert.NotNull(result);
    }

    [Fact]
    public async Task TestSetScreenSettingsOverrideCommand()
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

        await using BiDiDriver driver = new(TimeSpan.FromMilliseconds(500), new(connection));
        await driver.StartAsync("ws:localhost", TestContext.Current.CancellationToken);
        EmulationModule module = driver.Emulation;

        Task<SetScreenSettingsOverrideCommandResult> task = module.SetScreenSettingsOverrideAsync(new SetScreenSettingsOverrideCommandParameters(), cancellationToken: TestContext.Current.CancellationToken);
        SetScreenSettingsOverrideCommandResult result = await task.WaitAsync(TimeSpan.FromSeconds(1), TestContext.Current.CancellationToken);

        Assert.NotNull(result);
    }

    [Fact]
    public async Task TestSetScriptingEnabledCommand()
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

        await using BiDiDriver driver = new(TimeSpan.FromMilliseconds(500), new(connection));
        await driver.StartAsync("ws:localhost", TestContext.Current.CancellationToken);
        EmulationModule module = driver.Emulation;

        Task<SetScriptingEnabledCommandResult> task = module.SetScriptingEnabledAsync(new SetScriptingEnabledCommandParameters(), cancellationToken: TestContext.Current.CancellationToken);
        SetScriptingEnabledCommandResult result = await task.WaitAsync(TimeSpan.FromSeconds(1), TestContext.Current.CancellationToken);

        Assert.NotNull(result);
    }

    [Fact]
    public async Task TestSetTimeZoneOverrideCommandWithCoordinates()
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

        await using BiDiDriver driver = new(TimeSpan.FromMilliseconds(500), new(connection));
        await driver.StartAsync("ws:localhost", TestContext.Current.CancellationToken);
        EmulationModule module = driver.Emulation;

        Task<SetTimeZoneOverrideCommandResult> task = module.SetTimeZoneOverrideAsync(new SetTimeZoneOverrideCommandParameters(), cancellationToken: TestContext.Current.CancellationToken);
        SetTimeZoneOverrideCommandResult result = await task.WaitAsync(TimeSpan.FromSeconds(1), TestContext.Current.CancellationToken);

        Assert.NotNull(result);
    }

    [Fact]
    public async Task TestSetScrollbarTypeOverrideCommand()
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

        await using BiDiDriver driver = new(TimeSpan.FromMilliseconds(500), new(connection));
        await driver.StartAsync("ws:localhost", TestContext.Current.CancellationToken);
        EmulationModule module = driver.Emulation;

        Task<SetScrollbarTypeOverrideCommandResult> task = module.SetScrollbarTypeOverrideAsync(new SetScrollbarTypeOverrideCommandParameters(), cancellationToken: TestContext.Current.CancellationToken);
        SetScrollbarTypeOverrideCommandResult result = await task.WaitAsync(TimeSpan.FromSeconds(1), TestContext.Current.CancellationToken);

        Assert.NotNull(result);
    }

    [Fact]
    public async Task TestSetTouchOverrideCommandWithCoordinates()
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

        await using BiDiDriver driver = new(TimeSpan.FromMilliseconds(500), new(connection));
        await driver.StartAsync("ws:localhost", TestContext.Current.CancellationToken);
        EmulationModule module = driver.Emulation;

        Task<SetTouchOverrideCommandResult> task = module.SetTouchOverrideAsync(new SetTouchOverrideCommandParameters(), cancellationToken: TestContext.Current.CancellationToken);
        SetTouchOverrideCommandResult result = await task.WaitAsync(TimeSpan.FromSeconds(1), TestContext.Current.CancellationToken);

        Assert.NotNull(result);
    }

    [Fact]
    public async Task TestSetNetworkConditionsCommandWithCoordinates()
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

        await using BiDiDriver driver = new(TimeSpan.FromMilliseconds(500), new(connection));
        await driver.StartAsync("ws:localhost", TestContext.Current.CancellationToken);
        EmulationModule module = driver.Emulation;

        Task<SetNetworkConditionsCommandResult> task = module.SetNetworkConditionsAsync(new SetNetworkConditionsCommandParameters()
        {
            NetworkConditions = new NetworkConditionsOffline()
        }, cancellationToken: TestContext.Current.CancellationToken);
        SetNetworkConditionsCommandResult result = await task.WaitAsync(TimeSpan.FromSeconds(1), TestContext.Current.CancellationToken);

        Assert.NotNull(result);
    }

    [Fact]
    public async Task TestSetUserAgentOverrideCommand()
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

        await using BiDiDriver driver = new(TimeSpan.FromMilliseconds(500), new(connection));
        await driver.StartAsync("ws:localhost", TestContext.Current.CancellationToken);
        EmulationModule module = driver.Emulation;

        Task<SetUserAgentOverrideCommandResult> task = module.SetUserAgentOverrideAsync(new SetUserAgentOverrideCommandParameters()
        {
            UserAgent = "WebDriverBiDi.NET/1.0 (no platform)"
        }, cancellationToken: TestContext.Current.CancellationToken);
        SetUserAgentOverrideCommandResult result = await task.WaitAsync(TimeSpan.FromSeconds(1), TestContext.Current.CancellationToken);

        Assert.NotNull(result);
    }
}
