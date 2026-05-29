namespace WebDriverBiDi.Protocol;

public class TestIncomingMessage : IncomingMessage
{
    private readonly bool shouldThrowOnDeserialization;

    public TestIncomingMessage(ReadOnlyMemory<byte> data, int length, bool shouldThrowOnDeserialization)
        : base(data, length)
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