namespace WebDriverBiDi.Browser;

using TestUtilities;

[TestFixture]
public class BrowserModuleTests
{
    [Test]
    public void TestExecuteCloseCommand()
    {
        TestConnection connection = new();
        connection.DataSendComplete += (sender, e) =>
        {
            string responseJson = @"{ ""type"": ""success"", ""id"": " + e.SentCommandId + @", ""result"": { } }";
            connection.RaiseDataReceivedEvent(responseJson);
        };

        BiDiDriver driver = new(TimeSpan.FromMilliseconds(500), new(connection));
        BrowserModule module = new(driver);

        var task = module.CloseAsync(new CloseCommandParameters());
        task.Wait(TimeSpan.FromSeconds(1));
        var result = task.Result;

        Assert.That(result, Is.Not.Null);
    }

    [Test]
    public void TestExecuteCreateUserContextCommand()
    {
        TestConnection connection = new();
        connection.DataSendComplete += (sender, e) =>
        {
            string responseJson = @"{ ""type"": ""success"", ""id"": " + e.SentCommandId + @", ""result"": { ""userContext"": ""myUserContextId"" } }";
            connection.RaiseDataReceivedEvent(responseJson);
        };

        BiDiDriver driver = new(TimeSpan.FromMilliseconds(500), new(connection));
        BrowserModule module = new(driver);

        var task = module.CreateUserContextAsync(new CreateUserContextCommandParameters());
        task.Wait(TimeSpan.FromSeconds(1));
        var result = task.Result;

        Assert.That(result, Is.Not.Null);
        Assert.That(result.UserContextId, Is.EqualTo("myUserContextId"));
    }

    [Test]
    public void TestExecuteGetUserContextsCommand()
    {
        TestConnection connection = new();
        connection.DataSendComplete += (sender, e) =>
        {
            string responseJson = @"{ ""type"": ""success"", ""id"": " + e.SentCommandId + @", ""result"": { ""userContexts"": [ { ""userContext"": ""default"" }, { ""userContext"": ""myUserContextId"" } ] } }";
            connection.RaiseDataReceivedEvent(responseJson);
        };

        BiDiDriver driver = new(TimeSpan.FromMilliseconds(500), new(connection));
        BrowserModule module = new(driver);

        var task = module.GetUserContextsAsync(new GetUserContextsCommandParameters());
        task.Wait(TimeSpan.FromSeconds(1));
        var result = task.Result;

        Assert.That(result, Is.Not.Null);
        Assert.Multiple(() =>
        {
            Assert.That(result.UserContexts, Has.Count.EqualTo(2));
            Assert.That(result.UserContexts[0].UserContextId, Is.EqualTo("default"));
            Assert.That(result.UserContexts[1].UserContextId, Is.EqualTo("myUserContextId"));
        });

    }

    [Test]
    public void TestExecuteRemoveUserContextCommand()
    {
        TestConnection connection = new();
        connection.DataSendComplete += (sender, e) =>
        {
            string responseJson = @"{ ""type"": ""success"", ""id"": " + e.SentCommandId + @", ""result"": { } }";
            connection.RaiseDataReceivedEvent(responseJson);
        };

        BiDiDriver driver = new(TimeSpan.FromMilliseconds(500), new(connection));
        BrowserModule module = new(driver);

        var task = module.RemoveUserContextAsync(new RemoveUserContextCommandParameters("myUserContextId"));
        task.Wait(TimeSpan.FromSeconds(1));
        var result = task.Result;

        Assert.That(result, Is.Not.Null);
    }
}
