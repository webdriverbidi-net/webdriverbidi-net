// <copyright file="FakeBrowserSetUp.cs" company="WebDriverBiDi.NET Committers">
// Copyright (c) WebDriverBiDi.NET Committers. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.
// </copyright>

namespace Dramaturge.TUnit;

using Dramaturge.Browsers;
using Dramaturge.TestUtilities;
using global::TUnit.Core;

/// <summary>
/// Registers settings for the shared group, which connect it to a fake browser's "shared" session.
/// </summary>
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

    [Before(HookType.TestSession)]
    public static async Task StartAsync()
    {
        server = await FakeBrowserServer.StartAsync();
        DramaturgeAssemblyFixture.Register(new FakeBrowserFixture());
    }

    [After(HookType.TestSession)]
    public static async Task StopAsync()
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
