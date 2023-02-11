// <copyright file="EvaluateResultType.cs" company="WebDriverBidi.NET Committers">
// Copyright (c) WebDriverBidi.NET Committers. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.
// </copyright>

namespace WebDriverBidi.Script;

using Newtonsoft.Json;
using WebDriverBidi.JsonConverters;

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