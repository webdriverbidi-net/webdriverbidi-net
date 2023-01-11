namespace WebDriverBidi.Log;

using TestUtilities;

[TestFixture]
public class LogModuleTests
{
    [Test]
    public void TestCanReceiveEntryAddedEvent()
    {
        string eventJson = @"{ ""method"": ""log.entryAdded"", ""params"": { ""type"": ""javascript"", ""level"": ""debug"", ""source"": { ""realm"": ""myRealmId"", ""context"": ""browsingContextId"" }, ""text"": ""my log message"", ""timestamp"": 123 } }";
        bool eventRaised = false;
        TestDriver driver = new();
        LogModule module = new(driver);
        module.EntryAdded += (object? obj, EntryAddedEventArgs e) => {
            eventRaised = true;
            Assert.Multiple(() =>
            {
                Assert.That(e.Type, Is.EqualTo("javascript"));
                Assert.That(e.Text, Is.EqualTo("my log message"));
            });
        };

        driver.EmitResponse(eventJson);
        Assert.That(eventRaised, Is.True);
    }

    [Test]
    public void TestCanReceiveEntryAddedEventForConsoleLogType()
    {
        string eventJson = @"{ ""method"": ""log.entryAdded"", ""params"": { ""type"": ""console"", ""level"": ""debug"", ""source"": { ""realm"": ""myRealmId"", ""context"": ""browsingContextId"" }, ""text"": ""my log message"", ""timestamp"": 123, ""method"": ""myMethod"", ""args"": [] } }";
        bool eventRaised = false;
        TestDriver driver = new();
        LogModule module = new(driver);
        module.EntryAdded += (object? obj, EntryAddedEventArgs e) => {
            eventRaised = true;
            Assert.Multiple(() =>
            {
                Assert.That(e.Type, Is.EqualTo("console"));
                Assert.That(e.Text, Is.EqualTo("my log message"));
                Assert.That(e.Method, Is.Not.Null);
                Assert.That(e.Arguments, Is.Not.Null);
            });
        };

        driver.EmitResponse(eventJson);
        Assert.That(eventRaised, Is.True);
    }
}