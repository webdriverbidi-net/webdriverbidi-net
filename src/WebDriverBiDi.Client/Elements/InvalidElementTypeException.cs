// <copyright file="InvalidElementTypeException.cs" company="WebDriverBiDi.NET Committers">
// Copyright (c) WebDriverBiDi.NET Committers. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.
// </copyright>

namespace WebDriverBiDi.Client.Elements;

/// <summary>
/// The exception thrown when an operation is invoked on an element that doesn't support that operation.
/// </summary>
public class InvalidElementTypeException : LocatorException
{
    /// <summary>
    /// Initializes a new instance of the <see cref="InvalidElementTypeException" /> class.
    /// </summary>
    public InvalidElementTypeException()
        : base()
    {
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="InvalidElementTypeException" /> class with a given message.
    /// </summary>
    /// <param name="message">The message of the exception.</param>
    public InvalidElementTypeException(string message)
        : base(message)
    {
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="InvalidElementTypeException"/> class.
    /// </summary>
    /// <param name="message">The message of the exception.</param>
    /// <param name="innerException">The inner exception causing this exception.</param>
    public InvalidElementTypeException(string message, Exception innerException)
        : base(message, innerException)
    {
    }
}
