// <copyright file="LocatorException.cs" company="WebDriverBiDi.NET Committers">
// Copyright (c) WebDriverBiDi.NET Committers. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.
// </copyright>

namespace WebDriverBiDi.Client.Elements;

/// <summary>
/// The base exception class for all element locator errors.
/// </summary>
public class LocatorException : WebDriverBiDiException
{
    /// <summary>
    /// Initializes a new instance of the <see cref="LocatorException" /> class.
    /// </summary>
    public LocatorException()
        : base()
    {
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="LocatorException" /> class with a given message.
    /// </summary>
    /// <param name="message">The message of the exception.</param>
    public LocatorException(string message)
        : base(message)
    {
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="LocatorException"/> class.
    /// </summary>
    /// <param name="message">The message of the exception.</param>
    /// <param name="innerException">The inner exception causing this exception.</param>
    public LocatorException(string message, Exception innerException)
        : base(message, innerException)
    {
    }
}
