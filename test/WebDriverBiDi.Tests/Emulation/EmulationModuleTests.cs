namespace WebDriverBiDi.Emulation;

using TestUtilities;

[TestFixture]
public class EmulationModuleTests
{
    [Test]
    public async Task TestSetGeolocationOverrideCommandWithCoordinates()
    {
        TestConnection connection = new();
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
        EmulationModule module = new(driver);

        Task<SetGeolocationOverrideCommandResult> task = module.SetGeolocationOverrideAsync(new SetGeolocationOverrideCoordinatesCommandParameters());
        task.Wait(TimeSpan.FromSeconds(1));
        SetGeolocationOverrideCommandResult result = task.Result;

        Assert.That(result, Is.Not.Null);
    }

    [Test]
    public async Task TestSetLocaleOverrideCommandWithCoordinates()
    {
        TestConnection connection = new();
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
        EmulationModule module = new(driver);

        Task<SetLocaleOverrideCommandResult> task = module.SetLocaleOverrideAsync(new SetLocaleOverrideCommandParameters());
        task.Wait(TimeSpan.FromSeconds(1));
        SetLocaleOverrideCommandResult result = task.Result;

        Assert.That(result, Is.Not.Null);
    }

    [Test]
    public async Task TestSetForcedColorsModeThemeOverrideCommandWithCoordinates()
    {
        TestConnection connection = new();
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
        EmulationModule module = new(driver);

        Task<SetForcedColorsModeThemeOverrideCommandResult> task = module.SetForcedColorsModeThemeOverrideAsync(new SetForcedColorsModeThemeOverrideCommandParameters());
        task.Wait(TimeSpan.FromSeconds(1));
        SetForcedColorsModeThemeOverrideCommandResult result = task.Result;

        Assert.That(result, Is.Not.Null);
    }

    [Test]
    public async Task TestSetScreenOrientationOverrideCommand()
    {
        TestConnection connection = new();
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
        EmulationModule module = new(driver);

        Task<SetScreenOrientationOverrideCommandResult> task = module.SetScreenOrientationOverrideAsync(new SetScreenOrientationOverrideCommandParameters());
        task.Wait(TimeSpan.FromSeconds(1));
        SetScreenOrientationOverrideCommandResult result = task.Result;

        Assert.That(result, Is.Not.Null);
    }

    [Test]
    public async Task TestSetScreenSettingsOverrideCommand()
    {
        TestConnection connection = new();
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
        EmulationModule module = new(driver);

        Task<SetScreenSettingsOverrideCommandResult> task = module.SetScreenSettingsOverrideAsync(new SetScreenSettingsOverrideCommandParameters());
        task.Wait(TimeSpan.FromSeconds(1));
        SetScreenSettingsOverrideCommandResult result = task.Result;

        Assert.That(result, Is.Not.Null);
    }

    [Test]
    public async Task TestSetScriptingEnabledCommand()
    {
        TestConnection connection = new();
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
        EmulationModule module = new(driver);

        Task<SetScriptingEnabledCommandResult> task = module.SetScriptingEnabledAsync(new SetScriptingEnabledCommandParameters());
        task.Wait(TimeSpan.FromSeconds(1));
        SetScriptingEnabledCommandResult result = task.Result;

        Assert.That(result, Is.Not.Null);
    }

    [Test]
    public async Task TestSetTimeZoneOverrideCommandWithCoordinates()
    {
        TestConnection connection = new();
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
        EmulationModule module = new(driver);

        Task<SetTimeZoneOverrideCommandResult> task = module.SetTimeZoneOverrideAsync(new SetTimeZoneOverrideCommandParameters());
        task.Wait(TimeSpan.FromSeconds(1));
        SetTimeZoneOverrideCommandResult result = task.Result;

        Assert.That(result, Is.Not.Null);
    }

    [Test]
    public async Task TestSetScrollbarTypeOverrideCommand()
    {
        TestConnection connection = new();
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
        EmulationModule module = new(driver);

        Task<SetScrollbarTypeOverrideCommandResult> task = module.SetScrollbarTypeOverrideAsync(new SetScrollbarTypeOverrideCommandParameters());
        task.Wait(TimeSpan.FromSeconds(1));
        SetScrollbarTypeOverrideCommandResult result = task.Result;

        Assert.That(result, Is.Not.Null);
    }

    [Test]
    public async Task TestSetTouchOverrideCommandWithCoordinates()
    {
        TestConnection connection = new();
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
        EmulationModule module = new(driver);

        Task<SetTouchOverrideCommandResult> task = module.SetTouchOverrideAsync(new SetTouchOverrideCommandParameters());
        task.Wait(TimeSpan.FromSeconds(1));
        SetTouchOverrideCommandResult result = task.Result;

        Assert.That(result, Is.Not.Null);
    }

    [Test]
    public async Task TestSetNetworkConditionsCommandWithCoordinates()
    {
        TestConnection connection = new();
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
        EmulationModule module = new(driver);

        Task<SetNetworkConditionsCommandResult> task = module.SetNetworkConditions(new SetNetworkConditionsCommandParameters()
        {
            NetworkConditions = new NetworkConditionsOffline()
        });
        task.Wait(TimeSpan.FromSeconds(1));
        SetNetworkConditionsCommandResult result = task.Result;

        Assert.That(result, Is.Not.Null);
    }

    [Test]
    public async Task TestSetUserAgentOverrideCommand()
    {
        TestConnection connection = new();
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
        EmulationModule module = new(driver);

        Task<SetUserAgentOverrideCommandResult> task = module.SetUserAgentOverrideAsync(new SetUserAgentOverrideCommandParameters()
        {
            UserAgent = "WebDriverBiDi.NET/1.0 (no platform)"
        });
        task.Wait(TimeSpan.FromSeconds(1));
        SetUserAgentOverrideCommandResult result = task.Result;

        Assert.That(result, Is.Not.Null);
    }
}
