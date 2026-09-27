// <copyright file="FakeContext.cs" company="WebDriverBiDi.NET Committers">
// Copyright (c) WebDriverBiDi.NET Committers. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.
// </copyright>

namespace WebDriverBiDi.Automation.TestUtilities;

/// <summary>
/// A browsing context kept by a <see cref="FakeSession"/>.
/// </summary>
/// <param name="Id">The ID of the browsing context.</param>
/// <param name="UserContextId">The ID of its user context.</param>
/// <param name="ParentId">The ID of its parent, or <see langword="null"/> for a top-level context.</param>
/// <param name="Url">Its URL.</param>
public sealed record FakeContext(string Id, string UserContextId, string? ParentId, string Url);
