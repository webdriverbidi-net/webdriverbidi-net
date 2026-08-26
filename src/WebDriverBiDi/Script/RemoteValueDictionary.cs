// <copyright file="RemoteValueDictionary.cs" company="WebDriverBiDi.NET Committers">
// Copyright (c) WebDriverBiDi.NET Committers. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.
// </copyright>

namespace WebDriverBiDi.Script;

using System.Collections.ObjectModel;

/// <summary>
/// A read-only dictionary of RemoteValue objects.
/// </summary>
/// <remarks>
/// Keys are either <see cref="string"/> values or <see cref="RemoteValue"/> instances. String keys are
/// compared by value, so <c>dictionary["name"]</c> works as expected. <see cref="RemoteValue"/> keys
/// are compared by reference: each one denotes a distinct JavaScript object, and two object keys that
/// happen to serialize identically (for example, two functions with no handle) remain distinct entries.
/// Look such entries up by enumerating the dictionary, or by using the very key instance obtained from
/// <see cref="ReadOnlyDictionary{TKey, TValue}.Keys"/>; a separately constructed, structurally equal
/// <see cref="RemoteValue"/> will not match.
/// </remarks>
public class RemoteValueDictionary : ReadOnlyDictionary<object, RemoteValue>
{
    /// <summary>
    /// Initializes a new instance of the <see cref="RemoteValueDictionary"/> class.
    /// </summary>
    /// <param name="dictionary">The dictionary of RemoteValue objects to wrap as read-only.</param>
    internal RemoteValueDictionary(Dictionary<object, RemoteValue> dictionary)
        : base(dictionary)
    {
    }
}
