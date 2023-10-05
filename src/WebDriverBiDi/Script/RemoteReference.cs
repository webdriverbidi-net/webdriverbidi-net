// <copyright file="RemoteReference.cs" company="WebDriverBiDi.NET Committers">
// Copyright (c) WebDriverBiDi.NET Committers. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.
// </copyright>

namespace WebDriverBiDi.Script;

using Newtonsoft.Json;
using WebDriverBiDi.JsonConverters;

/// <summary>
/// Object containing a remote reference.
/// </summary>
[JsonObject(MemberSerialization.OptIn)]
public class RemoteReference : ArgumentValue
{
    private readonly Dictionary<string, object?> additionalData = new();
    private string? handle;
    private string? sharedId;

    /// <summary>
    /// Initializes a new instance of the <see cref="RemoteReference"/> class.
    /// </summary>
    /// <param name="handle">The handle of the remote object.</param>
    /// <param name="sharedId">The shared ID of the remote object.</param>
    protected RemoteReference(string? handle, string? sharedId)
    {
        this.handle = handle;
        this.sharedId = sharedId;
    }

    /// <summary>
    /// Gets the dictionary of additional data about the remote reference.
    /// </summary>
    [JsonExtensionData]
    [JsonConverter(typeof(ReceivedDataJsonConverter))]
    public Dictionary<string, object?> AdditionalData => this.additionalData;

    /// <summary>
    /// Gets or sets the internally accessible handle of the remote reference.
    /// </summary>
    [JsonProperty("handle", NullValueHandling = NullValueHandling.Ignore)]
    protected string? InternalHandle { get => this.handle; set => this.handle = value; }

    /// <summary>
    /// Gets or sets the internally accessible shared ID of the remote reference.
    /// </summary>
    [JsonProperty("sharedId", NullValueHandling = NullValueHandling.Ignore)]
    protected string? InternalSharedId { get => this.sharedId; set => this.sharedId = value; }
}
