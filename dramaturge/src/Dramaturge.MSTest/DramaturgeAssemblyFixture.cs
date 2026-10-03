// <copyright file="DramaturgeAssemblyFixture.cs" company="WebDriverBiDi.NET Committers">
// Copyright (c) WebDriverBiDi.NET Committers. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.
// </copyright>

namespace Dramaturge.MSTest;

using Dramaturge.Browsers;
using Dramaturge.Testing;

/// <summary>
/// The browser group shared by a test assembly's <see cref="BrowserTest"/> classes, launched when a test first needs
/// it and closed when the run ends. MSTest runs assembly hooks only from the test project's own classes, so register it
/// once there:
/// <code>
/// [TestClass]
/// public static class DramaturgeSetUp
/// {
///     [AssemblyInitialize]
///     public static void Initialize(TestContext context) => DramaturgeAssemblyFixture.Register(new());
///
///     [AssemblyCleanup]
///     public static Task CleanupAsync() => DramaturgeAssemblyFixture.CloseAsync();
/// }
/// </code>
/// To configure the shared group, register a class derived from this one instead.
/// </summary>
public class DramaturgeAssemblyFixture
{
    private static readonly string RegistrationClass = string.Join(
        Environment.NewLine,
        "[TestClass]",
        "public static class DramaturgeSetUp",
        "{",
        "    [AssemblyInitialize]",
        "    public static void Initialize(TestContext context) => Dramaturge.MSTest.DramaturgeAssemblyFixture.Register(new());",
        string.Empty,
        "    [AssemblyCleanup]",
        "    public static Task CleanupAsync() => Dramaturge.MSTest.DramaturgeAssemblyFixture.CloseAsync();",
        "}");

    private static DramaturgeAssemblyFixture? registered;

    private readonly SharedBrowserGroup group = new();

    /// <summary>
    /// Gets the options of the shared group.
    /// </summary>
    protected virtual DramaturgeOptions? GroupOptions => null;

    /// <summary>
    /// Registers the fixture whose settings launch the shared group.
    /// </summary>
    /// <param name="fixture">The fixture.</param>
    /// <exception cref="InvalidOperationException">Thrown when another fixture is registered.</exception>
    public static void Register(DramaturgeAssemblyFixture fixture)
    {
        DramaturgeAssemblyFixture? existing = Interlocked.CompareExchange(ref registered, fixture, null);
        if (existing is not null)
        {
            throw new InvalidOperationException($"A Dramaturge assembly fixture, {existing.GetType().FullName}, is already registered; register only one.");
        }
    }

    /// <summary>
    /// Closes the shared group, if it was launched, and removes the registration.
    /// </summary>
    /// <returns>A task that completes when the group is closed.</returns>
    public static async Task CloseAsync()
    {
        DramaturgeAssemblyFixture? fixture = Interlocked.Exchange(ref registered, null);
        if (fixture is not null)
        {
            await fixture.group.DisposeAsync().ConfigureAwait(false);
        }
    }

    /// <summary>
    /// Gets the shared group, launching it on the first call.
    /// </summary>
    /// <returns>The group.</returns>
    /// <exception cref="InvalidOperationException">Thrown when no fixture is registered.</exception>
    internal static Task<BrowserGroup> GetSharedGroupAsync()
    {
        DramaturgeAssemblyFixture fixture = Volatile.Read(ref registered)
            ?? throw new InvalidOperationException($"Dramaturge's shared browser group is not registered. Add this class to the test project, or register a class derived from DramaturgeAssemblyFixture in it:{Environment.NewLine}{RegistrationClass}");
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
