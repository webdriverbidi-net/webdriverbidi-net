// <copyright file="NodeAttributes.cs" company="WebDriverBidi.NET Committers">
// Copyright (c) WebDriverBidi.NET Committers. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.
// </copyright>

namespace WebDriverBidi.Script;

using System.Collections.ObjectModel;

/// <summary>
/// Provides a read-only dictionary of attributes for a node.
/// </summary>
public class NodeAttributes : ReadOnlyDictionary<string, string>
{
    /// <summary>
    /// Initializes a new instance of the <see cref="NodeAttributes"/> class.
    /// </summary>
    /// <param name="dictionary">The dictionary to wrap as a read-only construct.</param>
    internal NodeAttributes(Dictionary<string, string> dictionary)
        : base(dictionary)
    {
    }
}