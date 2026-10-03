// <copyright file="LaunchFailureTests.cs" company="WebDriverBiDi.NET Committers">
// Copyright (c) WebDriverBiDi.NET Committers. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.
// </copyright>

namespace Dramaturge.MSTest;

using Dramaturge.Browsers;

// The tests here create test objects themselves, which share the group of this class, as its tests would.
[TestClass]
public class LaunchFailureTests
{
    public TestContext TestContext { get; set; } = null!;

    [ClassCleanup]
    public static Task CloseGroupAsync(TestContext context) => BrowserTest.CloseClassGroupAsync(context);

    [TestMethod]
    public async Task FailedLaunchFailsEveryTestWithTheSameException()
    {
        FailingLaunchTest first = new() { TestContext = this.TestContext };
        FailingLaunchTest second = new() { TestContext = this.TestContext };

        InvalidOperationException firstException = await Assert.ThrowsExactlyAsync<InvalidOperationException>(first.SetUpBrowsersAsync);
        InvalidOperationException secondException = await Assert.ThrowsExactlyAsync<InvalidOperationException>(second.SetUpBrowsersAsync);

        Assert.AreSame(firstException, secondException);
        Assert.AreEqual(1, FailingLaunchTest.LaunchCount);

        // A test whose group never launched has nothing to close.
        EndingContext ending = await EndingContext.EndAsync(first, this.TestContext, UnitTestOutcome.Failed);
        Assert.AreEqual(string.Empty, ending.Output);
    }

    private sealed class FailingLaunchTest : BrowserTest
    {
        public static int LaunchCount { get; private set; }

        protected override BrowserLauncherBuilder ConfigureLauncher(BrowserLauncherBuilder builder)
        {
            LaunchCount++;
            throw new InvalidOperationException("The launcher cannot be configured.");
        }
    }
}
