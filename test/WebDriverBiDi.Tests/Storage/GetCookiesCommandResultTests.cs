namespace WebDriverBiDi.Storage;

using System.Text.Json;

using WebDriverBiDi.Network;

public class GetCookiesCommandResultTests
{
    [Fact]
    public void TestCanDeserialize()
    {
        DateTime now = DateTime.UtcNow.AddSeconds(10);
        DateTime expireTime = new(now.Ticks - (now.Ticks % TimeSpan.TicksPerSecond));
        ulong seconds = Convert.ToUInt64(expireTime.Subtract(DateTime.UnixEpoch).TotalSeconds);
        string json = $$"""
                      {
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
                          "sourceOrigin": "mySourceOrigin",
                          "extraPropertyName": "extraPropertyValue"
                        }
                      }
                      """;
        GetCookiesCommandResult? result = JsonSerializer.Deserialize<GetCookiesCommandResult>(json);
        Assert.NotNull(result);

        Assert.NotNull(result.Cookies);
        Assert.Single(result.Cookies);
        Assert.Equal("cookieName", result.Cookies[0].Name);
        Assert.Equal(BytesValueType.String, result.Cookies[0].Value.Type);
        Assert.Equal("cookieValue", result.Cookies[0].Value.Value);
        Assert.Equal("cookieDomain", result.Cookies[0].Domain);
        Assert.Equal("cookiePath", result.Cookies[0].Path);
        Assert.Equal(123, result.Cookies[0].Size);
        Assert.False(result.Cookies[0].HttpOnly);
        Assert.True(result.Cookies[0].Secure);
        Assert.Equal(CookieSameSiteValue.Lax, result.Cookies[0].SameSite);
        Assert.Equal(expireTime, result.Cookies[0].Expires);
        Assert.NotNull(result.PartitionKey);
        Assert.Equal("myUserContext", result.PartitionKey.UserContextId);
        Assert.Equal("mySourceOrigin", result.PartitionKey.SourceOrigin);
        Assert.Single(result.PartitionKey.AdditionalData);
        Assert.True(result.PartitionKey.AdditionalData.ContainsKey("extraPropertyName"));
        Assert.Equal("extraPropertyValue", result.PartitionKey.AdditionalData["extraPropertyName"]);
    }

    [Fact]
    public void TestCanDeserializeWithNoCookieData()
    {
        string json = """
                      {
                        "cookies": [],
                        "partitionKey": {}
                      }
                      """;
        GetCookiesCommandResult? result = JsonSerializer.Deserialize<GetCookiesCommandResult>(json);
        Assert.NotNull(result);

        Assert.NotNull(result.Cookies);
        Assert.Empty(result.Cookies);
        Assert.NotNull(result.PartitionKey);
        Assert.Null(result.PartitionKey.UserContextId);
        Assert.Null(result.PartitionKey.SourceOrigin);
        Assert.Empty(result.PartitionKey.AdditionalData);
    }

    [Fact]
    public void TestCopySemantics()
    {
        DateTime now = DateTime.UtcNow.AddSeconds(10);
        DateTime expireTime = new(now.Ticks - (now.Ticks % TimeSpan.TicksPerSecond));
        ulong seconds = Convert.ToUInt64(expireTime.Subtract(DateTime.UnixEpoch).TotalSeconds);
        string json = $$"""
                      {
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
                          "sourceOrigin": "mySourceOrigin",
                          "extraPropertyName": "extraPropertyValue"
                        }
                      }
                      """;
        GetCookiesCommandResult? result = JsonSerializer.Deserialize<GetCookiesCommandResult>(json);
        Assert.NotNull(result);
        GetCookiesCommandResult copy = result with { };
        Assert.Equal(result, copy);
    }

    [Fact]
    public void TestDeserializingWithMissingCookiesThrows()
    {
        string json = """
                      {
                        "partitionKey": {}
                      }
                      """;
        Assert.ThrowsAny<JsonException>(() => JsonSerializer.Deserialize<GetCookiesCommandResult>(json));
    }

    [Fact]
    public void TestDeserializingWithInvalidCookiesDataTypeThrows()
    {
        string json = """
                      {
                        "cookies": "invalidCookieArrayType", "partitionKey": {}
                      }
                      """;
        Assert.ThrowsAny<JsonException>(() => JsonSerializer.Deserialize<GetCookiesCommandResult>(json));
    }

    [Fact]
    public void TestDeserializingWithMissingPartitionThrows()
    {
        string json = """
                      {
                        "cookies": []
                      }
                      """;
        Assert.ThrowsAny<JsonException>(() => JsonSerializer.Deserialize<GetCookiesCommandResult>(json));
    }

    [Fact]
    public void TestDeserializingWithInvalidPartitionDataTypeThrows()
    {
        string json = """
                      {
                        "cookies": [], "partitionKey": "invalidPartitionType"
                      }
                      """;
        Assert.ThrowsAny<JsonException>(() => JsonSerializer.Deserialize<GetCookiesCommandResult>(json));
    }
}
