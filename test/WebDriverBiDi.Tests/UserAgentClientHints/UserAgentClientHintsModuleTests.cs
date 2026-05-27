namespace WebDriverBiDi.UserAgentClientHints;

using TestUtilities;

public class UserAgentClientHintsModuleTests
{
    [Fact]
    public async Task TestSetClientHintsOverrideCommand()
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
        UserAgentClientHintsModule module = driver.UserAgentClientHints;

        SetClientHintsOverrideCommandResult result = await module.SetClientHintsOverrideAsync(new SetClientHintsOverrideCommandParameters(), cancellationToken: TestContext.Current.CancellationToken);

        Assert.NotNull(result);
    }
}
