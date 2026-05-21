namespace WebDriverBiDi.Network;

using System.Text.Json;

public class ResponseDataTests
{
    [Fact]
    public void TestCanDeserializeResponseData()
    {
        string json = """
                      {
                        "url": "requestUrl",
                        "protocol": "http",
                        "status": 200,
                        "statusText": "OK",
                        "fromCache": false,
                        "headers": [],
                        "mimeType": "text/html",
                        "bytesReceived": 400,
                        "headersSize": 100,
                        "bodySize": 300,
                        "content": {
                          "size": 300 
                        }
                      }
                      """;
        ResponseData? response = JsonSerializer.Deserialize<ResponseData>(json);
        Assert.NotNull(response);

        Assert.Equal("requestUrl", response.Url);
        Assert.Equal("http", response.Protocol);
        Assert.Equal(200u, response.Status);
        Assert.Equal("OK", response.StatusText);
        Assert.False(response.FromCache);
        Assert.Empty(response.Headers);
        Assert.Equal("text/html", response.MimeType);
        Assert.Equal(400u, response.BytesReceived);
        Assert.Equal(100u, response.HeadersSize);
        Assert.Equal(300u, response.BodySize);
        Assert.NotNull(response.Content);
        Assert.Equal(300u, response.Content.Size);
        Assert.Null(response.AuthChallenges);
    }

    [Fact]
    public void TestCanDeserializeResponseDataWithHeaders()
    {
        string json = """
                      {
                        "url": "requestUrl",
                        "protocol": "http",
                        "status": 200,
                        "statusText": "OK",
                        "fromCache": false,
                        "headers": [
                          {
                            "name": "headerName", 
                            "value": { 
                              "type": "string",
                              "value": "headerValue" 
                            }
                          }
                        ],
                        "mimeType": "text/html",
                        "bytesReceived": 400,
                        "headersSize": 100,
                        "bodySize": 300,
                        "content": {
                          "size": 300 
                        }
                      }
                      """;
        ResponseData? response = JsonSerializer.Deserialize<ResponseData>(json);
        Assert.NotNull(response);

        Assert.Equal("requestUrl", response.Url);
        Assert.Equal("http", response.Protocol);
        Assert.Equal(200u, response.Status);
        Assert.Equal("OK", response.StatusText);
        Assert.False(response.FromCache);
        Assert.Single(response.Headers);
        Assert.Equal("headerName", response.Headers[0].Name);
        Assert.Equal(BytesValueType.String, response.Headers[0].Value.Type);
        Assert.Equal("headerValue", response.Headers[0].Value.Value);
        Assert.Equal("text/html", response.MimeType);
        Assert.Equal(400u, response.BytesReceived);
        Assert.Equal(100u, response.HeadersSize);
        Assert.Equal(300u, response.BodySize);
        Assert.NotNull(response.Content);
        Assert.Equal(300u, response.Content.Size);
        Assert.Null(response.AuthChallenges);
    }

    [Fact]
    public void TestCanDeserializeResponseDataWithNullHeadersSize()
    {
        string json = """
                      {
                        "url": "requestUrl",
                        "protocol": "http",
                        "status": 200,
                        "statusText": "OK",
                        "fromCache": false,
                        "headers": [],
                        "mimeType": "text/html",
                        "bytesReceived": 400,
                        "headersSize": null,
                        "bodySize": 300,
                        "content": {
                          "size": 300
                        }
                      }
                      """;
        ResponseData? response = JsonSerializer.Deserialize<ResponseData>(json);
        Assert.NotNull(response);

        Assert.Equal("requestUrl", response.Url);
        Assert.Equal("http", response.Protocol);
        Assert.Equal(200u, response.Status);
        Assert.Equal("OK", response.StatusText);
        Assert.False(response.FromCache);
        Assert.Empty(response.Headers);
        Assert.Equal("text/html", response.MimeType);
        Assert.Equal(400u, response.BytesReceived);
        Assert.Null(response.HeadersSize);
        Assert.Equal(300u, response.BodySize);
        Assert.NotNull(response.Content);
        Assert.Equal(300u, response.Content.Size);
        Assert.Null(response.AuthChallenges);
    }

