// <copyright file="CommandParametersContract.cs" company="WebDriverBiDi.NET Committers">
// Copyright (c) WebDriverBiDi.NET Committers. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.
// </copyright>

namespace WebDriverBiDi.JsonConverters;

using System.Reflection;
using System.Text.Json.Serialization.Metadata;

/// <summary>
/// Keeps the members that describe a command, rather than carry its parameters, out of the <c>params</c> object.
/// </summary>
/// <remarks>
/// <para>
/// <see cref="CommandParameters.MethodName"/> and <see cref="CommandParameters.ResponseType"/> tell the transport
/// which command to send and how to read its response; they are not parameters. The base declarations are marked
/// <c>[JsonIgnore]</c>, but System.Text.Json does not carry that attribute onto an override, so a parameters type
/// whose override omits it would write its method name inside <c>params</c>. The members are identified by the
/// declaration they override rather than by their JSON name, which a naming policy or a source-generation context
/// may change.
/// </para>
/// <para>
/// The transport adds <see cref="RemoveCommandDescriptionProperties"/> to every resolver it serializes through: the
/// library's own, and each one registered with <see cref="Protocol.Transport.RegisterTypeInfoResolverAsync"/>. The
/// rule therefore holds for a consumer's parameters types as well as the library's, and under source generation as
/// well as reflection. It is added before <see cref="ExtensionDataNameGuard"/>, so that an extension-data entry is
/// not rejected for a name only a removed member would have written.
/// </para>
/// </remarks>
internal static class CommandParametersContract
{
    /// <summary>
    /// Returns a resolver that applies <see cref="RemoveCommandDescriptionProperties"/> to every type info the given
    /// resolver creates.
    /// </summary>
    /// <param name="resolver">The resolver to modify.</param>
    /// <returns>The modified resolver.</returns>
    public static IJsonTypeInfoResolver AddTo(IJsonTypeInfoResolver resolver)
    {
        return resolver.WithAddedModifier(RemoveCommandDescriptionProperties);
    }

    /// <summary>
    /// Removes the overrides of <see cref="CommandParameters.MethodName"/> and <see cref="CommandParameters.ResponseType"/>
    /// from the contract of a parameters type.
    /// </summary>
    /// <param name="typeInfo">The type info to modify.</param>
    public static void RemoveCommandDescriptionProperties(JsonTypeInfo typeInfo)
    {
        if (typeInfo.Kind != JsonTypeInfoKind.Object || !typeof(CommandParameters).IsAssignableFrom(typeInfo.Type))
        {
            return;
        }

        for (int index = typeInfo.Properties.Count - 1; index >= 0; index--)
        {
            if (IsCommandDescriptionProperty(typeInfo.Properties[index]))
            {
                typeInfo.Properties.RemoveAt(index);
            }
        }
    }

    private static bool IsCommandDescriptionProperty(JsonPropertyInfo property)
    {
        // A member marked [JsonIgnore] is never written, and source-generated metadata may then omit its attribute
        // provider, so a property with no provider is left in place; it writes nothing either way. A property that
        // merely shares a name with one of these members, such as one declared with 'new', has no getter or one whose
        // base definition is its own, and is kept as well.
        return property.AttributeProvider is PropertyInfo { Name: nameof(CommandParameters.MethodName) or nameof(CommandParameters.ResponseType), GetMethod: { } getter }
            && getter.GetBaseDefinition().DeclaringType == typeof(CommandParameters);
    }
}
