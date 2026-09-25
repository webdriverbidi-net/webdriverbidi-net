// <copyright file="CapabilityWriter.cs" company="WebDriverBiDi.NET Committers">
// Copyright (c) WebDriverBiDi.NET Committers. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.
// </copyright>

namespace WebDriverBiDi.Browsers;

using System.Collections;
using System.Globalization;
using System.Text.Json;
using System.Text.Json.Serialization.Metadata;
using WebDriverBiDi.JsonConverters;
using WebDriverBiDi.Session;

/// <summary>
/// Writes WebDriver capabilities as JSON without reflection, so that it works under native AOT.
/// A value is <see langword="null"/>, a <see cref="string"/>, a <see cref="bool"/>, a number, a
/// dictionary with string keys whose values are capability values, a sequence of capability values,
/// or a <see cref="SerializedValue"/>.
/// </summary>
internal static class CapabilityWriter
{
    /// <summary>
    /// Writes a new session request, whose capabilities are matched as the only first-match entry.
    /// </summary>
    /// <param name="capabilities">The capabilities.</param>
    /// <returns>The request body as JSON.</returns>
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
    /// Serializes a proxy configuration as the core library writes it in a session.new command.
    /// </summary>
    /// <param name="proxy">The proxy configuration.</param>
    /// <returns>The serialized proxy configuration.</returns>
    /// <exception cref="BrowserLauncherConfigurationException">Thrown when the proxy configuration cannot be written.</exception>
    public static SerializedValue SerializeProxy(ProxyConfiguration proxy)
    {
        // The transport rejects an extension entry that repeats a property name; serializing with the context alone does not.
        JsonTypeInfo typeInfo = WebDriverBiDiJsonSerializerContext.Default.GetTypeInfo(proxy.GetType())!;
        if (proxy.AdditionalData.Keys.FirstOrDefault(key => typeInfo.Properties.Any(property => !property.IsExtensionData && property.Name == key)) is string repeated)
        {
            throw new BrowserLauncherConfigurationException($"The proxy capability's AdditionalData has an entry named {repeated}, which the proxy configuration already writes; set its property instead.");
        }

        try
        {
            return new SerializedValue(JsonSerializer.Serialize(proxy, WebDriverBiDiJsonSerializerContext.Default.ProxyConfiguration));
        }
        catch (Exception ex) when (ex is NotSupportedException || ex is InvalidOperationException || ex is JsonException)
        {
            throw new BrowserLauncherConfigurationException($"The proxy capability cannot be written: {ex.Message}", ex);
        }
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
            case null or string or bool or SerializedValue:
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
            case SerializedValue serialized:
                writer.WriteRawValue(serialized.Json, skipInputValidation: true);
                break;
            case IDictionary dictionary:
                writer.WriteStartObject();
                foreach (DictionaryEntry entry in dictionary)
                {
                    // Values are validated by FindUnsupportedValue before they are written.
                    string key = (string)entry.Key;
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
                writer.WriteNumberValue(Convert.ToInt64(value, CultureInfo.InvariantCulture));
                break;
        }
    }

    /// <summary>
    /// A capability value already serialized as JSON.
    /// </summary>
    /// <param name="json">The JSON.</param>
    internal sealed class SerializedValue(string json)
    {
        /// <summary>
        /// Gets the JSON.
        /// </summary>
        public string Json { get; } = json;
    }
}