    [Fact]
    public void TestCanDeserializeResponseDataWithNullBodySize()
    {
        string json = """
                      {
                        "url": "requestUrl",
                        "protocol": "http",
                        "status": 200,
                        "statusText": "OK",
                        "fromCache": false,
                        "headers": [],
                        "mimeType": "text/html",
                        "bytesReceived": 400,
                        "headersSize": 100,
                        "bodySize": null,
                        "content": {
                          "size": 300 
                        }
                      }
                      """;
        ResponseData? response = JsonSerializer.Deserialize<ResponseData>(json);
        Assert.NotNull(response);

        Assert.Equal("requestUrl", response.Url);
        Assert.Equal("http", response.Protocol);
        Assert.Equal(200u, response.Status);
        Assert.Equal("OK", response.StatusText);
        Assert.False(response.FromCache);
        Assert.Empty(response.Headers);
        Assert.Equal("text/html", response.MimeType);
        Assert.Equal(400u, response.BytesReceived);
        Assert.Equal(100u, response.HeadersSize);
        Assert.Null(response.BodySize);
        Assert.NotNull(response.Content);
        Assert.Equal(300u, response.Content.Size);
    }

    [Fact]
    public void TestCanDeserializeResponseDataWithAuthChallenges()
    {
        string json = """
                      {
                        "url": "requestUrl",
                        "protocol": "http",
                        "status": 200,
                        "statusText": "OK",
                        "fromCache": false,
                        "headers": [],
                        "mimeType": "text/html",
                        "bytesReceived": 400,
                        "headersSize": 100,
                        "bodySize": 300,
                        "content": {
                          "size": 300 
                        },
                        "authChallenges": [
                          {
                            "scheme": "basic",
                            "realm": "example.com"
                          }
                        ]
                      }
                      """;
        ResponseData? response = JsonSerializer.Deserialize<ResponseData>(json);
        Assert.NotNull(response);

        Assert.Equal("requestUrl", response.Url);
        Assert.Equal("http", response.Protocol);
        Assert.Equal(200u, response.Status);
        Assert.Equal("OK", response.StatusText);
        Assert.False(response.FromCache);
        Assert.Empty(response.Headers);
        Assert.Equal("text/html", response.MimeType);
        Assert.Equal(400u, response.BytesReceived);
        Assert.Equal(100u, response.HeadersSize);
        Assert.Equal(300u, response.BodySize);
        Assert.NotNull(response.Content);
        Assert.Equal(300u, response.Content.Size);
        Assert.NotNull(response.AuthChallenges);
        Assert.Single(response.AuthChallenges);
        Assert.Equal("basic", response.AuthChallenges[0].Scheme);
        Assert.Equal("example.com", response.AuthChallenges[0].Realm);
    }

    [Fact]
    public void TestCopySemantics()
    {
        string json = """
                      {
                        "url": "requestUrl",
                        "protocol": "http",
                        "status": 200,
                        "statusText": "OK",
                        "fromCache": false,
                        "headers": [],
                        "mimeType": "text/html",
                        "bytesReceived": 400,
                        "headersSize": 100,
                        "bodySize": 300,
                        "content": {
                          "size": 300 
                        }
                      }
                      """;
        ResponseData? response = JsonSerializer.Deserialize<ResponseData>(json);
        Assert.NotNull(response);
        ResponseData copy = response with { };
        Assert.Equal(response, copy);
    }

    [Fact]
    public void TestDeserializeWithMissingUrlThrows()
    {
        string json = """
                      {
                        "protocol": "http",
                        "status": 200,
                        "statusText": "OK",
                        "fromCache": false,
                        "headers": [],
                        "mimeType": "text/html",
                        "bytesReceived": 400,
                        "headersSize": 100,
                        "bodySize": 300,
                        "content": {
                          "size": 300 
                        }
                      }
                      """;
        Assert.Contains("missing required properties including: 'url", Assert.ThrowsAny<JsonException>(() => JsonSerializer.Deserialize<ResponseData>(json)).Message);
    }

