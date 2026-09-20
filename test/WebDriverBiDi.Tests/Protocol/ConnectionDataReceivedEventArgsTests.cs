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

    [Fact]
    public void TestToStringAfterTheBufferIsDisposedDoesNotThrow()
    {
        // The consumer of the event disposes the pooled buffer once it has processed the message, after which
        // reading Data throws. The printed form does not read it, so the event arguments still print.
        IMemoryOwner<byte> owner = MemoryPool<byte>.Shared.Rent(5);
        ConnectionDataReceivedEventArgs eventArgs = new(owner, 5);
        owner.Dispose();
        Assert.Throws<ObjectDisposedException>(() => eventArgs.Data);

        string printed = eventArgs.ToString();

        Assert.StartsWith("ConnectionDataReceivedEventArgs { AdditionalData = ", printed);
        Assert.EndsWith(", DataLength = 5 }", printed);
        Assert.DoesNotContain("Data = ", printed.Replace("AdditionalData = ", string.Empty).Replace("DataLength = ", string.Empty));
        Assert.DoesNotContain("BufferOwner", printed);
    }
}
