// <copyright file="TraceRecordingOptions.cs" company="WebDriverBiDi.NET Committers">
// Copyright (c) WebDriverBiDi.NET Committers. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.
// </copyright>

namespace Dramaturge;

/// <summary>
/// Settings of a trace recording.
/// </summary>
public sealed class TraceRecordingOptions
{
    /// <summary>
    /// Gets the title the trace viewer shows for the trace, such as the name of the test that recorded it.
    /// </summary>
    public string? Title { get; init; }
}
