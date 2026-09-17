// <copyright file="SentinelNullJsonConverter{T,TSentinelChecker,TValueConverter}.cs" company="WebDriverBiDi.NET Committers">
// Copyright (c) WebDriverBiDi.NET Committers. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.
// </copyright>

namespace WebDriverBiDi.JsonConverters;

using System.Text.Json;
using System.Text.Json.Serialization;

/// <summary>
/// Custom JSON serializer for properties that can be both missing and null, but with different semantics
/// for each case, and whose values need a converter of their own. When the property value equals a
/// type-specific sentinel (as determined by <typeparamref name="TSentinelChecker"/>), the converter writes
/// JSON <c>null</c>; every other value is written by <typeparamref name="TValueConverter"/>. Pair with
/// <c>[JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]</c> so that an absent C#
/// <see langword="null"/> omits the property entirely while the sentinel value emits an explicit JSON
/// <c>null</c>.
/// </summary>
/// <typeparam name="T">The type to serialize.</typeparam>
/// <typeparam name="TSentinelChecker">
/// A <see cref="SentinelValueChecker{T}"/> subclass that decides whether a given value is the sentinel. Must
/// have a parameterless constructor.
/// </typeparam>
/// <typeparam name="TValueConverter">
/// The converter that writes a value that is not the sentinel. Must have a parameterless constructor.
/// </typeparam>
/// <remarks>
/// A property can carry only one <see cref="JsonConverterAttribute"/>, so a property that needs both the
/// sentinel behavior and a converter for its values cannot declare the two separately. For example, a
/// resettable floating-point property uses
/// <c>SentinelNullJsonConverter&lt;double, NegativeDoubleSentinelChecker, FixedDoubleJsonConverter&gt;</c>, so that
/// a negative value is written as <c>null</c> and every other value keeps a decimal point on the wire.
/// </remarks>
public class SentinelNullJsonConverter<T, TSentinelChecker, TValueConverter> : SentinelNullJsonConverter<T, TSentinelChecker>
    where TSentinelChecker : SentinelValueChecker<T>, new()
    where TValueConverter : JsonConverter<T>, new()
{
    private readonly TValueConverter valueConverter = new();

    /// <summary>
    /// Initializes a new instance of the <see cref="SentinelNullJsonConverter{T, TSentinelChecker, TValueConverter}"/> class.
    /// </summary>
    public SentinelNullJsonConverter()
    {
    }

    /// <summary>
    /// Serializes a value that is not the sentinel, using <typeparamref name="TValueConverter"/>.
    /// </summary>
    /// <param name="writer">A Utf8JsonWriter used to write the JSON string.</param>
    /// <param name="value">The value to be serialized. It is never <see langword="null"/> and never the sentinel.</param>
    /// <param name="options">The JsonSerializationOptions used for serializing the object.</param>
    protected override void WriteValue(Utf8JsonWriter writer, T value, JsonSerializerOptions options)
    {
        this.valueConverter.Write(writer, value, options);
    }
}
