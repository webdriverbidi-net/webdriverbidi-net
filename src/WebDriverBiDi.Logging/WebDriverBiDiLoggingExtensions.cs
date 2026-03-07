// <copyright file="WebDriverBiDiLoggingExtensions.cs" company="WebDriverBiDi.NET Committers">
// Copyright (c) WebDriverBiDi.NET Committers. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.
// </copyright>

namespace Microsoft.Extensions.Logging;

using System.Diagnostics.Tracing;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using WebDriverBiDi.Logging;

/// <summary>
/// Extension methods for adding WebDriver BiDi diagnostic logging to ILoggingBuilder.
/// </summary>
public static class WebDriverBiDiLoggingExtensions
{
    /// <summary>
    /// Adds WebDriver BiDi diagnostic event logging to the logging pipeline.
    /// This enables WebDriverBiDi EventSource events to be captured by ILogger.
    /// </summary>
    /// <param name="builder">The <see cref="ILoggingBuilder"/> to add logging to.</param>
    /// <returns>The <see cref="ILoggingBuilder"/> so that additional calls can be chained.</returns>
    /// <remarks>
    /// <para>
    /// This method registers a singleton <see cref="WebDriverBiDiEventSourceLogger"/> that listens
    /// to WebDriverBiDi diagnostic events and forwards them to the configured logging infrastructure.
    /// </para>
    /// <para>
    /// By default, captures events at <see cref="EventLevel.Informational"/> and above.
    /// Use the overload to specify a different minimum event level.
    /// </para>
    /// <para>
    /// <strong>Example:</strong>
    /// </para>
    /// <code>
    /// services.AddLogging(builder =>
    /// {
    ///     builder.AddConsole();
    ///     builder.AddWebDriverBiDi();
    /// });
    /// </code>
    /// </remarks>
    public static ILoggingBuilder AddWebDriverBiDi(this ILoggingBuilder builder)
    {
        return builder.AddWebDriverBiDi(EventLevel.Informational);
    }

    /// <summary>
    /// Adds WebDriver BiDi diagnostic event logging to the logging pipeline with a specified minimum event level.
    /// This enables WebDriverBiDi EventSource events to be captured by ILogger.
    /// </summary>
    /// <param name="builder">The <see cref="ILoggingBuilder"/> to add logging to.</param>
    /// <param name="minimumLevel">The minimum <see cref="EventLevel"/> to capture.</param>
    /// <returns>The <see cref="ILoggingBuilder"/> so that additional calls can be chained.</returns>
    /// <remarks>
    /// <para>
    /// This method registers a singleton <see cref="WebDriverBiDiEventSourceLogger"/> that listens
    /// to WebDriverBiDi diagnostic events and forwards them to the configured logging infrastructure.
    /// </para>
    /// <para>
    /// <strong>Event Level Mapping:</strong>
    /// </para>
    /// <list type="bullet">
    /// <item><description><see cref="EventLevel.Verbose"/> → <see cref="LogLevel.Debug"/></description></item>
    /// <item><description><see cref="EventLevel.Informational"/> → <see cref="LogLevel.Information"/></description></item>
    /// <item><description><see cref="EventLevel.Warning"/> → <see cref="LogLevel.Warning"/></description></item>
    /// <item><description><see cref="EventLevel.Error"/> → <see cref="LogLevel.Error"/></description></item>
    /// <item><description><see cref="EventLevel.Critical"/> → <see cref="LogLevel.Critical"/></description></item>
    /// </list>
    /// <para>
    /// <strong>Example:</strong>
    /// </para>
    /// <code>
    /// services.AddLogging(builder =>
    /// {
    ///     builder.AddConsole();
    ///     builder.AddWebDriverBiDi(EventLevel.Verbose); // Capture all events including verbose
    /// });
    /// </code>
    /// </remarks>
    public static ILoggingBuilder AddWebDriverBiDi(this ILoggingBuilder builder, EventLevel minimumLevel)
    {
        if (builder == null)
        {
            throw new ArgumentNullException(nameof(builder));
        }

        // Register the EventSource logger as a singleton
        builder.Services.TryAddSingleton(serviceProvider =>
        {
            ILogger<WebDriverBiDiEventSourceLogger> logger = serviceProvider.GetRequiredService<ILogger<WebDriverBiDiEventSourceLogger>>();
            return new WebDriverBiDiEventSourceLogger(logger, minimumLevel);
        });

        return builder;
    }
}
