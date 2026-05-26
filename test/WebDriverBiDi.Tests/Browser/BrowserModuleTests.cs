namespace WebDriverBiDi.Browser;

using TestUtilities;

public class BrowserModuleTests
{
    [Fact]
    public async Task TestExecuteCloseCommand()
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

        await using BiDiDriver driver = new(TimeSpan.FromMilliseconds(500), new(connection));
        await driver.StartAsync("ws:localhost", TestContext.Current.CancellationToken);
        BrowserModule module = driver.Browser;

        Task<CloseCommandResult> task = module.CloseAsync(new CloseCommandParameters(), cancellationToken: TestContext.Current.CancellationToken);
        CloseCommandResult result = await task.WaitAsync(TimeSpan.FromSeconds(1), TestContext.Current.CancellationToken);

        Assert.NotNull(result);
    }

    [Fact]
    public async Task TestExecuteCloseCommandWithNoArgument()
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

        await using BiDiDriver driver = new(TimeSpan.FromMilliseconds(500), new(connection));
        await driver.StartAsync("ws:localhost", TestContext.Current.CancellationToken);
        BrowserModule module = driver.Browser;

        Task<CloseCommandResult> task = module.CloseAsync(cancellationToken: TestContext.Current.CancellationToken);
        CloseCommandResult result = await task.WaitAsync(TimeSpan.FromSeconds(1), TestContext.Current.CancellationToken);

        Assert.NotNull(result);
    }

    [Fact]
    public async Task TestExecuteCreateUserContextCommand()
    {
        TestWebSocketConnection connection = new();
        connection.OnDataSendComplete.AddObserver(async e =>
        {
            string responseJson = $$"""
                                  {
                                    "type": "success",
                                    "id": {{e.SentCommandId}},
                                    "result": {
                                      "userContext": "myUserContextId"
                                    }
                                  }
                                  """;
            await connection.RaiseDataReceivedEventAsync(responseJson);
        });

        await using BiDiDriver driver = new(TimeSpan.FromMilliseconds(500), new(connection));
        await driver.StartAsync("ws:localhost", TestContext.Current.CancellationToken);
        BrowserModule module = driver.Browser;

        Task<CreateUserContextCommandResult> task = module.CreateUserContextAsync(new CreateUserContextCommandParameters(), cancellationToken: TestContext.Current.CancellationToken);
        CreateUserContextCommandResult result = await task.WaitAsync(TimeSpan.FromSeconds(1), TestContext.Current.CancellationToken);

        Assert.NotNull(result);
        Assert.Equal("myUserContextId", result.UserContextId);
    }

    [Fact]
    public async Task TestExecuteCreateUserContextCommandWithNoArgument()
    {
        TestWebSocketConnection connection = new();
        connection.OnDataSendComplete.AddObserver(async e =>
        {
            string responseJson = $$"""
                                  {
                                    "type": "success",
                                    "id": {{e.SentCommandId}},
                                    "result": {
                                      "userContext": "myUserContextId" 
                                    }
                                  }
                                  """;
            await connection.RaiseDataReceivedEventAsync(responseJson);
        });

        await using BiDiDriver driver = new(TimeSpan.FromMilliseconds(500), new(connection));
        await driver.StartAsync("ws:localhost", TestContext.Current.CancellationToken);
        BrowserModule module = driver.Browser;

        Task<CreateUserContextCommandResult> task = module.CreateUserContextAsync(cancellationToken: TestContext.Current.CancellationToken);
        CreateUserContextCommandResult result = await task.WaitAsync(TimeSpan.FromSeconds(1), TestContext.Current.CancellationToken);

        Assert.NotNull(result);
        Assert.Equal("myUserContextId", result.UserContextId);
    }

    [Fact]
    public async Task TestExecuteGetClientWindowsCommand()
    {
        TestWebSocketConnection connection = new();
        connection.OnDataSendComplete.AddObserver(async e =>
        {
            string responseJson = $$"""
                                  {
                                    "type": "success",
                                    "id": {{e.SentCommandId}},
                                    "result": {
                                      "clientWindows": [
                                        {
                                          "clientWindow": "myClientWindow",
                                          "active": true,
                                          "state": "normal",
                                          "x": 100,
                                          "y": 200,
                                          "width": 640,
                                          "height": 480
                                        },
                                        {
                                          "clientWindow": "yourClientWindow",
                                          "active": false,
                                          "state": "normal",
                                          "x": 50,
                                          "y": 75,
                                          "width": 960,
                                          "height": 720
                                        }
                                      ]
                                    }
                                  }
                                  """;
            await connection.RaiseDataReceivedEventAsync(responseJson);
        });

        await using BiDiDriver driver = new(TimeSpan.FromMilliseconds(500), new(connection));
        await driver.StartAsync("ws:localhost", TestContext.Current.CancellationToken);
        BrowserModule module = driver.Browser;

        Task<GetClientWindowsCommandResult> task = module.GetClientWindowsAsync(new GetClientWindowsCommandParameters(), cancellationToken: TestContext.Current.CancellationToken);
        GetClientWindowsCommandResult result = await task.WaitAsync(TimeSpan.FromSeconds(1), TestContext.Current.CancellationToken);

        Assert.NotNull(result);

        Assert.Equal(2, result.ClientWindows.Count);
        Assert.Equal("myClientWindow", result.ClientWindows[0].ClientWindowId);
        Assert.True(result.ClientWindows[0].IsActive);
        Assert.Equal(ClientWindowState.Normal, result.ClientWindows[0].State);
        Assert.Equal(100u, result.ClientWindows[0].X);
        Assert.Equal(200u, result.ClientWindows[0].Y);
        Assert.Equal(640u, result.ClientWindows[0].Width);
        Assert.Equal(480u, result.ClientWindows[0].Height);
        Assert.Equal("yourClientWindow", result.ClientWindows[1].ClientWindowId);
        Assert.False(result.ClientWindows[1].IsActive);
        Assert.Equal(ClientWindowState.Normal, result.ClientWindows[1].State);
        Assert.Equal(50u, result.ClientWindows[1].X);
        Assert.Equal(75u, result.ClientWindows[1].Y);
        Assert.Equal(960u, result.ClientWindows[1].Width);
        Assert.Equal(720u, result.ClientWindows[1].Height);
    }

    [Fact]
    public async Task TestExecuteGetClientWindowsCommandWithNoArgument()
    {
        TestWebSocketConnection connection = new();
        connection.OnDataSendComplete.AddObserver(async e =>
        {
            string responseJson = $$"""
                                  {
                                    "type": "success",
                                    "id": {{e.SentCommandId}},
                                    "result": {
                                      "clientWindows": [
                                        {
                                          "clientWindow": "myClientWindow",
                                          "active": true,
                                          "state": "normal",
                                          "x": 100,
                                          "y": 200,
                                          "width": 640,
                                          "height": 480
                                        },
                                        {
                                          "clientWindow": "yourClientWindow",
                                          "active": false,
                                          "state": "normal",
                                          "x": 50,
                                          "y": 75,
                                          "width": 960,
                                          "height": 720
                                        }
                                      ]
                                    }
                                  }
                                  """;
            await connection.RaiseDataReceivedEventAsync(responseJson);
        });

        await using BiDiDriver driver = new(TimeSpan.FromMilliseconds(500), new(connection));
        await driver.StartAsync("ws:localhost", TestContext.Current.CancellationToken);
        BrowserModule module = driver.Browser;

        Task<GetClientWindowsCommandResult> task = module.GetClientWindowsAsync(cancellationToken: TestContext.Current.CancellationToken);
        GetClientWindowsCommandResult result = await task.WaitAsync(TimeSpan.FromSeconds(1), TestContext.Current.CancellationToken);

        Assert.NotNull(result);

        Assert.Equal(2, result.ClientWindows.Count);
        Assert.Equal("myClientWindow", result.ClientWindows[0].ClientWindowId);
        Assert.True(result.ClientWindows[0].IsActive);
        Assert.Equal(ClientWindowState.Normal, result.ClientWindows[0].State);
        Assert.Equal(100u, result.ClientWindows[0].X);
        Assert.Equal(200u, result.ClientWindows[0].Y);
        Assert.Equal(640u, result.ClientWindows[0].Width);
        Assert.Equal(480u, result.ClientWindows[0].Height);
        Assert.Equal("yourClientWindow", result.ClientWindows[1].ClientWindowId);
        Assert.False(result.ClientWindows[1].IsActive);
        Assert.Equal(ClientWindowState.Normal, result.ClientWindows[1].State);
        Assert.Equal(50u, result.ClientWindows[1].X);
        Assert.Equal(75u, result.ClientWindows[1].Y);
        Assert.Equal(960u, result.ClientWindows[1].Width);
        Assert.Equal(720u, result.ClientWindows[1].Height);
    }

    [Fact]
    public async Task TestExecuteGetUserContextsCommand()
    {
        TestWebSocketConnection connection = new();
        connection.OnDataSendComplete.AddObserver(async e =>
        {
            string responseJson = $$"""
                                  {
                                    "type": "success",
                                    "id": {{e.SentCommandId}},
                                    "result": {
                                      "userContexts": [
                                        {
                                          "userContext": "default"
                                        },
                                        {
                                          "userContext": "myUserContextId"
                                        }
                                      ]
                                    }
                                  }
                                  """;
            await connection.RaiseDataReceivedEventAsync(responseJson);
        });

        await using BiDiDriver driver = new(TimeSpan.FromMilliseconds(500), new(connection));
        await driver.StartAsync("ws:localhost", TestContext.Current.CancellationToken);
        BrowserModule module = driver.Browser;

        Task<GetUserContextsCommandResult> task = module.GetUserContextsAsync(new GetUserContextsCommandParameters(), cancellationToken: TestContext.Current.CancellationToken);
        GetUserContextsCommandResult result = await task.WaitAsync(TimeSpan.FromSeconds(1), TestContext.Current.CancellationToken);

        Assert.NotNull(result);

        Assert.Equal(2, result.UserContexts.Count);
        Assert.Equal("default", result.UserContexts[0].UserContextId);
        Assert.Equal("myUserContextId", result.UserContexts[1].UserContextId);
    }

    [Fact]
    public async Task TestExecuteGetUserContextsCommandWithNoArgument()
    {
        TestWebSocketConnection connection = new();
        connection.OnDataSendComplete.AddObserver(async e =>
        {
            string responseJson = $$"""
                                  {
                                    "type": "success",
                                    "id": {{e.SentCommandId}},
                                    "result": {
                                      "userContexts": [
                                        {
                                          "userContext": "default"
                                        },
                                        {
                                          "userContext": "myUserContextId"
                                        }
                                      ] 
                                    }
                                  }
                                  """;
            await connection.RaiseDataReceivedEventAsync(responseJson);
        });

        await using BiDiDriver driver = new(TimeSpan.FromMilliseconds(500), new(connection));
        await driver.StartAsync("ws:localhost", TestContext.Current.CancellationToken);
        BrowserModule module = driver.Browser;

        Task<GetUserContextsCommandResult> task = module.GetUserContextsAsync(cancellationToken: TestContext.Current.CancellationToken);
        GetUserContextsCommandResult result = await task.WaitAsync(TimeSpan.FromSeconds(1), TestContext.Current.CancellationToken);

        Assert.NotNull(result);

        Assert.Equal(2, result.UserContexts.Count);
        Assert.Equal("default", result.UserContexts[0].UserContextId);
        Assert.Equal("myUserContextId", result.UserContexts[1].UserContextId);
    }

    [Fact]
    public async Task TestExecuteRemoveUserContextCommand()
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

        await using BiDiDriver driver = new(TimeSpan.FromMilliseconds(500), new(connection));
        await driver.StartAsync("ws:localhost", TestContext.Current.CancellationToken);
        BrowserModule module = driver.Browser;

        Task<RemoveUserContextCommandResult> task = module.RemoveUserContextAsync(new RemoveUserContextCommandParameters("myUserContextId"), cancellationToken: TestContext.Current.CancellationToken);
        RemoveUserContextCommandResult result = await task.WaitAsync(TimeSpan.FromSeconds(1), TestContext.Current.CancellationToken);

        Assert.NotNull(result);
    }

    [Fact]
    public async Task TestExecuteSetClientWindowStateCommand()
    {
        TestWebSocketConnection connection = new();
        connection.OnDataSendComplete.AddObserver(async e =>
        {
            string responseJson = $$"""
                                  {
                                    "type": "success",
                                    "id": {{e.SentCommandId}},
                                    "result": {
                                      "clientWindow": "myClientWindow",
                                      "active": true,
                                      "state": "normal",
                                      "x": 100,
                                      "y": 200,
                                      "width": 640,
                                      "height": 480
                                    }
                                  }
                                  """;
            await connection.RaiseDataReceivedEventAsync(responseJson);
        });

        await using BiDiDriver driver = new(TimeSpan.FromMilliseconds(500), new(connection));
        await driver.StartAsync("ws:localhost", TestContext.Current.CancellationToken);
        BrowserModule module = driver.Browser;

        Task<SetClientWindowStateCommandResult> task = module.SetClientWindowStateAsync(new SetClientWindowStateCommandParameters("myClientWindow")
        {
            State = ClientWindowState.Normal,
            X = 100,
            Y = 200,
            Width = 640,
            Height = 480
        }, cancellationToken: TestContext.Current.CancellationToken);
        SetClientWindowStateCommandResult result = await task.WaitAsync(TimeSpan.FromSeconds(1), TestContext.Current.CancellationToken);

        Assert.NotNull(result);

        Assert.Equal("myClientWindow", result.ClientWindowId);
        Assert.True(result.IsActive);
        Assert.Equal(ClientWindowState.Normal, result.State);
        Assert.Equal(100u, result.X);
        Assert.Equal(200u, result.Y);
        Assert.Equal(640u, result.Width);
        Assert.Equal(480u, result.Height);
    }

    [Fact]
    public async Task TestSetDownloadBehaviorCommand()
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

        await using BiDiDriver driver = new(TimeSpan.FromMilliseconds(500), new(connection));
        await driver.StartAsync("ws:localhost", TestContext.Current.CancellationToken);
        BrowserModule module = driver.Browser;

        Task<SetDownloadBehaviorCommandResult> task = module.SetDownloadBehaviorAsync(new SetDownloadBehaviorCommandParameters(), cancellationToken: TestContext.Current.CancellationToken);
        SetDownloadBehaviorCommandResult result = await task.WaitAsync(TimeSpan.FromSeconds(1), TestContext.Current.CancellationToken);

        Assert.NotNull(result);
    }
}
