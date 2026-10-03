// <copyright file="PageCapture.cs" company="WebDriverBiDi.NET Committers">
// Copyright (c) WebDriverBiDi.NET Committers. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.
// </copyright>

namespace Dramaturge.Testing;

/// <summary>
/// A screenshot of a page taken after a failed test, or the failure to take or save it.
/// </summary>
/// <param name="Path">The file the screenshot was, or was to be, written to.</param>
/// <param name="Screenshot">The PNG image, or <see langword="null"/> if it could not be taken or saved.</param>
/// <param name="Error">The failure, or <see langword="null"/> if it was taken and saved.</param>
internal sealed record PageCapture(string Path, byte[]? Screenshot, Exception? Error);
