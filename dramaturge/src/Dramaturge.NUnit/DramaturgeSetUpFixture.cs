// <copyright file="DramaturgeSetUpFixture.cs" company="WebDriverBiDi.NET Committers">
// Copyright (c) WebDriverBiDi.NET Committers. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.
// </copyright>

namespace Dramaturge.NUnit;

using Dramaturge.Browsers;
using Dramaturge.Testing;
using global::NUnit.Framework;

/// <summary>
/// The browser group shared by the <see cref="BrowserTest"/> fixtures of a test assembly, launched when a test first
/// needs it and closed when the run ends. Register it once in the test project, outside any namespace so that it
/// covers every test:
/// <code>[SetUpFixture] public class DramaturgeSetUp : Dramaturge.NUnit.DramaturgeSetUpFixture { }</code>
/// </summary>
public class DramaturgeSetUpFixture
{
    private const string RegistrationLine = "[SetUpFixture] public class DramaturgeSetUp : Dramaturge.NUnit.DramaturgeSetUpFixture { }";

    private static DramaturgeSetUpFixture? registered;

    private readonly SharedBrowserGroup group = new();

    /// <summary>
    /// Gets the options of the shared group.
    /// </summary>
    protected virtual DramaturgeOptions? GroupOptions => null;

    /// <summary>
    /// Registers the fixture, so that tests can launch the shared group.
    /// </summary>
    /// <exception cref="InvalidOperationException">Thrown when another fixture is registered.</exception>
    [OneTimeSetUp]
    public void RegisterDramaturge()
    {
        DramaturgeSetUpFixture? existing = Interlocked.CompareExchange(ref registered, this, null);
        if (existing is not null)
        {
            throw new InvalidOperationException($"A Dramaturge set-up fixture, {existing.GetType().FullName}, is already registered; register only one.");
        }
    }

    /// <summary>
    /// Closes the shared group, if it was launched.
    /// </summary>
    /// <returns>A task that completes when the group is closed.</returns>
    [OneTimeTearDown]
    public async Task CloseDramaturgeAsync()
    {
        Interlocked.CompareExchange(ref registered, null, this);
        await this.group.DisposeAsync().ConfigureAwait(false);
    }

    /// <summary>
    /// Gets the shared group, launching it on the first call.
    /// </summary>
    /// <returns>The group.</returns>
    /// <exception cref="InvalidOperationException">Thrown when no fixture is registered.</exception>
    internal static Task<BrowserGroup> GetSharedGroupAsync()
    {
        DramaturgeSetUpFixture fixture = Volatile.Read(ref registered)
            ?? throw new InvalidOperationException($"Dramaturge's shared browser group is not registered. Add this class to the test project, outside any namespace, or derive one from DramaturgeSetUpFixture in it:{Environment.NewLine}{RegistrationLine}");
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
