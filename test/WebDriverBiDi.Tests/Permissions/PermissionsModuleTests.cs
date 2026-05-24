namespace WebDriverBiDi.Permissions;

using TestUtilities;

public class PermissionsModuleTests
{
    [Fact]
    public async Task TestExecuteSetPermissionCommand()
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
        PermissionsModule module = driver.Permissions;

        Task<SetPermissionCommandResult> task = module.SetPermissionAsync(new SetPermissionCommandParameters("myPermission", PermissionState.Granted, "https://example.com"), cancellationToken: TestContext.Current.CancellationToken);
        SetPermissionCommandResult result = await task.WaitAsync(TimeSpan.FromSeconds(1), TestContext.Current.CancellationToken);

        Assert.NotNull(result);
    }
}
