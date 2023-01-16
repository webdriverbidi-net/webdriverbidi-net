// <copyright file="ResultAdditionalData.cs" company="WebDriverBidi.NET Committers">
// Copyright (c) WebDriverBidi.NET Committers. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.
// </copyright>

namespace WebDriverBidi;

using System.Collections.ObjectModel;

/// <summary>
/// A read-only dictionary containing additional data from a command result.
/// </summary>
public class ResultAdditionalData : ReadOnlyDictionary<string, object?>
{
    /// <summary>
    /// Initializes a new instance of the <see cref="ResultAdditionalData"/> class.
    /// </summary>
    /// <param name="capabilities">The dictionary of additional data.</param>
    public ResultAdditionalData(Dictionary<string, object?> capabilities)
        : base(capabilities)
    {
    }

    /// <summary>
    /// Gets an empty set of additional data.
    /// </summary>
    public static ResultAdditionalData EmptyAdditionalData => new(new Dictionary<string, object?>());
}