// <copyright file="FixedDoubleJsonConverter.cs" company="WebDriverBiDi.NET Committers">
// Copyright (c) WebDriverBiDi.NET Committers. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.
// </copyright>

namespace WebDriverBiDi.JsonConverters;

using System.Globalization;
using System.Text.Json;
using System.Text.Json.Serialization;

/// <summary>
/// A converter to preserve double values format when serializing to JSON.
/// </summary>
public class FixedDoubleJsonConverter : JsonConverter<double>
{
#if !NETSTANDARD2_0
    // The longest "R" representation of a finite double is 24 characters
    // ("-1.7976931348623157E+308"), and the decimal point suffix adds two more.
    private const int MaxFormattedLength = 32;
#endif

    /// <summary>
    /// Deserializes the JSON string to a double value.
    /// </summary>
    /// <param name="reader">A Utf8JsonReader used to read the incoming JSON.</param>
    /// <param name="typeToConvert">The Type description of the type to convert.</param>
    /// <param name="options">The JsonSerializerOptions used for deserializing the JSON.</param>
    /// <returns>The deserialized double value.</returns>
    public override double Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        return reader.GetDouble();
    }

    /// <summary>
    /// Serializes a double value to a JSON string, emitting text that parses back to the exact
    /// same value and preserving a decimal point for integer values.
    /// </summary>
    /// <param name="writer">A Utf8JsonWriter used to write the JSON string.</param>
    /// <param name="value">The double value to be serialized.</param>
    /// <param name="options">The JsonSerializerOptions used for serializing the object.</param>
    /// <exception cref="JsonException">Thrown when <paramref name="value"/> is NaN or infinite.</exception>
    public override void Write(Utf8JsonWriter writer, double value, JsonSerializerOptions options)
    {
        // JSON has no representation for NaN or the infinities; report a clear error rather than
        // letting the writer reject the formatted text with a less descriptive exception.
        if (double.IsNaN(value) || double.IsInfinity(value))
        {
            throw new JsonException($"The value {value} cannot be serialized; only finite numbers can be represented in JSON");
        }

        // The formatted text must parse back to the exact same double: a fixed-precision format
        // rounds to 15 significant digits, which can alter an in-range value into an out-of-range
        // one (e.g., pi / 2, the inclusive maximum for a pointer altitude angle). The "R"
        // specifier produces the shortest exact representation on .NET Core 3.0 and later, but
        // is documented as unreliable for doubles on .NET Framework, so the netstandard2.0
        // build uses "G17", which always round-trips there.
        //
        // An integer-valued double must keep a decimal point on the wire so it reads as a JSON
        // float rather than a JSON int. Exponent notation (e.g., 1E-30) already reads as a float
        // and needs no suffix; the uppercase format specifiers always emit an uppercase 'E'.
#if NETSTANDARD2_0
        string numberAsString = value.ToString("G17", CultureInfo.InvariantCulture);
        if (numberAsString.IndexOf('.') < 0 && numberAsString.IndexOf('E') < 0)
        {
            numberAsString += ".0";
        }

        writer.WriteRawValue(numberAsString);
#else
        // Formatting into a stack buffer, rather than to a string, keeps this converter
        // allocation-free; every double of every command is written through it.
        Span<byte> buffer = stackalloc byte[MaxFormattedLength];
        if (!value.TryFormat(buffer, out int written, "R", CultureInfo.InvariantCulture))
        {
            // Unreachable for a finite double, which the guard above has established.
            throw new JsonException($"The value {value} could not be formatted for serialization");
        }

        ReadOnlySpan<byte> formatted = buffer.Slice(0, written);
        if (formatted.IndexOf((byte)'.') < 0 && formatted.IndexOf((byte)'E') < 0)
        {
            buffer[written] = (byte)'.';
            buffer[written + 1] = (byte)'0';
            formatted = buffer.Slice(0, written + 2);
        }

        // The text was produced here from a finite double, so it is valid JSON already and
        // needs no second parse to prove it.
        writer.WriteRawValue(formatted, skipInputValidation: true);
#endif
    }
}
