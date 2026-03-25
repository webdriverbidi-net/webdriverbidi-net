// <copyright file="RemoteObjectReference.cs" company="WebDriverBiDi.NET Committers">
// Copyright (c) WebDriverBiDi.NET Committers. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.
// </copyright>

namespace WebDriverBiDi.Script;

using System.Text.Json.Serialization;

/// <summary>
/// Object containing a remote reference to an existing ECMAScript object in the browser.
/// </summary>
public record RemoteObjectReference : RemoteReference
{
    private string handle = string.Empty;

    /// <summary>
    /// Initializes a new instance of the <see cref="RemoteObjectReference"/> class.
    /// </summary>
    /// <param name="handle">The handle of the remote object.</param>
    [JsonConstructor]
    public RemoteObjectReference(string handle)
        : base()
    {
        this.handle = handle;
    }

    /// <summary>
    /// Gets or sets the handle of the remote object.
    /// </summary>
    /// <exception cref="ArgumentNullException">Thrown if setting the value to <see langword="null"/>.</exception>
    [JsonPropertyName("handle")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    [JsonRequired]
    public string Handle
    {
        get => this.handle;
        set => this.handle = value ?? throw new ArgumentNullException(nameof(value));
    }

    /// <summary>
    /// Gets or sets the shared ID of the remote object.
    /// </summary>
    [JsonPropertyName("sharedId")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    [JsonInclude]
    public string? SharedId { get; set; }
}
