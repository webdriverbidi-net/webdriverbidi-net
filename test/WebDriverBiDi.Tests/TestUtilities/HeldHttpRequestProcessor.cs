namespace WebDriverBiDi.TestUtilities;

using System.Net;
using PinchHitter;

/// <summary>
/// An HTTP request processor that holds every request it receives unanswered until the test releases it, so
/// that a client's WebSocket upgrade request, and with it the client's connect, stays in flight for as long as
/// the test needs to observe the client.
/// </summary>
public class HeldHttpRequestProcessor : HttpRequestProcessor
{
    private readonly TaskCompletionSource requestReceived = new(TaskCreationOptions.RunContinuationsAsynchronously);
    private readonly TaskCompletionSource released = new(TaskCreationOptions.RunContinuationsAsynchronously);

    /// <summary>
    /// Gets a task that completes when the first request reaches this processor.
    /// </summary>
    public Task RequestReceived => this.requestReceived.Task;

    /// <summary>
    /// Releases every held request, answering each with a response that does not upgrade the connection.
    /// </summary>
    public void Release()
    {
        this.released.TrySetResult();
    }

    /// <inheritdoc/>
    public override async Task<HttpResponse> ProcessRequestAsync(string connectionId, HttpRequest request)
    {
        this.requestReceived.TrySetResult();
        await this.released.Task.ConfigureAwait(false);
        return new HttpResponse(request.Id)
        {
            StatusCode = HttpStatusCode.ServiceUnavailable,
        };
    }
}
