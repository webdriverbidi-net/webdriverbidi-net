// <copyright file="DramaturgeAssemblyFixture.cs" company="WebDriverBiDi.NET Committers">
// Copyright (c) WebDriverBiDi.NET Committers. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.
// </copyright>

namespace Dramaturge.Xunit;

using Dramaturge.Browsers;
using Dramaturge.Testing;
using global::Xunit;

/// <summary>
/// The browser group shared by a test assembly's <see cref="BrowserTest"/> classes, launched when a test first needs
/// it and closed when the run ends. Register it, or a class derived from it, once in the test project:
/// <code>[assembly: AssemblyFixture(typeof(Dramaturge.Xunit.DramaturgeAssemblyFixture))]</code>
/// </summary>
public class DramaturgeAssemblyFixture : IAsyncLifetime
{
    private const string RegistrationLine = "[assembly: AssemblyFixture(typeof(Dramaturge.Xunit.DramaturgeAssemblyFixture))]";

    private static DramaturgeAssemblyFixture? registered;

    private readonly SharedBrowserGroup group = new();

    /// <summary>
    /// Gets the options of the shared group.
    /// </summary>
    protected virtual DramaturgeOptions? GroupOptions => null;

    /// <summary>
    /// Registers the fixture, so that tests can launch the shared group. A derived class that overrides this must call it.
    /// </summary>
    /// <returns>A task that completes when the fixture is registered.</returns>
    /// <exception cref="InvalidOperationException">Thrown when another fixture is registered.</exception>
    public virtual ValueTask InitializeAsync()
    {
        DramaturgeAssemblyFixture? existing = Interlocked.CompareExchange(ref registered, this, null);
        if (existing is not null)
        {
            throw new InvalidOperationException($"A Dramaturge assembly fixture, {existing.GetType().FullName}, is already registered; register only one.");
        }

        return default;
    }

    /// <summary>
    /// Closes the shared group, if it was launched. A derived class that overrides this must call it.
    /// </summary>
    /// <returns>A task that completes when the group is closed.</returns>
    public virtual async ValueTask DisposeAsync()
    {
        Interlocked.CompareExchange(ref registered, null, this);
        await this.group.DisposeAsync().ConfigureAwait(false);
        GC.SuppressFinalize(this);
    }

    /// <summary>
    /// Gets the shared group, launching it on the first call.
    /// </summary>
    /// <returns>The group.</returns>
    /// <exception cref="InvalidOperationException">Thrown when no fixture is registered.</exception>
    internal static Task<BrowserGroup> GetSharedGroupAsync()
    {
        DramaturgeAssemblyFixture fixture = Volatile.Read(ref registered)
            ?? throw new InvalidOperationException($"Dramaturge's shared browser group is not registered. Add this line to the test project, or name a class derived from DramaturgeAssemblyFixture in it:{Environment.NewLine}{RegistrationLine}");
        return fixture.group.GetAsync(fixture.ConfigureLauncher, fixture.GroupOptions);
    }

    /// <summary>
    /// Configures the launcher of the shared group.
    /// </summary>
    /// <param name="builder">The launcher the environment chooses, from <see cref="BrowserLauncher.ConfigureFromEnvironment"/>.</param>
    /// <returns>The builder to launch with: <paramref name="builder"/>, changed or not, or another.</returns>
    protected virtual BrowserLauncherBuilder ConfigureLauncher(BrowserLauncherBuilder builder)
    {
        return builder;
    }
}
