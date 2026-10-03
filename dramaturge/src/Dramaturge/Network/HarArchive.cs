// <copyright file="HarArchive.cs" company="WebDriverBiDi.NET Committers">
// Copyright (c) WebDriverBiDi.NET Committers. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.
// </copyright>

namespace Dramaturge.Network;

using System.IO.Compression;
using System.Text;
using System.Text.Json;
using System.Text.RegularExpressions;
using WebDriverBiDi.Network;

/// <summary>
/// The entries of an HTTP Archive, read for replaying: a .har file, with its bodies inline or in files beside it, or
/// a .zip holding a .har and its body files, as Playwright writes them. Every body is read when the archive is
/// loaded, so a missing or unreadable one fails then, not while a page waits for a response.
/// </summary>
internal sealed class HarArchive
{
    private static readonly Regex BoundaryPattern = new("boundary=\"?([^\";\\s]+)", RegexOptions.IgnoreCase);

    private readonly List<HarArchiveEntry> entries;

    private HarArchive(List<HarArchiveEntry> entries)
    {
        this.entries = entries;
    }

    /// <summary>
    /// Gets the length of the longest recorded request body, or 0 if no entry records one.
    /// </summary>
    public int LongestRequestBody => this.entries.Select(entry => entry.RequestBody?.Length ?? 0).DefaultIfEmpty(0).Max();

    /// <summary>
    /// Reads an archive.
    /// </summary>
    /// <param name="path">The path of the .har or .zip file.</param>
    /// <param name="cancellationToken">A token that cancels reading.</param>
    /// <returns>The archive.</returns>
    /// <exception cref="InvalidDataException">Thrown when the file is not an archive this can read.</exception>
    public static async Task<HarArchive> LoadAsync(string path, CancellationToken cancellationToken)
    {
        byte[] file = await ReadFileAsync(path, cancellationToken).ConfigureAwait(false);
        if (!path.EndsWith(".zip", StringComparison.OrdinalIgnoreCase))
        {
            string directory = Path.GetDirectoryName(Path.GetFullPath(path))!;
            return Parse(file, path, name => ReadBodyFile(directory, name, path));
        }

        using MemoryStream stream = new(file);
        using ZipArchive zip = new(stream, ZipArchiveMode.Read);
        ZipArchiveEntry har = zip.Entries.FirstOrDefault(entry => entry.FullName.EndsWith(".har", StringComparison.Ordinal))
            ?? throw new InvalidDataException($"The archive {path} holds no .har file.");
        return Parse(ReadZipEntry(har), path, name => ReadZipEntry(zip.GetEntry(name) ?? throw new InvalidDataException($"The archive {path} has no file {name}, which an entry's _file names.")));
    }

    /// <summary>
    /// Gets a value indicating whether choosing among a request's entries needs its body: it has one, and an entry
    /// of its method and URL records one.
    /// </summary>
    /// <param name="request">The request.</param>
    /// <returns><see langword="true"/> if the request's body is needed.</returns>
    public bool NeedsBody(RequestData request)
    {
        return request.BodySize != 0 && this.Candidates(request).Any(entry => entry.RequestBody is not null);
    }

    /// <summary>
    /// Finds the entry that answers a request: of the entries of its method and URL, ignoring a fragment, one whose
    /// recorded body is the request's, comparing a multipart form's body without its boundary, else one that recorded
    /// no body, but never one that recorded another body; of several, the one with the most of the request's
    /// headers, then the first in the archive. A request whose body is not known is matched without it.
    /// </summary>
    /// <param name="request">The request.</param>
    /// <param name="body">The request's body, or <see langword="null"/> if it has none or it is not known.</param>
    /// <returns>The entry, or <see langword="null"/> if none matches.</returns>
    public HarArchiveEntry? Find(RequestData request, byte[]? body)
    {
        string? boundary = Boundary(request.Headers.Select(header => (header.Name, HeaderValue(header))));
        HarArchiveEntry? best = null;
        (int SameBody, int Headers) bestScore = (-1, -1);
        foreach (HarArchiveEntry entry in this.Candidates(request))
        {
            bool bodyCompared = body is not null && entry.RequestBody is not null;
            if (bodyCompared && !SameBody(body!, boundary, entry.RequestBody!, Boundary(entry.RequestHeaders)))
            {
                continue;
            }

            (int SameBody, int Headers) score = (bodyCompared ? 1 : 0, entry.RequestHeaders.Count(recorded => request.Headers.Any(header => string.Equals(header.Name, recorded.Name, StringComparison.OrdinalIgnoreCase) && HeaderValue(header) == recorded.Value)));
            if (score.CompareTo(bestScore) > 0)
            {
                best = entry;
                bestScore = score;
            }
        }

        return best;
    }

    private static string WithoutFragment(string url)
    {
        int fragment = url.IndexOf('#');
        return fragment < 0 ? url : url.Substring(0, fragment);
    }

    private static string HeaderValue(ReadOnlyHeader header)
    {
        return header.Value.Type == BytesValueType.String ? header.Value.Value : Encoding.UTF8.GetString(header.Value.ValueAsByteArray);
    }

