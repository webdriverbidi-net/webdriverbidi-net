// <copyright file="ResultOwnership.cs" company="WebDriverBidi.NET Committers">
// Copyright (c) WebDriverBidi.NET Committers. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.
// </copyright>

namespace WebDriverBidi.Script;

using Newtonsoft.Json;
using WebDriverBidi.JsonConverters;

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