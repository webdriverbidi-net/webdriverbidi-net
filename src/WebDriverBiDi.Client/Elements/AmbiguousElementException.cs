// <copyright file="AmbiguousElementException.cs" company="WebDriverBiDi.NET Committers">
// Copyright (c) WebDriverBiDi.NET Committers. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.
// </copyright>

namespace WebDriverBiDi.Client.Elements;

/// <summary>
/// The exception thrown when a single-element operation is invoked but the locator matches multiple elements.
/// </summary>
public class AmbiguousElementException : LocatorException
{
    /// <summary>
    /// Initializes a new instance of the <see cref="AmbiguousElementException" /> class.
    /// </summary>
    public AmbiguousElementException()
        : base()
    {
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="AmbiguousElementException" /> class with a given message.
    /// </summary>
    /// <param name="message">The message of the exception.</param>
    public AmbiguousElementException(string message)
        : base(message)
    {
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="AmbiguousElementException"/> class.
    /// </summary>
    /// <param name="message">The message of the exception.</param>
    /// <param name="innerException">The inner exception causing this exception.</param>
    public AmbiguousElementException(string message, Exception innerException)
        : base(message, innerException)
    {
    }
}
