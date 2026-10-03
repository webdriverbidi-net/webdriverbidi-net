// <copyright file="HarRouter.cs" company="WebDriverBiDi.NET Committers">
// Copyright (c) WebDriverBiDi.NET Committers. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.
// </copyright>

namespace Dramaturge.Network;

using System.Globalization;
using WebDriverBiDi.Network;

/// <summary>
/// Answers a route's requests from an HTTP Archive.
/// </summary>
internal sealed class HarRouter
{
    // Room for a multipart boundary longer than the recorded one.
    private const int BodySizeMargin = 4096;

    private static readonly HashSet<string> RecordedOnlyHeaders = new(StringComparer.OrdinalIgnoreCase) { "Content-Encoding", "Content-Length", "Transfer-Encoding" };

    private static readonly HashSet<int> RedirectStatuses = [301, 302, 303, 307, 308];

    private readonly BrowserGroup group;
    private readonly HarArchive archive;
    private readonly HarNotFound notFound;
    private readonly string? collectorId;

    private HarRouter(BrowserGroup group, HarArchive archive, HarNotFound notFound, string? collectorId)
    {
        this.group = group;
        this.archive = archive;
        this.notFound = notFound;
        this.collectorId = collectorId;
    }

    /// <summary>
    /// Reads an archive and, if it records request bodies, adds a data collector, so that a request's body can be
    /// read while it is stopped.
    /// </summary>
    /// <param name="group">The group whose driver answers requests.</param>
    /// <param name="harPath">The path of the archive.</param>
    /// <param name="options">The route's settings.</param>
    /// <param name="scope">Limits the data collector to the page or browser the route belongs to.</param>
    /// <param name="cancellationToken">A token that cancels reading and the commands.</param>
    /// <returns>The router.</returns>
    public static async Task<HarRouter> CreateAsync(BrowserGroup group, string harPath, HarRouteOptions? options, Action<AddDataCollectorCommandParameters> scope, CancellationToken cancellationToken)
    {
        HarArchive archive = await HarArchive.LoadAsync(harPath, cancellationToken).ConfigureAwait(false);
        string? collectorId = null;
        if (archive.LongestRequestBody > 0)
        {
            AddDataCollectorCommandParameters parameters = new((ulong)(archive.LongestRequestBody + BodySizeMargin), DataType.Request);
            scope(parameters);
            collectorId = (await group.Driver.Network.AddDataCollectorAsync(parameters, cancellationToken: cancellationToken).ConfigureAwait(false)).CollectorId;
            group.TrackDataCollector(collectorId);
        }

        return new HarRouter(group, archive, options?.NotFound ?? HarNotFound.Abort, collectorId);
    }

    /// <summary>
    /// Answers a request with its entry's response, or, if it has none, aborts it or leaves it to the next route.
    /// </summary>
    /// <param name="route">The stopped request.</param>
    /// <returns>A task that completes when the request is answered.</returns>
    public async Task HandleAsync(Route route)
    {
        byte[]? body = this.archive.NeedsBody(route.Request) ? await this.ReadBodyAsync(route.Request).ConfigureAwait(false) : null;
        HarArchiveEntry? entry = this.archive.Find(route.Request, body);
        if (entry is null)
        {
            if (this.notFound == HarNotFound.Abort)
            {
                await route.AbortAsync().ConfigureAwait(false);
            }

            return;
        }

        // The body is given decoded, and its length is the body's own, so the recorded encoding and length go.
        List<Header> headers = [.. entry.ResponseHeaders.Where(header => !header.Name.StartsWith(":", StringComparison.Ordinal) && !RecordedOnlyHeaders.Contains(header.Name)).Select(header => new Header(header.Name, header.Value))];
        if (RedirectStatuses.Contains(entry.Status) && entry.RedirectUrl.Length > 0 && !headers.Any(header => string.Equals(header.Name, "Location", StringComparison.OrdinalIgnoreCase)))
        {
            headers.Add(new Header("Location", entry.RedirectUrl));
        }

        headers.Add(new Header("Content-Length", entry.ResponseBody.Length.ToString(CultureInfo.InvariantCulture)));
        await route.FulfillAsync((ulong)entry.Status, entry.StatusText, BytesValue.FromByteArray(entry.ResponseBody), headers).ConfigureAwait(false);
    }

    /// <summary>
    /// Removes the data collector, if the router added one.
    /// </summary>
    /// <param name="cancellationToken">A token that cancels the command.</param>
    /// <returns>A task that completes when the collector is removed.</returns>
    public async Task RemoveAsync(CancellationToken cancellationToken)
    {
        if (this.collectorId is not null)
        {
            this.group.UntrackDataCollector(this.collectorId);
            await this.group.Driver.Network.RemoveDataCollectorAsync(new RemoveDataCollectorCommandParameters(this.collectorId), cancellationToken: cancellationToken).ConfigureAwait(false);
        }
    }

    // A body the browser did not keep, such as one larger than any recorded body, matches no recorded body. The body
    // is not disowned, which in Chrome makes the stopped request unknown, so that it can no longer be answered; the
    // collector's bodies are released when it is removed. Once chromium-bidi keeps a request whose data was disowned,
    // set DisownCollectedData here again.
    private async Task<byte[]?> ReadBodyAsync(RequestData request)
    {
        try
        {
            GetDataCommandParameters parameters = new(request.RequestId, DataType.Request) { CollectorId = this.collectorId };
            GetDataCommandResult result = await this.group.Driver.Network.GetDataAsync(parameters).ConfigureAwait(false);
            return result.Bytes.ValueAsByteArray;
        }
        catch (WebDriverBiDi.WebDriverBiDiException)
        {
            return null;
        }
    }
}
