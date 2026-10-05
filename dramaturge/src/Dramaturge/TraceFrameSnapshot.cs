// <copyright file="TraceFrameSnapshot.cs" company="WebDriverBiDi.NET Committers">
// Copyright (c) WebDriverBiDi.NET Committers. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.
// </copyright>

namespace Dramaturge;

/// <summary>
/// A snapshot of a frame's DOM, taken for an action recorded in a trace.
/// </summary>
/// <param name="Phase">When it was taken: before, action, or after.</param>
/// <param name="CallId">The action's ID.</param>
/// <param name="PageId">The ID of the frame's page.</param>
/// <param name="FrameId">The frame's ID.</param>
/// <param name="FrameUrl">The frame's URL.</param>
/// <param name="IsMainFrame">A value indicating whether the frame is its page's main frame.</param>
/// <param name="Doctype">The name of the document's type, or <see langword="null"/>.</param>
/// <param name="Html">The document element's snapshot, as JSON.</param>
/// <param name="ViewportWidth">The viewport's width, in CSS pixels.</param>
/// <param name="ViewportHeight">The viewport's height, in CSS pixels.</param>
/// <param name="WallTime">When the snapshot was taken, in milliseconds since the epoch.</param>
/// <param name="CollectionTime">How long the page took to take it, in milliseconds.</param>
internal sealed record TraceFrameSnapshot(string Phase, string CallId, string PageId, string FrameId, string FrameUrl, bool IsMainFrame, string? Doctype, string Html, double ViewportWidth, double ViewportHeight, double WallTime, double CollectionTime);
