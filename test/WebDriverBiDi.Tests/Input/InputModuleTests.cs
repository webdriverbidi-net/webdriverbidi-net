namespace WebDriverBiDi.Input;

using WebDriverBiDi.Script;
using WebDriverBiDi.TestUtilities;

public class InputModuleTests
{
    [Fact]
    public async Task TestExecutePerformActions()
    {
        TestWebSocketConnection connection = new();
        connection.OnDataSendComplete.AddObserver(async e =>
        {
            string responseJson = $$"""
                                  {
                                    "type": "success",
                                    "id": {{e.SentCommandId}},
                                    "result": {}
                                  }
                                  """;
            await connection.RaiseDataReceivedEventAsync(responseJson);
        });

        await using BiDiDriver driver = new(TimeSpan.FromSeconds(5), new(connection));
        await driver.StartAsync("ws:localhost", TestContext.Current.CancellationToken);
        InputModule module = driver.Input;

        Task<PerformActionsCommandResult> task = module.PerformActionsAsync(new PerformActionsCommandParameters("myContextId"), cancellationToken: TestContext.Current.CancellationToken);
        PerformActionsCommandResult result = await task.WaitAsync(TimeSpan.FromSeconds(1), TestContext.Current.CancellationToken);

        Assert.NotNull(result);
    }

    [Fact]
    public async Task TestExecuteReleaseActions()
    {
        TestWebSocketConnection connection = new();
        connection.OnDataSendComplete.AddObserver(async e =>
        {
            string responseJson = $$"""
                                  {
                                    "type": "success",
                                    "id": {{e.SentCommandId}},
                                    "result": {}
                                  }
                                  """;
            await connection.RaiseDataReceivedEventAsync(responseJson);
        });

        await using BiDiDriver driver = new(TimeSpan.FromSeconds(5), new(connection));
        await driver.StartAsync("ws:localhost", TestContext.Current.CancellationToken);
        InputModule module = driver.Input;

        Task<ReleaseActionsCommandResult> task = module.ReleaseActionsAsync(new ReleaseActionsCommandParameters("myContextId"), cancellationToken: TestContext.Current.CancellationToken);
        ReleaseActionsCommandResult result = await task.WaitAsync(TimeSpan.FromSeconds(1), TestContext.Current.CancellationToken);

        Assert.NotNull(result);
    }

    [Fact]
    public async Task TestExecuteSetFiles()
    {
        TestWebSocketConnection connection = new();
        connection.OnDataSendComplete.AddObserver(async e =>
        {
            string responseJson = $$"""
                                  {
                                    "type": "success",
                                    "id": {{e.SentCommandId}},
                                    "result": {}
                                  }
                                  """;
            await connection.RaiseDataReceivedEventAsync(responseJson);
        });

        await using BiDiDriver driver = new(TimeSpan.FromSeconds(5), new(connection));
        await driver.StartAsync("ws:localhost", TestContext.Current.CancellationToken);
        InputModule module = driver.Input;

        SharedReference element = new("mySharedId");
        Task<SetFilesCommandResult> task = module.SetFilesAsync(new SetFilesCommandParameters("myContextId", element), cancellationToken: TestContext.Current.CancellationToken);
        SetFilesCommandResult result = await task.WaitAsync(TimeSpan.FromSeconds(1), TestContext.Current.CancellationToken);

        Assert.NotNull(result);
    }

    [Fact]
    public async Task TestCanReceiveFileDialogOpenedEvent()
    {
        TestWebSocketConnection connection = new();
        await using BiDiDriver driver = new(TimeSpan.FromSeconds(5), new(connection));
        await driver.StartAsync("ws:localhost", TestContext.Current.CancellationToken);
        InputModule module = driver.Input;

        TaskCompletionSource taskCompletionSource = new(TaskCreationOptions.RunContinuationsAsynchronously);
        module.OnFileDialogOpened.AddObserver(e =>
        {
            Assert.Equal("myContext", e.BrowsingContextId);
            Assert.True(e.IsMultiple);
            Assert.Null(e.Element);

            taskCompletionSource.TrySetResult();
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
        await taskCompletionSource.Task.WaitAsync(TimeSpan.FromSeconds(5), TestContext.Current.CancellationToken);
    }

    [Fact]
    public async Task TestCanReceiveFileDialogOpenedEventWithElementReference()
    {
        TestWebSocketConnection connection = new();
        await using BiDiDriver driver = new(TimeSpan.FromSeconds(5), new(connection));
        await driver.StartAsync("ws:localhost", cancellationToken: TestContext.Current.CancellationToken);
        InputModule module = driver.Input;

        TaskCompletionSource taskCompletionSource = new(TaskCreationOptions.RunContinuationsAsynchronously);
        module.OnFileDialogOpened.AddObserver(e =>
        {
            Assert.Equal("myContext", e.BrowsingContextId);
            Assert.True(e.IsMultiple);
            Assert.NotNull(e.Element);
            Assert.Equal("mySharedId", e.Element.SharedId);

            taskCompletionSource.TrySetResult();
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
        await taskCompletionSource.Task.WaitAsync(TimeSpan.FromSeconds(5), TestContext.Current.CancellationToken);
    }
}
