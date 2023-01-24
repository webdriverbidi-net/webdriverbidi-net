// <copyright file="ReceivedDataDictionary.cs" company="WebDriverBidi.NET Committers">
// Copyright (c) WebDriverBidi.NET Committers. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.
// </copyright>

namespace WebDriverBidi;

using System.Collections.ObjectModel;

/// <summary>
/// A read-only dictionary containing a dictionary of additional data from a command result.
/// </summary>
public class ReceivedDataDictionary : ReadOnlyDictionary<string, object?>
{
    /// <summary>
    /// Initializes a new instance of the <see cref="ReceivedDataDictionary"/> class.
    /// </summary>
    /// <param name="dictionary">The dictionary of additional data.</param>
    public ReceivedDataDictionary(Dictionary<string, object?> dictionary)
        : base(dictionary)
    {
    }

    /// <summary>
    /// Gets an empty dictionary.
    /// </summary>
    public static ReceivedDataDictionary EmptyDictionary => new(new Dictionary<string, object?>());
}