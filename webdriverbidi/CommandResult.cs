// <copyright file="CommandResult.cs" company="WebDriverBidi.NET Committers">
// Copyright (c) WebDriverBidi.NET Committers. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.
// </copyright>

namespace WebDriverBidi;

/// <summary>
/// Base class for the result of a command.
/// </summary>
public class CommandResult
{
    /// <summary>
    /// Gets a value indicating whether the result is an error.
    /// </summary>
    public virtual bool IsError => false;
}