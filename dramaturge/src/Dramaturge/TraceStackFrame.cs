// <copyright file="TraceStackFrame.cs" company="WebDriverBiDi.NET Committers">
// Copyright (c) WebDriverBiDi.NET Committers. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.
// </copyright>

namespace Dramaturge;

/// <summary>
/// A frame of the code that called an action recorded in a trace.
/// </summary>
/// <param name="File">The source file.</param>
/// <param name="Line">The line.</param>
/// <param name="Column">The column.</param>
/// <param name="Function">The method, such as <c>CheckoutTests.PlacesAnOrder</c>.</param>
internal sealed record TraceStackFrame(string File, int Line, int Column, string Function);
