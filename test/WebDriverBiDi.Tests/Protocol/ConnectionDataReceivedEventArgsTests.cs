namespace WebDriverBiDi.Protocol;

using System.Buffers;

public class ConnectionDataReceivedEventArgsTests
{
    [Fact]
    public void TestCanCreateConnectionDataReceivedEventArgs()
    {
        byte[] data = new byte[] { 1 };
        IMemoryOwner<byte> owner = MemoryPool<byte>.Shared.Rent(data.Length);
        data.CopyTo(owner.Memory);
        ConnectionDataReceivedEventArgs eventArgs = new(owner, data.Length);
        Assert.Equal(1, eventArgs.Data.Length);
        Assert.Equal(1, eventArgs.Data.Span[0]);
        Assert.Empty(eventArgs.AdditionalData);
    }

    [Fact]
    public void TestCopySemantics()
    {
        byte[] data = new byte[] { 1 };
        IMemoryOwner<byte> owner = MemoryPool<byte>.Shared.Rent(data.Length);
        data.CopyTo(owner.Memory);
        ConnectionDataReceivedEventArgs eventArgs = new(owner, data.Length);
        ConnectionDataReceivedEventArgs copy = eventArgs with { };
        Assert.Equal(eventArgs, copy);
    }
}
