// <copyright file="StringEnumValueConverter{T}.cs" company="WebDriverBiDi.NET Committers">
// Copyright (c) WebDriverBiDi.NET Committers. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.
// </copyright>

namespace WebDriverBiDi;

using System.Diagnostics.CodeAnalysis;
using System.Reflection;

/// <summary>
/// Converter for enumerated types that are represented as strings in JSON. This converter
/// supports the use of the <see cref="StringEnumValueAttribute"/> to specify custom string
/// values for enumerated values, and the <see cref="StringEnumUnmatchedValueAttribute{T}"/>
/// to specify a default value if the supplied string does not match any of the enumerated
/// values.
/// </summary>
/// <typeparam name="T">An enumerated type.</typeparam>
public class StringEnumValueConverter<[DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.PublicFields)] T>
    where T : struct, Enum
{
    private readonly T? defaultValue;
    private readonly Dictionary<T, string> enumValuesToStrings = [];
    private readonly Dictionary<string, T> stringToEnumValues = [];

    /// <summary>
    /// Initializes a new instance of the <see cref="StringEnumValueConverter{T}"/> class.
    /// </summary>
    public StringEnumValueConverter()
    {
        Type enumType = typeof(T);
        StringEnumUnmatchedValueAttribute<T>? unmatchedValueAttribute = enumType.GetCustomAttribute<StringEnumUnmatchedValueAttribute<T>>();
        if (unmatchedValueAttribute is not null)
        {
            this.defaultValue = unmatchedValueAttribute.UnmatchedValue;
        }

        // A note about the below structure. We use Enum.GetValues<T>() to support AOT
        // compilation scenarios. However, .NET Standard 2.0 does not contain that
        // method definition. Luckily, .NET Standard 2.0 also does not have concrete
        // AOT publishing path.
        T[] values =
#if NET8_0_OR_GREATER
            Enum.GetValues<T>();
#else
            (T[])Enum.GetValues(typeof(T));
#endif

        foreach (T value in values)
        {
            string valueAsString = value.ToString().ToLowerInvariant();

            // Use GetField rather than GetMember so that the annotation on T
            // ([DynamicallyAccessedMembers(PublicFields)]) is sufficient. Enum
            // values are public static readonly fields on the enum type, so
            // GetField(name) is the correct query for getting values.
            FieldInfo member = enumType.GetField(value.ToString())!;
            StringEnumValueAttribute? attribute = member.GetCustomAttribute<StringEnumValueAttribute>();
            if (attribute is not null)
            {
                valueAsString = attribute.Value;
            }

            this.stringToEnumValues[valueAsString] = value;
            this.enumValuesToStrings[value] = valueAsString;
        }
    }

    /// <summary>
    /// Gets the enumerated value corresponding to the specified string. If the string does
    /// not match any of the enumerated values, and there is no default value specified,
    /// this method will throw an exception.
    /// </summary>
    /// <param name="stringValue">The string value to convert.</param>
    /// <returns>The enumerated value.</returns>
    /// <exception cref="ArgumentException">Thrown when the string value does not match any enumerated value.</exception>
    public T GetValue(string stringValue)
    {
        if (this.TryGetValue(stringValue, out T value))
        {
            return value;
        }

        if (this.defaultValue is not null)
        {
            return this.defaultValue.Value;
        }

        throw new ArgumentException($"Unable to convert string '{stringValue}' to value of type '{typeof(T)}'.");
    }

    /// <summary>
    /// Tries to get the enumerated value corresponding to the specified string.
    /// </summary>
    /// <param name="stringValue">The string value to convert.</param>
    /// <param name="value">The enumerated value.</param>
    /// <returns><see langword="true"/> if the string value matches an enumerated value; otherwise, <see langword="false"/>.</returns>
    public bool TryGetValue(string stringValue, out T value)
    {
        return this.stringToEnumValues.TryGetValue(stringValue, out value);
    }

    /// <summary>
    /// Gets the string corresponding to the specified enumerated value. If the
    /// enumerated value does not match any of the defined values, this method
    /// will throw an exception.
    /// </summary>
    /// <param name="value">The enumerated value.</param>
    /// <returns>The string value.</returns>
    /// <exception cref="ArgumentException">Thrown when the enumerated value does not match any defined value.</exception>
    public string GetString(T value)
    {
        if (this.enumValuesToStrings.TryGetValue(value, out string? stringValue))
        {
            return stringValue;
        }

        throw new ArgumentException($"Unable to convert value '{value}' of type '{typeof(T)}' to a string.");
    }
}
