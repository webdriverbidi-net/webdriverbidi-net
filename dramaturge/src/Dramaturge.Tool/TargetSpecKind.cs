// <copyright file="TargetSpecKind.cs" company="WebDriverBiDi.NET Committers">
// Copyright (c) WebDriverBiDi.NET Committers. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.
// </copyright>

namespace Dramaturge.Tool;

/// <summary>
/// The kinds of spec a <see cref="Target"/> can have after its '@'.
/// </summary>
internal enum TargetSpecKind
{
    /// <summary>
    /// No spec: the stable channel, or everything of the name.
    /// </summary>
    None,

    /// <summary>
    /// A release channel, such as "beta".
    /// </summary>
    Channel,

    /// <summary>
    /// A milestone, such as "131": the newest release of a major version.
    /// </summary>
    Milestone,

    /// <summary>
    /// A version, such as "131.0.6778.204".
    /// </summary>
    Version,
}
