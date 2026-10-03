// <copyright file="CloseFailureTests.cs" company="WebDriverBiDi.NET Committers">
// Copyright (c) WebDriverBiDi.NET Committers. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.
// </copyright>

namespace Dramaturge.TUnit;

using Dramaturge.Browsers;
using global::TUnit.Core;

// The class's session is left unable to close browsers, so it has a session of its own; ExpectedEnding checks what
// the test wrote once it ends.
[ExpectedEnding]
public class CloseFailureTests : PageTest
{
    private const string SessionName = "close-failure";

    [Test]
    public async Task FailureToCloseABrowserIsWrittenToTheOutputAndTheOthersStillClose()
    {
        await this.NewBrowserAsync();
        FakeBrowserSetUp.Server.SessionFor(SessionName).RemoteEnd.FailWith("browser.removeUserContext", "unknown error", "The browser cannot be closed.");

        ExpectedEndingAttribute.Expect(context =>
        {
            int closeCount = FakeBrowserSetUp.Server.SessionFor(SessionName).RemoteEnd.CommandsFor("browser.removeUserContext").Count;
            string[] lines = context.Output.GetStandardOutput().Split(Environment.NewLine, StringSplitOptions.RemoveEmptyEntries);
            return closeCount != 2 ? $"Closed {closeCount} browsers"
                : lines.Length != 2 || !lines.All(line => line.StartsWith("Dramaturge could not close a browser: ", StringComparison.Ordinal)) ? $"Wrote '{string.Join("|", lines)}'"
                : null;
        });
    }

    protected override BrowserLauncherBuilder ConfigureLauncher(BrowserLauncherBuilder builder)
    {
        return FakeBrowserSetUp.ConnectTo(SessionName);
    }
}
