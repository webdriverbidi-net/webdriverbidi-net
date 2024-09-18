namespace WebDriverBiDi.Input;

using WebDriverBiDi.Script;
using WebDriverBiDi.TestUtilities;

[TestFixture]
public class InputModuleTests
{
    [Test]
    public async Task TestExecutePerformActions()
    {
        TestConnection connection = new();
        connection.DataSendComplete += async (sender, e) =>
        {
            string responseJson = @"{ ""type"": ""success"", ""id"": " + e.SentCommandId + @", ""result"": {} }";
            await connection.RaiseDataReceivedEventAsync(responseJson);
        };

        BiDiDriver driver = new(TimeSpan.FromMilliseconds(500), new(connection));
        await driver.StartAsync("ws:localhost");
        InputModule module = new(driver);

        Task<EmptyResult> task = module.PerformActionsAsync(new PerformActionsCommandParameters("myContextId"));
        task.Wait(TimeSpan.FromSeconds(1));
        EmptyResult result = task.Result;
        
        Assert.That(result, Is.Not.Null);
    }

    [Test]
    public async Task TestExecuteReleaseActions()
    {
        TestConnection connection = new();
        connection.DataSendComplete += async (sender, e) =>
        {
            string responseJson = @"{ ""type"": ""success"", ""id"": " + e.SentCommandId + @", ""result"": {} }";
            await connection.RaiseDataReceivedEventAsync(responseJson);
        };

        BiDiDriver driver = new(TimeSpan.FromMilliseconds(500), new(connection));
        await driver.StartAsync("ws:localhost");
        InputModule module = new(driver);

        Task<EmptyResult> task = module.ReleaseActionsAsync(new ReleaseActionsCommandParameters("myContextId"));
        task.Wait(TimeSpan.FromSeconds(1));
        EmptyResult result = task.Result;
        
        Assert.That(result, Is.Not.Null);
    }

    [Test]
    public async Task TestExecuteSetFiles()
    {
        TestConnection connection = new();
        connection.DataSendComplete += async (sender, e) =>
        {
            string responseJson = @"{ ""type"": ""success"", ""id"": " + e.SentCommandId + @", ""result"": {} }";
            await connection.RaiseDataReceivedEventAsync(responseJson);
        };

        BiDiDriver driver = new(TimeSpan.FromMilliseconds(500), new(connection));
        await driver.StartAsync("ws:localhost");
        InputModule module = new(driver);

        SharedReference element = new("mySharedId");
        Task<EmptyResult> task = module.SetFilesAsync(new SetFilesCommandParameters("myContextId", element));
        task.Wait(TimeSpan.FromSeconds(1));
        EmptyResult result = task.Result;
        
        Assert.That(result, Is.Not.Null);
    }
}
