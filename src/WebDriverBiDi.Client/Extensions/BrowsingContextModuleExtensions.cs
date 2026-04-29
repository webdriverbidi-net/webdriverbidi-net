namespace WebDriverBiDi.BrowsingContext;

public static class BrowsingContextModuleExtensions
{
    public static Task<CloseCommandResult> CloseAsync(this BrowsingContextModule module, string browsingContextId, TimeSpan? timeout = null, CancellationToken cancellationToken = default)
    {
        return module.CloseAsync(new CloseCommandParameters(browsingContextId), timeout, cancellationToken);
    }

    public static async Task<List<BrowsingContextInfo>> GetTopLevelBrowsingContextsAsync(this BrowsingContextModule module, TimeSpan? timeout = null, CancellationToken cancellationToken = default)
    {
        GetTreeCommandResult result = await module.GetTreeAsync(new GetTreeCommandParameters() { MaxDepth = 1 }, timeout, cancellationToken).ConfigureAwait(false);
        return result.ContextTree.ToList();
    }

    public static Task<NavigateCommandResult> NavigateAsync(this BrowsingContextModule module, string browsingContextId, string url, ReadinessState? wait, TimeSpan? timeout = null, CancellationToken cancellationToken = default)
    {
        NavigateCommandParameters parameters = new(browsingContextId, url)
        {
            Wait = wait,
        };
        return module.NavigateAsync(parameters, timeout, cancellationToken);
    }

    public static Task<CaptureScreenshotCommandResult> CaptureScreenshotAsync(this BrowsingContextModule module, string browsingContextId, ScreenshotOrigin origin, TimeSpan? timeout = null, CancellationToken cancellationToken = default)
    {
        CaptureScreenshotCommandParameters parameters = new(browsingContextId)
        {
            Origin = origin,
        };
        return module.CaptureScreenshotAsync(parameters, timeout, cancellationToken);
    }
}
