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
public record LocalValue : ArgumentValue
{
    private LocalValue(string argType)
    {
        this.Type = argType;
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
    public static LocalValue NaN => new("number") { Value = double.NaN };

    /// <summary>
    /// Gets a LocalValue for negative zero (-0).
    /// </summary>
    public static LocalValue NegativeZero => new("number") { Value = decimal.Negate(decimal.Zero) };

    /// <summary>
    /// Gets a LocalValue for positive infinity.
    /// </summary>
    public static LocalValue Infinity => new("number") { Value = double.PositiveInfinity };

    /// <summary>
    /// Gets a LocalValue for negative infinity.
    /// </summary>
    public static LocalValue NegativeInfinity => new("number") { Value = double.NegativeInfinity };

    /// <summary>
    /// Gets the type of this LocalValue.
    /// </summary>
    [JsonPropertyName("type")]
    public string Type { get; internal set; }

    /// <summary>
    /// Gets the object containing the value of this LocalValue.
    /// </summary>
    [JsonIgnore]
    public object? Value { get; internal set; }

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
            if (this.Type == "number")
            {
                return this.GetSerializedNumericValue();
            }

            if (this.Type == "bigint")
            {
                // Note that static methods creating BigInteger values should
                // not allow a null value in the argValue member.
                return ((BigInteger)this.Value!).ToString(CultureInfo.InvariantCulture);
            }

            if (this.Type == "date")
            {
                // Note that static methods creating DateTime values should
                // not allow a null value in the argValue member.
                return ((DateTime)this.Value!).ToString("YYYY-MM-ddTHH:mm:ss.fffZ");
            }

            if (this.Type == "map" || this.Type == "object")
            {
                // One of the two cases (key is string, or key is LocalValue)
                // must be true.
                List<object> serializablePairList = [];
                Dictionary<LocalValue, LocalValue>? dictionaryValue = this.Value as Dictionary<LocalValue, LocalValue>;
                Dictionary<string, LocalValue>? stringDictionaryValue = this.Value as Dictionary<string, LocalValue>;
                if (dictionaryValue is not null)
                {
                    foreach (KeyValuePair<LocalValue, LocalValue> pair in dictionaryValue)
                    {
                        List<object> itemList = [pair.Key, pair.Value];
                        serializablePairList.Add(itemList);
                    }
                }

                if (stringDictionaryValue is not null)
                {
                    foreach (KeyValuePair<string, LocalValue> pair in stringDictionaryValue)
                    {
                        List<object> itemList = [pair.Key, pair.Value];
                        serializablePairList.Add(itemList);
                    }
                }

                return serializablePairList;
            }

            return this.Value;
        }
    }

    /// <summary>
    /// Creates a LocalValue for a string.
    /// </summary>
    /// <param name="stringValue">The string to wrap as a LocalValue.</param>
    /// <returns>A LocalValue for a string.</returns>
    public static LocalValue String(string stringValue) => new("string") { Value = stringValue };

    /// <summary>
    /// Creates a LocalValue for a number.
    /// </summary>
    /// <param name="numericValue">The integer to wrap as a LocalValue.</param>
    /// <returns>A LocalValue for a number.</returns>
    public static LocalValue Number(int numericValue) => new("number") { Value = numericValue };

    /// <summary>
    /// Creates a LocalValue for a number.
    /// </summary>
    /// <param name="numericValue">The long to wrap as a LocalValue.</param>
    /// <returns>A LocalValue for a number.</returns>
    public static LocalValue Number(long numericValue) => new("number") { Value = numericValue };

    /// <summary>
    /// Creates a LocalValue for a number.
    /// </summary>
    /// <param name="numericValue">The double to wrap as a LocalValue.</param>
    /// <returns>A LocalValue for a number.</returns>
    public static LocalValue Number(double numericValue) => new("number") { Value = numericValue };

    /// <summary>
    /// Creates a LocalValue for a number.
    /// </summary>
    /// <param name="numericValue">The decimal to wrap as a LocalValue.</param>
    /// <returns>A LocalValue for a number.</returns>
    public static LocalValue Number(decimal numericValue) => new("number") { Value = numericValue };

    /// <summary>
    /// Creates a LocalValue for a boolean value.
    /// </summary>
    /// <param name="boolValue">The boolean to wrap as a LocalValue.</param>
    /// <returns>A LocalValue for a boolean value.</returns>
    public static LocalValue Boolean(bool boolValue) => new("boolean") { Value = boolValue };

    /// <summary>
    /// Creates a LocalValue for a BigInteger.
    /// </summary>
    /// <param name="bigIntValue">The BigInteger to wrap as a LocalValue.</param>
    /// <returns>A LocalValue for a BigInteger.</returns>
    public static LocalValue BigInt(BigInteger bigIntValue) => new("bigint") { Value = bigIntValue };

    /// <summary>
    /// Creates a LocalValue for a DateTime value.
    /// </summary>
    /// <param name="dateTimeValue">The DateTime value to wrap as a LocalValue.</param>
    /// <returns>A LocalValue for a DateTime value.</returns>
    public static LocalValue Date(DateTime dateTimeValue) => new("date") { Value = dateTimeValue };

    /// <summary>
    /// Creates a LocalValue for an array.
    /// </summary>
    /// <param name="arrayValue">The list of LocalValues to wrap as an array LocalValue.</param>
    /// <returns>A LocalValue for an array.</returns>
    public static LocalValue Array(List<LocalValue> arrayValue) => new("array") { Value = arrayValue };

    /// <summary>
    /// Creates a LocalValue for a set.
    /// </summary>
    /// <param name="arrayValue">The list of LocalValues to wrap as a set LocalValue.</param>
    /// <returns>A LocalValue for a set.</returns>
    public static LocalValue Set(List<LocalValue> arrayValue) => new("set") { Value = arrayValue };

    /// <summary>
    /// Creates a LocalValue for a map with string keys.
    /// </summary>
    /// <param name="mapValue">The dictionary with strings for keys and LocalValues for values to wrap as a map LocalValue.</param>
    /// <returns>A LocalValue for a map.</returns>
    public static LocalValue Map(Dictionary<string, LocalValue> mapValue) => new("map") { Value = mapValue };

    /// <summary>
    /// Creates a LocalValue for a map with LocalValue keys.
    /// </summary>
    /// <param name="mapValue">The dictionary with LocalValues for keys and LocalValues for values to wrap as a map LocalValue.</param>
    /// <returns>A LocalValue for a map.</returns>
    public static LocalValue Map(Dictionary<LocalValue, LocalValue> mapValue) => new("map") { Value = mapValue };

    /// <summary>
    /// Creates a LocalValue for an object with string keys.
    /// </summary>
    /// <param name="mapValue">The dictionary with strings for keys and LocalValues for values to wrap as an object LocalValue.</param>
    /// <returns>A LocalValue for an object.</returns>
    public static LocalValue Object(Dictionary<string, LocalValue> mapValue) => new("object") { Value = mapValue };

    /// <summary>
    /// Creates a LocalValue for an object with LocalValue keys.
    /// </summary>
    /// <param name="mapValue">The dictionary with LocalValues for keys and LocalValues for values to wrap as an object LocalValue.</param>
    /// <returns>A LocalValue for an object.</returns>
    public static LocalValue Object(Dictionary<LocalValue, LocalValue> mapValue) => new("object") { Value = mapValue };

    /// <summary>
    /// Creates a LocalValue for regular expression.
    /// </summary>
    /// <param name="pattern">The pattern for the regular expression.</param>
    /// <param name="flags">The flags of the regular expression.</param>
    /// <returns>A LocalValue for regular expression.</returns>
    public static LocalValue RegExp(string pattern, string? flags = null) => new("regexp") { Value = new RegularExpressionValue(pattern, flags) };

    private object? GetSerializedNumericValue()
    {
        double? doubleValue = this.Value as double?;
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

        decimal? decimalValue = this.Value as decimal?;
        if (decimalValue is not null && decimalValue.HasValue && decimalValue.Value == decimal.Negate(decimal.Zero))
        {
            return "-0";
        }

        return this.Value;
    }
}
