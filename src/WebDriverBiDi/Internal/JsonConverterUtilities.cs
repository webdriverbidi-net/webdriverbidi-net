// <copyright file="JsonConverterUtilities.cs" company="WebDriverBiDi.NET Committers">
// Copyright (c) WebDriverBiDi.NET Committers. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.
// </copyright>

namespace WebDriverBiDi.Internal;

using System.Runtime.CompilerServices;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Text.Json.Serialization.Metadata;

/// <summary>
/// Provides utilities for converting deserialized JSON data to proper formats.
/// </summary>
internal static class JsonConverterUtilities
{
    /// <summary>
    /// Reads a nested value from within a custom converter by invoking the converter of its type directly,
    /// rather than by re-entering the serializer, after testing that the stack can accommodate it.
    /// </summary>
    /// <typeparam name="T">The type of the value to read.</typeparam>
    /// <param name="reader">The reader, positioned on the first token of the value.</param>
    /// <param name="typeInfo">The type info of the value, obtained from <paramref name="options"/>.</param>
    /// <param name="options">The options in effect for the enclosing deserialization.</param>
    /// <returns>The deserialized value.</returns>
    /// <remarks>
    /// <para>
    /// A converter reading a recursive protocol structure (a <c>script.RemoteValue</c> nests one inside
    /// another to any depth) calls this once per level. Re-entering the serializer at each level with
    /// <see cref="JsonSerializer.Deserialize{TValue}(ref Utf8JsonReader, JsonTypeInfo{TValue})"/> puts a
    /// complete serializer entry frame, with its own exception handling, on the stack per level. That costs
    /// several times the stack of a direct call per level, and far more still when a value deep in the
    /// structure is malformed and its exception propagates through every one of those frames: on a 1 MB stack,
    /// a malformed value fewer than a hundred levels deep exhausted the stack that way. Invoking the value's
    /// converter directly adds only the converter's own frame per level, so the depth a given stack can read,
    /// successfully or not, is many times greater.
    /// </para>
    /// <para>
    /// Every level of every recursive structure the library reads passes through this method, which a
    /// convention test enforces, so this is where the recursion tests the remaining stack. A value nested too
    /// deeply for the current thread fails with a catchable <see cref="InsufficientExecutionStackException"/>
    /// rather than overflowing the stack, which would terminate the process; the transport reports it as a
    /// failure to deserialize the message. The transport's maximum JSON depth keeps the recursion well within
    /// the stack of any ordinary thread, so this is a safeguard for an unusually small stack.
    /// </para>
    /// <para>
    /// The converter of a <see cref="JsonTypeInfo{T}"/> is always a <see cref="JsonConverter{T}"/>; a
    /// converter factory is resolved to the converter it creates before the type info is returned. Unlike
    /// the serializer, a converter does not treat a JSON <c>null</c> token specially, so callers test for it
    /// before calling this method, as every caller in this library already must to enforce the protocol's
    /// non-null values.
    /// </para>
    /// </remarks>
    /// <exception cref="InsufficientExecutionStackException">Thrown when the current thread's stack cannot accommodate reading the value.</exception>
    public static T? ReadNestedValue<T>(ref Utf8JsonReader reader, JsonTypeInfo<T> typeInfo, JsonSerializerOptions options)
    {
        RuntimeHelpers.EnsureSufficientExecutionStack();
        return ((JsonConverter<T>)typeInfo.Converter).Read(ref reader, typeof(T), options);
    }

    /// <summary>
    /// Converts overflow JSON data into appropriate read-only .NET data structures.
    /// </summary>
    /// <param name="overflowData">A dictionary containing JsonElements to be converted.</param>
    /// <returns>A read-only, immutable data structure of .NET objects.</returns>
    public static ReceivedDataDictionary ConvertIncomingExtensionData(Dictionary<string, JsonElement> overflowData)
    {
        Dictionary<string, object?> receivedData = [];
        foreach (KeyValuePair<string, JsonElement> entry in overflowData)
        {
            receivedData[entry.Key] = ProcessJsonElement(entry.Value);
        }

        return new ReceivedDataDictionary(receivedData);
    }

    private static object? ProcessJsonElement(JsonElement element)
    {
        if (element.ValueKind == JsonValueKind.Object)
        {
            return ProcessObject(element);
        }
        else if (element.ValueKind == JsonValueKind.Array)
        {
            return ProcessList(element);
        }
        else
        {
            return ProcessValue(element);
        }
    }

    private static ReceivedDataDictionary ProcessObject(JsonElement objectElement)
    {
        Dictionary<string, object?> processedObject = [];
        foreach (JsonProperty objectProperty in objectElement.EnumerateObject())
        {
            processedObject[objectProperty.Name] = ProcessJsonElement(objectProperty.Value);
        }

        return new ReceivedDataDictionary(processedObject);
    }

    private static ReceivedDataList ProcessList(JsonElement listElement)
    {
        List<object?> processedList = [];
        foreach (JsonElement listItem in listElement.EnumerateArray())
        {
            processedList.Add(ProcessJsonElement(listItem));
        }

        return new ReceivedDataList(processedList);
    }

    private static object? ProcessValue(JsonElement valueElement)
    {
        if (valueElement.ValueKind == JsonValueKind.Null)
        {
            return null;
        }
        else if (valueElement.ValueKind == JsonValueKind.True)
        {
            return true;
        }
        else if (valueElement.ValueKind == JsonValueKind.False)
        {
            return false;
        }
        else if (valueElement.ValueKind == JsonValueKind.Number)
        {
            if (valueElement.TryGetInt64(out long longValue))
            {
                return longValue;
            }

#if NETSTANDARD2_0
            if (valueElement.TryGetDouble(out double doubleValue))
            {
                return doubleValue;
            }

            // Only the netstandard2.0 build can run on .NET Framework, where parsing a
            // syntactically valid JSON number whose magnitude exceeds the range of double
            // fails rather than rounding to signed infinity as .NET Core 3.0 and later
            // runtimes do; such overflow is the only way parsing a number token can fail.
            // Produce the same signed infinity the modern runtimes produce, rather than
            // silently yielding 0.0 from the failed parse. This branch cannot execute on
            // a modern runtime (the parse above always succeeds there), so it is
            // exercised only when running on .NET Framework.
            return valueElement.GetRawText().StartsWith("-", StringComparison.Ordinal) ? double.NegativeInfinity : double.PositiveInfinity;
#else
            // A syntactically valid JSON number always parses successfully on the
            // runtimes that can load the modern builds; magnitudes beyond the range of
            // double round to signed infinity, so the return value of TryGetDouble
            // needs no inspection.
            _ = valueElement.TryGetDouble(out double doubleValue);
            return doubleValue;
#endif
        }
        else
        {
            return valueElement.ToString();
        }
    }
}
