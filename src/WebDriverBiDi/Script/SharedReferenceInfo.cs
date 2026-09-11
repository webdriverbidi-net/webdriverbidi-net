// <copyright file="SharedReferenceInfo.cs" company="WebDriverBiDi.NET Committers">
// Copyright (c) WebDriverBiDi.NET Committers. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.
// </copyright>

namespace WebDriverBiDi.Script;

using System.Text.Json;
using System.Text.Json.Serialization;
using WebDriverBiDi.Internal;

/// <summary>
/// Provides information about a received reference to a remote object identified by a shared ID, such as a
/// node. Use <see cref="ToSharedReference"/> to obtain a mutable <see cref="SharedReference"/> that can be
/// passed as an argument to subsequent commands.
/// </summary>
public record SharedReferenceInfo
{
    /// <summary>
    /// Initializes a new instance of the <see cref="SharedReferenceInfo"/> class.
    /// </summary>
    [JsonConstructor]
    internal SharedReferenceInfo()
    {
        this.AdditionalData = ReceivedDataDictionary.EmptyDictionary;
    }

    /// <summary>
    /// Gets the shared ID of the remote object.
    /// </summary>
    [JsonPropertyName("sharedId")]
    [JsonRequired]
    [JsonInclude]
    public string SharedId { get; internal set; } = string.Empty;

    /// <summary>
    /// Gets the handle of the remote object, if any.
    /// </summary>
    [JsonPropertyName("handle")]
    [JsonInclude]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public string? Handle { get; internal set; }

    /// <summary>
    /// Gets the read-only dictionary of additional properties received with this reference.
    /// </summary>
    /// <remarks>
    /// The protocol defines <c>script.SharedReference</c> as extensible, so a remote end may place
    /// vendor-specific properties beside <c>sharedId</c> and <c>handle</c>. They are preserved here, and
    /// <see cref="ToSharedReference"/> copies them into the <see cref="RemoteReference.AdditionalData"/> of
    /// the reference it returns so that they are sent back to the remote end with the reference.
    /// </remarks>
    [JsonIgnore]
    public ReceivedDataDictionary AdditionalData
    {
        get
        {
            if (this.SerializableAdditionalData.Count > 0 && field.Count == 0)
            {
                field = JsonConverterUtilities.ConvertIncomingExtensionData(this.SerializableAdditionalData);
            }

            return field;
        }
    }

    /// <summary>
    /// Gets or sets additional properties deserialized with this reference.
    /// </summary>
    [JsonExtensionData]
    [JsonInclude]
    internal Dictionary<string, JsonElement> SerializableAdditionalData { get; set; } = [];

    /// <summary>
    /// Converts this received reference to a mutable <see cref="SharedReference"/> for use as an argument
    /// to subsequent commands. Any additional properties received with this reference are copied to the
    /// <see cref="RemoteReference.AdditionalData"/> of the returned reference.
    /// </summary>
    /// <returns>A <see cref="SharedReference"/> representing this reference.</returns>
    public SharedReference ToSharedReference()
    {
        SharedReference reference = new(this.SharedId) { Handle = this.Handle };
        foreach (KeyValuePair<string, object?> entry in this.AdditionalData.ToWritableCopy())
        {
            reference.AdditionalData[entry.Key] = entry.Value;
        }

        return reference;
    }
}
