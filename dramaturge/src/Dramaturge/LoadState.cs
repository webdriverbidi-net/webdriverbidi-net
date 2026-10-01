// <copyright file="LoadState.cs" company="WebDriverBiDi.NET Committers">
// Copyright (c) WebDriverBiDi.NET Committers. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.
// </copyright>

namespace Dramaturge;

/// <summary>
/// How far a frame's document has loaded, in order.
/// </summary>
internal enum LoadState
{
    /// <summary>
    /// The document is being parsed.
    /// </summary>
    Loading,

    /// <summary>
    /// The document has been parsed.
    /// </summary>
    Interactive,

    /// <summary>
    /// The document and its resources have loaded.
    /// </summary>
    Complete,
}
