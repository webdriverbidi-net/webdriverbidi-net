namespace WebDriverBiDi.UserAgentClientHints;

using TestUtilities;

[TestFixture]
public class UserAgentClientHintsModuleTests
{
    [Test]
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
        await driver.StartAsync("ws:localhost");
        UserAgentClientHintsModule module = driver.UserAgentClientHints;

        Task<SetClientHintsOverrideCommandResult> task = module.SetClientHintsOverrideAsync(new SetClientHintsOverrideCommandParameters());
        task.Wait(TimeSpan.FromSeconds(1));
        SetClientHintsOverrideCommandResult result = task.Result;

        Assert.That(result, Is.Not.Null);
    }
}
