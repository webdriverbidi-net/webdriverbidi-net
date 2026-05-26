namespace WebDriverBiDi.Storage;

using WebDriverBiDi.Network;
using WebDriverBiDi.Protocol;
using WebDriverBiDi.TestUtilities;

public class StorageModuleTests()
{
    [Fact]
    public async Task TestGetCookiesCommand()
    {
        DateTime now = DateTime.UtcNow.AddSeconds(10);
        DateTime expireTime = new(now.Ticks - (now.Ticks % TimeSpan.TicksPerSecond));
        ulong seconds = Convert.ToUInt64(expireTime.Subtract(DateTime.UnixEpoch).TotalSeconds);
        TestWebSocketConnection connection = new();
        connection.OnDataSendComplete.AddObserver(async e =>
        {
            string responseJson = $$"""
                                  {
                                    "type": "success",
                                    "id": {{e.SentCommandId}},
                                    "result": {
                                      "cookies": [
                                        {
                                          "name": "cookieName",
                                          "value": {
                                            "type": "string",
                                            "value": "cookieValue"
                                          },
                                          "domain": "cookieDomain",
                                          "path": "cookiePath",
                                          "size": 123,
                                          "httpOnly": false,
                                          "secure": true,
                                          "sameSite": "lax",
                                          "expiry": {{seconds}}
                                        }
                                      ],
                                      "partitionKey": {
                                        "userContext": "myUserContext",
                                        "sourceOrigin": "mySourceOrigin"
                                      }
                                    }
                                  }
                                  """;
            await connection.RaiseDataReceivedEventAsync(responseJson);
        });

        await using BiDiDriver driver = new(TimeSpan.FromSeconds(5), new Transport(connection));
        StorageModule module = driver.Storage;
        await driver.StartAsync("ws:localhost", TestContext.Current.CancellationToken);

        Task<GetCookiesCommandResult> task = module.GetCookiesAsync(new GetCookiesCommandParameters(), cancellationToken: TestContext.Current.CancellationToken);
        GetCookiesCommandResult result = await task.WaitAsync(TimeSpan.FromSeconds(1), TestContext.Current.CancellationToken);

        Assert.NotNull(result);

        Assert.Single(result.Cookies);
        Assert.Equal("cookieName", result.Cookies[0].Name);
        Assert.Equal(BytesValueType.String, result.Cookies[0].Value.Type);
        Assert.Equal("cookieValue", result.Cookies[0].Value.Value);
        Assert.Equal("cookieDomain", result.Cookies[0].Domain);
        Assert.Equal("cookiePath", result.Cookies[0].Path);
        Assert.Equal(123, result.Cookies[0].Size);
        Assert.True(result.Cookies[0].Secure);
        Assert.False(result.Cookies[0].HttpOnly);
        Assert.Equal(CookieSameSiteValue.Lax, result.Cookies[0].SameSite);
        Assert.Equal(expireTime, result.Cookies[0].Expires);
        Assert.Equal("myUserContext", result.PartitionKey.UserContextId);
        Assert.Equal("mySourceOrigin", result.PartitionKey.SourceOrigin);
    }

    [Fact]
    public async Task TestGetCookiesCommandWithNoArgument()
    {
        TestWebSocketConnection connection = new();
        connection.OnDataSendComplete.AddObserver(async e =>
        {
            string responseJson = $$"""
                                  {
                                    "type": "success",
                                    "id": {{e.SentCommandId}},
                                    "result": {
                                      "cookies": [],
                                      "partitionKey": {}
                                    }
                                  }
                                  """;
            await connection.RaiseDataReceivedEventAsync(responseJson);
        });

        await using BiDiDriver driver = new(TimeSpan.FromSeconds(5), new Transport(connection));
        StorageModule module = driver.Storage;
        await driver.StartAsync("ws:localhost", TestContext.Current.CancellationToken);

        Task<GetCookiesCommandResult> task = module.GetCookiesAsync(cancellationToken: TestContext.Current.CancellationToken);
        GetCookiesCommandResult result = await task.WaitAsync(TimeSpan.FromSeconds(1), TestContext.Current.CancellationToken);

        Assert.NotNull(result);
        Assert.Empty(result.Cookies);
    }

