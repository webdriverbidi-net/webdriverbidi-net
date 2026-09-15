// <copyright file="ExtensionDataNameGuard.cs" company="WebDriverBiDi.NET Committers">
// Copyright (c) WebDriverBiDi.NET Committers. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.
// </copyright>

namespace WebDriverBiDi.JsonConverters;

using System.Text.Json;
using System.Text.Json.Serialization.Metadata;

/// <summary>
/// Rejects extension data that would be written under a property name its object already serializes.
/// </summary>
/// <remarks>
/// <para>
/// An object's <c>[JsonExtensionData]</c> entries are written alongside its declared properties, so an entry named
/// for one of those properties writes the name twice, and a JSON object with a duplicate name has no defined
/// meaning. Only the object's own metadata knows which names it writes, so the check is attached to that metadata
/// rather than made by walking a command before it is serialized.
/// </para>
/// <para>
/// The transport adds <see cref="GuardExtensionData"/> to every resolver it serializes through: the library's own,
/// and each one registered with <see cref="Protocol.Transport.RegisterTypeInfoResolverAsync"/>. The rule therefore
/// holds for every object in a command at any depth, for a consumer's types as well as the library's, and under
/// source generation as well as reflection.
/// </para>
/// </remarks>
internal static class ExtensionDataNameGuard
{
    /// <summary>
    /// Returns a resolver that applies the guard to every type info the given resolver creates.
    /// </summary>
    /// <param name="resolver">The resolver to guard.</param>
    /// <returns>The guarded resolver.</returns>
    public static IJsonTypeInfoResolver AddTo(IJsonTypeInfoResolver resolver)
    {
        return resolver.WithAddedModifier(GuardExtensionData);
    }

    /// <summary>
    /// Makes an object type's extension data throw when it is read for serialization while it holds an entry named
    /// for one of the type's serialized properties.
    /// </summary>
    /// <param name="typeInfo">The type info to modify.</param>
    public static void GuardExtensionData(JsonTypeInfo typeInfo)
    {
        if (typeInfo.Kind != JsonTypeInfoKind.Object)
        {
            return;
        }

        JsonPropertyInfo? extensionDataProperty = null;
        HashSet<string> serializedNames = new(StringComparer.Ordinal);
        foreach (JsonPropertyInfo property in typeInfo.Properties)
        {
            if (property.IsExtensionData)
            {
                extensionDataProperty = property;
            }
            else if (property.Get is not null)
            {
                // A property with no getter is never written, so it consumes no name in the payload.
                serializedNames.Add(property.Name);
            }
        }

        if (extensionDataProperty?.Get is not { } getExtensionData || serializedNames.Count == 0)
        {
            return;
        }

        string extensionDataName = extensionDataProperty.Name;
        Type ownerType = typeInfo.Type;
        extensionDataProperty.Get = owner =>
        {
            object? extensionData = getExtensionData(owner);

            // These are the two dictionary shapes System.Text.Json writes as extension data. It also accepts a
            // JsonObject, but writes that as an unnamed nested object, which is not valid JSON whatever its
            // entries are named, so guarding its names would protect nothing.
            string? shadowingName = extensionData switch
            {
                IDictionary<string, object?> objectEntries => FindShadowingName(objectEntries, serializedNames),
                IDictionary<string, JsonElement> elementEntries => FindShadowingName(elementEntries, serializedNames),
                _ => null,
            };

            if (shadowingName is not null)
            {
                throw new WebDriverBiDiSerializationException($"The {extensionDataName} entry '{shadowingName}' uses a property name that {ownerType} already serializes, which would produce a duplicate property in the same JSON object. Rename the entry, or set the typed property instead.");
            }

            return extensionData;
        };
    }

    private static string? FindShadowingName<TValue>(IDictionary<string, TValue> extensionData, HashSet<string> serializedNames)
    {
        // Extension data is empty for nearly every object, so check the count before allocating an enumerator.
        if (extensionData.Count == 0)
        {
            return null;
        }

        foreach (string name in extensionData.Keys)
        {
            if (serializedNames.Contains(name))
            {
                return name;
            }
        }

        return null;
    }
}
