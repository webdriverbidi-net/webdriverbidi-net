// <copyright file="EvaluateResultType.cs" company="WebDriverBiDi.NET Committers">
// Copyright (c) WebDriverBiDi.NET Committers. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.
// </copyright>

namespace WebDriverBiDi.Script;

using Newtonsoft.Json;
using WebDriverBiDi.JsonConverters;

/// <summary>
/// The result type of a script evaluation.
/// </summary>
[JsonConverter(typeof(EnumValueJsonConverter<EvaluateResultType>))]
public enum EvaluateResultType
{
    /// <summary>
    /// The script evaluation was successful.
    /// </summary>
    Success,

    /// <summary>
    /// The script evaluation threw an exception.
    /// </summary>
    Exception,
}
