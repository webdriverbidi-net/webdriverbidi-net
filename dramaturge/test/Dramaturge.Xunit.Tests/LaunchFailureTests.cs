// <copyright file="LaunchFailureTests.cs" company="WebDriverBiDi.NET Committers">
// Copyright (c) WebDriverBiDi.NET Committers. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.
// </copyright>

namespace Dramaturge.Xunit;

using Dramaturge.Browsers;

// The tests here create test objects themselves, which get the class group of this class, as tests of this class would.
public class LaunchFailureTests : IClassFixture<ClassBrowserGroup>
{
    [Fact]
    public async Task FailedLaunchFailsEveryTestWithTheSameException()
    {
        FailingLaunchTest first = new();
        FailingLaunchTest second = new();

        InvalidOperationException firstException = await Assert.ThrowsAsync<InvalidOperationException>(async () => await first.InitializeAsync());
        InvalidOperationException secondException = await Assert.ThrowsAsync<InvalidOperationException>(async () => await second.InitializeAsync());

        Assert.Same(firstException, secondException);
        Assert.Equal(1, FailingLaunchTest.LaunchCount);

        // A test whose group never launched has nothing to close.
        await first.DisposeAsync();
        Assert.Empty(TestContext.Current.Warnings ?? []);
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
