// <copyright file="DownloadServiceException.cs" company="WebDriverBiDi.NET Committers">
// Copyright (c) WebDriverBiDi.NET Committers. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.
// </copyright>

namespace WebDriverBiDi.Browsers;

using System.Globalization;
using System.Net;

/// <summary>
/// Thrown when a download service responds with an unsuccessful status. Wrapped in a
/// <see cref="BrowserDownloadException"/> before it leaves the library.
/// </summary>
internal sealed class DownloadServiceException : Exception
{
    private DownloadServiceException(string message, HttpStatusCode statusCode, bool isRateLimited)
        : base(message)
    {
        this.StatusCode = statusCode;
        this.IsRateLimited = isRateLimited;
    }

    /// <summary>
    /// Gets the status of the response.
    /// </summary>
    public HttpStatusCode StatusCode { get; }

    /// <summary>
    /// Gets a value indicating whether the request was refused by a rate limit.
    /// </summary>
    public bool IsRateLimited { get; }

    /// <summary>
    /// Gets a value indicating whether the service failed in a way that a repeated request may not.
    /// </summary>
    public bool IsServerError => (int)this.StatusCode >= 500;

    /// <summary>
    /// Throws if a response is unsuccessful.
    /// </summary>
    /// <param name="response">The response.</param>
    /// <param name="url">The requested URL.</param>
    /// <exception cref="DownloadServiceException">Thrown when the response is unsuccessful.</exception>
    public static void ThrowIfUnsuccessful(HttpResponseMessage response, Uri url)
    {
        if (response.IsSuccessStatusCode)
        {
            return;
        }

        HttpStatusCode statusCode = response.StatusCode;
        if ((statusCode == HttpStatusCode.Forbidden || (int)statusCode == 429) && GetHeader(response, "X-RateLimit-Remaining") == "0")
        {
            string resetTime = long.TryParse(GetHeader(response, "X-RateLimit-Reset"), NumberStyles.None, CultureInfo.InvariantCulture, out long resetSeconds)
                ? $" until {DateTimeOffset.FromUnixTimeSeconds(resetSeconds):u}"
                : string.Empty;
            throw new DownloadServiceException(
                $"{url.GetLeftPart(UriPartial.Authority)} refused the request for {url} because its rate limit is exhausted{resetTime}. Supply an authenticated client in {nameof(BrowserDownloadOptions)}.{nameof(BrowserDownloadOptions.HttpClient)} to raise the limit.",
                statusCode,
                isRateLimited: true);
        }

        string message = statusCode == HttpStatusCode.NotFound
            ? $"{url} was not found (404)."
            : $"{url} returned {(int)statusCode} {response.ReasonPhrase}.";
        throw new DownloadServiceException(message, statusCode, isRateLimited: false);
    }

    private static string? GetHeader(HttpResponseMessage response, string name)
    {
        return response.Headers.TryGetValues(name, out IEnumerable<string>? values) ? values.FirstOrDefault() : null;
    }
}
