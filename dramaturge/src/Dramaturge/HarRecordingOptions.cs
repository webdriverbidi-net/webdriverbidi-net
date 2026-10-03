// <copyright file="HarRecordingOptions.cs" company="WebDriverBiDi.NET Committers">
// Copyright (c) WebDriverBiDi.NET Committers. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.
// </copyright>

namespace Dramaturge;

using Dramaturge.Network;

/// <summary>
/// Settings of an HTTP Archive recording.
/// </summary>
public sealed class HarRecordingOptions
{
    /// <summary>
    /// Gets a condition a request must satisfy to be written to the archive, such as one on its URL, or
    /// <see langword="null"/> to write every request.
    /// </summary>
    public Func<NetworkRequest, bool>? Include { get; init; }

    /// <summary>
    /// Gets a value indicating whether request and response bodies are recorded. The default is <see langword="true"/>.
    /// </summary>
    public bool CaptureBodies { get; init; } = true;
}
