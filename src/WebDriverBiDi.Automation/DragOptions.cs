// <copyright file="DragOptions.cs" company="WebDriverBiDi.NET Committers">
// Copyright (c) WebDriverBiDi.NET Committers. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.
// </copyright>

namespace WebDriverBiDi.Automation;

/// <summary>
/// Options for dragging an element onto another. <see cref="PointerActionOptions.Offset"/> is where the element is
/// taken hold of, and <see cref="PointerActionOptions.Modifiers"/> are held for the whole drag.
/// </summary>
public sealed class DragOptions : PointerActionOptions
{
    /// <summary>
    /// Gets where on the target the element is dropped, relative to the center of the target's visible part, or
    /// <see langword="null"/> for the point the target's actionability check chooses.
    /// </summary>
    public PointerOffset? TargetOffset { get; init; }
}
