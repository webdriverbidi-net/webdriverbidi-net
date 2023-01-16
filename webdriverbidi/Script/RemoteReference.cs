// <copyright file="RemoteReference.cs" company="WebDriverBidi.NET Committers">
// Copyright (c) WebDriverBidi.NET Committers. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.
// </copyright>

namespace WebDriverBidi.Script;

using Newtonsoft.Json;
using WebDriverBidi.JsonConverters;

/// <summary>
/// Object containing a remote reference.
/// </summary>
[JsonObject(MemberSerialization.OptIn)]
public class RemoteReference : ArgumentValue
{
    private readonly Dictionary<string, object?> additionalData = new();
    private string handle;

    /// <summary>
    /// Initializes a new instance of the <see cref="RemoteReference"/> class.
    /// </summary>
    /// <param name="handle">The handle of the remote object.</param>
    public RemoteReference(string handle)
    {
        this.handle = handle;
    }

    /// <summary>
    /// Gets the handle of the remote object.
    /// </summary>
    [JsonProperty("handle")]
    public string Handle { get => this.handle; internal set => this.handle = value; }

    /// <summary>
    /// Gets the dictionary of additional data about the remote reference.
    /// </summary>
    [JsonExtensionData]
    [JsonConverter(typeof(ResponseValueJsonConverter))]
    public Dictionary<string, object?> AdditionalData => this.additionalData;
}