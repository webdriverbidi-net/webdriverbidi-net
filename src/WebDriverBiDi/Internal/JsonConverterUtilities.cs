// <copyright file="JsonConverterUtilities.cs" company="WebDriverBiDi.NET Committers">
// Copyright (c) WebDriverBiDi.NET Committers. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.
// </copyright>

namespace WebDriverBiDi.Internal;

using System.Text.Json;

/// <summary>
/// Provides utilities for converting deserialized JSON data to proper formats.
/// </summary>
public static class JsonConverterUtilities
{
    /// <summary>
    /// Converts overflow JSON data into appropriate read-only .NET data structures.
    /// </summary>
    /// <param name="overflowData">A dictionary containing JsonElements to be converted.</param>
    /// <returns>A read-only, immutable data structure of .NET objects.</returns>
    public static ReceivedDataDictionary ConvertIncomingExtensionData(Dictionary<string, JsonElement> overflowData)
    {
        Dictionary<string, object?> receivedData = new();
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
        Dictionary<string, object?> processedObject = new();
        foreach (JsonProperty objectProperty in objectElement.EnumerateObject())
        {
            processedObject[objectProperty.Name] = ProcessJsonElement(objectProperty.Value);
        }

        return new ReceivedDataDictionary(processedObject);
    }

    private static ReceivedDataList ProcessList(JsonElement listElement)
    {
        List<object?> processedList = new();
        foreach (JsonElement listItem in listElement.EnumerateArray())
        {
            processedList.Add(ProcessJsonElement(listItem));
        }

        return new ReceivedDataList(processedList);
    }

    private static object? ProcessValue(JsonElement valueElement)
    {
        if (valueElement.ValueKind == JsonValueKind.Undefined || valueElement.ValueKind == JsonValueKind.Null)
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
            else
            {
                _ = valueElement.TryGetDouble(out double doubleValue);
                return doubleValue;
            }
        }
        else
        {
            return valueElement.ToString();
        }
    }
}