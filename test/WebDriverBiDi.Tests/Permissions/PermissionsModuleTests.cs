namespace WebDriverBiDi.Permissions;

using TestUtilities;

[TestFixture]
public class PermissionsModuleTests
{
    [Test]
    public async Task TestExecuteActivateCommand()
    {
        TestConnection connection = new();
        connection.DataSendComplete += async (sender, e) =>
        {
            string responseJson = @"{ ""type"": ""success"", ""id"": " + e.SentCommandId + @", ""result"": {} }";
            await connection.RaiseDataReceivedEventAsync(responseJson);
        };

        BiDiDriver driver = new(TimeSpan.FromMilliseconds(500), new(connection));
        await driver.StartAsync("ws:localhost");
        PermissionsModule module = new(driver);

        var task = module.SetPermission(new SetPermissionCommandParameters("myPermission", PermissionState.Granted, "https://example.com"));
        task.Wait(TimeSpan.FromSeconds(1));
        var result = task.Result;

        Assert.That(result, Is.Not.Null);
    }
}