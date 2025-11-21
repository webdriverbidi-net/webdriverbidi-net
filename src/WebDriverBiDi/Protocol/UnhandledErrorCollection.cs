// <copyright file="UnhandledErrorCollection.cs" company="WebDriverBiDi.NET Committers">
// Copyright (c) WebDriverBiDi.NET Committers. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.
// </copyright>

namespace WebDriverBiDi.Protocol;

/// <summary>
/// The collection of unhandled errors in the protocol transport during execution of a WebDriver BiDi session.
/// </summary>
public class UnhandledErrorCollection
{
    private readonly List<UnhandledError> unhandledErrors = [];
    private readonly Dictionary<UnhandledErrorType, TransportErrorBehavior> errorBehaviors = [];

    /// <summary>
    /// Initializes a new instance of the <see cref="UnhandledErrorCollection"/> class.
    /// </summary>
    public UnhandledErrorCollection()
    {
        this.errorBehaviors[UnhandledErrorType.ProtocolError] = TransportErrorBehavior.Ignore;
        this.errorBehaviors[UnhandledErrorType.UnknownMessage] = TransportErrorBehavior.Ignore;
        this.errorBehaviors[UnhandledErrorType.UnexpectedError] = TransportErrorBehavior.Ignore;
        this.errorBehaviors[UnhandledErrorType.EventHandlerException] = TransportErrorBehavior.Ignore;
    }

    /// <summary>
    /// Gets or sets a value indicating the behavior for errors resulting from an improper protocol message.
    /// </summary>
    public TransportErrorBehavior ProtocolErrorBehavior { get => this.errorBehaviors[UnhandledErrorType.ProtocolError]; set => this.errorBehaviors[UnhandledErrorType.ProtocolError] = value; }

    /// <summary>
    /// Gets or sets a value indicating the behavior for errors resulting from a valid JSON message that
    /// corresponds to no defined protocol command response, error response, or event definition.
    /// </summary>
    public TransportErrorBehavior UnknownMessageBehavior { get => this.errorBehaviors[UnhandledErrorType.UnknownMessage]; set => this.errorBehaviors[UnhandledErrorType.UnknownMessage] = value; }

    /// <summary>
    /// Gets or sets a value indicating the behavior for errors resulting from valid error responses
    /// that do not correspond to a command sent by the user.
    /// </summary>
    public TransportErrorBehavior UnexpectedErrorBehavior { get => this.errorBehaviors[UnhandledErrorType.UnexpectedError]; set => this.errorBehaviors[UnhandledErrorType.UnexpectedError] = value; }

    /// <summary>
    /// Gets or sets a value indicating the behavior for errors resulting from unhandled exceptions
    /// in user-defined event handlers for protocol events.
    /// </summary>
    public TransportErrorBehavior EventHandlerExceptionBehavior { get => this.errorBehaviors[UnhandledErrorType.EventHandlerException]; set => this.errorBehaviors[UnhandledErrorType.EventHandlerException] = value; }

    /// <summary>
    /// Gets the list of exceptions for the unhandled errors in the collection.
    /// </summary>
    /// <exception cref="InvalidOperationException">Thrown when there are no unhandled errors in the collection.</exception>
    public IList<Exception> Exceptions
    {
        get
        {
            if (this.unhandledErrors.Count == 0)
            {
                throw new InvalidOperationException("No unhandled errors.");
            }

            return this.unhandledErrors.Select(error => error.Exception).ToList();
        }
    }

    /// <summary>
    /// Adds an unhandled error to the collection.
    /// </summary>
    /// <param name="errorType">The type of error to add.</param>
    /// <param name="exception">The exception that causes the unhandled error.</param>
    public void AddUnhandledError(UnhandledErrorType errorType, Exception exception)
    {
        if (this.errorBehaviors[errorType] == TransportErrorBehavior.Ignore)
        {
            return;
        }

        this.unhandledErrors.Add(new UnhandledError(errorType, exception));
    }

    /// <summary>
    /// Clears the collection of unhandled errors.
    /// </summary>
    public void ClearUnhandledErrors()
    {
        this.unhandledErrors.Clear();
    }

    /// <summary>
    /// Gets a value indicating whether the collection contains errors for the specified error behavior.
    /// </summary>
    /// <param name="errorBehavior">The error behavior for which to determine if errors exist.</param>
    /// <returns><see langword="true"/> if the collection contains errors for the specified behavior; otherwise, <see langword="false"/>.</returns>
    public bool HasUnhandledErrors(TransportErrorBehavior errorBehavior)
    {
        if (errorBehavior == TransportErrorBehavior.Ignore)
        {
            return false;
        }

        List<UnhandledErrorType> behaviors = this.errorBehaviors
            .Where(pair => pair.Value == errorBehavior)
            .Select(pair => pair.Key).ToList();
        return this.unhandledErrors.Count(error => behaviors.Contains(error.ErrorType)) > 0;
    }
}
