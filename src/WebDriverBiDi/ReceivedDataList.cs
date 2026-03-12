// <copyright file="ReceivedDataList.cs" company="WebDriverBiDi.NET Committers">
// Copyright (c) WebDriverBiDi.NET Committers. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.
// </copyright>

namespace WebDriverBiDi;

using System.Collections.ObjectModel;

/// <summary>
/// A read-only list containing a list of additional data from a command result.
/// </summary>
public sealed class ReceivedDataList : ReadOnlyCollection<object?>
{
    private static readonly ReceivedDataList BlankList = new([]);

    /// <summary>
    /// Initializes a new instance of the <see cref="ReceivedDataList"/> class.
    /// </summary>
    /// <param name="list">The list of received data.</param>
    public ReceivedDataList(List<object?> list)
        : base(SealList(list))
    {
    }

    /// <summary>
    /// Gets an empty list.
    /// </summary>
    public static ReceivedDataList EmptyList => BlankList;

    /// <summary>
    /// Gets a writable copy of this list.
    /// </summary>
    /// <returns>A writable list containing a copy of the data in this ReceivedDataList.</returns>
    public List<object?> ToWritableCopy()
    {
        List<object?> result = [];
        foreach (object? item in this.Items)
        {
            if (item is ReceivedDataDictionary dictionary)
            {
                result.Add(dictionary.ToWritableCopy());
            }
            else if (item is ReceivedDataList list)
            {
                result.Add(list.ToWritableCopy());
            }
            else
            {
                result.Add(item);
            }
        }

        return result;
    }

    private static List<object?> SealList(List<object?> listToSeal)
    {
        List<object?> sealedList = [];
        for (int i = 0; i < listToSeal.Count; i++)
        {
            sealedList.Add(SealValue(listToSeal[i]));
        }

        return sealedList;
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