    [Fact]
    public async Task TestDeleteCookiesCommandWithNoArgument()
    {
        TestWebSocketConnection connection = new();
        connection.OnDataSendComplete.AddObserver(async e =>
        {
            string responseJson = $$"""
                                  {
                                    "type": "success",
                                    "id": {{e.SentCommandId}},
                                    "result": {
                                      "partitionKey": {}
                                    }
                                  }
                                  """;
            await connection.RaiseDataReceivedEventAsync(responseJson);
        });

        await using BiDiDriver driver = new(TimeSpan.FromSeconds(5), new Transport(connection));
        StorageModule module = driver.Storage;
        await driver.StartAsync("ws:localhost", TestContext.Current.CancellationToken);

        Task<DeleteCookiesCommandResult> task = module.DeleteCookiesAsync(cancellationToken: TestContext.Current.CancellationToken);
        DeleteCookiesCommandResult result = await task.WaitAsync(TimeSpan.FromSeconds(1), TestContext.Current.CancellationToken);

        Assert.NotNull(result);
    }

    [Fact]
    public async Task TestSetCookieCommand()
    {
        TestWebSocketConnection connection = new();
        connection.OnDataSendComplete.AddObserver(async e =>
        {
            string responseJson = $$"""
                                  {
                                    "type": "success",
                                    "id": {{e.SentCommandId}},
                                    "result": {
                                      "partitionKey": {
                                        "userContext": "myUserContext",
                                        "sourceOrigin": "mySourceOrigin" 
                                      }
                                    }
                                  }
                                  """;
            await connection.RaiseDataReceivedEventAsync(responseJson);
        });

        await using BiDiDriver driver = new(TimeSpan.FromSeconds(5), new Transport(connection));
        StorageModule module = driver.Storage;
        await driver.StartAsync("ws:localhost", TestContext.Current.CancellationToken);

        Task<SetCookieCommandResult> task = module.SetCookieAsync(new SetCookieCommandParameters(new PartialCookie("cookieName", BytesValue.FromString("cookieValue"), "cookieDomain")), cancellationToken: TestContext.Current.CancellationToken);
        SetCookieCommandResult result = await task.WaitAsync(TimeSpan.FromSeconds(1), TestContext.Current.CancellationToken);

        Assert.NotNull(result);

        Assert.Equal("myUserContext", result.PartitionKey.UserContextId);
        Assert.Equal("mySourceOrigin", result.PartitionKey.SourceOrigin);
    }

    [Fact]
    public async Task TestDeleteCookiesCommand()
    {
        TestWebSocketConnection connection = new();
        connection.OnDataSendComplete.AddObserver(async e =>
        {
            string responseJson = $$"""
                                  {
                                    "type": "success",
                                    "id": {{e.SentCommandId}},
                                    "result": {
                                      "partitionKey": {
                                        "userContext": "myUserContext",
                                        "sourceOrigin": "mySourceOrigin"
                                      }
                                    }
                                  }
                                  """;
            await connection.RaiseDataReceivedEventAsync(responseJson);
        });

        await using BiDiDriver driver = new(TimeSpan.FromSeconds(5), new Transport(connection));
        StorageModule module = driver.Storage;
        await driver.StartAsync("ws:localhost", TestContext.Current.CancellationToken);

        Task<DeleteCookiesCommandResult> task = module.DeleteCookiesAsync(new DeleteCookiesCommandParameters(), cancellationToken: TestContext.Current.CancellationToken);
        DeleteCookiesCommandResult result = await task.WaitAsync(TimeSpan.FromSeconds(1), TestContext.Current.CancellationToken);

        Assert.NotNull(result);

        Assert.Equal("myUserContext", result.PartitionKey.UserContextId);
        Assert.Equal("mySourceOrigin", result.PartitionKey.SourceOrigin);
    }
}
