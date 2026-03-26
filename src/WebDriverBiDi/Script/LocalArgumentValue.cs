// <copyright file="LocalArgumentValue.cs" company="WebDriverBiDi.NET Committers">
// Copyright (c) WebDriverBiDi.NET Committers. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.
// </copyright>

namespace WebDriverBiDi.Script;

using System.Globalization;
using System.Numerics;
using System.Text.Json.Serialization;

/// <summary>
/// Represents a <see cref="LocalValue"/> for use as an argument in script execution, in particular,
/// one that contains a local .NET value to be serialized to JSON according to the WebDriver BiDi
/// protocol. According to the protocol, not every value sent from the client has a value. This
/// class is used to represent those that do have a value.
/// </summary>
public record LocalArgumentValue : LocalValue
{
    /// <summary>
    /// Initializes a new instance of the <see cref="LocalArgumentValue"/> class.
    /// </summary>
    /// <param name="argType">The type of value represented by this LocalValue.</param>
    internal LocalArgumentValue(string argType)
    {
        this.Type = argType;
    }

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
                return ((DateTime)this.Value!).ToString("yyyy-MM-ddTHH:mm:ss.fffZ");
            }

            if (this.Type == "map" || this.Type == "object")
            {
                // One of the two cases (key is string, or key is LocalValue)
                // must be true.
                // Note carefully, objects/maps are serialized in the protocol
                // as lists of key-value pairs
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
            else if (doubleValue.Value == 0.0 && BitConverter.DoubleToInt64Bits(doubleValue.Value) == BitConverter.DoubleToInt64Bits(-0.0))
            {
                return "-0";
            }
        }

        return this.Value;
    }
}
