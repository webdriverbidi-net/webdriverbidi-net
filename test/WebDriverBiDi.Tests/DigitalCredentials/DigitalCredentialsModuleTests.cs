namespace WebDriverBiDi.DigitalCredentials;

using TestUtilities;

public class DigitalCredentialsModuleTests
{
    [Fact]
    public async Task TestExecuteSetVirtualWalletBehavior()
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
        await driver.StartAsync("ws:localhost", TestContext.Current.CancellationToken);
        DigitalCredentialsModule module = driver.DigitalCredentials;

        SetVirtualWalletBehaviorCommandResult result = await module.SetVirtualWalletBehaviorAsync(new SetVirtualWalletBehaviorCommandParameters(VirtualWalletAction.Clear), cancellationToken: TestContext.Current.CancellationToken);

        Assert.NotNull(result);
    }
}
