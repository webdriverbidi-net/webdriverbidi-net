// <copyright file="RemoteValueConverter.cs" company="WebDriverBiDi.NET Committers">
// Copyright (c) WebDriverBiDi.NET Committers. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.
// </copyright>

namespace Dramaturge;

using System.Collections;
using System.Diagnostics.CodeAnalysis;
using System.Numerics;
using WebDriverBiDi.Script;

/// <summary>
/// Converts a script's result to a .NET type: a string, a Boolean, a number type, <see cref="BigInteger"/>,
/// <see cref="DateTime"/>, or a nullable one of those; <see cref="object"/>, for an untyped tree of those with
/// lists and string-keyed dictionaries; a <see cref="RemoteValue"/> type, unconverted; an array of any of these, at
/// any depth; or, at the top level only, a <see cref="List{T}"/> or <see cref="Dictionary{TKey, TValue}"/> with
/// string keys of any of these. Without reflection that ahead-of-time compilation cannot follow, a list or
/// dictionary cannot be created within another value.
/// </summary>
internal static class RemoteValueConverter
{
    /// <summary>
    /// Converts a script's result.
    /// </summary>
    /// <typeparam name="T">The type to convert to.</typeparam>
    /// <param name="value">The result.</param>
    /// <returns>The converted result.</returns>
    /// <exception cref="InvalidCastException">Thrown when the result cannot be converted to the type, such as text to a number, or a fraction to an integer type.</exception>
    /// <exception cref="NotSupportedException">Thrown when the type is not one a result converts to.</exception>
    /// <exception cref="OverflowException">Thrown when a number is outside the range of the type.</exception>
    public static T Convert<[DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.PublicParameterlessConstructor)] T>(RemoteValue value)
    {
        Type target = typeof(T);
        if (IsGeneric(target, typeof(List<>)) || IsStringKeyedDictionary(target))
        {
            return IsNullOrUndefined(value) ? default! : (T)Fill(Activator.CreateInstance(target)!, target, value);
        }

        return (T)ConvertValue(target, value)!;
    }

    private static object? ConvertValue(Type target, RemoteValue value)
    {
        if (typeof(RemoteValue).IsAssignableFrom(target))
        {
            return target.IsInstanceOfType(value) ? value : throw CannotConvert(value, target);
        }

        Type? underlying = Nullable.GetUnderlyingType(target);
        if (IsNullOrUndefined(value))
        {
            return target.IsValueType && underlying is null ? throw CannotConvert(value, target) : null;
        }

        target = underlying ?? target;
        if (target == typeof(object))
        {
            return ToObject(value);
        }

        if (target.IsArray)
        {
            return ToArray(target, value);
        }

        if (IsGeneric(target, typeof(List<>)) || IsGeneric(target, typeof(Dictionary<,>)))
        {
            throw new NotSupportedException($"A {target} within another value cannot be created; use an array, or object for an untyped value.");
        }

        return value switch
        {
            StringRemoteValue text when target == typeof(string) => text.Value,
            BooleanRemoteValue boolean when target == typeof(bool) => boolean.Value,
            DateRemoteValue date when target == typeof(DateTime) => date.Value,
            NumberRemoteValue number => ConvertNumber(number.Value, target, value),
            BigIntegerRemoteValue bigInteger => ConvertBigInteger(bigInteger.Value, target, value),
            _ when IsSupportedScalar(target) => throw CannotConvert(value, target),
            _ => throw new NotSupportedException($"{target} is not a type a script result converts to."),
        };
    }

    private static object ConvertNumber(double number, Type target, RemoteValue value)
    {
        if (target == typeof(double))
        {
            return number;
        }

        if (target == typeof(float))
        {
            return (float)number;
        }

        if (target == typeof(decimal))
        {
            return double.IsNaN(number) || double.IsInfinity(number) ? throw new OverflowException($"{number} cannot be converted to {target}.") : System.Convert.ToDecimal(number);
        }

        if (!IsIntegral(target) && target != typeof(BigInteger))
        {
            throw IsSupportedScalar(target) ? CannotConvert(value, target) : new NotSupportedException($"{target} is not a type a script result converts to.");
        }

        if (double.IsNaN(number) || double.IsInfinity(number))
        {
            throw new OverflowException($"{number} cannot be converted to {target}.");
        }

        if (Math.Floor(number) != number)
        {
            throw new InvalidCastException($"The script returned {number}, which is not a whole number, so it cannot be converted to {target}.");
        }

        return ConvertBigInteger(new BigInteger(number), target, value);
    }

    private static object ConvertBigInteger(BigInteger number, Type target, RemoteValue value)
    {
        return target switch
        {
            _ when target == typeof(BigInteger) => (object)number,
            _ when target == typeof(long) => (object)(long)number,
            _ when target == typeof(int) => (object)(int)number,
            _ when target == typeof(short) => (object)(short)number,
            _ when target == typeof(sbyte) => (object)(sbyte)number,
            _ when target == typeof(ulong) => (object)(ulong)number,
            _ when target == typeof(uint) => (object)(uint)number,
            _ when target == typeof(ushort) => (object)(ushort)number,
            _ when target == typeof(byte) => (object)(byte)number,
            _ when IsSupportedScalar(target) => throw CannotConvert(value, target),
            _ => throw new NotSupportedException($"{target} is not a type a script result converts to."),
        };
    }

    private static object? ToObject(RemoteValue value)
    {
        return value switch
        {
            StringRemoteValue text => text.Value,
            NumberRemoteValue number => number.Value,
            BooleanRemoteValue boolean => boolean.Value,
            BigIntegerRemoteValue bigInteger => bigInteger.Value,
            DateRemoteValue date => date.Value,
            CollectionRemoteValue => Fill(new List<object?>(), typeof(List<object?>), value),
            KeyValuePairCollectionRemoteValue => Fill(new Dictionary<string, object?>(), typeof(Dictionary<string, object?>), value),
            _ => throw CannotConvert(value, typeof(object)),
        };
    }

    private static Array ToArray(Type arrayType, RemoteValue value)
    {
        IReadOnlyList<RemoteValue> items = Items(value, arrayType);
        Type elementType = arrayType.GetElementType()!;
#if NET9_0_OR_GREATER
        Array array = Array.CreateInstanceFromArrayType(arrayType, items.Count);
#else
        Array array = Array.CreateInstance(elementType, items.Count);
#endif
        for (int i = 0; i < items.Count; i++)
        {
            array.SetValue(ConvertValue(elementType, items[i]), i);
        }

        return array;
    }

    // Adds a list's items or a dictionary's entries, converted to the collection's element type.
    private static object Fill(object collection, Type collectionType, RemoteValue value)
    {
        Type[] typeArguments = collectionType.GetGenericArguments();
        if (collection is IDictionary dictionary)
        {
            if (value is not KeyValuePairCollectionRemoteValue entries)
            {
                throw CannotConvert(value, collectionType);
            }

            // An object's keys arrive as strings; a map's, as remote values.
            foreach (KeyValuePair<object, RemoteValue> entry in entries.Value!)
            {
                string key = entry.Key switch
                {
                    string text => text,
                    StringRemoteValue text => text.Value,
                    _ => throw new InvalidCastException($"The script returned a map with a key that is not a string, so it cannot be converted to {collectionType}."),
                };
                dictionary[key] = ConvertValue(typeArguments[1], entry.Value);
            }

            return collection;
        }

        IList list = (IList)collection;
        foreach (RemoteValue item in Items(value, collectionType))
        {
            list.Add(ConvertValue(typeArguments[0], item));
        }

        return collection;
    }

    private static IReadOnlyList<RemoteValue> Items(RemoteValue value, Type target)
    {
        return value is CollectionRemoteValue collection ? collection.Value! : throw CannotConvert(value, target);
    }

    private static bool IsNullOrUndefined(RemoteValue value)
    {
        return value is NullRemoteValue or UndefinedRemoteValue;
    }

    private static bool IsGeneric(Type type, Type definition)
    {
        return type.IsGenericType && type.GetGenericTypeDefinition() == definition;
    }

    private static bool IsStringKeyedDictionary(Type type)
    {
        return IsGeneric(type, typeof(Dictionary<,>)) && type.GetGenericArguments()[0] == typeof(string);
    }

    private static bool IsIntegral(Type type)
    {
        return type == typeof(long) || type == typeof(int) || type == typeof(short) || type == typeof(sbyte)
            || type == typeof(ulong) || type == typeof(uint) || type == typeof(ushort) || type == typeof(byte);
    }

    private static bool IsSupportedScalar(Type type)
    {
        return type == typeof(string) || type == typeof(bool) || type == typeof(DateTime) || type == typeof(double) || type == typeof(float)
            || type == typeof(decimal) || type == typeof(BigInteger) || IsIntegral(type);
    }

    private static InvalidCastException CannotConvert(RemoteValue value, Type target)
    {
        return new InvalidCastException($"The script returned a value of type {value.Type}, which cannot be converted to {target}.");
    }
}
