namespace WebDriverBiDi.WebExtension;

using TestUtilities;

public class WebExtensionModuleTests
{
    [Fact]
    public async Task TestInstallActivateCommand()
    {
        TestWebSocketConnection connection = new();
        connection.OnDataSendComplete.AddObserver(async e =>
        {
            string responseJson = $$"""
                                  {
                                    "type": "success",
                                    "id": {{e.SentCommandId}},
                                    "result": {
                                      "extension": "myExtensionId"
                                    }
                                  }
                                  """;
            await connection.RaiseDataReceivedEventAsync(responseJson);
        });

        await using BiDiDriver driver = new(TimeSpan.FromMilliseconds(500), new(connection));
        await driver.StartAsync("ws:localhost", TestContext.Current.CancellationToken);
        WebExtensionModule module = driver.WebExtension;

        Task<InstallCommandResult> task = module.InstallAsync(new InstallCommandParameters(new ExtensionPath("myExtensionPath")), cancellationToken: TestContext.Current.CancellationToken);
        InstallCommandResult result = await task.WaitAsync(TimeSpan.FromSeconds(1), TestContext.Current.CancellationToken);

        Assert.NotNull(result);
        Assert.Equal("myExtensionId", result.ExtensionId);
    }

    [Fact]
    public async Task TestUninstallActivateCommand()
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
        WebExtensionModule module = driver.WebExtension;

        Task<UninstallCommandResult> task = module.UninstallAsync(new UninstallCommandParameters("myExtensionPath"), cancellationToken: TestContext.Current.CancellationToken);
        UninstallCommandResult result = await task.WaitAsync(TimeSpan.FromSeconds(1), TestContext.Current.CancellationToken);

        Assert.NotNull(result);
    }
}
