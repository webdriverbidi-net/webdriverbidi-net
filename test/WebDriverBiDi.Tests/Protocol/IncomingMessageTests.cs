namespace WebDriverBiDi.Protocol;

using System.Buffers;
using System.Text;
using System.Text.Json;

public class IncomingMessageTests
{
    [Fact]
    public async Task TestCanCreate()
    {
        byte[] bytes = Encoding.UTF8.GetBytes("Hello, World!");
        IMemoryOwner<byte> owner = MemoryPool<byte>.Shared.Rent(bytes.Length);
        bytes.CopyTo(owner.Memory);
        using IncomingMessage message = new(owner, bytes.Length);
        Assert.Equal("Hello, World!", Encoding.UTF8.GetString(message.MessageData.Span));
        Assert.Equal(bytes.Length, message.MessageLength);
        Assert.Equal(IncomingMessageKind.Uninitialized, message.MessageKind);
        Assert.Equal("Hello, World!", message.MessageText);
    }

    [Fact]
    public async Task TestCanParseValidJson()
    {
        byte[] bytes = Encoding.UTF8.GetBytes("{}");
        IMemoryOwner<byte> owner = MemoryPool<byte>.Shared.Rent(bytes.Length);
        bytes.CopyTo(owner.Memory);
        using IncomingMessage message = new(owner, bytes.Length);
        message.Parse();

        Assert.Equal(bytes.Length, message.MessageLength);
        Assert.Equal("{}", message.MessageText);
        Assert.Equal(IncomingMessageKind.Unknown, message.MessageKind);
    }

    [Fact]
    public async Task TestCanParseValidNonObjectJson()
    {
        byte[] bytes = Encoding.UTF8.GetBytes("[1,2]");
        IMemoryOwner<byte> owner = MemoryPool<byte>.Shared.Rent(bytes.Length);
        bytes.CopyTo(owner.Memory);
        using IncomingMessage message = new(owner, bytes.Length);
        message.Parse();

        Assert.Equal(bytes.Length, message.MessageLength);
        Assert.Equal("[1,2]", message.MessageText);
        Assert.Equal(IncomingMessageKind.Unknown, message.MessageKind);
    }

    [Fact]
    public async Task TestCanRepeatParseWithoutThrowing()
    {
        byte[] bytes = Encoding.UTF8.GetBytes("{}");
        IMemoryOwner<byte> owner = MemoryPool<byte>.Shared.Rent(bytes.Length);
        bytes.CopyTo(owner.Memory);
        using IncomingMessage message = new(owner, bytes.Length);
        message.Parse();
        message.Parse();
    }

    [Fact]
    public async Task TestParsingInvalidJsonThrows()
    {
        byte[] bytes = Encoding.UTF8.GetBytes("{ foo }");
        IMemoryOwner<byte> owner = MemoryPool<byte>.Shared.Rent(bytes.Length);
        bytes.CopyTo(owner.Memory);
        using IncomingMessage message = new(owner, bytes.Length);
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
        byte[] bytes = Encoding.UTF8.GetBytes(json);
        IMemoryOwner<byte> owner = MemoryPool<byte>.Shared.Rent(bytes.Length);
        bytes.CopyTo(owner.Memory);
        using IncomingMessage message = new(owner, bytes.Length, Transformer);
        message.Parse();
        Assert.Equal(IncomingMessageKind.Event, message.MessageKind);
    }

    [Fact]
    public async Task TestCanParseWithDocumentTransformerReturningSameDocument()
    {
        // A pass-through transformer returns the document it was handed. The message must keep
        // using that document (not dispose it), so the message kind and payload remain readable
        // and a second Parse is a no-op.
        string json = """{ "type": "event", "method": "protocol.event", "params": { "paramName": "paramValue" } }""";
        int transformerCallCount = 0;
        JsonDocument Transformer(JsonDocument doc)
        {
            transformerCallCount++;
            return doc;
        }
        byte[] bytes = Encoding.UTF8.GetBytes(json);
        IMemoryOwner<byte> owner = MemoryPool<byte>.Shared.Rent(bytes.Length);
        bytes.CopyTo(owner.Memory);
        using IncomingMessage message = new(owner, bytes.Length, Transformer);
        message.Parse();
        Assert.Equal(IncomingMessageKind.Event, message.MessageKind);
        Assert.Equal(json, message.MessageText);
        message.Parse();
        Assert.Equal(IncomingMessageKind.Event, message.MessageKind);
        Assert.Equal(1, transformerCallCount);
    }

