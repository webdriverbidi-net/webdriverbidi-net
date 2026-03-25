// <copyright file="SharedReference.cs" company="WebDriverBiDi.NET Committers">
// Copyright (c) WebDriverBiDi.NET Committers. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.
// </copyright>

namespace WebDriverBiDi.Script;

using System.Text.Json.Serialization;

/// <summary>
/// Object containing a remote reference to an object in the browser containing a shared ID, such as a node.
/// </summary>
public record SharedReference : RemoteReference
{
    private string sharedId = string.Empty;

    /// <summary>
    /// Initializes a new instance of the <see cref="SharedReference"/> class.
    /// </summary>
    /// <param name="sharedId">The shared ID of the remote object.</param>
    [JsonConstructor]
    public SharedReference(string sharedId)
        : base()
    {
        this.sharedId = sharedId;
    }

    /// <summary>
    /// Gets or sets the shared ID of the remote object.
    /// </summary>
    /// <exception cref="ArgumentNullException">Thrown if setting the value to <see langword="null"/>.</exception>
    [JsonPropertyName("sharedId")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    [JsonRequired]
    public string SharedId
    {
        get => this.sharedId;
        set => this.sharedId = value ?? throw new ArgumentNullException(nameof(value));
    }

    /// <summary>
    /// Gets or sets the handle of the remote object.
    /// </summary>
    [JsonPropertyName("handle")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    [JsonInclude]
    public string? Handle { get; set; }
}
