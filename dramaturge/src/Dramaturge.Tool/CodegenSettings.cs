// <copyright file="CodegenSettings.cs" company="WebDriverBiDi.NET Committers">
// Copyright (c) WebDriverBiDi.NET Committers. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.
// </copyright>

namespace Dramaturge.Tool;

using Dramaturge.Browsers;

/// <summary>
/// The settings of the codegen command.
/// </summary>
/// <param name="Url">The address to open first, or <see langword="null"/> for none.</param>
/// <param name="Target">The kind of file to write.</param>
/// <param name="Browser">The browser to record in.</param>
/// <param name="Channel">The browser's release channel, as a target names it, or <see langword="null"/> for the default.</param>
/// <param name="TestIdAttribute">The test ID attribute, or <see langword="null"/> for the default.</param>
/// <param name="OutputPath">The file to keep the whole code in, or <see langword="null"/> for none.</param>
internal sealed record CodegenSettings(string? Url, CodeTarget Target, BrowserKind Browser, string? Channel, string? TestIdAttribute, string? OutputPath);
