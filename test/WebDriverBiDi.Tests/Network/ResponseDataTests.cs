namespace WebDriverBiDi.Network;

using System.Text.Json;
using WebDriverBiDi.JsonConverters;

[TestFixture]
public class ResponseDataTests
{
    private JsonSerializerOptions deserializationOptions = new()
    {
        TypeInfoResolver = new PrivateConstructorContractResolver(),
    };

    [Test]
    public void TestCanDeserializeResponseData()
    {
        string json = @"{ ""url"": ""requestUrl"", ""protocol"": ""http"", ""status"": 200, ""statusText"": ""OK"", ""fromCache"": false, ""headers"": [], ""mimeType"": ""text/html"", ""bytesReceived"": 400, ""headersSize"": 100, ""bodySize"": 300, ""content"": { ""size"": 300 } }";
        ResponseData? response = JsonSerializer.Deserialize<ResponseData>(json, deserializationOptions);
        Assert.That(response, Is.Not.Null);
        Assert.Multiple(() =>
        {
            Assert.That(response!.Url, Is.EqualTo("requestUrl"));
            Assert.That(response.Protocol, Is.EqualTo("http"));
            Assert.That(response.Status, Is.EqualTo(200));
            Assert.That(response.StatusText, Is.EqualTo("OK"));
            Assert.That(response.FromCache, Is.False);
            Assert.That(response.Headers, Is.Empty);
            Assert.That(response.MimeType, Is.EqualTo("text/html"));
            Assert.That(response.BytesReceived, Is.EqualTo(400));
            Assert.That(response.HeadersSize, Is.EqualTo(100));
            Assert.That(response.BodySize, Is.EqualTo(300));
            Assert.That(response.Content, Is.Not.Null);
            Assert.That(response.Content.Size, Is.EqualTo(300));
            Assert.That(response.AuthChallenge, Is.Null);
        });
    }

    [Test]
    public void TestCanDeserializeResponseDataWithHeaders()
    {
        string json = @"{ ""url"": ""requestUrl"", ""protocol"": ""http"", ""status"": 200, ""statusText"": ""OK"", ""fromCache"": false, ""headers"": [{ ""name"": ""headerName"", ""value"": { ""type"": ""string"", ""value"": ""headerValue"" } }], ""mimeType"": ""text/html"", ""bytesReceived"": 400, ""headersSize"": 100, ""bodySize"": 300, ""content"": { ""size"": 300 } }";
        ResponseData? response = JsonSerializer.Deserialize<ResponseData>(json, deserializationOptions);
        Assert.That(response, Is.Not.Null);
        Assert.Multiple(() =>
        {
            Assert.That(response!.Url, Is.EqualTo("requestUrl"));
            Assert.That(response.Protocol, Is.EqualTo("http"));
            Assert.That(response.Status, Is.EqualTo(200));
            Assert.That(response.StatusText, Is.EqualTo("OK"));
            Assert.That(response.FromCache, Is.False);
            Assert.That(response.Headers, Has.Count.EqualTo(1));
            Assert.That(response.Headers[0].Name, Is.EqualTo("headerName"));
            Assert.That(response.Headers[0].Value.Type, Is.EqualTo(BytesValueType.String));
            Assert.That(response.Headers[0].Value.Value, Is.EqualTo("headerValue"));
            Assert.That(response.MimeType, Is.EqualTo("text/html"));
            Assert.That(response.BytesReceived, Is.EqualTo(400));
            Assert.That(response.HeadersSize, Is.EqualTo(100));
            Assert.That(response.BodySize, Is.EqualTo(300));
            Assert.That(response.Content, Is.Not.Null);
            Assert.That(response.Content.Size, Is.EqualTo(300));
            Assert.That(response.AuthChallenge, Is.Null);
        });       
    }

    [Test]
    public void TestCanDeserializeResponseDataWithNullHeadersSize()
    {
        string json = @"{ ""url"": ""requestUrl"", ""protocol"": ""http"", ""status"": 200, ""statusText"": ""OK"", ""fromCache"": false, ""headers"": [], ""mimeType"": ""text/html"", ""bytesReceived"": 400, ""headersSize"": null, ""bodySize"": 300, ""content"": { ""size"": 300 } }";
        ResponseData? response = JsonSerializer.Deserialize<ResponseData>(json, deserializationOptions);
        Assert.That(response, Is.Not.Null);
        Assert.Multiple(() =>
        {
            Assert.That(response!.Url, Is.EqualTo("requestUrl"));
            Assert.That(response.Protocol, Is.EqualTo("http"));
            Assert.That(response.Status, Is.EqualTo(200));
            Assert.That(response.StatusText, Is.EqualTo("OK"));
            Assert.That(response.FromCache, Is.False);
            Assert.That(response.Headers, Is.Empty);
            Assert.That(response.MimeType, Is.EqualTo("text/html"));
            Assert.That(response.BytesReceived, Is.EqualTo(400));
            Assert.That(response.HeadersSize, Is.Null);
            Assert.That(response.BodySize, Is.EqualTo(300));
            Assert.That(response.Content, Is.Not.Null);
            Assert.That(response.Content.Size, Is.EqualTo(300));
            Assert.That(response.AuthChallenge, Is.Null);
        });       
    }

