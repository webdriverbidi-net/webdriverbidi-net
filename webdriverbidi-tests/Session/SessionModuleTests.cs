namespace WebDriverBidi.Session;

using TestUtilities;

[TestFixture]
public class SessionModuleTests
{
    [Test]
    public void TestExecuteStatusCommand()
    {
        string responseJson = @"{ ""result"": { ""ready"": true, ""message"": ""ready for connection"" } }";
        TestDriver driver = new TestDriver();
        SessionModule module = new SessionModule(driver);

        var task = module.Status(new StatusCommandSettings());
        driver.WaitForCommandSet(TimeSpan.FromSeconds(1));

        driver.EmitResponse(responseJson);
        task.Wait(TimeSpan.FromSeconds(1));
        var result = task.Result;

        Assert.That(result, Is.Not.Null);
        Assert.That(result.IsReady, Is.True);
        Assert.That(result.Message, Is.EqualTo("ready for connection"));
    }

    [Test]
    public void TestExecuteSubscribeCommand()
    {
        string responseJson = @"{ ""result"": {} }";
        TestDriver driver = new TestDriver();
        SessionModule module = new SessionModule(driver);

        var subscribeParameters = new SubscribeCommandSettings();
        subscribeParameters.Events.Add("log.entryAdded");
        var task = module.Subscribe(subscribeParameters);
        driver.WaitForCommandSet(TimeSpan.FromSeconds(1));

        driver.EmitResponse(responseJson);
        task.Wait(TimeSpan.FromSeconds(1));
        var result = task.Result;
        
        Assert.That(result, Is.Not.Null);
    }

    [Test]
    public void TestExecuteUnsubscribeCommand()
    {
        string responseJson = @"{ ""result"": {} }";
        TestDriver driver = new TestDriver();
        SessionModule module = new SessionModule(driver);

        var unsubscribeParameters = new UnsubscribeCommandSettings();
        unsubscribeParameters.Events.Add("log.entryAdded");
        var task = module.Unsubscribe(unsubscribeParameters);
        driver.WaitForCommandSet(TimeSpan.FromSeconds(1));

        driver.EmitResponse(responseJson);
        task.Wait(TimeSpan.FromSeconds(1));
        var result = task.Result;
        
        Assert.That(result, Is.Not.Null);
    }

    [Test]
    public void TestExecuteNewCommand()
    {
        string responseJson = @"{ ""result"": { ""sessionId"": ""mySession"", ""capabilities"": { ""browserName"": ""greatBrowser"", ""browserVersion"": ""101.5b"", ""platformName"": ""otherOS"", ""acceptInsecureCertificates"": true, ""proxy"": {}, ""setWindowRect"": true, ""additionalCapName"": ""additionalCapValue"" } } }";
        TestDriver driver = new TestDriver();
        SessionModule module = new SessionModule(driver);

        var newCommandParameters = new NewCommandSettings();
        var task = module.NewSession(newCommandParameters);
        driver.WaitForCommandSet(TimeSpan.FromSeconds(1));

        driver.EmitResponse(responseJson);
        task.Wait(TimeSpan.FromSeconds(1));
        var result = task.Result;
        
        Assert.That(result, Is.Not.Null);
        Assert.That(result.SessionId, Is.EqualTo("mySession"));
        Assert.That(result.Capabilities.BrowserName, Is.EqualTo("greatBrowser"));
        Assert.That(result.Capabilities.BrowserVersion, Is.EqualTo("101.5b"));
        Assert.That(result.Capabilities.PlatformName, Is.EqualTo("otherOS"));
        Assert.That(result.Capabilities.AcceptInsecureCertificates, Is.True);
        Assert.That(result.Capabilities.SetWindowRect, Is.True);
        Assert.That(result.Capabilities.Proxy, Is.Not.Null);
        Assert.That(result.Capabilities.AdditionalCapabilities.Count, Is.EqualTo(1));
        Assert.That(result.Capabilities.AdditionalCapabilities.ContainsKey("additionalCapName"));
        Assert.That(result.Capabilities.AdditionalCapabilities["additionalCapName"], Is.Not.Null);
        Assert.That(result.Capabilities.AdditionalCapabilities["additionalCapName"], Is.TypeOf<string>());
        Assert.That(result.Capabilities.AdditionalCapabilities["additionalCapName"]!.ToString(), Is.EqualTo("additionalCapValue"));
    }
}