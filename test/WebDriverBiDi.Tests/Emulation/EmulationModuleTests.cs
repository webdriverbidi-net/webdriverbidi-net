namespace WebDriverBiDi.Emulation;

using TestUtilities;

[TestFixture]
public class EmulationModuleTests
{
    [Test]
    public async Task TestSetGeolocationOverrideCommand()
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

        Task<EmptyResult> task = module.SetGeolocationOverride(new SetGeolocationOverrideCommandParameters());
        task.Wait(TimeSpan.FromSeconds(1));
        EmptyResult result = task.Result;

        Assert.That(result, Is.Not.Null);
    }
}