// <copyright file="TraceSourceLocation.cs" company="WebDriverBiDi.NET Committers">
// Copyright (c) WebDriverBiDi.NET Committers. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.
// </copyright>

namespace Dramaturge;

/// <summary>
/// A place in a page's scripts.
/// </summary>
/// <param name="Url">The script's URL.</param>
/// <param name="Line">The line.</param>
/// <param name="Column">The column.</param>
internal sealed record TraceSourceLocation(string Url, long Line, long Column);
