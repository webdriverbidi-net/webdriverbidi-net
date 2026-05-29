namespace WebDriverBiDi.Protocol;

using System.Buffers;

public class TestIncomingMessage : IncomingMessage
{
    private readonly bool shouldThrowOnDeserialization;

    public TestIncomingMessage(IMemoryOwner<byte> owner, int length, bool shouldThrowOnDeserialization)
        : base(owner, length)
    {
        this.shouldThrowOnDeserialization = shouldThrowOnDeserialization;
    }

    public override void Parse()
    {
        if (this.shouldThrowOnDeserialization)
        {
            throw new InvalidOperationException("Simulated deserialization failure");
        }

        base.Parse();
    }
}
