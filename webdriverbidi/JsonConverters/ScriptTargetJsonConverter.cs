// <copyright file="ScriptTargetJsonConverter.cs" company="WebDriverBidi.NET Committers">
// Copyright (c) WebDriverBidi.NET Committers. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.
// </copyright>

namespace WebDriverBidi.JsonConverters;

using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using WebDriverBidi.Script;

/// <summary>
/// The JSON converter for the ScriptTarget object.
/// </summary>
public class ScriptTargetJsonConverter : JsonConverter<ScriptTarget>
{
    /// <summary>
    /// Gets a value indicating whether this converter can read JSON values.
    /// Returns true for this converter (converter used for deserialization
    /// only).
    /// </summary>
    public override bool CanRead => true;

    /// <summary>
    /// Gets a value indicating whether this converter can write JSON values.
    /// Returns false for this converter (converter not used for
    /// serialization).
    /// </summary>
    public override bool CanWrite => false;

    /// <summary>
    /// Reads a JSON string and deserializes it to an object.
    /// </summary>
    /// <param name="reader">The JSON reader to use during deserialization.</param>
    /// <param name="objectType">The type of object to which to deserialize.</param>
    /// <param name="existingValue">The existing value of the object.</param>
    /// <param name="hasExistingValue">A value indicating whether the existing value is null.</param>
    /// <param name="serializer">The JSON serializer to use in deserialization.</param>
    /// <returns>The deserialized object created from JSON.</returns>
    public override ScriptTarget ReadJson(JsonReader reader, Type objectType, ScriptTarget? existingValue, bool hasExistingValue, JsonSerializer serializer)
    {
        var jsonObject = JObject.Load(reader);
        ScriptTarget target;
        if (jsonObject.ContainsKey("realm"))
        {
            target = new RealmTarget(string.Empty);
            serializer.Populate(jsonObject.CreateReader(), target);
            return target;
        }

        if (jsonObject.ContainsKey("context"))
        {
            target = new ContextTarget(string.Empty);
            serializer.Populate(jsonObject.CreateReader(), target);
            return target;
        }

        throw new WebDriverBidiException("Malformed response: ScriptTarget must contain either a 'realm' or a 'context' property");
    }

    /// <summary>
    /// Serializes an object and writes it to a JSON string.
    /// </summary>
    /// <param name="writer">The JSON writer to use during serialization.</param>
    /// <param name="value">The object to serialize.</param>
    /// <param name="serializer">The JSON serializer to use in serialization.</param>
    public override void WriteJson(JsonWriter writer, ScriptTarget? value, JsonSerializer serializer)
    {
        throw new NotImplementedException();
    }
}