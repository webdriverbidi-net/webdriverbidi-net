// <copyright file="LocalValue.cs" company="WebDriverBiDi.NET Committers">
// Copyright (c) WebDriverBiDi.NET Committers. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.
// </copyright>

namespace WebDriverBiDi.Script;

using System.Numerics;
using System.Text.Json.Serialization;

/// <summary>
/// Object representing a local value for use as an argument in script execution.
/// </summary>
[JsonDerivedType(typeof(LocalArgumentValue))]
[JsonDerivedType(typeof(RemoteReference))]
[JsonDerivedType(typeof(RemoteObjectReference))]
[JsonDerivedType(typeof(SharedReference))]
[JsonDerivedType(typeof(ChannelValue))]
public record LocalValue
{
    /// <summary>
    /// Gets a LocalValue for "undefined".
    /// </summary>
    public static LocalValue Undefined => new LocalArgumentValue("undefined");

    /// <summary>
    /// Gets a LocalValue for a null value.
    /// </summary>
    public static LocalValue Null => new LocalArgumentValue("null");

    /// <summary>
    /// Gets a LocalValue for "NaN".
    /// </summary>
    public static LocalValue NaN => new LocalArgumentValue("number") { Value = double.NaN };

    /// <summary>
    /// Gets a LocalValue for negative zero (-0).
    /// </summary>
    public static LocalValue NegativeZero => new LocalArgumentValue("number") { Value = -0.0 };

    /// <summary>
    /// Gets a LocalValue for positive infinity.
    /// </summary>
    public static LocalValue Infinity => new LocalArgumentValue("number") { Value = double.PositiveInfinity };

    /// <summary>
    /// Gets a LocalValue for negative infinity.
    /// </summary>
    public static LocalValue NegativeInfinity => new LocalArgumentValue("number") { Value = double.NegativeInfinity };

    /// <summary>
    /// Creates a LocalValue for a string.
    /// </summary>
    /// <param name="stringValue">The string to wrap as a LocalValue.</param>
    /// <returns>A LocalValue for a string.</returns>
    public static LocalValue String(string stringValue) => new LocalArgumentValue("string") { Value = stringValue };

    /// <summary>
    /// Creates a LocalValue for a number.
    /// </summary>
    /// <param name="numericValue">The integer to wrap as a LocalValue.</param>
    /// <returns>A LocalValue for a number.</returns>
    public static LocalValue Number(int numericValue) => new LocalArgumentValue("number") { Value = numericValue };

    /// <summary>
    /// Creates a LocalValue for a number.
    /// </summary>
    /// <param name="numericValue">The long to wrap as a LocalValue.</param>
    /// <returns>A LocalValue for a number.</returns>
    public static LocalValue Number(long numericValue) => new LocalArgumentValue("number") { Value = numericValue };

    /// <summary>
    /// Creates a LocalValue for a number.
    /// </summary>
    /// <param name="numericValue">The double to wrap as a LocalValue.</param>
    /// <returns>A LocalValue for a number.</returns>
    public static LocalValue Number(double numericValue) => new LocalArgumentValue("number") { Value = numericValue };

    /// <summary>
    /// Creates a LocalValue for a number.
    /// </summary>
    /// <param name="numericValue">The decimal to wrap as a LocalValue.</param>
    /// <returns>A LocalValue for a number.</returns>
    public static LocalValue Number(decimal numericValue) => new LocalArgumentValue("number") { Value = numericValue };

    /// <summary>
    /// Creates a LocalValue for a boolean value.
    /// </summary>
    /// <param name="boolValue">The boolean to wrap as a LocalValue.</param>
    /// <returns>A LocalValue for a boolean value.</returns>
    public static LocalValue Boolean(bool boolValue) => new LocalArgumentValue("boolean") { Value = boolValue };

    /// <summary>
    /// Creates a LocalValue for a BigInteger.
    /// </summary>
    /// <param name="bigIntValue">The BigInteger to wrap as a LocalValue.</param>
    /// <returns>A LocalValue for a BigInteger.</returns>
    public static LocalValue BigInt(BigInteger bigIntValue) => new LocalArgumentValue("bigint") { Value = bigIntValue };

    /// <summary>
    /// Creates a LocalValue for a DateTime value.
    /// </summary>
    /// <param name="dateTimeValue">The DateTime value to wrap as a LocalValue.</param>
    /// <returns>A LocalValue for a DateTime value.</returns>
    public static LocalValue Date(DateTime dateTimeValue) => new LocalArgumentValue("date") { Value = dateTimeValue };

