// <copyright file="DiscriminatedUnionJsonConverter{T}.cs" company="WebDriverBiDi.NET Committers">
// Copyright (c) WebDriverBiDi.NET Committers. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.
// </copyright>

namespace WebDriverBiDi.JsonConverters;

using System.Diagnostics.CodeAnalysis;
using System.Reflection;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Text.Json.Serialization.Metadata;
using WebDriverBiDi.Internal;

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

    // The readers for the derived types are constructed at run time (see CreateDerivedTypeReader). Under native
    // AOT, a generic type can be instantiated at run time over reference types only when the compiler has
    // emitted the shared code for that generic type, which it does only for an instantiation it can see. This
    // instantiation, over T itself, is that instantiation; it is never used to read anything. Removing it makes
    // every union read fail under native AOT, which the AOT test application exercises.
    private static readonly DerivedTypeReader CanonicalCodeRoot = new DerivedTypeReader<T>();

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
    /// <returns>An instance of a derived type of <typeparamref name="T"/> as described by the JSON.</returns>
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
                    propertyDescriptionMessage = $"one of the following properties: {string.Join(", ", LazyTypeInfo.Value.DiscriminatorToReaderMap.Keys)}";
                }

                throw new JsonException($"JSON for '{typeToConvert.Name}' must contain {propertyDescriptionMessage}");
            }
        }

        if (!LazyTypeInfo.Value.DiscriminatorToReaderMap.TryGetValue(discriminatedTypeValue, out DerivedTypeReader? derivedTypeReader))
        {
            if (LazyTypeInfo.Value.UnmatchedTypeReader is not null)
            {
                derivedTypeReader = LazyTypeInfo.Value.UnmatchedTypeReader;
            }
            else
            {
                throw new JsonException($"JSON for '{typeToConvert.Name}' {LazyTypeInfo.Value.DiscriminatorPropertyName} property contains unknown value '{discriminatedTypeValue}'");
            }
        }

        // Read through the derived type's own converter rather than by re-entering the serializer; see
        // JsonConverterUtilities.ReadNestedValue for why this matters for a union that nests inside itself,
        // as script.RemoteValue does.
        return derivedTypeReader.Read(ref reader, options);
    }

    /// <summary>
    /// Serializes an object of base type T to a JSON string.
    /// </summary>
    /// <param name="writer">A Utf8JsonWriter used to write the JSON string.</param>
    /// <param name="value">The object to be serialized.</param>
    /// <param name="options">The JsonSerializationOptions used for serializing the object.</param>
    public override void Write(Utf8JsonWriter writer, T value, JsonSerializerOptions options)
    {
        // Use the JsonSerializer.Serialize() overload that takes a JsonTypeInfo
        // to remove warnings when publishing AOT compiled applications.
        JsonTypeInfo typeInfo = options.GetTypeInfo(value.GetType());
        JsonSerializer.Serialize(writer, value, typeInfo);
    }

    /// <summary>
    /// Creates the reader that deserializes a derived type through its own converter.
    /// </summary>
    /// <param name="derivedType">The derived type to read.</param>
    /// <returns>The reader for <paramref name="derivedType"/>.</returns>
    /// <exception cref="InvalidOperationException">Thrown when <paramref name="derivedType"/> does not derive from <typeparamref name="T"/>.</exception>
    /// <remarks>
    /// The derived types are named by <see cref="DiscriminatedDerivedTypeAttribute"/> as <see cref="Type"/>
    /// values, but a converter can be invoked directly only through its generic type, so the reader is
    /// constructed over the derived type at run time, once per derived type, when the union's metadata is
    /// first read. Every type argument is a reference type, so under native AOT the construction needs no
    /// code generation: it uses the shared code compiled for the instantiation rooted by
    /// <c>CanonicalCodeRoot</c>.
    /// </remarks>
    [UnconditionalSuppressMessage("AOT", "IL3050:RequiresDynamicCode", Justification = "Every type argument is a reference type (the base type is constrained to class and each derived type is verified to derive from it), so the instantiation uses the shared canonical code that the CanonicalCodeRoot field causes to be compiled ahead of time.")]
    [UnconditionalSuppressMessage("Trimming", "IL2072:DynamicallyAccessedMembers", Justification = "The reader's parameterless constructor is preserved by the DynamicDependency attribute on this method.")]
    [DynamicDependency(DynamicallyAccessedMemberTypes.PublicParameterlessConstructor, typeof(DerivedTypeReader<>))]
    private static DerivedTypeReader CreateDerivedTypeReader(Type derivedType)
    {
        if (!typeof(T).IsAssignableFrom(derivedType))
        {
            throw new InvalidOperationException($"Derived type {derivedType.FullName} must derive from {typeof(T).FullName}");
        }

        // Activator.CreateInstance returns null only for a nullable value type, and the reader is a class,
        // so the null-forgiving operator is appropriate here.
        return (DerivedTypeReader)Activator.CreateInstance(typeof(DerivedTypeReader<>).MakeGenericType(typeof(T), derivedType))!;
    }

    private static DiscriminatedTypeInfo InitializeDiscriminatedTypeInfo(Type baseType)
    {
        DiscriminatedTypePropertyAttribute? typePropertyAttribute = baseType.GetCustomAttribute<DiscriminatedTypePropertyAttribute>(false);
        DiscriminatedTypePresenceAttribute? typePresenceAttribute = baseType.GetCustomAttribute<DiscriminatedTypePresenceAttribute>(false);
        DiscriminatedTypeInfo discriminatedTypeInfo = (typePropertyAttribute, typePresenceAttribute) switch
        {
            ({ } valueAttr, _) => new()
            {
                DiscriminatorPropertyName = valueAttr.PropertyName,
                UnmatchedTypeReader = valueAttr.UnmatchedValueType is null ? null : CreateDerivedTypeReader(valueAttr.UnmatchedValueType),
                PropertyMissingBehavior = valueAttr.PropertyMissingBehavior,
                PropertyMatchingBehavior = DiscriminatorPropertyMatchingBehavior.Value,
            },
            (null, { } presenceAttr) => new()
            {
                DiscriminatorPropertyName = string.Empty,
                UnmatchedTypeReader = null,
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

            discriminatedTypeInfo.DiscriminatorToReaderMap[discriminatorValue] = CreateDerivedTypeReader(attr.DerivedType);
        }

        return discriminatedTypeInfo;
    }

    private bool TryGetDiscriminatorPropertyValue(ref Utf8JsonReader reader, [NotNullWhen(true)] out string? discriminatorValue)
    {
        // Utf8JsonReader is a forward-only reader, so we create a copy to read
        // the JSON to find the value of the discriminator property. This leaves
        // the original reader in the correct position to read the JSON again for
        // deserialization. The object's prefix is scanned twice (once to find
        // the discriminator, once during deserialization), but no intermediate
        // JsonDocument is ever materialized.
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
                if (LazyTypeInfo.Value.DiscriminatorToReaderMap.ContainsKey(propertyName))
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

        public DerivedTypeReader? UnmatchedTypeReader { get; init; }

        public Dictionary<string, DerivedTypeReader> DiscriminatorToReaderMap { get; } = [];
    }

    /// <summary>
    /// Reads one derived type of the union through that type's own converter.
    /// </summary>
    private abstract class DerivedTypeReader
    {
        /// <summary>
        /// Reads a value of the derived type.
        /// </summary>
        /// <param name="reader">The reader, positioned on the start of the value's object.</param>
        /// <param name="options">The options in effect for the enclosing deserialization.</param>
        /// <returns>The deserialized value.</returns>
        public abstract T? Read(ref Utf8JsonReader reader, JsonSerializerOptions options);
    }

    /// <summary>
    /// Reads the derived type <typeparamref name="TDerived"/> through its own converter.
    /// </summary>
    /// <typeparam name="TDerived">The derived type to read.</typeparam>
    private sealed class DerivedTypeReader<TDerived> : DerivedTypeReader
        where TDerived : T
    {
        /// <inheritdoc/>
        public override T? Read(ref Utf8JsonReader reader, JsonSerializerOptions options)
        {
            return JsonConverterUtilities.ReadNestedValue(ref reader, (JsonTypeInfo<TDerived>)options.GetTypeInfo(typeof(TDerived)), options);
        }
    }
}
