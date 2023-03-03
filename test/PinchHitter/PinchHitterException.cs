// <copyright file="PinchHitterException.cs" company="PinchHitter Committers">
// Copyright (c) PinchHitter Committers. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.
// </copyright>

namespace PinchHitter;

/// <summary>
/// An exception thrown by the test web server.
/// </summary>
public class PinchHitterException : Exception
{
    /// <summary>
    /// Initializes a new instance of the <see cref="PinchHitterException"/> class.
    /// </summary>
    /// <param name="message">The message to use when throwing the exception.</param>
    public PinchHitterException(string message)
        : base(message)
    {
    }
}