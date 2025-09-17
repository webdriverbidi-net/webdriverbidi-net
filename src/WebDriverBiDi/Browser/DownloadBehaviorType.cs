// <copyright file="DownloadBehaviorType.cs" company="WebDriverBiDi.NET Committers">
// Copyright (c) WebDriverBiDi.NET Committers. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.
// </copyright>

using System.Text.Json.Serialization;
using WebDriverBiDi.JsonConverters;

/// <summary>
/// Values used for setting the download behavior of a browser's user contexts.
/// </summary>
[JsonConverter(typeof(EnumValueJsonConverter<DownloadBehaviorType>))]
public enum DownloadBehaviorType
{
    /// <summary>
    /// Allow the browser to download files.
    /// </summary>
    Allowed,

    /// <summary>
    /// Do not allow the browser to download files.
    /// </summary>
    Denied,
}
