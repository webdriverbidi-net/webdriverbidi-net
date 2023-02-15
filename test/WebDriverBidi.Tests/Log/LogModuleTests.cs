namespace WebDriverBidi.Log;

using TestUtilities;
using WebDriverBidi.Protocol;

[TestFixture]
public class LogModuleTests
{
    [Test]
    public void TestCanReceiveEntryAddedEvent()
    {
        TestConnection connection = new();
        Driver driver = new(new Transport(TimeSpan.FromMilliseconds(500), connection));
        LogModule module = new(driver);

        bool eventRaised = false;
        module.EntryAdded += (object? obj, EntryAddedEventArgs e) => {
            eventRaised = true;
            Assert.Multiple(() =>
            {
                Assert.That(e.Type, Is.EqualTo("javascript"));
                Assert.That(e.Text, Is.EqualTo("my log message"));
                Assert.That(e.Method, Is.Null);
                Assert.That(e.Arguments, Is.Null);
                Assert.That(e.StackTrace, Is.Not.Null);
                Assert.That(e.StackTrace!.CallFrames, Is.Empty);
            });
        };

        long epochTimestamp = Convert.ToInt64((DateTime.Now - DateTime.UnixEpoch).TotalMilliseconds);
        string eventJson = @"{ ""method"": ""log.entryAdded"", ""params"": { ""type"": ""javascript"", ""level"": ""debug"", ""source"": { ""realm"": ""myRealmId"", ""context"": ""browsingContextId"" }, ""text"": ""my log message"", ""timestamp"": " + epochTimestamp + @", ""stackTrace"": { ""callFrames"": [] } } }";
        connection.RaiseDataReceivedEvent(eventJson);
        Assert.That(eventRaised, Is.True);
    }

    [Test]
    public void TestCanReceiveEntryAddedEventForConsoleLogType()
    {
        TestConnection connection = new();
        Driver driver = new(new Transport(TimeSpan.FromMilliseconds(500), connection));
        LogModule module = new(driver);

        long epochTimestamp = Convert.ToInt64((DateTime.Now - DateTime.UnixEpoch).TotalMilliseconds);
        bool eventRaised = false;
        module.EntryAdded += (object? obj, EntryAddedEventArgs e) => {
            eventRaised = true;
            Assert.Multiple(() =>
            {
                Assert.That(e.Type, Is.EqualTo("console"));
                Assert.That(e.Text, Is.EqualTo("my log message"));
                Assert.That(e.Method, Is.Not.Null);
                Assert.That(e.Arguments, Is.Not.Null);
                Assert.That(e.Source, Is.Not.Null);
                Assert.That(e.Timestamp, Is.EqualTo(DateTime.UnixEpoch.AddMilliseconds(epochTimestamp)));
                Assert.That(e.StackTrace, Is.Not.Null);
                Assert.That(e.StackTrace!.CallFrames, Is.Empty);
            });
        };

        string eventJson = @"{ ""method"": ""log.entryAdded"", ""params"": { ""type"": ""console"", ""level"": ""debug"", ""source"": { ""realm"": ""myRealmId"", ""context"": ""browsingContextId"" }, ""text"": ""my log message"", ""timestamp"": " + epochTimestamp + @", ""method"": ""myMethod"", ""args"": [], ""stackTrace"": { ""callFrames"": [] } } }";
        connection.RaiseDataReceivedEvent(eventJson);
        Assert.That(eventRaised, Is.True);
    }
}