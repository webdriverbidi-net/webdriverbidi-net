namespace WebDriverBiDi.Permissions;

using TestUtilities;

[TestFixture]
public class PermissionsModuleTests
{
    [Test]
    public void TestExecuteActivateCommand()
    {
        TestConnection connection = new();
        connection.DataSendComplete += (sender, e) =>
        {
            string responseJson = @"{ ""type"": ""success"", ""id"": " + e.SentCommandId + @", ""result"": {} }";
            connection.RaiseDataReceivedEvent(responseJson);
        };

        BiDiDriver driver = new(TimeSpan.FromMilliseconds(500), new(connection));
        PermissionsModule module = new(driver);

        var task = module.SetPermission(new SetPermissionCommandParameters("myPermission", PermissionState.Granted, "https://example.com"));
        task.Wait(TimeSpan.FromSeconds(1));
        var result = task.Result;

        Assert.That(result, Is.Not.Null);
    }
}