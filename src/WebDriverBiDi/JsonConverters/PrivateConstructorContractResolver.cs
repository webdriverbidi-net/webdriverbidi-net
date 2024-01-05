// <copyright file="PrivateConstructorContractResolver.cs" company="WebDriverBiDi.NET Committers">
// Copyright (c) WebDriverBiDi.NET Committers. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.
// </copyright>

namespace WebDriverBiDi.JsonConverters;

using System.Reflection;
using System.Text.Json;
using System.Text.Json.Serialization.Metadata;

/// <summary>
/// A contract resolver to allow serialization of objects with non-public constructors.
/// </summary>
public class PrivateConstructorContractResolver : DefaultJsonTypeInfoResolver
{
    /// <summary>
    /// Gets the JsonTypeInfo for the specified type.
    /// </summary>
    /// <param name="type">The Type for which to get the JsonTypeInfo.</param>
    /// <param name="options">The JsonSerializerOptions to use in serialization.</param>
    /// <returns>The JsonTypeInfo for the given type.</returns>
    public override JsonTypeInfo GetTypeInfo(Type type, JsonSerializerOptions options)
    {
        JsonTypeInfo jsonTypeInfo = base.GetTypeInfo(type, options);

        if (jsonTypeInfo.Kind == JsonTypeInfoKind.Object && jsonTypeInfo.CreateObject is null)
        {
            if (jsonTypeInfo.Type.GetConstructors(BindingFlags.Public | BindingFlags.Instance).Length == 0)
            {
                // The type doesn't have public constructors
                jsonTypeInfo.CreateObject = () => Activator.CreateInstance(jsonTypeInfo.Type, true);
            }
        }

        return jsonTypeInfo;
    }
}
