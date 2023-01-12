// <copyright file="ScriptEvaluateResultType.cs" company="WebDriverBidi.NET Committers">
// Copyright (c) WebDriverBidi.NET Committers. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.
// </copyright>

namespace WebDriverBidi.Script;

/// <summary>
/// The result type of a script evaluation.
/// </summary>
public enum ScriptEvaluateResultType
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