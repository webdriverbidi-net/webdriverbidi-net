namespace WebDriverBiDi.TestUtilities;

using System.Buffers;

/// <summary>
/// An <see cref="IMemoryOwner{T}"/> wrapper that records whether it has been disposed,
/// so tests can verify that a consumer returned a pooled buffer.
/// </summary>
public sealed class TrackingMemoryOwner : IMemoryOwner<byte>
{
    private readonly IMemoryOwner<byte> inner;

    public TrackingMemoryOwner(byte[] contents)
    {
        this.inner = MemoryPool<byte>.Shared.Rent(contents.Length);
        contents.CopyTo(this.inner.Memory);
        this.Length = contents.Length;
    }

    public int Length { get; }

    public bool IsDisposed { get; private set; }

    public Memory<byte> Memory => this.inner.Memory;

    public void Dispose()
    {
        this.IsDisposed = true;
        this.inner.Dispose();
    }
}
