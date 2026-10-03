// <copyright file="FakeBrowserFixture.cs" company="WebDriverBiDi.NET Committers">
// Copyright (c) WebDriverBiDi.NET Committers. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.
// </copyright>

[assembly: AssemblyFixture(typeof(Dramaturge.Xunit.FakeBrowserFixture))]

namespace Dramaturge.Xunit;

using Dramaturge.Browsers;
using Dramaturge.TestUtilities;

/// <summary>
/// The test assembly's registration of the shared group, which connects to a fake browser's "shared" session.
/// </summary>
public sealed class FakeBrowserFixture : DramaturgeAssemblyFixture
{
    /// <summary>
    /// The name of the shared group's session.
    /// </summary>
    public const string SharedSessionName = "shared";

    private FakeBrowserServer? server;

    /// <summary>
    /// Gets the fake browser, whose sessions answer the shared group and the test classes' own groups.
    /// </summary>
    public FakeBrowserServer Server => this.server!;

    /// <summary>
    /// Connects a launcher to a session of the fake browser.
    /// </summary>
    /// <param name="sessionName">The session's name.</param>
    /// <returns>The launcher.</returns>
    public BrowserLauncherBuilder ConnectTo(string sessionName)
    {
        return BrowserLauncher.Configure(BrowserKind.Chrome).ConnectToExisting(this.Server.UrlFor(sessionName));
    }

    /// <inheritdoc/>
    public override async ValueTask InitializeAsync()
    {
        this.server = await FakeBrowserServer.StartAsync();
        await base.InitializeAsync();
    }

    /// <inheritdoc/>
    public override async ValueTask DisposeAsync()
    {
        await base.DisposeAsync();
        await this.Server.DisposeAsync();
    }

    /// <inheritdoc/>
    protected override BrowserLauncherBuilder ConfigureLauncher(BrowserLauncherBuilder builder)
    {
        return this.ConnectTo(SharedSessionName);
    }
}
