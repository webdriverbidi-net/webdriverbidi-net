// <copyright file="DownloadEndEventArgs.cs" company="WebDriverBiDi.NET Committers">
// Copyright (c) WebDriverBiDi.NET Committers. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.
// </copyright>

namespace WebDriverBiDi.BrowsingContext;

using System.Diagnostics.CodeAnalysis;
using System.Text.Json.Serialization;
using WebDriverBiDi.JsonConverters;

/// <summary>
/// Object containing event data for the browsingContext.downloadEnd event. The event data is a
/// <see cref="DownloadCompleteEventArgs"/> when the download completed, and a
/// <see cref="DownloadCanceledEventArgs"/> when it was canceled; only a completed download has a file path.
/// </summary>
[JsonConverter(typeof(DiscriminatedUnionJsonConverter<DownloadEndEventArgs>))]
[DiscriminatedTypeProperty("status")]
[DiscriminatedDerivedType(typeof(DownloadCompleteEventArgs), "complete")]
[DiscriminatedDerivedType(typeof(DownloadCanceledEventArgs), "canceled")]
public abstract record DownloadEndEventArgs : NavigationEventArgs
{
    /// <summary>
    /// Initializes a new instance of the <see cref="DownloadEndEventArgs" /> class.
    /// </summary>
    internal DownloadEndEventArgs()
        : base()
    {
    }

    /// <summary>
    /// Gets the ID for this download.
    /// </summary>
    [JsonPropertyName("download")]
    [JsonRequired]
    [JsonInclude]
    public string DownloadId { get; internal set; } = string.Empty;

    /// <summary>
    /// Gets the status of the download. The status determines the type of this object:
    /// <see cref="DownloadEndStatus.Complete"/> for <see cref="DownloadCompleteEventArgs"/>, and
    /// <see cref="DownloadEndStatus.Canceled"/> for <see cref="DownloadCanceledEventArgs"/>.
    /// </summary>
    [JsonPropertyName("status")]
    [JsonInclude]
    [JsonRequired]
    public DownloadEndStatus Status { get; internal set; }

    /// <summary>
    /// Casts this <see cref="DownloadEndEventArgs"/> to a status-specific download end event args, throwing
    /// if it is not of that type. Use <see cref="TryAs{T}"/> to test without throwing.
    /// </summary>
    /// <typeparam name="T">The specific type of DownloadEndEventArgs to return.</typeparam>
    /// <returns>This instance cast to the specified type.</returns>
    /// <exception cref="WebDriverBiDiException">Thrown if this DownloadEndEventArgs is not the specified type.</exception>
    public T As<T>()
        where T : DownloadEndEventArgs
    {
        if (this is T castValue)
        {
            return castValue;
        }

        throw new WebDriverBiDiException($"This DownloadEndEventArgs cannot be cast to {typeof(T)}");
    }

    /// <summary>
    /// Attempts to cast this <see cref="DownloadEndEventArgs"/> to a status-specific download end event
    /// args, returning <see langword="false"/> rather than throwing when it is not of that type.
    /// </summary>
    /// <typeparam name="T">The specific type of DownloadEndEventArgs to return.</typeparam>
    /// <param name="result">When this method returns, contains the cast value or null if the cast failed.</param>
    /// <returns><see langword="true"/> if the cast was successful; otherwise, <see langword="false"/>.</returns>
    public bool TryAs<T>([NotNullWhen(true)] out T? result)
        where T : DownloadEndEventArgs
    {
        if (this is T converted)
        {
            result = converted;
            return true;
        }

        result = null;
        return false;
    }
}
