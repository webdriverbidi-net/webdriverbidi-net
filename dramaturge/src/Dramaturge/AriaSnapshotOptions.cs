// <copyright file="AriaSnapshotOptions.cs" company="WebDriverBiDi.NET Committers">
// Copyright (c) WebDriverBiDi.NET Committers. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.
// </copyright>

namespace Dramaturge;

/// <summary>
/// Options for taking an accessibility snapshot.
/// </summary>
public sealed class AriaSnapshotOptions
{
    /// <summary>
    /// Gets a value indicating whether the snapshot gives each node a ref, which <see cref="AriaSnapshot.Locator"/>
    /// turns into a locator for its element. Without refs, the text is easier to read and compare, and the snapshot
    /// has no locators. Defaults to <see langword="true"/>.
    /// </summary>
    public bool IncludeRefs { get; init; } = true;

    /// <summary>
    /// Gets a value indicating whether the snapshot includes the content of frames, beneath their iframe nodes.
    /// Without it, a frame is an iframe node with nothing beneath it. Defaults to <see langword="true"/>.
    /// </summary>
    public bool IncludeFrames { get; init; } = true;
}
