// <copyright file="WebDriverBiDiLoggerActivator.cs" company="WebDriverBiDi.NET Committers">
// Copyright (c) WebDriverBiDi.NET Committers. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.
// </copyright>

namespace WebDriverBiDi.Logging;

using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;

/// <summary>
/// An <see cref="ILoggerProvider"/> whose sole purpose is to force the
/// <see cref="WebDriverBiDiEventSourceLogger"/> singleton to be constructed as soon as the logging
/// pipeline is built, so that it subscribes to the WebDriver BiDi <see cref="System.Diagnostics.Tracing.EventSource"/>
/// without the application having to resolve the listener itself.
/// </summary>
/// <remarks>
/// The logging infrastructure constructs every registered <see cref="ILoggerProvider"/> when the
/// <see cref="ILoggerFactory"/> is built. Because this provider takes the
/// <see cref="WebDriverBiDiEventSourceLogger"/> as a constructor dependency, resolving this provider
/// resolves (and therefore constructs and subscribes) the listener. The provider itself contributes
/// nothing to application logging; <see cref="CreateLogger"/> returns a no-op logger.
/// </remarks>
internal sealed class WebDriverBiDiLoggerActivator : ILoggerProvider
{
    /// <summary>
    /// Initializes a new instance of the <see cref="WebDriverBiDiLoggerActivator"/> class.
    /// </summary>
    /// <param name="listener">
    /// The event-source listener to activate. Resolving it constructs it, which subscribes it to the
    /// WebDriver BiDi EventSource. The dependency-injection container owns the listener's lifetime, so
    /// no reference is retained here.
    /// </param>
    public WebDriverBiDiLoggerActivator(WebDriverBiDiEventSourceLogger listener)
    {
        _ = listener;
    }

    /// <inheritdoc />
    public ILogger CreateLogger(string categoryName) => NullLogger.Instance;

    /// <inheritdoc />
    public void Dispose()
    {
        // Nothing to dispose here: the WebDriverBiDiEventSourceLogger is a dependency-injection
        // singleton, and the container disposes it (unsubscribing the listener) when the provider is
        // disposed.
    }
}
