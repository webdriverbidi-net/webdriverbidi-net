// <copyright file="RemoteValueList.cs" company="WebDriverBidi.NET Committers">
// Copyright (c) WebDriverBidi.NET Committers. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.
// </copyright>

namespace WebDriverBidi.Script;

using System.Collections.ObjectModel;

/// <summary>
/// A read-only list of RemoteValue objects.
/// </summary>
public class RemoteValueList : ReadOnlyCollection<RemoteValue>
{
    /// <summary>
    /// Initializes a new instance of the <see cref="RemoteValueList"/> class.
    /// </summary>
    /// <param name="list">The list of RemoteValue objects to wrap as read-only.</param>
    internal RemoteValueList(List<RemoteValue> list)
        : base(list)
    {
    }
}