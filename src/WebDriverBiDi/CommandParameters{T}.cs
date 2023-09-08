// <copyright file="CommandParameters{T}.cs" company="WebDriverBiDi.NET Committers">
// Copyright (c) WebDriverBiDi.NET Committers. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.
// </copyright>

namespace WebDriverBiDi;

using WebDriverBiDi.Protocol;

/// <summary>
/// Represents data for a WebDriver Bidi command where the response type is known.
/// </summary>
/// <typeparam name="T">The type of the response for this command.</typeparam>
public abstract class CommandParameters<T> : CommandParameters
    where T : CommandResult
{
    /// <summary>
    /// Gets the type of the response for this command.
    /// </summary>
    public override Type ResponseType => typeof(CommandResponseMessage<T>);
}