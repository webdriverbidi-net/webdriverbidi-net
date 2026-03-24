// <copyright file="UndefinedRemoteValue.cs" company="WebDriverBiDi.NET Committers">
// Copyright (c) WebDriverBiDi.NET Committers. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.
// </copyright>

namespace WebDriverBiDi.Script;

using System.Text.Json.Serialization;
using WebDriverBiDi.JsonConverters;

/// <summary>
/// Represents a remote value for an undefined value. providing the ability to
/// convert to a local value for use as an argument for script execution on the
/// remote end.
/// </summary>
[JsonConverter(typeof(RemoteValueJsonConverter))]
public record UndefinedRemoteValue : RemoteValue
{
    /// <summary>
    /// Initializes a new instance of the <see cref="UndefinedRemoteValue"/> class.
    /// </summary>
    internal UndefinedRemoteValue()
        : base(RemoteValueType.Undefined)
    {
    }

    /// <summary>
    /// Converts this remote value to a local value for use as an argument for script execution on the remote end.
    /// </summary>
    /// <returns>A LocalValue representing the undefined value.</returns>
    public override LocalValue ToLocalValue() => LocalValue.Undefined;
}
