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
/// <remarks>
/// This custom converter is used for <see cref="Command"/> objects so that we do not have
/// to use the System.Text.Json attribute-based polymorphic serialization. That would require
/// the base <see cref="CommandParameters"/> class to maintain a list of <see cref="JsonDerivedType"/>
/// attributes for each command parameter objects. This makes it easier for consumers of this
/// library to create command parameters classes for custom commands.
/// </remarks>
public class CommandJsonConverter : JsonConverter<Command>
{
    /// <summary>
    /// The property names the command envelope writes itself. An entry in
    /// <see cref="Command.AdditionalCommandProperties"/> under one of these names would be emitted as a
    /// second property of the same name alongside the one the envelope already wrote.
    /// </summary>
    private static readonly HashSet<string> ReservedEnvelopePropertyNames = new(StringComparer.Ordinal)
    {
        "id",
        "method",
        "params",
    };

    /// <summary>
    /// Deserializes the JSON string to a Command object.
    /// </summary>
    /// <param name="reader">A Utf8JsonReader used to read the incoming JSON.</param>
    /// <param name="typeToConvert">The Type description of the type to convert.</param>
    /// <param name="options">The JsonSerializationOptions used for deserializing the JSON.</param>
    /// <returns>A Command object.</returns>
    /// <exception cref="NotSupportedException">Thrown when called, as this converter is only used for serialization.</exception>
    public override Command? Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        throw new NotSupportedException("CommandJsonConverter does not support deserialization; commands never need to be deserialized for incoming use.");
    }

    /// <summary>
    /// Serializes a Command object to a JSON string.
    /// </summary>
    /// <param name="writer">A Utf8JsonWriter used to write the JSON string.</param>
    /// <param name="value">The Command to be serialized.</param>
    /// <param name="options">The JsonSerializationOptions used for serializing the object.</param>
    /// <remarks>
    /// The IL2026/IL3050 suppressions on this method cover the single call site that
    /// serializes entries from <see cref="Command.AdditionalCommandProperties"/>, the
    /// envelope-level extension properties written as siblings of <c>id</c>, <c>method</c>
    /// and <c>params</c>. Those entries are typed as <see cref="object"/> by design: they
    /// exist to let a custom <see cref="Transport"/> add envelope fields whose runtime types
    /// are not known at library build time, so the reflection-based serialization path is
    /// the only correct choice. (<see cref="CommandParameters.AdditionalData"/>, the extension
    /// properties inside <c>params</c>, is not serialized here; it flows through the
    /// parameters type's own <see cref="JsonTypeInfo"/> as extension data.) Users who need
    /// AOT-safe extension fields must register a <see cref="JsonTypeInfo"/> for every runtime
    /// type they add to either dictionary via
    /// <see cref="BiDiDriver.RegisterTypeInfoResolverAsync(IJsonTypeInfoResolver, CancellationToken)"/>
    /// before sending a command that uses it.
    /// </remarks>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="value"/> is <see langword="null"/>.</exception>
    /// <exception cref="WebDriverBiDiSerializationException">
    /// Thrown when an extension-data entry would shadow a property the message already writes: an entry in
    /// <see cref="Command.AdditionalCommandProperties"/> named <c>id</c>, <c>method</c> or <c>params</c>, or an
    /// entry in <see cref="CommandParameters.AdditionalData"/> whose name matches one the parameters type
    /// serializes.
    /// </exception>
    [UnconditionalSuppressMessage("Trimming", "IL2026:RequiresUnreferencedCode", Justification = "AdditionalCommandProperties entries are typed as object by design; see remarks.")]
    [UnconditionalSuppressMessage("AOT", "IL3050:RequiresDynamicCode", Justification = "AdditionalCommandProperties entries are typed as object by design; see remarks.")]
    public override void Write(Utf8JsonWriter writer, Command value, JsonSerializerOptions options)
    {
        if (value is null)
        {
            throw new ArgumentNullException(nameof(value));
        }

        // Resolve the parameters type info and validate both extension-data dictionaries before writing
        // anything, so a command that cannot be represented is rejected outright rather than emitted as a
        // message whose meaning depends on how the remote end resolves a duplicate property name.
        // Use the JsonSerializer.Serialize() overload that takes a JsonTypeInfo
        // to remove warnings when publishing AOT compiled applications.
        JsonTypeInfo paramsTypeInfo = options.GetTypeInfo(value.CommandParameters.GetType());
        ThrowIfEnvelopePropertyIsShadowed(value);
        ThrowIfParametersPropertyIsShadowed(value.CommandParameters, paramsTypeInfo);

        writer.WriteStartObject();
        writer.WritePropertyName("id");
        writer.WriteNumberValue(value.CommandId);
        writer.WritePropertyName("method");
        writer.WriteStringValue(value.CommandName);
        writer.WritePropertyName("params");
        JsonSerializer.Serialize(writer, value.CommandParameters, paramsTypeInfo);

        // This will now serialize the additional overflow properties for the
        // command envelope.
        foreach (KeyValuePair<string, object?> pair in value.AdditionalCommandProperties)
        {
            writer.WritePropertyName(pair.Key);
            JsonSerializer.Serialize(writer, pair.Value, options);
        }

        writer.WriteEndObject();
    }

    /// <summary>
    /// Throws when an envelope-level extension property would be written under a name the envelope itself uses.
    /// </summary>
    /// <param name="command">The command being serialized.</param>
    /// <exception cref="WebDriverBiDiSerializationException">Thrown when such an entry is present.</exception>
    private static void ThrowIfEnvelopePropertyIsShadowed(Command command)
    {
        foreach (KeyValuePair<string, object?> pair in command.AdditionalCommandProperties)
        {
            if (ReservedEnvelopePropertyNames.Contains(pair.Key))
            {
                throw new WebDriverBiDiSerializationException($"Could not serialize command '{command.CommandName}' (command ID: {command.CommandId}): the AdditionalCommandProperties entry '{pair.Key}' uses a property name the command envelope writes itself, which would produce a duplicate property in the message. Rename the entry; 'id', 'method' and 'params' are reserved.");
            }
        }
    }

    /// <summary>
    /// Throws when a parameters-level extension property would be written under a name the parameters type
    /// already serializes.
    /// </summary>
    /// <param name="parameters">The command parameters being serialized.</param>
    /// <param name="parametersTypeInfo">The type info through which the parameters are serialized.</param>
    /// <exception cref="WebDriverBiDiSerializationException">Thrown when such an entry is present.</exception>
    /// <remarks>
    /// Only properties the serializer can write are considered: a member the type ignores, or one that has no
    /// getter, consumes no name in the payload, and the extension-data property itself holds these entries
    /// rather than competing with them.
    /// </remarks>
    private static void ThrowIfParametersPropertyIsShadowed(CommandParameters parameters, JsonTypeInfo parametersTypeInfo)
    {
        // AdditionalData is empty for all but a handful of commands, so this loop usually does not run at all.
        foreach (KeyValuePair<string, object?> pair in parameters.AdditionalData)
        {
            foreach (JsonPropertyInfo property in parametersTypeInfo.Properties)
            {
                if (!property.IsExtensionData && property.Get is not null && string.Equals(property.Name, pair.Key, StringComparison.Ordinal))
                {
                    throw new WebDriverBiDiSerializationException($"Could not serialize command '{parameters.MethodName}': the AdditionalData entry '{pair.Key}' uses a property name that {parameters.GetType()} already serializes, which would produce a duplicate property in the command parameters. Rename the entry, or set the typed property instead.");
                }
            }
        }
    }
}
