// <copyright file="PageErrorEventArgs.cs" company="WebDriverBiDi.NET Committers">
// Copyright (c) WebDriverBiDi.NET Committers. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.
// </copyright>

namespace Dramaturge;

using WebDriverBiDi;
using WebDriverBiDi.Script;

/// <summary>
/// Data about an error a script in a frame of a page threw and did not catch.
/// </summary>
/// <param name="Frame">The frame whose document threw the error.</param>
/// <param name="Message">The error's message.</param>
/// <param name="StackTrace">Where the error was thrown, if the browser reports it.</param>
/// <param name="Timestamp">When the error was thrown.</param>
public record PageErrorEventArgs(Frame Frame, string Message, StackTrace? StackTrace, DateTime Timestamp) : WebDriverBiDiEventArgs
{
    /// <summary>
    /// Gets the page of the frame that threw the error.
    /// </summary>
    public Page Page => this.Frame.Page;
}
