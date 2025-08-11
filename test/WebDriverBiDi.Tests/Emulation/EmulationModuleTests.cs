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

        Task<EmptyResult> task = module.SetGeolocationOverrideAsync(new SetGeolocationOverrideCoordinatesCommandParameters());
        task.Wait(TimeSpan.FromSeconds(1));
        EmptyResult result = task.Result;

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

        Task<EmptyResult> task = module.SetLocaleOverrideAsync(new SetLocaleOverrideCommandParameters());
        task.Wait(TimeSpan.FromSeconds(1));
        EmptyResult result = task.Result;

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

        Task<EmptyResult> task = module.SetForcedColorsModeThemeOverrideAsync(new SetForcedColorsModeThemeOverrideCommandParameters());
        task.Wait(TimeSpan.FromSeconds(1));
        EmptyResult result = task.Result;

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

        Task<EmptyResult> task = module.SetScreenOrientationOverrideAsync(new SetScreenOrientationOverrideCommandParameters());
        task.Wait(TimeSpan.FromSeconds(1));
        EmptyResult result = task.Result;

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

        Task<EmptyResult> task = module.SetScriptingEnabledAsync(new SetScriptingEnabledCommandParameters());
        task.Wait(TimeSpan.FromSeconds(1));
        EmptyResult result = task.Result;

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

        Task<EmptyResult> task = module.SetTimeZoneOverrideAsync(new SetTimeZoneOverrideCommandParameters());
        task.Wait(TimeSpan.FromSeconds(1));
        EmptyResult result = task.Result;

        Assert.That(result, Is.Not.Null);
    }
}