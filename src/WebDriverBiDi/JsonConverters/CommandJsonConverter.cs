// <copyright file="CommandJsonConverter.cs" company="WebDriverBiDi.NET Committers">
// Copyright (c) WebDriverBiDi.NET Committers. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.
// </copyright>

namespace WebDriverBiDi.JsonConverters;

using System.Diagnostics.CodeAnalysis;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Text.Json.Serialization.Metadata;
using WebDriverBiDi.Protocol;

/// <summary>
/// A converter for a serializing a Command object.
/// </summary>
public class CommandJsonConverter : JsonConverter<Command>
{
    /// <summary>
    /// Deserializes the JSON string to a Command object.
    /// </summary>
    /// <param name="reader">A Utf8JsonReader used to read the incoming JSON.</param>
    /// <param name="typeToConvert">The Type description of the type to convert.</param>
    /// <param name="options">The JsonSerializationOptions used for deserializing the JSON.</param>
    /// <returns>A Command object.</returns>
    /// <exception cref="NotImplementedException">Thrown when called, as this converter is only used for serialization.</exception>
    public override Command? Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        throw new NotImplementedException();
    }

    /// <summary>
    /// Serializes a Command object to a JSON string.
    /// </summary>
    /// <param name="writer">A Utf8JsonWriter used to write the JSON string.</param>
    /// <param name="value">The Command to be serialized.</param>
    /// <param name="options">The JsonSerializationOptions used for serializing the object.</param>
    /// <remarks>
    /// The IL2026/IL3050 suppressions on this method cover the single call site that
    /// serializes entries from <see cref="CommandParameters.AdditionalData"/>. Those entries
    /// are typed as <see cref="object"/> by design. They exist specifically to let users
    /// pass protocol extension fields whose runtime types are not known at library build
    /// time, so the reflection-based serialization path is the only correct choice. The
    /// trade-off is documented on <see cref="CommandParameters.AdditionalData"/>; users who
    /// need AOT-safe command extension must register a <see cref="JsonTypeInfo"/> for every
    /// runtime type they add to the dictionary via
    /// <see cref="BiDiDriver.RegisterTypeInfoResolverAsync(IJsonTypeInfoResolver, CancellationToken)"/>
    /// before sending a command that uses it.
    /// </remarks>
    [UnconditionalSuppressMessage("Trimming", "IL2026:RequiresUnreferencedCode", Justification = "AdditionalData entries are typed as object by design; see remarks.")]
    [UnconditionalSuppressMessage("AOT", "IL3050:RequiresDynamicCode", Justification = "AdditionalData entries are typed as object by design; see remarks.")]
    public override void Write(Utf8JsonWriter writer, Command value, JsonSerializerOptions options)
    {
        if (value is null)
        {
            throw new ArgumentNullException(nameof(value));
        }

        writer.WriteStartObject();
        writer.WritePropertyName("id");
        writer.WriteNumberValue(value.CommandId);
        writer.WritePropertyName("method");
        writer.WriteStringValue(value.CommandName);
        writer.WritePropertyName("params");

        // Use the JsonSerializer.Serialize() overload that takes a JsonTypeInfo
        // to remove warnings when publishing AOT compiled applications.
        JsonTypeInfo paramsTypeInfo = options.GetTypeInfo(value.CommandParameters.GetType());
        JsonSerializer.Serialize(writer, value.CommandParameters, paramsTypeInfo);
        foreach (KeyValuePair<string, object?> pair in value.AdditionalData)
        {
            writer.WritePropertyName(pair.Key);
            writer.WriteRawValue(JsonSerializer.Serialize(pair.Value, options));
        }

        writer.WriteEndObject();
    }
}
