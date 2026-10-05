// <copyright file="CodeRecordingOptions.cs" company="WebDriverBiDi.NET Committers">
// Copyright (c) WebDriverBiDi.NET Committers. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.
// </copyright>

namespace Dramaturge;

/// <summary>
/// Settings of a code recording.
/// </summary>
public sealed class CodeRecordingOptions
{
    /// <summary>
    /// Gets the kind of file the recording writes. The default is <see cref="CodeTarget.Program"/>.
    /// </summary>
    public CodeTarget Target { get; init; } = CodeTarget.Program;
}
