// <copyright file="BoundingBox.cs" company="WebDriverBiDi.NET Committers">
// Copyright (c) WebDriverBiDi.NET Committers. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.
// </copyright>

namespace WebDriverBiDi.Automation;

/// <summary>
/// The box an element occupies, in CSS pixels relative to the top left corner of its frame's viewport.
/// </summary>
/// <param name="X">The distance of the box's left edge from the viewport's left edge.</param>
/// <param name="Y">The distance of the box's top edge from the viewport's top edge.</param>
/// <param name="Width">The width of the box.</param>
/// <param name="Height">The height of the box.</param>
public sealed record BoundingBox(double X, double Y, double Width, double Height);