    private static string? Boundary(IEnumerable<(string Name, string Value)> headers)
    {
        string? contentType = headers.FirstOrDefault(header => string.Equals(header.Name, "Content-Type", StringComparison.OrdinalIgnoreCase)).Value;
        if (contentType is null || contentType.IndexOf("multipart/form-data", StringComparison.OrdinalIgnoreCase) < 0)
        {
            return null;
        }

        Match match = BoundaryPattern.Match(contentType);
        return match.Success ? match.Groups[1].Value : null;
    }

    // A browser makes a new boundary for each form it sends, so two multipart bodies are compared without theirs.
    private static bool SameBody(byte[] body, string? boundary, byte[] recorded, string? recordedBoundary)
    {
        if (body.AsSpan().SequenceEqual(recorded))
        {
            return true;
        }

        return boundary is not null && recordedBoundary is not null
            && Without(body, Encoding.ASCII.GetBytes(boundary)).AsSpan().SequenceEqual(Without(recorded, Encoding.ASCII.GetBytes(recordedBoundary)));
    }

    private static byte[] Without(byte[] data, byte[] pattern)
    {
        List<byte> kept = new(data.Length);
        for (int index = 0; index < data.Length;)
        {
            if (index + pattern.Length <= data.Length && data.AsSpan(index, pattern.Length).SequenceEqual(pattern))
            {
                index += pattern.Length;
            }
            else
            {
                kept.Add(data[index++]);
            }
        }

        return [.. kept];
    }

    private static async Task<byte[]> ReadFileAsync(string path, CancellationToken cancellationToken)
    {
        using FileStream stream = new(path, FileMode.Open, FileAccess.Read, FileShare.Read, 4096, useAsync: true);
        using MemoryStream contents = new();
        await stream.CopyToAsync(contents, 81920, cancellationToken).ConfigureAwait(false);
        return contents.ToArray();
    }

    private static byte[] ReadBodyFile(string directory, string name, string harPath)
    {
        string resolved = Path.GetFullPath(Path.Combine(directory, name));
        if (!resolved.StartsWith(directory + Path.DirectorySeparatorChar, StringComparison.Ordinal))
        {
            throw new InvalidDataException($"The HAR {harPath} names a body file, {name}, outside its directory.");
        }

        return File.ReadAllBytes(resolved);
    }

    private static byte[] ReadZipEntry(ZipArchiveEntry entry)
    {
        using Stream stream = entry.Open();
        using MemoryStream contents = new();
        stream.CopyTo(contents);
        return contents.ToArray();
    }

    private static HarArchive Parse(byte[] json, string path, Func<string, byte[]> readFile)
    {
        try
        {
            using JsonDocument document = JsonDocument.Parse(json);
            if (!document.RootElement.TryGetProperty("log", out JsonElement log) || !log.TryGetProperty("entries", out JsonElement entries) || entries.ValueKind != JsonValueKind.Array)
            {
                throw new InvalidDataException($"The file {path} is not a HAR: it has no log.entries.");
            }

            return new HarArchive([.. entries.EnumerateArray().Select(entry => ParseEntry(entry, readFile))]);
        }
        catch (Exception exception) when (exception is JsonException or InvalidOperationException or KeyNotFoundException)
        {
            throw new InvalidDataException($"The file {path} is not a HAR: {exception.Message}", exception);
        }
    }

    private static HarArchiveEntry ParseEntry(JsonElement entry, Func<string, byte[]> readFile)
    {
        JsonElement request = entry.GetProperty("request");
        JsonElement response = entry.GetProperty("response");
        byte[]? requestBody = request.TryGetProperty("postData", out JsonElement postData) && postData.ValueKind == JsonValueKind.Object ? ReadContent(postData, readFile) : null;
        return new HarArchiveEntry(
            request.GetProperty("method").GetString()!,
            WithoutFragment(request.GetProperty("url").GetString()!),
            ReadHeaders(request),
            requestBody,
            response.GetProperty("status").GetInt32(),
            response.TryGetProperty("statusText", out JsonElement statusText) ? statusText.GetString() ?? string.Empty : string.Empty,
            ReadHeaders(response),
            response.TryGetProperty("content", out JsonElement content) ? ReadContent(content, readFile) : [],
            response.TryGetProperty("redirectURL", out JsonElement redirect) ? redirect.GetString() ?? string.Empty : string.Empty);
    }

    private static List<(string Name, string Value)> ReadHeaders(JsonElement message)
    {
        return message.TryGetProperty("headers", out JsonElement headers)
            ? [.. headers.EnumerateArray().Select(header => (header.GetProperty("name").GetString()!, header.GetProperty("value").GetString()!))]
            : [];
    }

    private static byte[] ReadContent(JsonElement content, Func<string, byte[]> readFile)
    {
        if (content.TryGetProperty("_file", out JsonElement file) && file.GetString() is string name)
        {
            return readFile(name);
        }

        string text = content.TryGetProperty("text", out JsonElement value) ? value.GetString() ?? string.Empty : string.Empty;
        bool isBase64 = content.TryGetProperty("encoding", out JsonElement encoding) && encoding.GetString() == "base64";
        return isBase64 ? Convert.FromBase64String(text) : Encoding.UTF8.GetBytes(text);
    }

    private IEnumerable<HarArchiveEntry> Candidates(RequestData request)
    {
        string url = WithoutFragment(request.Url);
        return this.entries.Where(entry => entry.Url == url && string.Equals(entry.Method, request.Method, StringComparison.OrdinalIgnoreCase));
    }
}