    [Fact]
    public async Task TestParseWithDocumentTransformerReturningNewDocumentDisposesOriginal()
    {
        // When the transformer returns a different document, the original parsed document is
        // no longer referenced by the message and must be disposed to release its pooled buffer.
        string json = """{ "method": "CDP.someEvent", "params": { "payload": "{ \"type\": \"event\", \"method\": \"protocol.event\", \"params\": {} }" } }""";
        JsonDocument? originalDocument = null;
        JsonDocument Transformer(JsonDocument doc)
        {
            originalDocument = doc;
            return JsonDocument.Parse(doc.RootElement.GetProperty("params").GetProperty("payload").GetString()!);
        }
        byte[] bytes = Encoding.UTF8.GetBytes(json);
        IMemoryOwner<byte> owner = MemoryPool<byte>.Shared.Rent(bytes.Length);
        bytes.CopyTo(owner.Memory);
        using IncomingMessage message = new(owner, bytes.Length, Transformer);
        message.Parse();
        Assert.Equal(IncomingMessageKind.Event, message.MessageKind);
        Assert.NotNull(originalDocument);
        Assert.Throws<ObjectDisposedException>(() => originalDocument.RootElement.GetProperty("method"));
    }

    [Fact]
    public async Task TestParseWithDocumentTransformerReturningNullSetsFilteredKind()
    {
        string json = """{ "method": "CDP.someEvent", "params": {} }""";
        static JsonDocument? Transformer(JsonDocument doc) => null;
        byte[] bytes = Encoding.UTF8.GetBytes(json);
        IMemoryOwner<byte> owner = MemoryPool<byte>.Shared.Rent(bytes.Length);
        bytes.CopyTo(owner.Memory);
        using IncomingMessage message = new(owner, bytes.Length, Transformer);
        message.Parse();
        Assert.Equal(IncomingMessageKind.Filtered, message.MessageKind);
    }

    [Fact]
    public async Task TestParseIsNoOpWhenAlreadyFiltered()
    {
        string json = """{ "method": "CDP.someEvent", "params": {} }""";
        int transformerCallCount = 0;
        JsonDocument? Transformer(JsonDocument doc)
        {
            transformerCallCount++;
            return null;
        }
        byte[] bytes = Encoding.UTF8.GetBytes(json);
        IMemoryOwner<byte> owner = MemoryPool<byte>.Shared.Rent(bytes.Length);
        bytes.CopyTo(owner.Memory);
        using IncomingMessage message = new(owner, bytes.Length, Transformer);
        message.Parse();
        message.Parse();
        Assert.Equal(1, transformerCallCount);
        Assert.Equal(IncomingMessageKind.Filtered, message.MessageKind);
    }

    [Fact]
    public async Task TestCanDispose()
    {
        byte[] bytes = Encoding.UTF8.GetBytes("{}");
        IMemoryOwner<byte> owner = MemoryPool<byte>.Shared.Rent(bytes.Length);
        bytes.CopyTo(owner.Memory);
        IncomingMessage message = new(owner, bytes.Length);
        message.Dispose();
    }

    [Fact]
    public async Task TestCanDisposeAfterParsing()
    {
        byte[] bytes = Encoding.UTF8.GetBytes("{}");
        IMemoryOwner<byte> owner = MemoryPool<byte>.Shared.Rent(bytes.Length);
        bytes.CopyTo(owner.Memory);
        IncomingMessage message = new(owner, bytes.Length);
        message.Parse();
        message.Dispose();
    }

    [Fact]
    public async Task TestCanDisposeRepeatedly()
    {
        byte[] bytes = Encoding.UTF8.GetBytes("{}");
        IMemoryOwner<byte> owner = MemoryPool<byte>.Shared.Rent(bytes.Length);
        bytes.CopyTo(owner.Memory);
        IncomingMessage message = new(owner, bytes.Length);
        message.Parse();
        message.Dispose();
        message.Dispose();
    }
}
