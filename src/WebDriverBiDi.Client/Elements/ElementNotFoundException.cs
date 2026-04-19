// <copyright file="ElementNotFoundException.cs" company="WebDriverBiDi.NET Committers">
// Copyright (c) WebDriverBiDi.NET Committers. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.
// </copyright>

namespace WebDriverBiDi.Client.Elements;

/// <summary>
/// The exception thrown when a locator matches zero elements during an operation requiring at least one.
/// </summary>
public class ElementNotFoundException : LocatorException
{
    /// <summary>
    /// Initializes a new instance of the <see cref="ElementNotFoundException" /> class.
    /// </summary>
    public ElementNotFoundException()
        : base()
    {
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="ElementNotFoundException" /> class with a given message.
    /// </summary>
    /// <param name="message">The message of the exception.</param>
    public ElementNotFoundException(string message)
        : base(message)
    {
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="ElementNotFoundException"/> class.
    /// </summary>
    /// <param name="message">The message of the exception.</param>
    /// <param name="innerException">The inner exception causing this exception.</param>
    public ElementNotFoundException(string message, Exception innerException)
        : base(message, innerException)
    {
    }
}
