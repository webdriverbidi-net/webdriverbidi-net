// <copyright file="DownloadEventArgs.cs" company="WebDriverBiDi.NET Committers">
// Copyright (c) WebDriverBiDi.NET Committers. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.
// </copyright>

namespace WebDriverBiDi.Automation;

/// <summary>
/// Data about a download a page began.
/// </summary>
/// <param name="Download">The download.</param>
public record DownloadEventArgs(Download Download) : WebDriverBiDiEventArgs;