    [Fact]
    public void TestDeserializeWithMissingProtocolThrows()
    {
        string json = """
                      {
                        "url": "requestUrl",
                        "status": 200,
                        "statusText": "OK",
                        "fromCache": false,
                        "headers": [],
                        "mimeType": "text/html",
                        "bytesReceived": 400,
                        "headersSize": 100,
                        "bodySize": 300,
                        "content": {
                          "size": 300 
                        }
                      }
                      """;
        Assert.Contains("missing required properties including: 'protocol", Assert.ThrowsAny<JsonException>(() => JsonSerializer.Deserialize<ResponseData>(json)).Message);
    }

    [Fact]
    public void TestDeserializeWithMissingStatusThrows()
    {
        string json = """
                      {
                        "url": "requestUrl",
                        "protocol": "http",
                        "statusText": "OK",
                        "fromCache": false,
                        "headers": [],
                        "mimeType": "text/html",
                        "bytesReceived": 400,
                        "headersSize": 100,
                        "bodySize": 300,
                        "content": {
                          "size": 300 
                        }
                      }
                      """;
        Assert.Contains("missing required properties including: 'status", Assert.ThrowsAny<JsonException>(() => JsonSerializer.Deserialize<ResponseData>(json)).Message);
    }

    [Fact]
    public void TestDeserializeWithMissingStatusTextThrows()
    {
        string json = """
                      {
                        "url": "requestUrl",
                        "protocol": "http",
                        "status": 200,
                        "fromCache": false,
                        "headers": [],
                        "mimeType": "text/html",
                        "bytesReceived": 400,
                        "headersSize": 100,
                        "bodySize": 300,
                        "content": {
                          "size": 300 
                        }
                      }
                      """;
        Assert.Contains("missing required properties including: 'statusText", Assert.ThrowsAny<JsonException>(() => JsonSerializer.Deserialize<ResponseData>(json)).Message);
    }

    [Fact]
    public void TestDeserializeWithMissingFromCacheThrows()
    {
        string json = """
                      {
                        "url": "requestUrl",
                        "protocol": "http",
                        "status": 200,
                        "statusText": "OK",
                        "headers": [],
                        "mimeType": "text/html",
                        "bytesReceived": 400,
                        "headersSize": 100,
                        "bodySize": 300,
                        "content": {
                          "size": 300 
                        }
                      }
                      """;
        Assert.Contains("missing required properties including: 'fromCache", Assert.ThrowsAny<JsonException>(() => JsonSerializer.Deserialize<ResponseData>(json)).Message);
    }

    [Fact]
    public void TestDeserializeWithMissingHeadersThrows()
    {
        string json = """
                      {
                        "url": "requestUrl",
                        "protocol": "http",
                        "status": 200,
                        "statusText": "OK",
                        "fromCache": false,
                        "mimeType": "text/html",
                        "bytesReceived": 400,
                        "headersSize": 100,
                        "bodySize": 300,
                        "content": {
                          "size": 300 
                        }
                      }
                      """;
        Assert.Contains("missing required properties including: 'headers", Assert.ThrowsAny<JsonException>(() => JsonSerializer.Deserialize<ResponseData>(json)).Message);
    }

    [Fact]
    public void TestDeserializeWithMissingMimeTypeThrows()
    {
        string json = """
                      {
                        "url": "requestUrl",
                        "protocol": "http",
                        "status": 200,
                        "statusText": "OK",
                        "fromCache": false,
                        "headers": [],
                        "bytesReceived": 400,
                        "headersSize": 100,
                        "bodySize": 300,
                        "content": {
                          "size": 300 
                        }
                      }
                      """;
        Assert.Contains("missing required properties including: 'mimeType", Assert.ThrowsAny<JsonException>(() => JsonSerializer.Deserialize<ResponseData>(json)).Message);
    }

