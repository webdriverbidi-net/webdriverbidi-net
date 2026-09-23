// <copyright file="ScriptException.cs" company="WebDriverBiDi.NET Committers">
// Copyright (c) WebDriverBiDi.NET Committers. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.
// </copyright>

namespace WebDriverBiDi.Script;

/// <summary>
/// Thrown when a script run through the Script module's extension methods throws a JavaScript exception.
/// </summary>
public class ScriptException : WebDriverBiDiException
{
    /// <summary>
    /// Initializes a new instance of the <see cref="ScriptException"/> class.
    /// </summary>
    public ScriptException()
        : this("The script threw an exception.")
    {
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="ScriptException"/> class with a message.
    /// </summary>
    /// <param name="message">The message.</param>
    public ScriptException(string message)
        : base(message)
    {
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="ScriptException"/> class with a message and the exception that caused it.
    /// </summary>
    /// <param name="message">The message.</param>
    /// <param name="innerException">The exception that caused this one.</param>
    public ScriptException(string message, Exception innerException)
        : base(message, innerException)
    {
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="ScriptException"/> class from the details the browser reported.
    /// </summary>
    /// <param name="details">The details of the JavaScript exception.</param>
    public ScriptException(ExceptionDetails details)
        : base($"{details.Text} (line {details.LineNumber}, column {details.ColumnNumber})")
    {
        this.Details = details;
    }

    /// <summary>
    /// Gets the details of the JavaScript exception, including its stack trace and the thrown value,
    /// or <see langword="null"/> if the exception was not created from a browser's report.
    /// </summary>
    public ExceptionDetails? Details { get; }
}
