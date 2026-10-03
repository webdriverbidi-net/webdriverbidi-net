// <copyright file="HarArchiveEntry.cs" company="WebDriverBiDi.NET Committers">
// Copyright (c) WebDriverBiDi.NET Committers. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.
// </copyright>

namespace Dramaturge.Network;

/// <summary>
/// A request recorded in an HTTP Archive, and its response.
/// </summary>
/// <param name="Method">The request's method.</param>
/// <param name="Url">The request's URL, without a fragment.</param>
/// <param name="RequestHeaders">The request's headers.</param>
/// <param name="RequestBody">The request's body, or <see langword="null"/> if none was recorded.</param>
/// <param name="Status">The response's status code.</param>
/// <param name="StatusText">The response's status text.</param>
/// <param name="ResponseHeaders">The response's headers.</param>
/// <param name="ResponseBody">The response's body.</param>
/// <param name="RedirectUrl">The URL a redirect leads to, or an empty string.</param>
internal sealed record HarArchiveEntry(
    string Method,
    string Url,
    IReadOnlyList<(string Name, string Value)> RequestHeaders,
    byte[]? RequestBody,
    int Status,
    string StatusText,
    IReadOnlyList<(string Name, string Value)> ResponseHeaders,
    byte[] ResponseBody,
    string RedirectUrl);
