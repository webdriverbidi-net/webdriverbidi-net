// <copyright file="NoneInputSource.cs" company="WebDriverBiDi.NET Committers">
// Copyright (c) WebDriverBiDi.NET Committers. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.
// </copyright>

namespace WebDriverBiDi.Input;

/// <summary>
/// An input source that performs only pauses, which sets how long a tick lasts without any device acting.
/// </summary>
public class NoneInputSource : InputSource
{
    /// <summary>
    /// Initializes a new instance of the <see cref="NoneInputSource"/> class.
    /// </summary>
    /// <param name="sourceId">The ID of the input source.</param>
    internal NoneInputSource(string sourceId)
        : base(sourceId)
    {
    }

    /// <summary>
    /// Gets the kind of device, <see cref="InputSourceKind.None"/>.
    /// </summary>
    public override InputSourceKind DeviceKind => InputSourceKind.None;
}
