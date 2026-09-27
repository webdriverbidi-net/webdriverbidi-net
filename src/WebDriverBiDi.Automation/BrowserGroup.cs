// <copyright file="BrowserGroup.cs" company="WebDriverBiDi.NET Committers">
// Copyright (c) WebDriverBiDi.NET Committers. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.
// </copyright>

namespace WebDriverBiDi.Automation;

using WebDriverBiDi.Browsers;
using WebDriverBiDi.Session;

/// <summary>
/// A browser process and the WebDriver BiDi session driving it, from which browsers (user contexts) and their
/// pages are automated.
/// </summary>
public sealed class BrowserGroup : IAsyncDisposable
{
    /// <summary>
    /// The component name of the group's log messages.
    /// </summary>
    public const string LoggerComponentName = "Browser Group";

    private readonly BrowserLauncher? launcher;
    private readonly bool ownsSession;
    private readonly ObservableEventInvocable<LogMessageEventArgs> onLogMessage = new("browserGroup.logMessage");
    private int isDisposed;

    private BrowserGroup(BiDiDriver driver, AutomationOptions options, BrowserLauncher? launcher, bool ownsSession)
    {
        this.Driver = driver;
        this.Options = options;
        this.launcher = launcher;
        this.ownsSession = ownsSession;
    }

    /// <summary>
    /// Gets the driver that sends the group's commands.
    /// </summary>
    public BiDiDriver Driver { get; }

    /// <summary>
    /// Gets the options for the group and everything it creates.
    /// </summary>
    public AutomationOptions Options { get; }

    /// <summary>
    /// Gets an observable event raised when the group logs a message, such as a failure while it is disposed.
    /// </summary>
    public ObservableEvent<LogMessageEventArgs> OnLogMessage => this.onLogMessage;

    /// <summary>
    /// Launches a browser, connects a driver to it, and starts a session, if the launcher did not start one.
    /// Disposing the group ends that session and closes the browser.
    /// </summary>
    /// <param name="launcherBuilder">The configured builder of the browser's launcher.</param>
    /// <param name="options">The options for the group, or <see langword="null"/> for the defaults.</param>
    /// <param name="cancellationToken">A token that cancels the launch; anything already started is stopped.</param>
    /// <returns>The group.</returns>
    public static async Task<BrowserGroup> LaunchAsync(BrowserLauncherBuilder launcherBuilder, AutomationOptions? options = null, CancellationToken cancellationToken = default)
    {
        BrowserLauncher launcher = launcherBuilder.Build();
        BiDiDriver? driver = null;
        try
        {
            await launcher.LaunchAsync(cancellationToken).ConfigureAwait(false);
            driver = new BiDiDriver(launcher.CreateTransport());
            await driver.StartAsync(launcher.ConnectionString, cancellationToken).ConfigureAwait(false);
            bool ownsSession = !launcher.IsBiDiSessionInitialized;
            if (ownsSession)
            {
                await driver.Session.NewSessionAsync(new NewCommandParameters(), cancellationToken: cancellationToken).ConfigureAwait(false);
            }

            return new BrowserGroup(driver, options ?? new AutomationOptions(), launcher, ownsSession);
        }
        catch
        {
            if (driver is not null)
            {
                await driver.DisposeAsync().ConfigureAwait(false);
            }

            await launcher.DisposeAsync().ConfigureAwait(false);
            throw;
        }
    }

    /// <summary>
    /// Creates a group for a driver that is already connected, with its session started. Disposing the group
    /// leaves the driver, its session, and the browser as they are.
    /// </summary>
    /// <param name="driver">The connected driver.</param>
    /// <param name="options">The options for the group, or <see langword="null"/> for the defaults.</param>
    /// <param name="cancellationToken">A token that cancels the connection.</param>
    /// <returns>The group.</returns>
    public static Task<BrowserGroup> ConnectAsync(BiDiDriver driver, AutomationOptions? options = null, CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();
        return Task.FromResult(new BrowserGroup(driver, options ?? new AutomationOptions(), null, false));
    }

    /// <summary>
    /// Ends the session and closes the browser, if the group started them. Disposal does not throw; a step that
    /// fails is reported through <see cref="OnLogMessage"/>, and the steps after it still run.
    /// </summary>
    /// <returns>A task that completes when the group is disposed.</returns>
    public async ValueTask DisposeAsync()
    {
        if (Interlocked.Exchange(ref this.isDisposed, 1) == 1 || this.launcher is null)
        {
            return;
        }

        if (this.ownsSession)
        {
            try
            {
                await this.Driver.Session.EndAsync(new EndCommandParameters()).ConfigureAwait(false);
            }
            catch (Exception ex)
            {
                await this.LogAsync($"Ending the session failed: {ex.Message}", WebDriverBiDiLogLevel.Warn).ConfigureAwait(false);
            }
        }

        await this.Driver.DisposeAsync().ConfigureAwait(false);
        await this.launcher.DisposeAsync().ConfigureAwait(false);
    }

    private Task LogAsync(string message, WebDriverBiDiLogLevel level)
    {
        return this.onLogMessage.InvokeNotifyObserversAsync(new LogMessageEventArgs(message, level, LoggerComponentName));
    }
}
