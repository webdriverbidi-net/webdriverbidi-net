namespace WebDriverBiDi.TestUtilities;

/// <summary>
/// A stream that passes everything through to another stream, and that can hold one write after its bytes have
/// left: the inner write and flush complete, so the remote end receives and answers the data, but the caller is
/// not told until the test releases the write.
/// </summary>
/// <remarks>
/// This is the deterministic form of a thread being preempted between a socket write returning and the caller
/// acting on its return. A <see cref="System.Net.WebSockets.ClientWebSocket"/> records that it has sent a Close
/// frame only after the write returns, so holding that write keeps the socket reporting that no Close frame has
/// been sent for as long as the test needs, with no dependence on scheduling.
/// </remarks>
public sealed class HeldWriteStream : Stream
{
    private readonly Stream inner;
    private readonly TaskCompletionSource heldWriteSent = new(TaskCreationOptions.RunContinuationsAsynchronously);
    private readonly TaskCompletionSource heldWriteReleased = new(TaskCreationOptions.RunContinuationsAsynchronously);
    private int holdNextWrite;

    public HeldWriteStream(Stream inner)
    {
        this.inner = inner;
    }

    /// <summary>
    /// Gets a task completed once the held write's bytes have been written and flushed to the inner stream.
    /// </summary>
    public Task HeldWriteSent => this.heldWriteSent.Task;

    public override bool CanRead => true;

    public override bool CanSeek => false;

    public override bool CanWrite => true;

    public override long Length => throw new NotSupportedException();

    public override long Position
    {
        get => throw new NotSupportedException();
        set => throw new NotSupportedException();
    }

    /// <summary>
    /// Arranges for the next write, and only that one, to be held after its bytes have left.
    /// </summary>
    public void HoldNextWrite()
    {
        Interlocked.Exchange(ref this.holdNextWrite, 1);
    }

    /// <summary>
    /// Lets the held write return to its caller.
    /// </summary>
    public void ReleaseHeldWrite()
    {
        this.heldWriteReleased.TrySetResult();
    }

    public override async ValueTask WriteAsync(ReadOnlyMemory<byte> buffer, CancellationToken cancellationToken = default)
    {
        await this.inner.WriteAsync(buffer, cancellationToken).ConfigureAwait(false);
        await this.inner.FlushAsync(cancellationToken).ConfigureAwait(false);
        if (Interlocked.Exchange(ref this.holdNextWrite, 0) == 1)
        {
            this.heldWriteSent.TrySetResult();
            await this.heldWriteReleased.Task.ConfigureAwait(false);
        }
    }

    public override Task WriteAsync(byte[] buffer, int offset, int count, CancellationToken cancellationToken)
    {
        return this.WriteAsync(buffer.AsMemory(offset, count), cancellationToken).AsTask();
    }

    public override ValueTask<int> ReadAsync(Memory<byte> buffer, CancellationToken cancellationToken = default)
    {
        return this.inner.ReadAsync(buffer, cancellationToken);
    }

    public override Task<int> ReadAsync(byte[] buffer, int offset, int count, CancellationToken cancellationToken)
    {
        return this.inner.ReadAsync(buffer, offset, count, cancellationToken);
    }

    public override Task FlushAsync(CancellationToken cancellationToken)
    {
        return this.inner.FlushAsync(cancellationToken);
    }

    public override void Flush()
    {
        this.inner.Flush();
    }

    public override int Read(byte[] buffer, int offset, int count)
    {
        return this.inner.Read(buffer, offset, count);
    }

    public override void Write(byte[] buffer, int offset, int count)
    {
        this.inner.Write(buffer, offset, count);
    }

    public override long Seek(long offset, SeekOrigin origin)
    {
        throw new NotSupportedException();
    }

    public override void SetLength(long value)
    {
        throw new NotSupportedException();
    }

    protected override void Dispose(bool disposing)
    {
        if (disposing)
        {
            this.inner.Dispose();
        }

        base.Dispose(disposing);
    }
}
