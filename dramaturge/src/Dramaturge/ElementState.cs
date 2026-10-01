// <copyright file="ElementState.cs" company="WebDriverBiDi.NET Committers">
// Copyright (c) WebDriverBiDi.NET Committers. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.
// </copyright>

namespace Dramaturge;

/// <summary>
/// A state an element can be waited for.
/// </summary>
public enum ElementState
{
    /// <summary>
    /// The element is in the document.
    /// </summary>
    Attached,

    /// <summary>
    /// No element matches.
    /// </summary>
    Detached,

    /// <summary>
    /// The element is in the document and visible: it has a box of non-zero size and is not styled hidden.
    /// </summary>
    Visible,

    /// <summary>
    /// No element matches, or the one that does is not visible.
    /// </summary>
    Hidden,
}
