// <copyright file="LaunchFailureTests.cs" company="WebDriverBiDi.NET Committers">
// Copyright (c) WebDriverBiDi.NET Committers. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.
// </copyright>

namespace Dramaturge.NUnit;

using global::NUnit.Framework.Interfaces;

using Dramaturge.Browsers;

// The tests here create test objects themselves, which share the group of this fixture, as its tests would.
public class LaunchFailureTests
{
    [OneTimeTearDown]
    public Task CloseGroupAsync() => BrowserTest.CloseFixtureGroupAsync();

    [Test]
    public async Task FailedLaunchFailsEveryTestWithTheSameException()
    {
        FailingLaunchTest first = new();
        FailingLaunchTest second = new();

        InvalidOperationException firstException = (await Assert.ThrowsAsync<InvalidOperationException>(first.SetUpBrowsersAsync))!;
        InvalidOperationException secondException = (await Assert.ThrowsAsync<InvalidOperationException>(second.SetUpBrowsersAsync))!;

        Assert.That(secondException, Is.SameAs(firstException));
        Assert.That(FailingLaunchTest.LaunchCount, Is.EqualTo(1));

        // A test whose group never launched has nothing to close.
        TestEnding ending = await TestEnding.EndAsync(first, ResultState.Failure);
        Assert.That(ending.Output, Is.Empty);
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
