// <copyright file="ResultOwnership.cs" company="WebDriverBiDi.NET Committers">
// Copyright (c) WebDriverBiDi.NET Committers. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.
// </copyright>

namespace WebDriverBiDi.Script;

using System.Text.Json.Serialization;
using WebDriverBiDi.JsonConverters;

/// <summary>
/// Value of the ownership model of values returned from script execution.
/// </summary>
[JsonConverter(typeof(EnumValueJsonConverter<ResultOwnership>))]
public enum ResultOwnership
{
    /// <summary>
    /// Use no ownership model.
    /// </summary>
    None,

    /// <summary>
    /// Values are owned by root.
    /// </summary>
    Root,
}
