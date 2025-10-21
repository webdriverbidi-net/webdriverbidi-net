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
            string responseJson = $$"""
                                  {
                                    "type": "success",
                                    "id": {{e.SentCommandId}},
                                    "result": {}
                                  }
                                  """;
            await connection.RaiseDataReceivedEventAsync(responseJson);
        };

        BiDiDriver driver = new(TimeSpan.FromMilliseconds(500), new(connection));
        await driver.StartAsync("ws:localhost");
        InputModule module = new(driver);

        Task<PerformActionsCommandResult> task = module.PerformActionsAsync(new PerformActionsCommandParameters("myContextId"));
        task.Wait(TimeSpan.FromSeconds(1));
        PerformActionsCommandResult result = task.Result;

        Assert.That(result, Is.Not.Null);
    }

    [Test]
    public async Task TestExecuteReleaseActions()
    {
        TestConnection connection = new();
        connection.DataSendComplete += async (sender, e) =>
        {
            string responseJson = $$"""
                                  {
                                    "type": "success",
                                    "id": {{e.SentCommandId}},
                                    "result": {}
                                  }
                                  """;
            await connection.RaiseDataReceivedEventAsync(responseJson);
        };

        BiDiDriver driver = new(TimeSpan.FromMilliseconds(500), new(connection));
        await driver.StartAsync("ws:localhost");
        InputModule module = new(driver);

        Task<ReleaseActionsCommandResult> task = module.ReleaseActionsAsync(new ReleaseActionsCommandParameters("myContextId"));
        task.Wait(TimeSpan.FromSeconds(1));
        ReleaseActionsCommandResult result = task.Result;

        Assert.That(result, Is.Not.Null);
    }

    [Test]
    public async Task TestExecuteSetFiles()
    {
        TestConnection connection = new();
        connection.DataSendComplete += async (sender, e) =>
        {
            string responseJson = $$"""
                                  {
                                    "type": "success",
                                    "id": {{e.SentCommandId}},
                                    "result": {}
                                  }
                                  """;
            await connection.RaiseDataReceivedEventAsync(responseJson);
        };

        BiDiDriver driver = new(TimeSpan.FromMilliseconds(500), new(connection));
        await driver.StartAsync("ws:localhost");
        InputModule module = new(driver);

        SharedReference element = new("mySharedId");
        Task<SetFilesCommandResult> task = module.SetFilesAsync(new SetFilesCommandParameters("myContextId", element));
        task.Wait(TimeSpan.FromSeconds(1));
        SetFilesCommandResult result = task.Result;

        Assert.That(result, Is.Not.Null);
    }

    [Test]
    public async Task TestCanReceiveFileDialogOpenedEvent()
    {
        TestConnection connection = new();
        BiDiDriver driver = new(TimeSpan.FromMilliseconds(500), new(connection));
        await driver.StartAsync("ws:localhost");
        InputModule module = new(driver);

        ManualResetEvent syncEvent = new(false);
        module.OnFileDialogOpened.AddObserver((FileDialogOpenedEventArgs e) => {
            Assert.Multiple(() =>
            {
                Assert.That(e.BrowsingContextId, Is.EqualTo("myContext"));
                Assert.That(e.IsMultiple, Is.True);
                Assert.That(e.Element, Is.Null);
            });
            syncEvent.Set();
        });

        string eventJson = $$"""
                           {
                             "type": "event",
                             "method": "input.fileDialogOpened",
                             "params": {
                               "context": "myContext",
                               "multiple": true
                             }
                           }
                           """;
        await connection.RaiseDataReceivedEventAsync(eventJson);
        bool eventRaised = syncEvent.WaitOne(TimeSpan.FromMilliseconds(250));
        Assert.That(eventRaised, Is.True);
    }

    [Test]
    public async Task TestCanReceiveFileDialogOpenedEventWithElementReference()
    {
        TestConnection connection = new();
        BiDiDriver driver = new(TimeSpan.FromMilliseconds(500), new(connection));
        await driver.StartAsync("ws:localhost");
        InputModule module = new(driver);

        ManualResetEvent syncEvent = new(false);
        module.OnFileDialogOpened.AddObserver((FileDialogOpenedEventArgs e) => {
            Assert.Multiple(() =>
            {
                Assert.That(e.BrowsingContextId, Is.EqualTo("myContext"));
                Assert.That(e.IsMultiple, Is.True);
                Assert.That(e.Element, Is.Not.Null);
                Assert.That(e.Element!.SharedId, Is.EqualTo("mySharedId"));
            });
            syncEvent.Set();
        });

        string eventJson = $$"""
                           {
                             "type": "event",
                             "method": "input.fileDialogOpened",
                             "params": {
                               "context": "myContext",
                               "multiple": true,
                               "element": {
                                 "type": "node",
                                 "sharedId": "mySharedId",
                                 "value": {
                                   "nodeType": 1,
                                   "nodeValue": "",
                                   "childNodeCount": 0
                                 }
                               }
                             }
                           }
                           """;
        await connection.RaiseDataReceivedEventAsync(eventJson);
        bool eventRaised = syncEvent.WaitOne(TimeSpan.FromMilliseconds(250));
        Assert.That(eventRaised, Is.True);
    }
}
