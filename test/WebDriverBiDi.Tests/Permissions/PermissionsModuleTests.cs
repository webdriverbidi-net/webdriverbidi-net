namespace WebDriverBiDi.Permissions;

using TestUtilities;

public class PermissionsModuleTests
{
    [Fact]
    public async Task TestExecuteSetPermissionCommand()
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
        PermissionsModule module = driver.Permissions;

        Task<SetPermissionCommandResult> task = module.SetPermissionAsync(new SetPermissionCommandParameters("myPermission", PermissionState.Granted, "https://example.com"), cancellationToken: TestContext.Current.CancellationToken);
        SetPermissionCommandResult result = await task.WaitAsync(TimeSpan.FromSeconds(1), TestContext.Current.CancellationToken);

        Assert.NotNull(result);
    }
}
