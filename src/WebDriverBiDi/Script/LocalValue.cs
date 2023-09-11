// <copyright file="LocalValue.cs" company="WebDriverBiDi.NET Committers">
// Copyright (c) WebDriverBiDi.NET Committers. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.
// </copyright>

namespace WebDriverBiDi.Script;

using System.Globalization;
using System.Numerics;
using System.Text.Json.Serialization;

/// <summary>
/// Object representing a local value for use as an argument in script execution.
/// </summary>
public class LocalValue : ArgumentValue
{
    private string argType;
    private object? argValue;

    private LocalValue(string argType)
    {
        this.argType = argType;
    }

    /// <summary>
    /// Gets a LocalValue for "undefined".
    /// </summary>
    public static LocalValue Undefined => new("undefined");

    /// <summary>
    /// Gets a LocalValue for a null value.
    /// </summary>
    public static LocalValue Null => new("null");

    /// <summary>
    /// Gets a LocalValue for "NaN".
    /// </summary>
    public static LocalValue NaN => new("number") { argValue = double.NaN };

    /// <summary>
    /// Gets a LocalValue for negative zero (-0).
    /// </summary>
    public static LocalValue NegativeZero => new("number") { argValue = decimal.Negate(decimal.Zero) };

    /// <summary>
    /// Gets a LocalValue for positive infinity.
    /// </summary>
    public static LocalValue Infinity => new("number") { argValue = double.PositiveInfinity };

    /// <summary>
    /// Gets a LocalValue for negative infinity.
    /// </summary>
    public static LocalValue NegativeInfinity => new("number") { argValue = double.NegativeInfinity };

    /// <summary>
    /// Gets the type of this LocalValue.
    /// </summary>
    [JsonPropertyName("type")]
    public string Type { get => this.argType; private set => this.argType = value; }

    /// <summary>
    /// Gets the object containing the value of this LocalValue.
    /// </summary>
    [JsonIgnore]
    public object? Value { get => this.argValue; private set => this.argValue = value; }