    [Test]
    public void TestCanDeserializeResponseDataWithNullBodySize()
    {
        string json = @"{ ""url"": ""requestUrl"", ""protocol"": ""http"", ""status"": 200, ""statusText"": ""OK"", ""fromCache"": false, ""headers"": [], ""mimeType"": ""text/html"", ""bytesReceived"": 400, ""headersSize"": 100, ""bodySize"": null, ""content"": { ""size"": 300 } }";
        ResponseData? response = JsonSerializer.Deserialize<ResponseData>(json, deserializationOptions);
        Assert.That(response, Is.Not.Null);
        Assert.Multiple(() =>
        {
            Assert.That(response!.Url, Is.EqualTo("requestUrl"));
            Assert.That(response.Protocol, Is.EqualTo("http"));
            Assert.That(response.Status, Is.EqualTo(200));
            Assert.That(response.StatusText, Is.EqualTo("OK"));
            Assert.That(response.FromCache, Is.False);
            Assert.That(response.Headers, Is.Empty);
            Assert.That(response.MimeType, Is.EqualTo("text/html"));
            Assert.That(response.BytesReceived, Is.EqualTo(400));
            Assert.That(response.HeadersSize, Is.EqualTo(100));
            Assert.That(response.BodySize, Is.Null);
            Assert.That(response.Content, Is.Not.Null);
            Assert.That(response.Content.Size, Is.EqualTo(300));
        });       
    }

    [Test]
    public void TestCanDeserializeResponseDataWithAuthChallenge()
    {
        string json = @"{ ""url"": ""requestUrl"", ""protocol"": ""http"", ""status"": 200, ""statusText"": ""OK"", ""fromCache"": false, ""headers"": [], ""mimeType"": ""text/html"", ""bytesReceived"": 400, ""headersSize"": 100, ""bodySize"": 300, ""content"": { ""size"": 300 }, ""authChallenge"": { ""scheme"": ""basic"", ""realm"": ""example.com"" } }";
        ResponseData? response = JsonSerializer.Deserialize<ResponseData>(json, deserializationOptions);
        Assert.That(response, Is.Not.Null);
        Assert.Multiple(() =>
        {
            Assert.That(response!.Url, Is.EqualTo("requestUrl"));
            Assert.That(response.Protocol, Is.EqualTo("http"));
            Assert.That(response.Status, Is.EqualTo(200));
            Assert.That(response.StatusText, Is.EqualTo("OK"));
            Assert.That(response.FromCache, Is.False);
            Assert.That(response.Headers, Is.Empty);
            Assert.That(response.MimeType, Is.EqualTo("text/html"));
            Assert.That(response.BytesReceived, Is.EqualTo(400));
            Assert.That(response.HeadersSize, Is.EqualTo(100));
            Assert.That(response.BodySize, Is.EqualTo(300));
            Assert.That(response.Content, Is.Not.Null);
            Assert.That(response.Content.Size, Is.EqualTo(300));
            Assert.That(response.AuthChallenge, Is.Not.Null);
            Assert.That(response.AuthChallenge!.Scheme, Is.EqualTo("basic"));
            Assert.That(response.AuthChallenge.Realm, Is.EqualTo("example.com"));
        });
    }

    [Test]
    public void TestDeserializeWithMissingUrlThrows()
    {
        string json = @"{ ""protocol"": ""http"", ""status"": 200, ""statusText"": ""OK"", ""fromCache"": false, ""headers"": [], ""mimeType"": ""text/html"", ""bytesReceived"": 400, ""headersSize"": 100, ""bodySize"": 300, ""content"": { ""size"": 300 } }";
        Assert.That(() => JsonSerializer.Deserialize<ResponseData>(json, deserializationOptions), Throws.InstanceOf<JsonException>().With.Message.Contains("missing required properties, including the following: url"));
    }

    [Test]
    public void TestDeserializeWithMissingProtocolThrows()
    {
        string json = @"{ ""url"": ""requestUrl"", ""status"": 200, ""statusText"": ""OK"", ""fromCache"": false, ""headers"": [], ""mimeType"": ""text/html"", ""bytesReceived"": 400, ""headersSize"": 100, ""bodySize"": 300, ""content"": { ""size"": 300 } }";
        Assert.That(() => JsonSerializer.Deserialize<ResponseData>(json, deserializationOptions), Throws.InstanceOf<JsonException>().With.Message.Contains("missing required properties, including the following: protocol"));
    }

    [Test]
    public void TestDeserializeWithMissingStatusThrows()
    {
        string json = @"{ ""url"": ""requestUrl"", ""protocol"": ""http"", ""statusText"": ""OK"", ""fromCache"": false, ""headers"": [], ""mimeType"": ""text/html"", ""bytesReceived"": 400, ""headersSize"": 100, ""bodySize"": 300, ""content"": { ""size"": 300 } }";
        Assert.That(() => JsonSerializer.Deserialize<ResponseData>(json, deserializationOptions), Throws.InstanceOf<JsonException>().With.Message.Contains("missing required properties, including the following: status"));
    }

