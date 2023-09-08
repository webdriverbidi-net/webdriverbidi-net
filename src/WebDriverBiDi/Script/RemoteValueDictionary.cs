// <copyright file="RemoteValueDictionary.cs" company="WebDriverBiDi.NET Committers">
// Copyright (c) WebDriverBiDi.NET Committers. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.
// </copyright>

namespace WebDriverBiDi.Script;

using System.Collections.ObjectModel;

/// <summary>
/// A read-only dictionary of RemoteValue objects.
/// </summary>
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