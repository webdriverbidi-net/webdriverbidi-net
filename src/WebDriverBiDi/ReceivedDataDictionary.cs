// <copyright file="ReceivedDataDictionary.cs" company="WebDriverBiDi.NET Committers">
// Copyright (c) WebDriverBiDi.NET Committers. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.
// </copyright>

namespace WebDriverBiDi;

using System.Collections.ObjectModel;

/// <summary>
/// A read-only dictionary containing a dictionary of additional data from a command result.
/// </summary>
public sealed class ReceivedDataDictionary : ReadOnlyDictionary<string, object?>
{
    /// <summary>
    /// Initializes a new instance of the <see cref="ReceivedDataDictionary"/> class.
    /// </summary>
    /// <param name="dictionary">The dictionary of additional data.</param>
    public ReceivedDataDictionary(Dictionary<string, object?> dictionary)
        : base(dictionary)
    {
        foreach (KeyValuePair<string, object?> pair in this.Dictionary)
        {
            this.Dictionary[pair.Key] = SealValue(pair.Value);
        }
    }

    /// <summary>
    /// Gets an empty dictionary.
    /// </summary>
    public static ReceivedDataDictionary EmptyDictionary => new([]);

    /// <summary>
    /// Gets a writable copy of this dictionary.
    /// </summary>
    /// <returns>A writable dictionary containing a copy of the data in this ReceivedDataDictionary.</returns>
    public Dictionary<string, object?> ToWritableCopy()
    {
        Dictionary<string, object?> result = [];
        foreach (KeyValuePair<string, object?> pair in this.Dictionary)
        {
            if (pair.Value is ReceivedDataDictionary dictionary)
            {
                result[pair.Key] = dictionary.ToWritableCopy();
            }
            else if (pair.Value is ReceivedDataList list)
            {
                result[pair.Key] = list.ToWritableCopy();
            }
            else
            {
                result[pair.Key] = pair.Value;
            }
        }

        return result;
    }

    private static object? SealValue(object? valueToSeal)
    {
        if (valueToSeal is Dictionary<string, object?> dictionaryValue)
        {
            return new ReceivedDataDictionary(dictionaryValue);
        }

        if (valueToSeal is List<object?> listValue)
        {
            return new ReceivedDataList(listValue);
        }

        return valueToSeal;
    }
}