    [Fact]
    public void TestDeserializeWithMissingBytesReceivedThrows()
    {
        string json = """
                      {
                        "url": "requestUrl",
                        "protocol": "http",
                        "status": 200,
                        "statusText": "OK",
                        "fromCache": false,
                        "headers": [],
                        "mimeType": "text/html",
                        "headersSize": 100,
                        "bodySize": 300,
                        "content": {
                          "size": 300 
                        }
                      }
                      """;
        Assert.Contains("missing required properties including: 'bytesReceived", Assert.ThrowsAny<JsonException>(() => JsonSerializer.Deserialize<ResponseData>(json)).Message);
    }

    [Fact]
    public void TestDeserializeWithMissingHeadersSizeThrows()
    {
        string json = """
                      {
                        "url": "requestUrl",
                        "protocol": "http",
                        "status": 200,
                        "statusText": "OK",
                        "fromCache": false,
                        "headers": [],
                        "mimeType": "text/html",
                        "bytesReceived": 400,
                        "bodySize": 300,
                        "content": {
                          "size": 300 
                        }
                      }
                      """;
        Assert.Contains("missing required properties including: 'headersSize", Assert.ThrowsAny<JsonException>(() => JsonSerializer.Deserialize<ResponseData>(json)).Message);
    }

    [Fact]
    public void TestDeserializeWithMissingBodySizeThrows()
    {
        string json = """
                      {
                        "url": "requestUrl",
                        "protocol": "http",
                        "status": 200,
                        "statusText": "OK",
                        "fromCache": false,
                        "headers": [],
                        "mimeType": "text/html",
                        "bytesReceived": 400,
                        "headersSize": 100,
                        "content": {
                          "size": 300 
                        }
                      }
                      """;
        Assert.Contains("missing required properties including: 'bodySize", Assert.ThrowsAny<JsonException>(() => JsonSerializer.Deserialize<ResponseData>(json)).Message);
    }

    [Fact]
    public void TestDeserializeWithMissingContentThrows()
    {
        string json = """
                      {
                        "url": "requestUrl",
                        "protocol": "http",
                        "status": 200,
                        "statusText": "OK",
                        "fromCache": false,
                        "headers": [],
                        "mimeType": "text/html",
                        "bytesReceived": 400,
                        "headersSize": 100,
                        "bodySize": 300
                      }
                      """;
        Assert.Contains("missing required properties including: 'content", Assert.ThrowsAny<JsonException>(() => JsonSerializer.Deserialize<ResponseData>(json)).Message);
    }

    [Fact]
    public void TestDeserializeWithInvalidAuthChallengesTypeThrows()
    {
        string json = """
                      {
                        "url": "requestUrl",
                        "protocol": "http",
                        "status": 200,
                        "statusText": "OK",
                        "fromCache": false,
                        "headers": [],
                        "mimeType": "text/html",
                        "bytesReceived": 400,
                        "headersSize": 100,
                        "bodySize": 300,
                        "content": {
                          "size": 300 
                        },
                        "authChallenges": {
                          "scheme": "basic",
                          "realm": "example.com"
                        }
                      }
                      """;
        JsonException exception = Assert.ThrowsAny<JsonException>(() => JsonSerializer.Deserialize<ResponseData>(json));
        Assert.Contains("JSON value could not be converted", exception.Message);
        Assert.Contains("authChallenges", exception.Message);
    }

    [Fact]
    public void TestDeserializeWithInvalidHeadersTypeThrows()
    {
        string json = """
                      {
                        "url": "requestUrl",
                        "protocol": "http",
                        "status": 200,
                        "statusText": "OK",
                        "fromCache": false,
                        "headers": {},
                        "mimeType": "text/html",
                        "bytesReceived": 400,
                        "headersSize": 100,
                        "bodySize": 300,
                        "content": {
                          "size": 300 
                        }
                      }
                      """;
        JsonException exception = Assert.ThrowsAny<JsonException>(() => JsonSerializer.Deserialize<ResponseData>(json));
        Assert.Contains("JSON value could not be converted", exception.Message);
        Assert.Contains("headers", exception.Message);
    }
}
