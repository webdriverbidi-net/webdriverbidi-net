// <copyright file="LaunchFailureTests.cs" company="WebDriverBiDi.NET Committers">
// Copyright (c) WebDriverBiDi.NET Committers. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.
// </copyright>

namespace Dramaturge.TUnit;

using Dramaturge.Browsers;
using global::TUnit.Core;

// The tests here create test objects themselves, which share the group of this class, as its tests would.
public class LaunchFailureTests
{
    [After(HookType.Class)]
    public static Task CloseGroupAsync(ClassHookContext context) => BrowserTest.CloseClassGroupAsync(context);

    [Test]
    public async Task FailedLaunchFailsEveryTestWithTheSameException()
    {
        FailingLaunchTest first = new();
        FailingLaunchTest second = new();

        InvalidOperationException? firstException = await Assert.That(first.SetUpBrowsersAsync).Throws<InvalidOperationException>();
        InvalidOperationException? secondException = await Assert.That(second.SetUpBrowsersAsync).Throws<InvalidOperationException>();

        await Assert.That(secondException).IsSameReferenceAs(firstException);
        await Assert.That(FailingLaunchTest.LaunchCount).IsEqualTo(1);

        // A test whose group never launched has nothing to close.
        await first.TearDownBrowsersAsync();
        await Assert.That(TestContext.Current!.Output.GetStandardOutput()).IsEmpty();
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
