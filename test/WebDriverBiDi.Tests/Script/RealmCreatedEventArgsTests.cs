namespace WebDriverBiDi.Script;

using WebDriverBiDi.Protocol;
using WebDriverBiDi.TestUtilities;

public class RealmCreatedEventArgsTests
{
    [Fact]
    public async Task TestCanCreateWithWindowRealmInfo()
    {
        string json = """
                      {
                        "type": "event",
                        "method": "script.realmCreated",
                        "params": {
                          "realm": "myRealm",
                          "origin": "myOrigin",
                          "type": "window",
                          "context": "myContext"
                        }
                      }
                      """;
        RealmCreatedEventArgs? eventArgs = await this.GenerateEventArgs(json);
        Assert.NotNull(eventArgs);

        Assert.Equal("myRealm", eventArgs.RealmId);
        Assert.Equal("myOrigin", eventArgs.Origin);
        Assert.Equal(RealmType.Window, eventArgs.Type);
    }

    [Fact]
    public async Task TestCanCreateWithNonWindowRealmInfo()
    {
        string json = """
                      { 
                        "type": "event",
                        "method": "script.realmCreated",
                        "params": {
                          "realm": "myRealm",
                          "origin": "myOrigin",
                          "type": "worker"
                        }
                      }
                      """;
        RealmCreatedEventArgs? eventArgs = await this.GenerateEventArgs(json);
        Assert.NotNull(eventArgs);

        Assert.Equal("myRealm", eventArgs.RealmId);
        Assert.Equal("myOrigin", eventArgs.Origin);
        Assert.Equal(RealmType.Worker, eventArgs.Type);
    }

    [Fact]
    public async Task TestCanCastToSpecificRealmType()
    {
        string json = """
                      {
                        "type": "event",
                        "method": "script.realmCreated",
                        "params": {
                          "realm": "myRealm",
                          "origin": "myOrigin",
                          "type": "window",
                          "context": "myContext"
                        }
                      }
                      """;
        RealmCreatedEventArgs? eventArgs = await this.GenerateEventArgs(json);
        Assert.NotNull(eventArgs);
        WindowRealmInfo castInfo = eventArgs.As<WindowRealmInfo>();

        Assert.Equal("myRealm", castInfo.RealmId);
        Assert.Equal("myOrigin", castInfo.Origin);
        Assert.Equal(RealmType.Window, castInfo.Type);
    }

    [Fact]
    public async Task TestCopySemantics()
    {
        string json = """
                      {
                        "type": "event",
                        "method": "script.realmCreated",
                        "params": {
                          "realm": "myRealm",
                          "origin": "myOrigin",
                          "type": "window",
                          "context": "myContext"
                        }
                      }
                      """;
        RealmCreatedEventArgs? eventArgs = await this.GenerateEventArgs(json);
        Assert.NotNull(eventArgs);
        RealmCreatedEventArgs copy = eventArgs with { };
        Assert.Equal(eventArgs, copy);
    }

    private async Task<RealmCreatedEventArgs?> GenerateEventArgs(string json)
    {
        TestWebSocketConnection connection = new();
        await using BiDiDriver driver = new(TimeSpan.FromSeconds(5), new(connection));
        await driver.StartAsync("ws:localhost", TestContext.Current.CancellationToken);

        RealmCreatedEventArgs? eventArgs = null;
        using EventObserver<RealmCreatedEventArgs> observer = driver.Script.OnRealmCreated.AddObserver(e => eventArgs = e);

        observer.StartCapturingTasks();
        await connection.RaiseDataReceivedEventAsync(json);
        await observer.WaitForCapturedTasksCompleteAsync(1, TimeSpan.FromMilliseconds(500), TestContext.Current.CancellationToken);
        return eventArgs;
    }
}
