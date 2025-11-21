namespace WebDriverBiDi.Browser;

using TestUtilities;

[TestFixture]
public class BrowserModuleTests
{
    [Test]
    public async Task TestExecuteCloseCommand()
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
        BrowserModule module = new(driver);

        Task<CloseCommandResult> task = module.CloseAsync(new CloseCommandParameters());
        task.Wait(TimeSpan.FromSeconds(1));
        CloseCommandResult result = task.Result;

        Assert.That(result, Is.Not.Null);
    }
    
    [Test]
    public async Task TestExecuteCloseCommandWithNoArgument()
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
        BrowserModule module = new(driver);

        Task<CloseCommandResult> task = module.CloseAsync();
        task.Wait(TimeSpan.FromSeconds(1));
        CloseCommandResult result = task.Result;

        Assert.That(result, Is.Not.Null);
    }

    [Test]
    public async Task TestExecuteCreateUserContextCommand()
    {
        TestConnection connection = new();
        connection.DataSendComplete += async (sender, e) =>
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
        };

        BiDiDriver driver = new(TimeSpan.FromMilliseconds(500), new(connection));
        await driver.StartAsync("ws:localhost");
        BrowserModule module = new(driver);

        Task<CreateUserContextCommandResult> task = module.CreateUserContextAsync(new CreateUserContextCommandParameters());
        task.Wait(TimeSpan.FromSeconds(1));
        CreateUserContextCommandResult result = task.Result;

        Assert.That(result, Is.Not.Null);
        Assert.That(result.UserContextId, Is.EqualTo("myUserContextId"));
    }

    [Test]
    public async Task TestExecuteCreateUserContextCommandWithNoArgument()
    {
        TestConnection connection = new();
        connection.DataSendComplete += async (sender, e) =>
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
        };

        BiDiDriver driver = new(TimeSpan.FromMilliseconds(500), new(connection));
        await driver.StartAsync("ws:localhost");
        BrowserModule module = new(driver);

        Task<CreateUserContextCommandResult> task = module.CreateUserContextAsync();
        task.Wait(TimeSpan.FromSeconds(1));
        CreateUserContextCommandResult result = task.Result;

        Assert.That(result, Is.Not.Null);
        Assert.That(result.UserContextId, Is.EqualTo("myUserContextId"));
    }

    [Test]
    public async Task TestExecuteGetClientWindowsCommand()
    {
        TestConnection connection = new();
        connection.DataSendComplete += async (sender, e) =>
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
        };

        BiDiDriver driver = new(TimeSpan.FromMilliseconds(500), new(connection));
        await driver.StartAsync("ws:localhost");
        BrowserModule module = new(driver);

        Task<GetClientWindowsCommandResult> task = module.GetClientWindowsAsync(new GetClientWindowsCommandParameters());
        task.Wait(TimeSpan.FromSeconds(1));
        GetClientWindowsCommandResult result = task.Result;

        Assert.That(result, Is.Not.Null);
        using (Assert.EnterMultipleScope())
        {
            Assert.That(result.ClientWindows, Has.Count.EqualTo(2));
            Assert.That(result.ClientWindows[0].ClientWindowId, Is.EqualTo("myClientWindow"));
            Assert.That(result.ClientWindows[0].IsActive, Is.True);
            Assert.That(result.ClientWindows[0].State, Is.EqualTo(ClientWindowState.Normal));
            Assert.That(result.ClientWindows[0].X, Is.EqualTo(100));
            Assert.That(result.ClientWindows[0].Y, Is.EqualTo(200));
            Assert.That(result.ClientWindows[0].Width, Is.EqualTo(640));
            Assert.That(result.ClientWindows[0].Height, Is.EqualTo(480));
            Assert.That(result.ClientWindows[1].ClientWindowId, Is.EqualTo("yourClientWindow"));
            Assert.That(result.ClientWindows[1].IsActive, Is.False);
            Assert.That(result.ClientWindows[1].State, Is.EqualTo(ClientWindowState.Normal));
            Assert.That(result.ClientWindows[1].X, Is.EqualTo(50));
            Assert.That(result.ClientWindows[1].Y, Is.EqualTo(75));
            Assert.That(result.ClientWindows[1].Width, Is.EqualTo(960));
            Assert.That(result.ClientWindows[1].Height, Is.EqualTo(720));
        }
    }

    [Test]
    public async Task TestExecuteGetClientWindowsCommandWithNoArgument()
    {
        TestConnection connection = new();
        connection.DataSendComplete += async (sender, e) =>
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
        };

        BiDiDriver driver = new(TimeSpan.FromMilliseconds(500), new(connection));
        await driver.StartAsync("ws:localhost");
        BrowserModule module = new(driver);

        Task<GetClientWindowsCommandResult> task = module.GetClientWindowsAsync();
        task.Wait(TimeSpan.FromSeconds(1));
        GetClientWindowsCommandResult result = task.Result;

        Assert.That(result, Is.Not.Null);
        using (Assert.EnterMultipleScope())
        {
            Assert.That(result.ClientWindows, Has.Count.EqualTo(2));
            Assert.That(result.ClientWindows[0].ClientWindowId, Is.EqualTo("myClientWindow"));
            Assert.That(result.ClientWindows[0].IsActive, Is.True);
            Assert.That(result.ClientWindows[0].State, Is.EqualTo(ClientWindowState.Normal));
            Assert.That(result.ClientWindows[0].X, Is.EqualTo(100));
            Assert.That(result.ClientWindows[0].Y, Is.EqualTo(200));
            Assert.That(result.ClientWindows[0].Width, Is.EqualTo(640));
            Assert.That(result.ClientWindows[0].Height, Is.EqualTo(480));
            Assert.That(result.ClientWindows[1].ClientWindowId, Is.EqualTo("yourClientWindow"));
            Assert.That(result.ClientWindows[1].IsActive, Is.False);
            Assert.That(result.ClientWindows[1].State, Is.EqualTo(ClientWindowState.Normal));
            Assert.That(result.ClientWindows[1].X, Is.EqualTo(50));
            Assert.That(result.ClientWindows[1].Y, Is.EqualTo(75));
            Assert.That(result.ClientWindows[1].Width, Is.EqualTo(960));
            Assert.That(result.ClientWindows[1].Height, Is.EqualTo(720));
        }
    }

    [Test]
    public async Task TestExecuteGetUserContextsCommand()
    {
        TestConnection connection = new();
        connection.DataSendComplete += async (sender, e) =>
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
        };

        BiDiDriver driver = new(TimeSpan.FromMilliseconds(500), new(connection));
        await driver.StartAsync("ws:localhost");
        BrowserModule module = new(driver);

        Task<GetUserContextsCommandResult> task = module.GetUserContextsAsync(new GetUserContextsCommandParameters());
        task.Wait(TimeSpan.FromSeconds(1));
        GetUserContextsCommandResult result = task.Result;

        Assert.That(result, Is.Not.Null);
        using (Assert.EnterMultipleScope())
        {
            Assert.That(result.UserContexts, Has.Count.EqualTo(2));
            Assert.That(result.UserContexts[0].UserContextId, Is.EqualTo("default"));
            Assert.That(result.UserContexts[1].UserContextId, Is.EqualTo("myUserContextId"));
        }

    }

    [Test]
    public async Task TestExecuteGetUserContextsCommandWithNoArgument()
    {
        TestConnection connection = new();
        connection.DataSendComplete += async (sender, e) =>
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
        };

        BiDiDriver driver = new(TimeSpan.FromMilliseconds(500), new(connection));
        await driver.StartAsync("ws:localhost");
        BrowserModule module = new(driver);

        Task<GetUserContextsCommandResult> task = module.GetUserContextsAsync();
        task.Wait(TimeSpan.FromSeconds(1));
        GetUserContextsCommandResult result = task.Result;

        Assert.That(result, Is.Not.Null);
        using (Assert.EnterMultipleScope())
        {
            Assert.That(result.UserContexts, Has.Count.EqualTo(2));
            Assert.That(result.UserContexts[0].UserContextId, Is.EqualTo("default"));
            Assert.That(result.UserContexts[1].UserContextId, Is.EqualTo("myUserContextId"));
        }
    }

    [Test]
    public async Task TestExecuteRemoveUserContextCommand()
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
        BrowserModule module = new(driver);

        Task<RemoveUserContextCommandResult> task = module.RemoveUserContextAsync(new RemoveUserContextCommandParameters("myUserContextId"));
        task.Wait(TimeSpan.FromSeconds(1));
        RemoveUserContextCommandResult result = task.Result;

        Assert.That(result, Is.Not.Null);
    }

    [Test]
    public async Task TestExecuteSetClientWindowStateCommand()
    {
        TestConnection connection = new();
        connection.DataSendComplete += async (sender, e) =>
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
        };

        BiDiDriver driver = new(TimeSpan.FromMilliseconds(500), new(connection));
        await driver.StartAsync("ws:localhost");
        BrowserModule module = new(driver);

        Task<SetClientWindowStateCommandResult> task = module.SetClientWindowStateAsync(new SetClientWindowStateCommandParameters("myClientWindow")
        {
            State = ClientWindowState.Normal,
            X = 100,
            Y = 200,
            Width = 640,
            Height = 480
        });
        task.Wait(TimeSpan.FromSeconds(1));
        SetClientWindowStateCommandResult result = task.Result;

        Assert.That(result, Is.Not.Null);
        using (Assert.EnterMultipleScope())
        {
            Assert.That(result.ClientWindowId, Is.EqualTo("myClientWindow"));
            Assert.That(result.IsActive, Is.True);
            Assert.That(result.State, Is.EqualTo(ClientWindowState.Normal));
            Assert.That(result.X, Is.EqualTo(100));
            Assert.That(result.Y, Is.EqualTo(200));
            Assert.That(result.Width, Is.EqualTo(640));
            Assert.That(result.Height, Is.EqualTo(480));
        }
    }

    [Test]
    public async Task TestSetDownloadBehaviorCommand()
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
        BrowserModule module = new(driver);

        Task<SetDownloadBehaviorCommandResult> task = module.SetDownloadBehaviorAsync(new SetDownloadBehaviorCommandParameters());
        task.Wait(TimeSpan.FromSeconds(1));
        SetDownloadBehaviorCommandResult result = task.Result;

        Assert.That(result, Is.Not.Null);
    }
}
