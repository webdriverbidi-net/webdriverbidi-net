// <copyright file="FakeBrowserSetUp.cs" company="WebDriverBiDi.NET Committers">
// Copyright (c) WebDriverBiDi.NET Committers. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.
// </copyright>

namespace Dramaturge.MSTest;

using Dramaturge.Browsers;
using Dramaturge.TestUtilities;

/// <summary>
/// The test assembly's registration of the shared group, which connects to a fake browser's "shared" session.
/// </summary>
[TestClass]
public static class FakeBrowserSetUp
{
    /// <summary>
    /// The name of the shared group's session.
    /// </summary>
    public const string SharedSessionName = "shared";

    private static FakeBrowserServer? server;

    /// <summary>
    /// Gets the fake browser, whose sessions answer the shared group and the classes' own groups.
    /// </summary>
    public static FakeBrowserServer Server => server!;

    /// <summary>
    /// Connects a launcher to a session of the fake browser.
    /// </summary>
    /// <param name="sessionName">The session's name.</param>
    /// <returns>The launcher.</returns>
    public static BrowserLauncherBuilder ConnectTo(string sessionName)
    {
        return BrowserLauncher.Configure(BrowserKind.Chrome).ConnectToExisting(Server.UrlFor(sessionName));
    }

    [AssemblyInitialize]
    public static async Task InitializeAsync(TestContext context)
    {
        server = await FakeBrowserServer.StartAsync();
        DramaturgeAssemblyFixture.Register(new FakeBrowserFixture());
    }

    [AssemblyCleanup]
    public static async Task CleanupAsync()
    {
        await DramaturgeAssemblyFixture.CloseAsync();
        await Server.DisposeAsync();
    }

    private sealed class FakeBrowserFixture : DramaturgeAssemblyFixture
    {
        protected override BrowserLauncherBuilder ConfigureLauncher(BrowserLauncherBuilder builder)
        {
            return ConnectTo(SharedSessionName);
        }
    }
}
