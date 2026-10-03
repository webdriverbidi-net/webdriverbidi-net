// <copyright file="SharedBrowserGroup.cs" company="WebDriverBiDi.NET Committers">
// Copyright (c) WebDriverBiDi.NET Committers. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.
// </copyright>

namespace Dramaturge.Testing;

using Dramaturge.Browsers;

/// <summary>
/// A browser group launched the first time a test needs it, once however many tests ask at the same time. A failed
/// launch is not retried: every test that asks gets the same exception.
/// </summary>
internal sealed class SharedBrowserGroup : IAsyncDisposable
{
    private readonly object lockObject = new();
    private Task<BrowserGroup>? launch;

    /// <summary>
    /// Gets the group, launching it on the first call.
    /// </summary>
    /// <param name="configureLauncher">Configures the launcher, given the one the environment chooses.</param>
    /// <param name="options">The group's options, or <see langword="null"/> for the defaults.</param>
    /// <returns>The group.</returns>
    public Task<BrowserGroup> GetAsync(Func<BrowserLauncherBuilder, BrowserLauncherBuilder> configureLauncher, DramaturgeOptions? options)
    {
        lock (this.lockObject)
        {
            // Run apart from the first caller, so that its cancellation or context does not affect later callers.
            this.launch ??= Task.Run(() => BrowserGroup.LaunchAsync(configureLauncher(BrowserLauncher.ConfigureFromEnvironment()), options));
            return this.launch;
        }
    }

    /// <summary>
    /// Closes the group, if it was launched.
    /// </summary>
    /// <returns>A task that completes when the group is closed.</returns>
    public async ValueTask DisposeAsync()
    {
        Task<BrowserGroup>? launched;
        lock (this.lockObject)
        {
            launched = this.launch;
        }

        if (launched is null)
        {
            return;
        }

        BrowserGroup group;
        try
        {
            group = await launched.ConfigureAwait(false);
        }
        catch
        {
            // The tests that needed the group have reported the failure.
            return;
        }

        await group.DisposeAsync().ConfigureAwait(false);
    }
}
