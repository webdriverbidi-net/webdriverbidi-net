namespace WebDriverBiDi.Protocol;

using System.Text;
using System.Text.Json;

public class IncomingMessageTests
{
    [Fact]
    public async Task TestCanCreate()
    {
        byte[] messageData = Encoding.UTF8.GetBytes("Hello, World!");
        using IncomingMessage message = new(messageData, messageData.Length);
        Assert.Equal("Hello, World!", Encoding.UTF8.GetString(message.MessageData.Span));
        Assert.Equal(messageData.Length, message.MessageLength);
        Assert.Equal(IncomingMessageKind.Uninitialized, message.MessageKind);
        Assert.Equal("Hello, World!", message.MessageText);
    }

    [Fact]
    public async Task TestCanParseValidJson()
    {
        string json = "{}";
        byte[] messageData = Encoding.UTF8.GetBytes(json);
        using IncomingMessage message = new(messageData, messageData.Length);
        message.Parse();
    }

    [Fact]
    public async Task TestCanRepeatParseWithoutThrowing()
    {
        string json = "{}";
        byte[] messageData = Encoding.UTF8.GetBytes(json);
        using IncomingMessage message = new(messageData, messageData.Length);
        message.Parse();
        message.Parse();
    }

    [Fact]
    public async Task TestParsingInvalidJsonThrows()
    {
        string json = "{ foo }";
        byte[] messageData = Encoding.UTF8.GetBytes(json);
        using IncomingMessage message = new(messageData, messageData.Length);
        Assert.ThrowsAny<JsonException>(() => message.Parse());
    }

    [Fact]
    public async Task TestCanParseWithDocumentTransformer()
    {
        string json =
            """
            {
              "method": "Runtime.bindingCalled",
              "params": {
                "name": "sendBidiResponse",
                "payload": "{ \"type\": \"event\", \"method\": \"protocol.event\", \"params\": { \"paramName\": \"paramValue\" } }"
              }
            }
            """;
        static JsonDocument Transformer(JsonDocument doc)
        {
            JsonElement deserialized = doc.RootElement;
            deserialized.TryGetProperty("method", out JsonElement methodNameElement);
            string methodName = methodNameElement.GetString()!;

            deserialized.TryGetProperty("params", out JsonElement valueElement);

            JsonElement bindingNameElement = valueElement.GetProperty("name");
            string bindingName = bindingNameElement.GetString()!;

            JsonElement payloadElement = valueElement.GetProperty("payload");
            string payload = payloadElement.GetString()!;
            return JsonDocument.Parse(payload);
        }
        byte[] messageData = Encoding.UTF8.GetBytes(json);
        using IncomingMessage message = new(messageData, messageData.Length, Transformer);
        message.Parse();
        Assert.Equal(IncomingMessageKind.Event, message.MessageKind);
    }

    [Fact]
    public async Task TestCanDispose()
    {
        string json = "{}";
        byte[] messageData = Encoding.UTF8.GetBytes(json);
        IncomingMessage message = new(messageData, messageData.Length);
        message.Dispose();
    }

    [Fact]
    public async Task TestCanDisposeAfterParsing()
    {
        string json = "{}";
        byte[] messageData = Encoding.UTF8.GetBytes(json);
        IncomingMessage message = new(messageData, messageData.Length);
        message.Parse();
        message.Dispose();
    }

    [Fact]
    public async Task TestCanDisposeRepeatedly()
    {
        string json = "{}";
        byte[] messageData = Encoding.UTF8.GetBytes(json);
        IncomingMessage message = new(messageData, messageData.Length);
        message.Parse();
        message.Dispose();
        message.Dispose();
    }
}
