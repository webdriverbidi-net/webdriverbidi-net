// <copyright file="DownloadEndStatus.cs" company="WebDriverBiDi.NET Committers">
// Copyright (c) WebDriverBiDi.NET Committers. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.
// </copyright>

namespace WebDriverBiDi.BrowsingContext;

using System.Text.Json.Serialization;
using WebDriverBiDi.JsonConverters;

/// <summary>
/// Values used for the status of downloaded items.
/// </summary>
[JsonConverter(typeof(EnumValueJsonConverter<DownloadEndStatus>))]
public enum DownloadEndStatus
{
    /// <summary>
    /// The download was canceled.
    /// </summary>
    Canceled,

    /// <summary>
    /// The download completed.
    /// </summary>
    Complete,
}
