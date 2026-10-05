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

    /// <summary>
    /// Gets a value indicating whether a snapshot of the page's DOM, which the trace viewer shows, is taken before and
    /// after each action, and, for an action on an element, as it acts, with the element marked. The default is
    /// <see langword="false"/>.
    /// </summary>
    public bool Snapshots { get; init; }

    /// <summary>
    /// Gets a value indicating whether a screenshot of the page is taken after each action and each time it loads a
    /// document, for the viewer's filmstrip. The default is <see langword="false"/>.
    /// </summary>
    public bool Screenshots { get; init; }

    /// <summary>
    /// Gets a value indicating whether the source files of the code that called each action, where this machine has
    /// them, are included in the trace, for the viewer's source tab. The default is <see langword="false"/>.
    /// </summary>
    public bool Sources { get; init; }
}
