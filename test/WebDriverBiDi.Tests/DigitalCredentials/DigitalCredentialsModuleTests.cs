namespace WebDriverBiDi.DigitalCredentials;

using TestUtilities;

public class DigitalCredentialsModuleTests
{
    [Fact]
    public async Task TestExecuteSetVirtualWalletBehavior()
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
        await driver.StartAsync("ws:localhost", cancellationToken: TestContext.Current.CancellationToken);
        DigitalCredentialsModule module = driver.DigitalCredentials;

        Task<SetVirtualWalletBehaviorCommandResult> task = module.SetVirtualWalletBehaviorAsync(new SetVirtualWalletBehaviorCommandParameters(VirtualWalletAction.Clear), cancellationToken: TestContext.Current.CancellationToken);
        SetVirtualWalletBehaviorCommandResult result = await task.WaitAsync(TimeSpan.FromSeconds(1), TestContext.Current.CancellationToken);

        Assert.NotNull(result);
    }
}
