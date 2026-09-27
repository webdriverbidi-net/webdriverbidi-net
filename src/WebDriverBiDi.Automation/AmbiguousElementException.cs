// <copyright file="AmbiguousElementException.cs" company="WebDriverBiDi.NET Committers">
// Copyright (c) WebDriverBiDi.NET Committers. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.
// </copyright>

namespace WebDriverBiDi.Automation;

/// <summary>
/// The exception thrown when a locator that must match one element matches more than one.
/// </summary>
public class AmbiguousElementException : WebDriverBiDiException
{
    /// <summary>
    /// Initializes a new instance of the <see cref="AmbiguousElementException"/> class.
    /// </summary>
    /// <param name="message">The message.</param>
    public AmbiguousElementException(string message)
        : base(message)
    {
    }
}