    /// <summary>
    /// Creates a LocalValue for an array.
    /// </summary>
    /// <param name="arrayValue">The list of LocalValues to wrap as an array LocalValue.</param>
    /// <returns>A LocalValue for an array.</returns>
    public static LocalValue Array(List<LocalValue> arrayValue) => new LocalArgumentValue("array") { Value = arrayValue };

    /// <summary>
    /// Creates a LocalValue for a set.
    /// </summary>
    /// <param name="arrayValue">The list of LocalValues to wrap as a set LocalValue.</param>
    /// <returns>A LocalValue for a set.</returns>
    public static LocalValue Set(List<LocalValue> arrayValue) => new LocalArgumentValue("set") { Value = arrayValue };

    /// <summary>
    /// Creates a LocalValue for a map with string keys.
    /// </summary>
    /// <param name="mapValue">The dictionary with strings for keys and LocalValues for values to wrap as a map LocalValue.</param>
    /// <returns>A LocalValue for a map.</returns>
    public static LocalValue Map(Dictionary<string, LocalValue> mapValue) => new LocalArgumentValue("map") { Value = mapValue };

    /// <summary>
    /// Creates a LocalValue for a map with LocalValue keys.
    /// </summary>
    /// <param name="mapValue">The dictionary with LocalValues for keys and LocalValues for values to wrap as a map LocalValue.</param>
    /// <returns>A LocalValue for a map.</returns>
    public static LocalValue Map(Dictionary<LocalValue, LocalValue> mapValue) => new LocalArgumentValue("map") { Value = mapValue };

    /// <summary>
    /// Creates a LocalValue for a map with LocalValue keys.
    /// </summary>
    /// <param name="mapValue">The dictionary with LocalValues for keys and LocalValues for values to wrap as a map LocalValue.</param>
    /// <returns>A LocalValue for a map.</returns>
    public static LocalValue Map(Dictionary<object, LocalValue> mapValue)
    {
        foreach (object key in mapValue.Keys)
        {
            if (key is not string and not LocalValue)
            {
                throw new WebDriverBiDiException($"Map keys must be either a string or a LocalValue; got {key.GetType().FullName}");
            }
        }

        return new LocalArgumentValue("map") { Value = mapValue };
    }

    /// <summary>
    /// Creates a LocalValue for an object with string keys.
    /// </summary>
    /// <param name="mapValue">The dictionary with strings for keys and LocalValues for values to wrap as an object LocalValue.</param>
    /// <returns>A LocalValue for an object.</returns>
    public static LocalValue Object(Dictionary<string, LocalValue> mapValue) => new LocalArgumentValue("object") { Value = mapValue };

    /// <summary>
    /// Creates a LocalValue for an object with LocalValue keys.
    /// </summary>
    /// <param name="mapValue">The dictionary with LocalValues for keys and LocalValues for values to wrap as an object LocalValue.</param>
    /// <returns>A LocalValue for an object.</returns>
    public static LocalValue Object(Dictionary<LocalValue, LocalValue> mapValue) => new LocalArgumentValue("object") { Value = mapValue };

    /// <summary>
    /// Creates a LocalValue for an object with LocalValue keys.
    /// </summary>
    /// <param name="mapValue">The dictionary with LocalValues for keys and LocalValues for values to wrap as an object LocalValue.</param>
    /// <returns>A LocalValue for an object.</returns>
    public static LocalValue Object(Dictionary<object, LocalValue> mapValue)
    {
        foreach (object key in mapValue.Keys)
        {
            if (key is not string and not LocalValue)
            {
                throw new WebDriverBiDiException($"Object keys must be either a string or a LocalValue; got {key.GetType().FullName}");
            }
        }

        return new LocalArgumentValue("object") { Value = mapValue };
    }

    /// <summary>
    /// Creates a LocalValue for a regular expression.
    /// </summary>
    /// <param name="pattern">The pattern for the regular expression.</param>
    /// <param name="flags">The flags of the regular expression.</param>
    /// <returns>A LocalValue for a regular expression.</returns>
    public static LocalValue RegExp(string pattern, string? flags = null) => new LocalArgumentValue("regexp") { Value = new RegularExpressionValue(pattern, flags) };
}
