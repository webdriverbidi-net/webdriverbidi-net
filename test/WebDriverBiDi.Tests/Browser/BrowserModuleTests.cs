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

        Driver driver = new(TimeSpan.FromMilliseconds(500), new(connection));
        BrowserModule module = new(driver);

        var task = module.CloseAsync(new CloseCommandParameters());
        task.Wait(TimeSpan.FromSeconds(1));
        var result = task.Result;

        Assert.That(result, Is.Not.Null);
    }
}