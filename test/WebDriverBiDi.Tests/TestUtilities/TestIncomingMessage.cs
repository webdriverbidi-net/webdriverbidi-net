namespace WebDriverBiDi.TestUtilities;

using System.Buffers;
using System.Text.Json;
using WebDriverBiDi.Protocol;

public class TestIncomingMessage : IncomingMessage
{
    private readonly bool shouldThrowOnDeserialization;
    private readonly TaskCompletionSource? parseTaskCompletionSource;

    public TestIncomingMessage(IMemoryOwner<byte> owner, int length, bool shouldThrowOnDeserialization, Func<JsonDocument, JsonDocument?>? documentTransformer = null, TaskCompletionSource? parseTaskCompletsionSource = null)
        : base(owner, length, documentTransformer)
    {
        this.shouldThrowOnDeserialization = shouldThrowOnDeserialization;
        this.parseTaskCompletionSource = parseTaskCompletsionSource;
    }

    public override void Parse()
    {
        if (this.shouldThrowOnDeserialization)
        {
            throw new InvalidOperationException("Simulated deserialization failure");
        }

        base.Parse();
        this.parseTaskCompletionSource?.TrySetResult();
    }
}
