namespace WebDriverBiDi.Input;

using WebDriverBiDi.TestUtilities;

[TestFixture]
public class InputModuleTests
{
    [Test]
    public void TestExecutePerformActions()
    {
        TestConnection connection = new();
        connection.DataSendComplete += (sender, e) =>
        {
            string responseJson = @"{ ""type"": ""success"", ""id"": " + e.SentCommandId + @", ""result"": {} }";
            connection.RaiseDataReceivedEvent(responseJson);
        };

        Driver driver = new(TimeSpan.FromMilliseconds(500), new(connection));
        InputModule module = new(driver);

        var task = module.PerformActionsAsync(new PerformActionsCommandParameters("myContextId"));
        task.Wait(TimeSpan.FromSeconds(1));
        var result = task.Result;
        
        Assert.That(result, Is.Not.Null);
    }

    [Test]
    public void TestExecuteReleaseActions()
    {
        TestConnection connection = new();
        connection.DataSendComplete += (sender, e) =>
        {
            string responseJson = @"{ ""type"": ""success"", ""id"": " + e.SentCommandId + @", ""result"": {} }";
            connection.RaiseDataReceivedEvent(responseJson);
        };

        Driver driver = new(TimeSpan.FromMilliseconds(500), new(connection));
        InputModule module = new(driver);

        var task = module.ReleaseActionsAsync(new ReleaseActionsCommandParameters("myContextId"));
        task.Wait(TimeSpan.FromSeconds(1));
        var result = task.Result;
        
        Assert.That(result, Is.Not.Null);
    }
}