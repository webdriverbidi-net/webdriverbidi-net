// <copyright file="DiscriminatedUnionJsonConverter{T}.cs" company="WebDriverBiDi.NET Committers">
// Copyright (c) WebDriverBiDi.NET Committers. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.
// </copyright>

namespace WebDriverBiDi.JsonConverters;

using System.Diagnostics.CodeAnalysis;
using System.Reflection;
using System.Text.Json;
using System.Text.Json.Serialization;

/// <summary>
/// The JSON converter for the objects that form a discriminated union. That is, a base type
/// T with multiple derived types, where the JSON for each derived type contains a property
/// (the discriminator) that indicates which derived type the JSON represents. The converter
/// uses either the value of the discriminator property or the presence of specific properties
/// to determine which derived type to which to deserialize the JSON.
/// </summary>
/// <typeparam name="T">The base type of the discriminated union to be deserialized.</typeparam>
public class DiscriminatedUnionJsonConverter<[DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.All)] T> : JsonConverter<T>
    where T : class
{
    // Each closed generic type of this converter will have its own static field,
    // so this is effectively a thread-safe, lazily-initialized cache of the type
    // information for each base type T for which this converter is used.
    private static readonly Lazy<DiscriminatedTypeInfo> LazyTypeInfo = new(() => InitializeDiscriminatedTypeInfo(typeof(T)));

    /// <summary>
    /// Initializes a new instance of the <see cref="DiscriminatedUnionJsonConverter{T}"/> class.
    /// </summary>
    public DiscriminatedUnionJsonConverter()
    {
    }

    /// <summary>
    /// Deserializes the JSON string to an value of base type T.
    /// </summary>
    /// <param name="reader">A Utf8JsonReader used to read the incoming JSON.</param>
    /// <param name="typeToConvert">The Type description of the type to convert.</param>
    /// <param name="options">The JsonSerializationOptions used for deserializing the JSON.</param>
    /// <returns>A subclass of an EvaluateResult object as described by the JSON.</returns>
    /// <exception cref="JsonException">Thrown when invalid JSON is encountered.</exception>
    public override T? Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        if (!this.TryGetDiscriminatorPropertyValue(ref reader, out string? discriminatedTypeValue))
        {
            if (LazyTypeInfo.Value.PropertyMissingBehavior == DiscriminatorPropertyMissingValueBehavior.ReturnNull)
            {
                // We know the value of the discriminator property must be an object start,
                // because TryGetDiscriminatorPropertyValue would throw if it is not.
                // Consume the JSON tokens for the object value, and return null.
                reader.Read();
                while (reader.TokenType != JsonTokenType.EndObject)
                {
                    reader.Skip();
                    reader.Read();
                }

                return null;
            }
            else
            {
                string propertyDescriptionMessage = $"a '{LazyTypeInfo.Value.DiscriminatorPropertyName}' property";
                if (LazyTypeInfo.Value.PropertyMatchingBehavior == DiscriminatorPropertyMatchingBehavior.Presence)
                {
                    propertyDescriptionMessage = $"one of the following properties: {string.Join(", ", LazyTypeInfo.Value.DiscriminatorToTypeMap.Keys)}";
                }

                throw new JsonException($"JSON for '{typeToConvert.Name}' must contain {propertyDescriptionMessage}");
            }
        }

        if (!LazyTypeInfo.Value.DiscriminatorToTypeMap.TryGetValue(discriminatedTypeValue, out Type? targetType))
        {
            if (LazyTypeInfo.Value.UnmatchedType is not null)
            {
                targetType = LazyTypeInfo.Value.UnmatchedType;
            }
            else
            {
                throw new JsonException($"JSON for '{typeToConvert.Name}' {LazyTypeInfo.Value.DiscriminatorPropertyName} property contains unknown value '{discriminatedTypeValue}'");
            }
        }

        return (T?)JsonSerializer.Deserialize(ref reader, options.GetTypeInfo(targetType));
    }

    /// <summary>
    /// Serializes an object of base type T to a JSON string.
    /// </summary>
    /// <param name="writer">A Utf8JsonWriter used to write the JSON string.</param>
    /// <param name="value">The object to be serialized.</param>
    /// <param name="options">The JsonSerializationOptions used for serializing the object.</param>
    /// <exception cref="ArgumentNullException">Thrown if the value of T is null.</exception>
    public override void Write(Utf8JsonWriter writer, T value, JsonSerializerOptions options)
    {
        JsonSerializer.Serialize(writer, value, value.GetType(), options);
    }

    private static DiscriminatedTypeInfo InitializeDiscriminatedTypeInfo(Type baseType)
    {
        DiscriminatedTypePropertyAttribute? typePropertyAttribute = baseType.GetCustomAttribute<DiscriminatedTypePropertyAttribute>(false);
        DiscriminatedTypePresenceAttribute? typePresenceAttribute = baseType.GetCustomAttribute<DiscriminatedTypePresenceAttribute>(false);
        DiscriminatedTypeInfo discriminiatedTypeInfo = (typePropertyAttribute, typePresenceAttribute) switch
        {
            ({ } valueAttr, _) => new()
            {
                DiscriminatorPropertyName = valueAttr.PropertyName,
                UnmatchedType = valueAttr.UnmatchedValueType,
                PropertyMissingBehavior = valueAttr.PropertyMissingBehavior,
                PropertyMatchingBehavior = DiscriminatorPropertyMatchingBehavior.Value,
            },
            (null, { } presenceAttr) => new()
            {
                DiscriminatorPropertyName = string.Empty,
                UnmatchedType = null,
                PropertyMissingBehavior = presenceAttr.PropertyMissingBehavior,
                PropertyMatchingBehavior = DiscriminatorPropertyMatchingBehavior.Presence,
            },
            _ => throw new InvalidOperationException($"Type '{baseType.FullName}' must have a [DiscriminatedTypeProperty] or [DiscriminatedTypePresence] attribute to use {nameof(DiscriminatedUnionJsonConverter<>)}."),
        };

        IEnumerable<DiscriminatedDerivedTypeAttribute> derivedTypeAttributes = baseType.GetCustomAttributes<DiscriminatedDerivedTypeAttribute>(false);
        foreach (DiscriminatedDerivedTypeAttribute attr in derivedTypeAttributes)
        {
            string discriminatorValue = attr.Discriminator;
            if (string.IsNullOrEmpty(discriminatorValue))
            {
                throw new InvalidOperationException($"Derived type {attr.DerivedType.FullName} must have a non-empty Discriminator");
            }

            discriminiatedTypeInfo.DiscriminatorToTypeMap[discriminatorValue] = attr.DerivedType;
        }

        return discriminiatedTypeInfo;
    }

    private bool TryGetDiscriminatorPropertyValue(ref Utf8JsonReader reader, [NotNullWhen(true)] out string? discriminatorValue)
    {
        // Utf8JsonReader is a forward-only reader, so we create a copy to read
        // the JSON to find the value of the discriminator property. This leaves
        // the original reader in the correct position to read the JSON again for
        // deserialization. This also prevents double-parsing the document, once
        // to find the discriminator and once to deserialize.
        Utf8JsonReader readerCopy = reader;
        if (readerCopy.TokenType != JsonTokenType.StartObject)
        {
            throw new JsonException($"JSON for '{typeof(T).Name}' must be an object, but starting token was {readerCopy.TokenType}");
        }

        readerCopy.Read();
        while (readerCopy.TokenType == JsonTokenType.PropertyName)
        {
            string propertyName = readerCopy.GetString()!;
            if (LazyTypeInfo.Value.PropertyMatchingBehavior == DiscriminatorPropertyMatchingBehavior.Presence)
            {
                if (LazyTypeInfo.Value.DiscriminatorToTypeMap.ContainsKey(propertyName))
                {
                    discriminatorValue = propertyName;
                    return true;
                }
            }
            else
            {
                if (propertyName == LazyTypeInfo.Value.DiscriminatorPropertyName)
                {
                    // Consume the property name token and confirm the value is a string.
                    readerCopy.Read();
                    if (readerCopy.TokenType != JsonTokenType.String)
                    {
                        throw new JsonException($"JSON '{LazyTypeInfo.Value.DiscriminatorPropertyName}' property must be a string");
                    }

                    // Get the property value as a string, and break out of the loop.
                    discriminatorValue = readerCopy.GetString()!;
                    return true;
                }
            }

            // Skip the value of this property and continue searching for the discriminator property.
            readerCopy.Skip();
            readerCopy.Read();
        }

        discriminatorValue = null;
        return false;
    }

    private class DiscriminatedTypeInfo
    {
        public required string DiscriminatorPropertyName { get; init; }

        public required DiscriminatorPropertyMissingValueBehavior PropertyMissingBehavior { get; init; }

        public required DiscriminatorPropertyMatchingBehavior PropertyMatchingBehavior { get; init; }

        public Type? UnmatchedType { get; init; }

        public Dictionary<string, Type> DiscriminatorToTypeMap { get; } = [];
    }
}
