namespace WebDriverBidi.Session;

using TestUtilities;
using WebDriverBidi.Protocol;

[TestFixture]
public class SessionModuleTests
{
    [Test]
    public void TestExecuteStatusCommand()
    {
        TestConnection connection = new();
        connection.DataSendComplete += (sender, e) =>
        {
            string responseJson = @"{ ""id"": " + e.SentCommandId + @", ""result"": { ""ready"": true, ""message"": ""ready for connection"" } }";
            connection.RaiseDataReceivedEvent(responseJson);
        };

        Driver driver = new(new(TimeSpan.FromMilliseconds(500), connection));
        var task = driver.Session.Status(new StatusCommandParameters());
        task.Wait(TimeSpan.FromSeconds(1));
        var result = task.Result;

        Assert.That(result, Is.Not.Null);
        Assert.Multiple(() =>
        {
            Assert.That(result.IsReady, Is.True);
            Assert.That(result.Message, Is.EqualTo("ready for connection"));
        });
    }

    [Test]
    public void TestExecuteSubscribeCommand()
    {
        TestConnection connection = new();
        connection.DataSendComplete += (sender, e) =>
        {
            string responseJson = @"{ ""id"": " + e.SentCommandId + @", ""result"": {} }";
            connection.RaiseDataReceivedEvent(responseJson);
        };

        Driver driver = new(new(TimeSpan.FromMilliseconds(500), connection));
        SessionModule module = new(driver);

        var subscribeParameters = new SubscribeCommandParameters();
        subscribeParameters.Events.Add("log.entryAdded");
        var task = module.Subscribe(subscribeParameters);
        task.Wait(TimeSpan.FromSeconds(1));
        var result = task.Result;
        
        Assert.That(result, Is.Not.Null);
    }

    [Test]
    public void TestExecuteUnsubscribeCommand()
    {
        TestConnection connection = new();
        connection.DataSendComplete += (sender, e) =>
        {
            string responseJson = @"{ ""id"": " + e.SentCommandId + @", ""result"": {} }";
            connection.RaiseDataReceivedEvent(responseJson);
        };

        Driver driver = new(new(TimeSpan.FromMilliseconds(500), connection));
        SessionModule module = new(driver);

        var unsubscribeParameters = new UnsubscribeCommandParameters();
        unsubscribeParameters.Events.Add("log.entryAdded");
        var task = module.Unsubscribe(unsubscribeParameters);
        task.Wait(TimeSpan.FromSeconds(1));
        var result = task.Result;
        
        Assert.That(result, Is.Not.Null);
    }

    [Test]
    public void TestExecuteNewCommand()
    {
        TestConnection connection = new();
        connection.DataSendComplete += (sender, e) =>
        {
            string responseJson = @"{ ""id"": " + e.SentCommandId + @", ""result"": { ""sessionId"": ""mySession"", ""capabilities"": { ""browserName"": ""greatBrowser"", ""browserVersion"": ""101.5b"", ""platformName"": ""otherOS"", ""acceptInsecureCerts"": true, ""proxy"": {}, ""setWindowRect"": true, ""additionalCapName"": ""additionalCapValue"" } } }";
            connection.RaiseDataReceivedEvent(responseJson);
        };

        Driver driver = new(new(TimeSpan.FromMilliseconds(500), connection));
        SessionModule module = new(driver);

        var newCommandParameters = new NewCommandParameters();
        var task = module.NewSession(newCommandParameters);
        task.Wait(TimeSpan.FromSeconds(1));
        var result = task.Result;
        
        Assert.That(result, Is.Not.Null);
        Assert.Multiple(() =>
        {
            Assert.That(result.SessionId, Is.EqualTo("mySession"));
            Assert.That(result.Capabilities.BrowserName, Is.EqualTo("greatBrowser"));
            Assert.That(result.Capabilities.BrowserVersion, Is.EqualTo("101.5b"));
            Assert.That(result.Capabilities.PlatformName, Is.EqualTo("otherOS"));
            Assert.That(result.Capabilities.AcceptInsecureCertificates, Is.True);
            Assert.That(result.Capabilities.SetWindowRect, Is.True);
            Assert.That(result.Capabilities.Proxy, Is.Not.Null);
            Assert.That(result.Capabilities.AdditionalCapabilities, Has.Count.EqualTo(1));
            Assert.That(result.Capabilities.AdditionalCapabilities, Contains.Key("additionalCapName"));
            Assert.That(result.Capabilities.AdditionalCapabilities["additionalCapName"], Is.Not.Null);
            Assert.That(result.Capabilities.AdditionalCapabilities["additionalCapName"], Is.TypeOf<string>());
            Assert.That(result.Capabilities.AdditionalCapabilities["additionalCapName"]!.ToString(), Is.EqualTo("additionalCapValue"));
        });
    }
}