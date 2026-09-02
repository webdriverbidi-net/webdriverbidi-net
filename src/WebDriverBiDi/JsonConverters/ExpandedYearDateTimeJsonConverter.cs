// <copyright file="ExpandedYearDateTimeJsonConverter.cs" company="WebDriverBiDi.NET Committers">
// Copyright (c) WebDriverBiDi.NET Committers. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.
// </copyright>

namespace WebDriverBiDi.JsonConverters;

using System.Globalization;
using System.Text.Json;
using System.Text.Json.Serialization;

/// <summary>
/// A converter to read DateTime values serialized in the ECMAScript
/// <c>Date.prototype.toISOString()</c> format, including its expanded-year forms.
/// </summary>
/// <remarks>
/// The WebDriver BiDi protocol serializes JavaScript Date objects using the format of
/// <c>Date.prototype.toISOString()</c>. JavaScript dates cover approximately the years
/// -271821 to +275760; for years outside 0000-9999 that format uses a sign and a
/// six-digit year (for example, <c>"+275760-09-13T00:00:00.000Z"</c>), and year zero
/// itself is written as <c>"0000"</c>. None of those forms is representable by
/// <see cref="DateTime"/>, whose range is years 0001-9999, so a structurally valid
/// date string for an instant before that range deserializes as
/// <see cref="DateTime.MinValue"/> and one for an instant after it as
/// <see cref="DateTime.MaxValue"/>, consistent with how the library clamps
/// out-of-range protocol timestamps elsewhere. A structurally invalid date string is
/// still rejected with a <see cref="JsonException"/>.
/// </remarks>
public class ExpandedYearDateTimeJsonConverter : JsonConverter<DateTime>
{
    private const int ExpandedYearDigitCount = 6;
    private const int MaxRepresentableYear = 9999;

    // Date.prototype.toISOString() emits the fixed layout [sign]year-MM-ddTHH:mm:ss.sssZ.
    // Once the year is normalized to four digits, this is the exact remaining shape; FFF
    // (rather than fff) also tolerates fewer than three fractional digits.
    private const string NormalizedYearFormat = "yyyy-MM-dd'T'HH:mm:ss.FFF'Z'";

    // The year substituted to validate the structure of a date string whose own year is
    // outside the range of DateTime. Year 4 is a leap year, so a February 29 in an
    // out-of-range leap year (such as year zero) still validates as structurally sound.
    private const string LeapYearSubstitute = "0004";

    private const DateTimeStyles ParseStyles = DateTimeStyles.AssumeUniversal | DateTimeStyles.AdjustToUniversal;

    /// <summary>
    /// Deserializes a JSON string in <c>Date.prototype.toISOString()</c> format to a DateTime value.
    /// </summary>
    /// <param name="reader">A Utf8JsonReader used to read the incoming JSON.</param>
    /// <param name="typeToConvert">The Type description of the type to convert.</param>
    /// <param name="options">The JsonSerializationOptions used for deserializing the JSON.</param>
    /// <returns>
    /// The deserialized DateTime value. Instants before the range of <see cref="DateTime"/>
    /// are clamped to <see cref="DateTime.MinValue"/>, and instants after it to
    /// <see cref="DateTime.MaxValue"/>.
    /// </returns>
    /// <exception cref="JsonException">Thrown when the JSON token is not a string, or the string is not a structurally valid date.</exception>
    public override DateTime Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        if (reader.TokenType != JsonTokenType.String)
        {
            throw new JsonException($"JSON serialization of date value should be a string, but was {reader.TokenType}");
        }

        // Fast path: years 0001-9999 use the plain four-digit-year form, which the
        // reader's own ISO 8601 parser handles.
        if (reader.TryGetDateTime(out DateTime standardValue))
        {
            return standardValue;
        }

        // We have determined that the token type is a string, so cannot be null.
        // We can legitimately use the null-forgiving operator in this case.
        string dateString = reader.GetString()!;
        if (dateString.Length > ExpandedYearDigitCount + 1
            && (dateString[0] == '+' || dateString[0] == '-')
            && IsAsciiDigitRun(dateString, 1, ExpandedYearDigitCount))
        {
            int year = int.Parse(dateString.Substring(1, ExpandedYearDigitCount), NumberStyles.None, CultureInfo.InvariantCulture);
            string remainder = dateString.Substring(ExpandedYearDigitCount + 1);
            if (dateString[0] == '+' && year >= 1 && year <= MaxRepresentableYear)
            {
                // An expanded-year form whose instant is nonetheless within the range
                // of DateTime (for example, "+002020-07-19T23:47:19.856Z").
                if (DateTime.TryParseExact(year.ToString("D4", CultureInfo.InvariantCulture) + remainder, NormalizedYearFormat, CultureInfo.InvariantCulture, ParseStyles, out DateTime inRangeValue))
                {
                    return inRangeValue;
                }
            }
            else if (DateTime.TryParseExact(LeapYearSubstitute + remainder, NormalizedYearFormat, CultureInfo.InvariantCulture, ParseStyles, out _))
            {
                // The string is structurally valid, but its instant is outside the
                // representable range of DateTime, so clamp it. A negative year, and
                // year zero, precede DateTime.MinValue; years beyond 9999 exceed
                // DateTime.MaxValue.
                return dateString[0] == '-' || year == 0 ? DateTime.MinValue : DateTime.MaxValue;
            }
        }
        else if (dateString.StartsWith("0000-", StringComparison.Ordinal)
            && DateTime.TryParseExact(LeapYearSubstitute + dateString.Substring(4), NormalizedYearFormat, CultureInfo.InvariantCulture, ParseStyles, out _))
        {
            // Date.prototype.toISOString() writes year zero itself in the plain
            // four-digit form "0000", which precedes the range of DateTime; clamp.
            return DateTime.MinValue;
        }

        throw new JsonException($"Cannot parse invalid value '{dateString}' for date");
    }

    /// <summary>
    /// Not implemented. This converter is read-only; date remote values are never
    /// serialized. Outbound date values are serialized via <see cref="WebDriverBiDi.Script.LocalArgumentValue"/>.
    /// </summary>
    /// <param name="writer">A Utf8JsonWriter used to write the JSON string.</param>
    /// <param name="value">The DateTime value to be serialized.</param>
    /// <param name="options">The JsonSerializationOptions used for serializing the object.</param>
    /// <exception cref="NotSupportedException">Always thrown.</exception>
    public override void Write(Utf8JsonWriter writer, DateTime value, JsonSerializerOptions options)
    {
        throw new NotSupportedException("ExpandedYearDateTimeJsonConverter does not support serialization; use LocalArgumentValue for outbound date values.");
    }

    private static bool IsAsciiDigitRun(string value, int startIndex, int count)
    {
        for (int index = startIndex; index < startIndex + count; index++)
        {
            if (value[index] < '0' || value[index] > '9')
            {
                return false;
            }
        }

        return true;
    }
}
