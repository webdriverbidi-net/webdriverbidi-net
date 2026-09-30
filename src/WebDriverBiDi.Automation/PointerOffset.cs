// <copyright file="PointerOffset.cs" company="WebDriverBiDi.NET Committers">
// Copyright (c) WebDriverBiDi.NET Committers. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.
// </copyright>

namespace WebDriverBiDi.Automation;

/// <summary>
/// A position relative to the center of the visible part of an element, in CSS pixels.
/// </summary>
/// <param name="X">The horizontal distance from the center; positive values are to the right.</param>
/// <param name="Y">The vertical distance from the center; positive values are downward.</param>
public readonly record struct PointerOffset(double X, double Y);
