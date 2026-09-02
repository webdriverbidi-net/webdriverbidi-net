// <copyright file="JsonConverterUtilities.cs" company="WebDriverBiDi.NET Committers">
// Copyright (c) WebDriverBiDi.NET Committers. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.
// </copyright>

namespace WebDriverBiDi.Internal;

using System.Text.Json;

/// <summary>
/// Provides utilities for converting deserialized JSON data to proper formats.
/// </summary>
internal static class JsonConverterUtilities
{
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
