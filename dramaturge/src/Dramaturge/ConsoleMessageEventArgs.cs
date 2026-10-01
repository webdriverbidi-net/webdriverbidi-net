// <copyright file="ConsoleMessageEventArgs.cs" company="WebDriverBiDi.NET Committers">
// Copyright (c) WebDriverBiDi.NET Committers. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.
// </copyright>

namespace Dramaturge;

using WebDriverBiDi;
using WebDriverBiDi.Log;
using WebDriverBiDi.Script;

/// <summary>
/// Data about a call to a <c>console</c> method, such as <c>console.log</c>, in a frame of a page.
/// </summary>
/// <param name="Frame">The frame whose document made the call.</param>
/// <param name="Method">The method called, such as "log" or "error".</param>
/// <param name="Level">The level of the message.</param>
/// <param name="Text">The message, as the browser formats the arguments.</param>
/// <param name="Arguments">The arguments of the call.</param>
/// <param name="StackTrace">Where the call was made, if the browser reports it.</param>
/// <param name="Timestamp">When the call was made.</param>
public record ConsoleMessageEventArgs(Frame Frame, string Method, LogLevel Level, string Text, IReadOnlyList<RemoteValue> Arguments, StackTrace? StackTrace, DateTime Timestamp) : WebDriverBiDiEventArgs
{
    /// <summary>
    /// Gets the page of the frame that made the call.
    /// </summary>
    public Page Page => this.Frame.Page;
}
