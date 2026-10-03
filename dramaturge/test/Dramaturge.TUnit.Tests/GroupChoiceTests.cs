// <copyright file="GroupChoiceTests.cs" company="WebDriverBiDi.NET Committers">
// Copyright (c) WebDriverBiDi.NET Committers. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.
// </copyright>

namespace Dramaturge.TUnit;

using Dramaturge.Browsers;
using global::TUnit.Core;

// The tests here create test objects themselves, which share the group of this class, as its tests would.
public class GroupChoiceTests
{
    private const string SessionName = "group-choice";
    private static readonly DramaturgeOptions Options = new();

    [After(HookType.Class)]
    public static Task CloseGroupAsync(ClassHookContext context) => BrowserTest.CloseClassGroupAsync(context);

    [Test]
    public async Task ClassThatSetsOnlyGroupOptionsHasItsOwnGroupThatClosesWithTheClass()
    {
        // The first test to need the class's group launches it, with its class's launcher and options.
        LauncherTest launcherTest = new();
        await launcherTest.SetUpBrowsersAsync();
        OptionsTest optionsTest = new();
        await optionsTest.SetUpBrowsersAsync();
        SharedTest sharedTest = new();
        await sharedTest.SetUpBrowsersAsync();

        await Assert.That(optionsTest.Group).IsSameReferenceAs(launcherTest.Group);
        await Assert.That(optionsTest.Group.Options).IsSameReferenceAs(Options);
        await Assert.That(optionsTest.Group).IsNotSameReferenceAs(sharedTest.Group);
        await launcherTest.TearDownBrowsersAsync();
        await optionsTest.TearDownBrowsersAsync();
        await sharedTest.TearDownBrowsersAsync();

        await BrowserTest.CloseClassGroupAsync(TestContext.Current!.ClassContext);

        await Assert.That(FakeBrowserSetUp.Server.SessionFor(SessionName).RemoteEnd.CommandsFor("session.end")).Count().IsEqualTo(1);
    }

    private sealed class LauncherTest : BrowserTest
    {
        protected override DramaturgeOptions? GroupOptions => Options;

        protected override BrowserLauncherBuilder ConfigureLauncher(BrowserLauncherBuilder builder)
        {
            return FakeBrowserSetUp.ConnectTo(SessionName);
        }
    }

    private sealed class OptionsTest : BrowserTest
    {
        protected override DramaturgeOptions? GroupOptions => new();
    }

    private sealed class SharedTest : BrowserTest
    {
    }
}
