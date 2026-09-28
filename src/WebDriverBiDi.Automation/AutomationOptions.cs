// <copyright file="AutomationOptions.cs" company="WebDriverBiDi.NET Committers">
// Copyright (c) WebDriverBiDi.NET Committers. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.
// </copyright>

namespace WebDriverBiDi.Automation;

using System.Text.RegularExpressions;

/// <summary>
/// Options for a <see cref="BrowserGroup"/> and everything it creates, read when the group is launched or connected.
/// </summary>
public sealed class AutomationOptions
{
    private readonly TimeSpan actionTimeout = TimeSpan.FromSeconds(30);
    private readonly TimeSpan navigationTimeout = TimeSpan.FromSeconds(30);
    private readonly TimeSpan expectTimeout = TimeSpan.FromSeconds(5);
    private readonly TimeSpan pollInterval = TimeSpan.FromMilliseconds(100);
    private readonly TimeProvider timeProvider = TimeProvider.System;
    private readonly string sandboxName = "webdriverbidi-automation";
    private readonly string testIdAttribute = "data-testid";

    /// <summary>
    /// Gets the time an action, such as a click, waits for its element before failing. Defaults to 30 seconds.
    /// </summary>
    /// <exception cref="ArgumentOutOfRangeException">Thrown when set to a value that is not positive.</exception>
    public TimeSpan ActionTimeout
    {
        get => this.actionTimeout;
        init => this.actionTimeout = RequirePositive(value, nameof(this.ActionTimeout));
    }

    /// <summary>
    /// Gets the time a navigation waits to complete before failing. Defaults to 30 seconds.
    /// </summary>
    /// <exception cref="ArgumentOutOfRangeException">Thrown when set to a value that is not positive.</exception>
    public TimeSpan NavigationTimeout
    {
        get => this.navigationTimeout;
        init => this.navigationTimeout = RequirePositive(value, nameof(this.NavigationTimeout));
    }

    /// <summary>
    /// Gets the time an expectation retries before failing. Defaults to 5 seconds.
    /// </summary>
    /// <exception cref="ArgumentOutOfRangeException">Thrown when set to a value that is not positive.</exception>
    public TimeSpan ExpectTimeout
    {
        get => this.expectTimeout;
        init => this.expectTimeout = RequirePositive(value, nameof(this.ExpectTimeout));
    }

    /// <summary>
    /// Gets the interval between checks while waiting for an element or a condition. Defaults to 100 milliseconds.
    /// </summary>
    /// <exception cref="ArgumentOutOfRangeException">Thrown when set to a value that is not positive.</exception>
    public TimeSpan PollInterval
    {
        get => this.pollInterval;
        init => this.pollInterval = RequirePositive(value, nameof(this.PollInterval));
    }

    /// <summary>
    /// Gets the <see cref="System.TimeProvider"/> that times waits and timeouts.
    /// </summary>
    /// <exception cref="ArgumentNullException">Thrown when set to <see langword="null"/>.</exception>
    public TimeProvider TimeProvider
    {
        get => this.timeProvider;
        init => this.timeProvider = value ?? throw new ArgumentNullException(nameof(this.TimeProvider));
    }

    /// <summary>
    /// Gets the name of the sandbox in which the library's scripts run, isolated from the page's own scripts.
    /// Defaults to "webdriverbidi-automation".
    /// </summary>
    /// <exception cref="ArgumentException">Thrown when set to <see langword="null"/> or an empty string.</exception>
    public string SandboxName
    {
        get => this.sandboxName;
        init => this.sandboxName = string.IsNullOrEmpty(value) ? throw new ArgumentException("The sandbox name must not be empty.", nameof(this.SandboxName)) : value;
    }

    /// <summary>
    /// Gets the attribute that holds an element's test ID, for <see cref="Frame.GetByTestId"/>. Defaults to
    /// "data-testid".
    /// </summary>
    /// <exception cref="ArgumentException">Thrown when set to something that is not an attribute name: letters, digits, hyphens, and underscores, starting with a letter or underscore.</exception>
    public string TestIdAttribute
    {
        get => this.testIdAttribute;
        init => this.testIdAttribute = value is not null && Regex.IsMatch(value, "^[A-Za-z_][A-Za-z0-9_-]*$")
            ? value
            : throw new ArgumentException($"'{value}' is not an attribute name.", nameof(this.TestIdAttribute));
    }

    private static TimeSpan RequirePositive(TimeSpan value, string name)
    {
        return value > TimeSpan.Zero ? value : throw new ArgumentOutOfRangeException(name, value, "The value must be positive.");
    }
}
