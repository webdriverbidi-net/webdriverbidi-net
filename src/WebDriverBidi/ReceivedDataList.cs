// <copyright file="ReceivedDataList.cs" company="WebDriverBidi.NET Committers">
// Copyright (c) WebDriverBidi.NET Committers. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.
// </copyright>

namespace WebDriverBidi;

using System.Collections.ObjectModel;

/// <summary>
/// A read-only list containing a list of additional data from a command result.
/// </summary>
public sealed class ReceivedDataList : ReadOnlyCollection<object?>
{
    /// <summary>
    /// Initializes a new instance of the <see cref="ReceivedDataList"/> class.
    /// </summary>
    /// <param name="list">The list of received data.</param>
    public ReceivedDataList(List<object?> list)
        : base(list)
    {
        for (int i = 0; i < this.Items.Count; i++)
        {
            this.Items[i] = SealValue(this.Items[i]);
        }
    }

    /// <summary>
    /// Gets an empty list.
    /// </summary>
    public static ReceivedDataList EmptyList => new(new List<object?>());

    /// <summary>
    /// Gets a writable copy of this list.
    /// </summary>
    /// <returns>A writable list containing a copy of the data in this ReceivedDataList.</returns>
    public List<object?> ToWritableCopy()
    {
        List<object?> result = new();
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