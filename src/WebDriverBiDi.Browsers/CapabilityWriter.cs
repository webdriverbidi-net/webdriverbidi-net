// <copyright file="CapabilityWriter.cs" company="WebDriverBiDi.NET Committers">
// Copyright (c) WebDriverBiDi.NET Committers. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.
// </copyright>

namespace WebDriverBiDi.Browsers;

using System.Collections;
using System.Globalization;
using System.Text.Json;

/// <summary>
/// Writes WebDriver capabilities as JSON without reflection, so that it works under native AOT.
/// A value is <see langword="null"/>, a <see cref="string"/>, a <see cref="bool"/>, a number, a
/// dictionary with string keys whose values are capability values, or a sequence of capability values.
/// </summary>
internal static class CapabilityWriter
{
    /// <summary>
    /// Writes a new session request, whose capabilities are matched as the only first-match entry.
    /// </summary>
    /// <param name="capabilities">The capabilities.</param>
    /// <returns>The request body as JSON.</returns>
    /// <exception cref="ArgumentException">Thrown when a value is not a capability value.</exception>
    public static string WriteNewSessionRequest(IDictionary<string, object?> capabilities)
    {
        using MemoryStream stream = new();
        using (Utf8JsonWriter writer = new(stream))
        {
            writer.WriteStartObject();
            writer.WriteStartObject("capabilities");
            writer.WriteStartArray("firstMatch");
            WriteValue(writer, capabilities, "capabilities");
            writer.WriteEndArray();
            writer.WriteEndObject();
            writer.WriteEndObject();
        }

        return System.Text.Encoding.UTF8.GetString(stream.ToArray());
    }

    /// <summary>
    /// Finds a value, within a capability value, that cannot be written.
    /// </summary>
    /// <param name="value">The capability value.</param>
    /// <param name="path">The path naming the value, such as "goog:chromeOptions".</param>
    /// <returns>A description of the first value that cannot be written, or <see langword="null"/> if every value can be.</returns>
    public static string? FindUnsupportedValue(object? value, string path)
    {
        switch (value)
        {
            case null or string or bool:
                return null;
            case double number when double.IsNaN(number) || double.IsInfinity(number):
                return $"{path} is {number}, which JSON cannot represent";
            case float number when float.IsNaN(number) || float.IsInfinity(number):
                return $"{path} is {number}, which JSON cannot represent";
            case IDictionary dictionary:
                foreach (DictionaryEntry entry in dictionary)
                {
                    if (entry.Key is not string key)
                    {
                        return $"{path} has a key of type {entry.Key.GetType().Name}; keys must be strings";
                    }

                    if (FindUnsupportedValue(entry.Value, $"{path}.{key}") is string unsupported)
                    {
                        return unsupported;
                    }
                }

                return null;
            case IEnumerable sequence:
                int index = 0;
                foreach (object? item in sequence)
                {
                    if (FindUnsupportedValue(item, $"{path}[{index++}]") is string unsupported)
                    {
                        return unsupported;
                    }
                }

                return null;
            default:
                return IsNumber(value) ? null : $"{path} has a value of type {value.GetType().Name}; values must be null, strings, Booleans, numbers, dictionaries with string keys, or sequences";
        }
    }

    private static bool IsNumber(object value)
    {
        return value is sbyte or byte or short or ushort or int or uint or long or ulong or float or double or decimal;
    }

    private static void WriteValue(Utf8JsonWriter writer, object? value, string path)
    {
        switch (value)
        {
            case null:
                writer.WriteNullValue();
                break;
            case string text:
                writer.WriteStringValue(text);
                break;
            case bool flag:
                writer.WriteBooleanValue(flag);
                break;
            case IDictionary dictionary:
                writer.WriteStartObject();
                foreach (DictionaryEntry entry in dictionary)
                {
                    string key = entry.Key as string ?? throw new ArgumentException(FindUnsupportedValue(dictionary, path));
                    writer.WritePropertyName(key);
                    WriteValue(writer, entry.Value, $"{path}.{key}");
                }

                writer.WriteEndObject();
                break;
            case IEnumerable sequence:
                writer.WriteStartArray();
                int index = 0;
                foreach (object? item in sequence)
                {
                    WriteValue(writer, item, $"{path}[{index++}]");
                }

                writer.WriteEndArray();
                break;
            case double number:
                writer.WriteNumberValue(number);
                break;
            case float number:
                writer.WriteNumberValue(number);
                break;
            case decimal number:
                writer.WriteNumberValue(number);
                break;
            case ulong unsignedLong:
                writer.WriteNumberValue(unsignedLong);
                break;
            default:
                writer.WriteNumberValue(IsNumber(value) ? Convert.ToInt64(value, CultureInfo.InvariantCulture) : throw new ArgumentException(FindUnsupportedValue(value, path)));
                break;
        }
    }
}
