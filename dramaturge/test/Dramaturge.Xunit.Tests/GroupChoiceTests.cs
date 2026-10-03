// <copyright file="GroupChoiceTests.cs" company="WebDriverBiDi.NET Committers">
// Copyright (c) WebDriverBiDi.NET Committers. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.
// </copyright>

namespace Dramaturge.Xunit;

using Dramaturge.Browsers;

// The tests here create test objects themselves, which get the class group of this class, as tests of this class would.
public class GroupChoiceTests(FakeBrowserFixture fixture) : IClassFixture<ClassBrowserGroup>
{
    private static readonly DramaturgeOptions Options = new();

    [Fact]
    public async Task ClassThatSetsOnlyGroupOptionsHasItsOwnGroup()
    {
        // The first test to need the class group launches it, with its class's launcher and options.
        LauncherTest launcherTest = new(fixture);
        await launcherTest.InitializeAsync();
        OptionsTest optionsTest = new();
        await optionsTest.InitializeAsync();
        SharedTest sharedTest = new();
        await sharedTest.InitializeAsync();

        Assert.Same(launcherTest.Group, optionsTest.Group);
        Assert.Same(Options, optionsTest.Group.Options);
        Assert.NotSame(sharedTest.Group, optionsTest.Group);
        await launcherTest.DisposeAsync();
        await optionsTest.DisposeAsync();
        await sharedTest.DisposeAsync();
    }

    private sealed class LauncherTest(FakeBrowserFixture fixture) : BrowserTest
    {
        protected override DramaturgeOptions? GroupOptions => Options;

        protected override BrowserLauncherBuilder ConfigureLauncher(BrowserLauncherBuilder builder)
        {
            return fixture.ConnectTo("group-choice");
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
