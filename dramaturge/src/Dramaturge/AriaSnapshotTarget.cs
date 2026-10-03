// <copyright file="AriaSnapshotTarget.cs" company="WebDriverBiDi.NET Committers">
// Copyright (c) WebDriverBiDi.NET Committers. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.
// </copyright>

namespace Dramaturge;

using WebDriverBiDi.Script;

/// <summary>
/// The element a ref in an accessibility snapshot refers to, and the frame it is in.
/// </summary>
/// <param name="Frame">The frame.</param>
/// <param name="Node">The element.</param>
internal sealed record AriaSnapshotTarget(Frame Frame, NodeRemoteValue Node);
