// <copyright file="ExpectationFailedException.cs" company="WebDriverBiDi.NET Committers">
// Copyright (c) WebDriverBiDi.NET Committers. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.
// </copyright>

namespace WebDriverBiDi.Automation;

/// <summary>
/// The exception thrown when an expectation is not met in time.
/// </summary>
public class ExpectationFailedException : WebDriverBiDiException
{
    /// <summary>
    /// Initializes a new instance of the <see cref="ExpectationFailedException"/> class.
    /// </summary>
    /// <param name="message">The message.</param>
    /// <param name="expected">What the expectation required, such as <c>to be visible</c>.</param>
    /// <param name="actual">What the last check saw, or <see langword="null"/> if it saw no element.</param>
    /// <param name="timeout">The time the expectation was retried.</param>
    public ExpectationFailedException(string message, string expected, string? actual, TimeSpan timeout)
        : base(message)
    {
        this.Expected = expected;
        this.Actual = actual;
        this.Timeout = timeout;
    }

    /// <summary>
    /// Gets what the expectation required, such as <c>to be visible</c> or <c>not to have count 3</c>.
    /// </summary>
    public string Expected { get; }

    /// <summary>
    /// Gets what the last check saw, such as <c>hidden</c> or <c>2</c>, or <see langword="null"/> if it saw no
    /// element.
    /// </summary>
    public string? Actual { get; }

    /// <summary>
    /// Gets the time the expectation was retried.
    /// </summary>
    public TimeSpan Timeout { get; }
}
