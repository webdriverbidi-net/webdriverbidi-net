// <copyright file="AdditionalCapabilities.cs" company="WebDriverBidi.NET Committers">
// Copyright (c) WebDriverBidi.NET Committers. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.
// </copyright>

namespace WebDriverBidi.Session;

using System.Collections.ObjectModel;

/// <summary>
/// A read-only dictionary containing additional capabilities.
/// </summary>
public class AdditionalCapabilities : ReadOnlyDictionary<string, object?>
{
    /// <summary>
    /// Initializes a new instance of the <see cref="AdditionalCapabilities"/> class.
    /// </summary>
    /// <param name="capabilities">The dictionary of additional capabilities.</param>
    public AdditionalCapabilities(Dictionary<string, object?> capabilities)
        : base(capabilities)
    {
    }

    /// <summary>
    /// Gets an empty set of additional capabilities.
    /// </summary>
    public static AdditionalCapabilities EmptyAdditionalCapabilities => new(new Dictionary<string, object?>());
}