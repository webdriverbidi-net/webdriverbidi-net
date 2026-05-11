namespace WebDriverBiDi.DigitalCredentials;

using TestUtilities;

[TestFixture]
public class DigitalCredentialsModuleTests
{
    [Test]
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
        await driver.StartAsync("ws:localhost");
        DigitalCredentialsModule module = driver.DigitalCredentials;

        Task<SetVirtualWalletBehaviorCommandResult> task = module.SetVirtualWalletBehaviorAsync(new SetVirtualWalletBehaviorCommandParameters(VirtualWalletAction.Clear));
        task.Wait(TimeSpan.FromSeconds(1));
        SetVirtualWalletBehaviorCommandResult result = task.Result;

        Assert.That(result, Is.Not.Null);
    }
}
