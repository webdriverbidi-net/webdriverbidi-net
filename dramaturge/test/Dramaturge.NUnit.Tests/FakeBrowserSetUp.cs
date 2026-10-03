// <copyright file="FakeBrowserSetUp.cs" company="WebDriverBiDi.NET Committers">
// Copyright (c) WebDriverBiDi.NET Committers. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.
// </copyright>

namespace Dramaturge.NUnit;

using Dramaturge.Browsers;
using Dramaturge.TestUtilities;

/// <summary>
/// The test assembly's registration of the shared group, which connects to a fake browser's "shared" session. In the
/// tests' namespace, it covers every test.
/// </summary>
[SetUpFixture]
public sealed class FakeBrowserSetUp : DramaturgeSetUpFixture
{
    /// <summary>
    /// The name of the shared group's session.
    /// </summary>
    public const string SharedSessionName = "shared";

    private static FakeBrowserServer? server;

    /// <summary>
    /// Gets the fake browser, whose sessions answer the shared group and the fixtures' own groups.
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

    [OneTimeSetUp]
    public async Task StartServerAsync()
    {
        server = await FakeBrowserServer.StartAsync();
    }

    // Runs before the base class's teardown, so it closes the group itself before stopping the server.
    [OneTimeTearDown]
    public async Task StopServerAsync()
    {
        await this.CloseDramaturgeAsync();
        await Server.DisposeAsync();
    }

    /// <inheritdoc/>
    protected override BrowserLauncherBuilder ConfigureLauncher(BrowserLauncherBuilder builder)
    {
        return ConnectTo(SharedSessionName);
    }
}
