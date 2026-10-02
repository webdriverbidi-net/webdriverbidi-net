// <copyright file="BrowserFixture.cs" company="WebDriverBiDi.NET Committers">
// Copyright (c) WebDriverBiDi.NET Committers. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.
// </copyright>

namespace Dramaturge.Examples;

using Dramaturge.Browsers;

/// <summary>
/// Launches one browser for every test in a class, and closes it after them. The browser is Firefox if
/// FIREFOX_EXECUTABLE names one, else Chrome, at CHROME_EXECUTABLE if it is set, or downloaded on first use.
/// </summary>
public sealed class BrowserFixture : IAsyncLifetime
{
    private BrowserGroup? group;

    /// <summary>
    /// Gets the launched browser.
    /// </summary>
    public BrowserGroup Group => this.group ?? throw new InvalidOperationException("The browser has not been launched.");

    /// <inheritdoc/>
    public async ValueTask InitializeAsync()
    {
        string? firefox = Environment.GetEnvironmentVariable("FIREFOX_EXECUTABLE");
        string? chrome = Environment.GetEnvironmentVariable("CHROME_EXECUTABLE");
        BrowserLauncherBuilder builder = !string.IsNullOrEmpty(firefox)
            ? BrowserLauncher.Configure(BrowserKind.Firefox).AtLocation(firefox)
            : BrowserLauncher.Configure(BrowserKind.Chrome);
        if (string.IsNullOrEmpty(firefox) && !string.IsNullOrEmpty(chrome))
        {
            builder.AtLocation(chrome);
        }

        // Settings for every test, made once.
        DramaturgeOptions options = new() { ActionTimeout = TimeSpan.FromSeconds(10), TestIdAttribute = "data-test" };
        this.group = await BrowserGroup.LaunchAsync(builder.WithHeadlessOption(), options);
    }

    /// <inheritdoc/>
    public async ValueTask DisposeAsync()
    {
        if (this.group is not null)
        {
            await this.group.DisposeAsync();
        }
    }
}
