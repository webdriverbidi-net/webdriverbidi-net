namespace WebDriverBiDi.UserAgentClientHints;

using TestUtilities;

public class UserAgentClientHintsModuleTests
{
    [Fact]
    public async Task TestSetClientHintsOverrideCommand()
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
        await driver.StartAsync("ws:localhost", TestContext.Current.CancellationToken);
        UserAgentClientHintsModule module = driver.UserAgentClientHints;

        Task<SetClientHintsOverrideCommandResult> task = module.SetClientHintsOverrideAsync(new SetClientHintsOverrideCommandParameters(), cancellationToken: TestContext.Current.CancellationToken);
        SetClientHintsOverrideCommandResult result = await task.WaitAsync(TimeSpan.FromSeconds(1), TestContext.Current.CancellationToken);

        Assert.NotNull(result);
    }
}
