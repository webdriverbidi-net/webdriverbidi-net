// <copyright file="DownloadHttpClient.cs" company="WebDriverBiDi.NET Committers">
// Copyright (c) WebDriverBiDi.NET Committers. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.
// </copyright>

namespace WebDriverBiDi.Browsers;

/// <summary>
/// Issues the HTTP requests made while locating and downloading browsers and drivers,
/// using the <see cref="HttpClient"/> supplied in <see cref="BrowserDownloadOptions"/> if any.
/// </summary>
internal static class DownloadHttpClient
{
    private const int MaximumAttempts = 3;

    private static readonly TimeSpan MetadataRequestTimeout = TimeSpan.FromSeconds(30);
    private static readonly TimeSpan[] RetryDelays = [TimeSpan.FromMilliseconds(500), TimeSpan.FromSeconds(1)];
    private static readonly Lazy<HttpClient> SharedClient = new(() => new HttpClient());
    private static readonly Lazy<HttpClient> SharedNonRedirectingClient = new(() => new HttpClient(new HttpClientHandler() { AllowAutoRedirect = false }));

    /// <summary>
    /// Gets the client to use for downloading files.
    /// </summary>
    /// <param name="options">The download options.</param>
    /// <returns>The supplied client, or a shared client owned by this library.</returns>
    public static HttpClient GetClient(BrowserDownloadOptions options)
    {
        return options.HttpClient ?? SharedClient.Value;
    }

    /// <summary>
    /// Gets the body of a version information response as a string.
    /// </summary>
    /// <param name="options">The download options.</param>
    /// <param name="url">The URL to request.</param>
    /// <param name="cancellationToken">A token that cancels the request.</param>
    /// <returns>The response body.</returns>
    /// <exception cref="DownloadServiceException">Thrown when the service responds unsuccessfully.</exception>
    public static Task<string> GetStringAsync(BrowserDownloadOptions options, Uri url, CancellationToken cancellationToken)
    {
        return RetryAsync(
            async () =>
            {
                using CancellationTokenSource timeoutTokenSource = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);
                timeoutTokenSource.CancelAfter(MetadataRequestTimeout);
                using HttpRequestMessage request = new(HttpMethod.Get, url);

                // The GitHub API rejects requests without a User-Agent.
                request.Headers.UserAgent.ParseAdd("WebDriverBiDi.NET");
                using HttpResponseMessage response = await GetClient(options).SendAsync(request, timeoutTokenSource.Token).ConfigureAwait(false);
                DownloadServiceException.ThrowIfUnsuccessful(response, url);
                return await response.Content.ReadAsStringAsync().ConfigureAwait(false);
            },
            cancellationToken);
    }

    /// <summary>
    /// Gets the URL to which a request is redirected, without downloading the redirect target.
    /// </summary>
    /// <param name="options">The download options.</param>
    /// <param name="url">The URL to request.</param>
    /// <param name="cancellationToken">A token that cancels the request.</param>
    /// <returns>The redirect target, or <paramref name="url"/> if the request is not redirected.</returns>
    /// <exception cref="DownloadServiceException">Thrown when the service responds unsuccessfully.</exception>
    public static Task<Uri> GetRedirectTargetAsync(BrowserDownloadOptions options, Uri url, CancellationToken cancellationToken)
    {
        return RetryAsync(
            async () =>
            {
                using CancellationTokenSource timeoutTokenSource = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);
                timeoutTokenSource.CancelAfter(MetadataRequestTimeout);
                HttpClient client = options.HttpClient ?? SharedNonRedirectingClient.Value;
                using HttpResponseMessage response = await client.GetAsync(url, HttpCompletionOption.ResponseHeadersRead, timeoutTokenSource.Token).ConfigureAwait(false);
                if (response.Headers.Location is not null)
                {
                    return new Uri(url, response.Headers.Location);
                }

                // A supplied client may follow redirects itself, leaving the target as the final request URL.
                DownloadServiceException.ThrowIfUnsuccessful(response, url);
                return response.RequestMessage?.RequestUri ?? url;
            },
            cancellationToken);
    }

    /// <summary>
    /// Runs an operation, running it again after a short delay when it fails in a way that may not recur.
    /// </summary>
    /// <typeparam name="T">The type of the operation's result.</typeparam>
    /// <param name="operation">The operation.</param>
    /// <param name="cancellationToken">A token that cancels the operation and the delays between attempts.</param>
    /// <returns>The result of the first attempt that succeeds.</returns>
    public static async Task<T> RetryAsync<T>(Func<Task<T>> operation, CancellationToken cancellationToken)
    {
        for (int attempt = 1; ; attempt++)
        {
            try
            {
                return await operation().ConfigureAwait(false);
            }
            catch (Exception ex) when (attempt < MaximumAttempts && IsTransient(ex, cancellationToken))
            {
                await Task.Delay(RetryDelays[attempt - 1], cancellationToken).ConfigureAwait(false);
            }
        }
    }

    // A request timeout is an OperationCanceledException that the caller's token did not cause.
    private static bool IsTransient(Exception exception, CancellationToken cancellationToken)
    {
        return exception is HttpRequestException
            || exception is TransientDownloadException
            || exception is DownloadServiceException { IsServerError: true }
            || (exception is OperationCanceledException && !cancellationToken.IsCancellationRequested);
    }
}