    /// <summary>
    /// Gets the object containing the value of this LocalValue for serialization purposes.
    /// </summary>
    [JsonPropertyName("value")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    [JsonInclude]
    internal object? SerializableValue
    {
        get
        {
            if (this.argType == "number")
            {
                return this.GetSerializedNumericValue();
            }

            if (this.argType == "bigint")
            {
                // Note that static methods creating BigInteger values should
                // not allow a null value in the argValue member.
                return ((BigInteger)this.argValue!).ToString(CultureInfo.InvariantCulture);
            }

            if (this.argType == "date")
            {
                // Note that static methods creating DateTime values should
                // not allow a null value in the argValue member.
                return ((DateTime)this.argValue!).ToString("YYYY-MM-ddTHH:mm:ss.fffZ");
            }

            if (this.argType == "map" || this.argType == "object")
            {
                // One of the two cases (key is string, or key is LocalValue)
                // must be true.
                List<object> serializablePairList = new();
                Dictionary<LocalValue, LocalValue>? dictionaryValue = this.argValue as Dictionary<LocalValue, LocalValue>;
                Dictionary<string, LocalValue>? stringDictionaryValue = this.argValue as Dictionary<string, LocalValue>;
                if (dictionaryValue is not null)
                {
                    foreach (KeyValuePair<LocalValue, LocalValue> pair in dictionaryValue)
                    {
                        List<object> itemList = new() { pair.Key, pair.Value };
                        serializablePairList.Add(itemList);
                    }
                }

                if (stringDictionaryValue is not null)
                {
                    foreach (KeyValuePair<string, LocalValue> pair in stringDictionaryValue)
                    {
                        List<object> itemList = new() { pair.Key, pair.Value };
                        serializablePairList.Add(itemList);
                    }
                }

                return serializablePairList;
            }

            return this.argValue;
        }
    }

    /// <summary>
    /// Creates a LocalValue for a string.
    /// </summary>
    /// <param name="stringValue">The string to wrap as a LocalValue.</param>
    /// <returns>A LocalValue for a string.</returns>
    public static LocalValue String(string stringValue) => new("string") { argValue = stringValue };

    /// <summary>
    /// Creates a LocalValue for a number.
    /// </summary>
    /// <param name="numericValue">The integer to wrap as a LocalValue.</param>
    /// <returns>A LocalValue for a number.</returns>
    public static LocalValue Number(int numericValue) => new("number") { argValue = numericValue };

    /// <summary>
    /// Creates a LocalValue for a number.
    /// </summary>
    /// <param name="numericValue">The long to wrap as a LocalValue.</param>
    /// <returns>A LocalValue for a number.</returns>
    public static LocalValue Number(long numericValue) => new("number") { argValue = numericValue };

    /// <summary>
    /// Creates a LocalValue for a number.
    /// </summary>
    /// <param name="numericValue">The double to wrap as a LocalValue.</param>
    /// <returns>A LocalValue for a number.</returns>
    public static LocalValue Number(double numericValue) => new("number") { argValue = numericValue };

    /// <summary>
    /// Creates a LocalValue for a boolean value.
    /// </summary>
    /// <param name="boolValue">The boolean to wrap as a LocalValue.</param>
    /// <returns>A LocalValue for a boolean value.</returns>
    public static LocalValue Boolean(bool boolValue) => new("boolean") { argValue = boolValue };

    /// <summary>
    /// Creates a LocalValue for a BigInteger.
    /// </summary>
    /// <param name="bigIntValue">The BigInteger to wrap as a LocalValue.</param>
    /// <returns>A LocalValue for a BigInteger.</returns>
    public static LocalValue BigInt(BigInteger bigIntValue) => new("bigint") { argValue = bigIntValue };

    /// <summary>
    /// Creates a LocalValue for a DateTime value.
    /// </summary>
    /// <param name="dateTimeValue">The DateTime value to wrap as a LocalValue.</param>
    /// <returns>A LocalValue for a DateTime value.</returns>
    public static LocalValue Date(DateTime dateTimeValue) => new("date") { argValue = dateTimeValue };

    /// <summary>
    /// Creates a LocalValue for an array.
    /// </summary>
    /// <param name="arrayValue">The list of LocalValues to wrap as an array LocalValue.</param>
    /// <returns>A LocalValue for an array.</returns>
    public static LocalValue Array(List<LocalValue> arrayValue) => new("array") { argValue = arrayValue };

    /// <summary>
    /// Creates a LocalValue for a set.
    /// </summary>
    /// <param name="arrayValue">The list of LocalValues to wrap as a set LocalValue.</param>
    /// <returns>A LocalValue for a set.</returns>
    public static LocalValue Set(List<LocalValue> arrayValue) => new("set") { argValue = arrayValue };

    /// <summary>
    /// Creates a LocalValue for a map with string keys.
    /// </summary>
    /// <param name="mapValue">The dictionary with strings for keys and LocalValues for values to wrap as a map LocalValue.</param>
    /// <returns>A LocalValue for a map.</returns>
    public static LocalValue Map(Dictionary<string, LocalValue> mapValue) => new("map") { argValue = mapValue };

    /// <summary>
    /// Creates a LocalValue for a map with LocalValue keys.
    /// </summary>
    /// <param name="mapValue">The dictionary with LocalValues for keys and LocalValues for values to wrap as a map LocalValue.</param>
    /// <returns>A LocalValue for a map.</returns>
    public static LocalValue Map(Dictionary<LocalValue, LocalValue> mapValue) => new("map") { argValue = mapValue };

    /// <summary>
    /// Creates a LocalValue for an object with string keys.
    /// </summary>
    /// <param name="mapValue">The dictionary with strings for keys and LocalValues for values to wrap as an object LocalValue.</param>
    /// <returns>A LocalValue for an object.</returns>
    public static LocalValue Object(Dictionary<string, LocalValue> mapValue) => new("object") { argValue = mapValue };

    /// <summary>
    /// Creates a LocalValue for an object with LocalValue keys.
    /// </summary>
    /// <param name="mapValue">The dictionary with LocalValues for keys and LocalValues for values to wrap as an object LocalValue.</param>
    /// <returns>A LocalValue for an object.</returns>
    public static LocalValue Object(Dictionary<LocalValue, LocalValue> mapValue) => new("object") { argValue = mapValue };

    /// <summary>
    /// Creates a LocalValue for regular expression.
    /// </summary>
    /// <param name="pattern">The pattern for the regular expression.</param>
    /// <param name="flags">The flags of the regular expression.</param>
    /// <returns>A LocalValue for regular expression.</returns>
    public static LocalValue RegExp(string pattern, string? flags = null) => new("regexp") { argValue = new RegularExpressionValue(pattern, flags) };

    private object? GetSerializedNumericValue()
    {
        double? doubleValue = this.argValue as double?;
        if (doubleValue is not null && doubleValue.HasValue)
        {
            if (double.IsNaN(doubleValue.Value))
            {
                return "NaN";
            }
            else if (double.IsPositiveInfinity(doubleValue.Value))
            {
                return "Infinity";
            }
            else if (double.IsNegativeInfinity(doubleValue.Value))
            {
                return "-Infinity";
            }
        }

        decimal? decimalValue = this.argValue as decimal?;
        if (decimalValue is not null && decimalValue.HasValue && decimalValue.Value == decimal.Negate(decimal.Zero))
        {
            return "-0";
        }

        return this.argValue;
    }
}