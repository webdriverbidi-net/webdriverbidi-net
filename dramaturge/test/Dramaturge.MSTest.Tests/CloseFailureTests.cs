// <copyright file="CloseFailureTests.cs" company="WebDriverBiDi.NET Committers">
// Copyright (c) WebDriverBiDi.NET Committers. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.
// </copyright>

namespace Dramaturge.MSTest;

using Dramaturge.Browsers;

// The class's session is left unable to close browsers, so it has a session of its own.
[TestClass]
public class CloseFailureTests
{
    private const string SessionName = "close-failure";

    public TestContext TestContext { get; set; } = null!;

    [ClassCleanup]
    public static Task CloseGroupAsync(TestContext context) => BrowserTest.CloseClassGroupAsync(context);

    [TestMethod]
    public async Task FailureToCloseABrowserIsWrittenToTheOutputAndTheOthersStillClose()
    {
        ClosingTest test = new() { TestContext = this.TestContext };
        await test.SetUpBrowsersAsync();
        await test.OpenPageAsync();
        await test.NewBrowserAsync();
        FakeBrowserSetUp.Server.SessionFor(SessionName).RemoteEnd.FailWith("browser.removeUserContext", "unknown error", "The browser cannot be closed.");

        EndingContext ending = await EndingContext.EndAsync(test, this.TestContext, UnitTestOutcome.Passed);

        Assert.HasCount(2, FakeBrowserSetUp.Server.SessionFor(SessionName).RemoteEnd.CommandsFor("browser.removeUserContext"));
        string[] lines = ending.Output.Split(Environment.NewLine, StringSplitOptions.RemoveEmptyEntries);
        Assert.HasCount(2, lines);
        Assert.IsTrue(lines.All(line => line.StartsWith("Dramaturge could not close a browser: ", StringComparison.Ordinal)));
    }

    private sealed class ClosingTest : PageTest
    {
        protected override BrowserLauncherBuilder ConfigureLauncher(BrowserLauncherBuilder builder)
        {
            return FakeBrowserSetUp.ConnectTo(SessionName);
        }
    }
}