    [Test]
    public void TestDeserializeWithMissingStatusTextThrows()
    {
        string json = @"{ ""url"": ""requestUrl"", ""protocol"": ""http"", ""status"": 200, ""fromCache"": false, ""headers"": [], ""mimeType"": ""text/html"", ""bytesReceived"": 400, ""headersSize"": 100, ""bodySize"": 300, ""content"": { ""size"": 300 } }";
        Assert.That(() => JsonSerializer.Deserialize<ResponseData>(json, deserializationOptions), Throws.InstanceOf<JsonException>().With.Message.Contains("missing required properties, including the following: statusText"));
    }

    [Test]
    public void TestDeserializeWithMissingFromCacheThrows()
    {
        string json = @"{ ""url"": ""requestUrl"", ""protocol"": ""http"", ""status"": 200, ""statusText"": ""OK"", ""headers"": [], ""mimeType"": ""text/html"", ""bytesReceived"": 400, ""headersSize"": 100, ""bodySize"": 300, ""content"": { ""size"": 300 } }";
        Assert.That(() => JsonSerializer.Deserialize<ResponseData>(json, deserializationOptions), Throws.InstanceOf<JsonException>().With.Message.Contains("missing required properties, including the following: fromCache"));
    }

    [Test]
    public void TestDeserializeWithMissingHeadersThrows()
    {
        string json = @"{ ""url"": ""requestUrl"", ""protocol"": ""http"", ""status"": 200, ""statusText"": ""OK"", ""fromCache"": false, ""mimeType"": ""text/html"",""bytesReceived"": 400,  ""headersSize"": 100, ""bodySize"": 300, ""content"": { ""size"": 300 } }";
        Assert.That(() => JsonSerializer.Deserialize<ResponseData>(json, deserializationOptions), Throws.InstanceOf<JsonException>().With.Message.Contains("missing required properties, including the following: headers"));
    }

    [Test]
    public void TestDeserializeWithMissingMimeTypeThrows()
    {
        string json = @"{ ""url"": ""requestUrl"", ""protocol"": ""http"", ""status"": 200, ""statusText"": ""OK"", ""fromCache"": false, ""headers"": [], ""bytesReceived"": 400, ""headersSize"": 100, ""bodySize"": 300, ""content"": { ""size"": 300 } }";
        Assert.That(() => JsonSerializer.Deserialize<ResponseData>(json, deserializationOptions), Throws.InstanceOf<JsonException>().With.Message.Contains("missing required properties, including the following: mimeType"));
    }

    [Test]
    public void TestDeserializeWithMissingBytesReceivedThrows()
    {
        string json = @"{ ""url"": ""requestUrl"", ""protocol"": ""http"", ""status"": 200, ""statusText"": ""OK"", ""fromCache"": false, ""headers"": [], ""mimeType"": ""text/html"", ""headersSize"": 100, ""bodySize"": 300, ""content"": { ""size"": 300 } }";
        Assert.That(() => JsonSerializer.Deserialize<ResponseData>(json, deserializationOptions), Throws.InstanceOf<JsonException>().With.Message.Contains("missing required properties, including the following: bytesReceived"));
    }

    [Test]
    public void TestDeserializeWithMissingHeadersSizeThrows()
    {
        string json = @"{ ""url"": ""requestUrl"", ""protocol"": ""http"", ""status"": 200, ""statusText"": ""OK"", ""fromCache"": false, ""headers"": [], ""mimeType"": ""text/html"", ""bytesReceived"": 400, ""bodySize"": 300, ""content"": { ""size"": 300 } }";
        Assert.That(() => JsonSerializer.Deserialize<ResponseData>(json, deserializationOptions), Throws.InstanceOf<JsonException>().With.Message.Contains("missing required properties, including the following: headersSize"));
    }

    [Test]
    public void TestDeserializeWithMissingBodySizeThrows()
    {
        string json = @"{ ""url"": ""requestUrl"", ""protocol"": ""http"", ""status"": 200, ""statusText"": ""OK"", ""fromCache"": false, ""headers"": [], ""mimeType"": ""text/html"", ""bytesReceived"": 400, ""headersSize"": 100, ""content"": { ""size"": 300 } }";
        Assert.That(() => JsonSerializer.Deserialize<ResponseData>(json, deserializationOptions), Throws.InstanceOf<JsonException>().With.Message.Contains("missing required properties, including the following: bodySize"));
    }

    [Test]
    public void TestDeserializeWithMissingContentThrows()
    {
        string json = @"{ ""url"": ""requestUrl"", ""protocol"": ""http"", ""status"": 200, ""statusText"": ""OK"", ""fromCache"": false, ""headers"": [], ""mimeType"": ""text/html"", ""bytesReceived"": 400, ""headersSize"": 100, ""bodySize"": 300 }";
        Assert.That(() => JsonSerializer.Deserialize<ResponseData>(json, deserializationOptions), Throws.InstanceOf<JsonException>().With.Message.Contains("missing required properties, including the following: content"));
    }
}
