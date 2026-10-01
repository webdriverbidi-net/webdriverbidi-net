// <copyright file="DownloadOutcome.cs" company="WebDriverBiDi.NET Committers">
// Copyright (c) WebDriverBiDi.NET Committers. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.
// </copyright>

namespace Dramaturge;

using WebDriverBiDi.BrowsingContext;

/// <summary>
/// How a download ended.
/// </summary>
/// <param name="Status">Whether the download completed or was canceled.</param>
/// <param name="FilePath">The path of the downloaded file, if it completed and the browser reports where it saved it.</param>
public sealed record DownloadOutcome(DownloadEndStatus Status, string? FilePath);
