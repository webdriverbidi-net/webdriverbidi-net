namespace WebDriverBiDi.Protocol;

using System.Buffers;

public class MessageBufferTests
{
    [Fact]
    public void TestNewMessageBufferIsEmpty()
    {
        using MessageBuffer messageBuffer = new();
        Assert.False(messageBuffer.HasData);
        Assert.Equal(0, messageBuffer.Length);
    }

    [Fact]
    public void TestAppendAccumulatesFragmentsInOrder()
    {
        using MessageBuffer messageBuffer = new();
        messageBuffer.Append("Hello"u8);
        messageBuffer.Append(", "u8);
        messageBuffer.Append("World!"u8);

        Assert.True(messageBuffer.HasData);
        Assert.Equal(13, messageBuffer.Length);

        using IMemoryOwner<byte> owner = messageBuffer.TakeOwnership(out int length);
        Assert.Equal(13, length);
        Assert.True(owner.Memory.Length >= length);
        Assert.Equal("Hello, World!", System.Text.Encoding.UTF8.GetString(owner.Memory.Span.Slice(0, length)));
    }

    [Fact]
    public void TestAppendGrowsPooledBufferPreservingContent()
    {
        // The first rent is sized to the first fragment; a much larger second fragment forces
        // the accumulator to rent a bigger block and copy the existing content across.
        byte[] first = new byte[100];
        byte[] second = new byte[200_000];
        for (int i = 0; i < first.Length; i++)
        {
            first[i] = (byte)(i % 251);
        }

        for (int i = 0; i < second.Length; i++)
        {
            second[i] = (byte)((i * 7) % 253);
        }

        using MessageBuffer messageBuffer = new();
        messageBuffer.Append(first);
        messageBuffer.Append(second);

        using IMemoryOwner<byte> owner = messageBuffer.TakeOwnership(out int length);
        Assert.Equal(first.Length + second.Length, length);
        Assert.True(owner.Memory.Length >= length);
        Assert.True(owner.Memory.Span.Slice(0, first.Length).SequenceEqual(first));
        Assert.True(owner.Memory.Span.Slice(first.Length, second.Length).SequenceEqual(second));
    }

    [Fact]
    public void TestAppendGrowsGeometricallyAcrossManySmallFragments()
    {
        // Many fragments that together exceed the initial rent several times over exercise
        // repeated growth; every byte must survive each copy.
        byte[] expected = new byte[50_000];
        for (int i = 0; i < expected.Length; i++)
        {
            expected[i] = (byte)(i % 256);
        }

        using MessageBuffer messageBuffer = new();
        for (int offset = 0; offset < expected.Length; offset += 17)
        {
            int count = Math.Min(17, expected.Length - offset);
            messageBuffer.Append(expected.AsSpan(offset, count));
        }

        using IMemoryOwner<byte> owner = messageBuffer.TakeOwnership(out int length);
        Assert.Equal(expected.Length, length);
        Assert.True(owner.Memory.Span.Slice(0, length).SequenceEqual(expected));
    }

    [Fact]
    public void TestAppendIgnoresEmptyFragments()
    {
        using MessageBuffer messageBuffer = new();
        messageBuffer.Append(ReadOnlySpan<byte>.Empty);
        Assert.False(messageBuffer.HasData);
        Assert.Equal(0, messageBuffer.Length);

        messageBuffer.Append("data"u8);
        messageBuffer.Append(ReadOnlySpan<byte>.Empty);
        Assert.Equal(4, messageBuffer.Length);
    }

    [Fact]
    public void TestTakeOwnershipWithoutDataThrows()
    {
        using MessageBuffer messageBuffer = new();
        Assert.Throws<InvalidOperationException>(() => messageBuffer.TakeOwnership(out _));
    }

    [Fact]
    public void TestTakeOwnershipResetsAccumulatorForReuse()
    {
        using MessageBuffer messageBuffer = new();
        messageBuffer.Append("first"u8);
        using IMemoryOwner<byte> firstOwner = messageBuffer.TakeOwnership(out int firstLength);

        Assert.False(messageBuffer.HasData);
        Assert.Equal(0, messageBuffer.Length);

        messageBuffer.Append("second message"u8);
        using IMemoryOwner<byte> secondOwner = messageBuffer.TakeOwnership(out int secondLength);

        Assert.NotSame(firstOwner, secondOwner);
        Assert.Equal("first", System.Text.Encoding.UTF8.GetString(firstOwner.Memory.Span.Slice(0, firstLength)));
        Assert.Equal("second message", System.Text.Encoding.UTF8.GetString(secondOwner.Memory.Span.Slice(0, secondLength)));
    }

    [Fact]
    public void TestDiscardDropsPartialDataAndAllowsReuse()
    {
        using MessageBuffer messageBuffer = new();
        messageBuffer.Append("partial"u8);
        messageBuffer.Discard();

        Assert.False(messageBuffer.HasData);
        Assert.Equal(0, messageBuffer.Length);
        Assert.Throws<InvalidOperationException>(() => messageBuffer.TakeOwnership(out _));

        messageBuffer.Append("fresh"u8);
        using IMemoryOwner<byte> owner = messageBuffer.TakeOwnership(out int length);
        Assert.Equal("fresh", System.Text.Encoding.UTF8.GetString(owner.Memory.Span.Slice(0, length)));
    }

    [Fact]
    public void TestDiscardWhenEmptyIsNoOp()
    {
        using MessageBuffer messageBuffer = new();
        messageBuffer.Discard();
        Assert.False(messageBuffer.HasData);
    }

    [Fact]
    public void TestDisposeDiscardsDataAndIsIdempotent()
    {
        MessageBuffer messageBuffer = new();
        messageBuffer.Append("data"u8);
        messageBuffer.Dispose();
        Assert.False(messageBuffer.HasData);
        messageBuffer.Dispose();
        Assert.False(messageBuffer.HasData);
    }
}
