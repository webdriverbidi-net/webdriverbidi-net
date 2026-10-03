// <copyright file="SharedGroupTests.cs" company="WebDriverBiDi.NET Committers">
// Copyright (c) WebDriverBiDi.NET Committers. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.
// </copyright>

namespace Dramaturge.NUnit;

using global::NUnit.Framework.Interfaces;

using Dramaturge.TestUtilities;

public class SharedGroupTests : PageTest
{
    [Test]
    public void TestHasAPageInItsOwnBrowser()
    {
        Assert.That(this.Browser.IsDefault, Is.False);
        Assert.That(this.Browser.Pages, Does.Contain(this.Page));
        Assert.That(this.Browser.Group, Is.SameAs(this.Group));
        Assert.That(FakeBrowserSetUp.Server.SessionFor(FakeBrowserSetUp.SharedSessionName).UserContextIds, Does.Contain(this.Browser.Id));
    }

    [Test]
    public async Task FixturesThatConfigureNothingShareTheGroup()
    {
        OtherSharedTest other = new();
        await other.SetUpBrowsersAsync();
        try
        {
            Assert.That(other.Group, Is.SameAs(this.Group));
        }
        finally
        {
            await other.TearDownBrowsersAsync();
        }
    }

    [Test]
    public async Task NewBrowserIsIsolated()
    {
        Browser second = await this.NewBrowserAsync();

        Assert.That(second.Id, Is.Not.EqualTo(this.Browser.Id));
        Assert.That(this.Group.Browsers, Does.Contain(second));
    }

    [Test]
    public async Task BrowsersAreClosedWhenTheTestEnds()
    {
        FakeSession session = FakeBrowserSetUp.Server.SessionFor(FakeBrowserSetUp.SharedSessionName);
        OtherSharedTest other = new();
        await other.SetUpBrowsersAsync();
        Browser first = await other.NewBrowserAsync();
        Browser second = await other.NewBrowserAsync();

        TestEnding ending = await TestEnding.EndAsync(other, ResultState.Success);

        Assert.That(session.UserContextIds, Does.Not.Contain(first.Id).And.Not.Contain(second.Id));
        Assert.That(ending.Output, Is.Empty);
    }

    [Test]
    public void MembersThrowBeforeTheTestStarts()
    {
        OtherSharedTest other = new();

        Assert.That(() => other.Group, Throws.InvalidOperationException.With.Message.EqualTo("The browser group is available once the test has started."));
        Assert.That(() => other.Browser, Throws.InvalidOperationException.With.Message.EqualTo("The browser is available once the test has started."));
        Assert.That(() => other.Page, Throws.InvalidOperationException.With.Message.EqualTo("The page is available once the test has started."));
    }

    [Test]
    public async Task OnlyOneSetUpFixtureCanBeRegistered()
    {
        DramaturgeSetUpFixture second = new();

        Assert.That(second.RegisterDramaturge, Throws.InvalidOperationException.With.Message.EqualTo("A Dramaturge set-up fixture, Dramaturge.NUnit.FakeBrowserSetUp, is already registered; register only one."));

        // Closing an unregistered fixture leaves the registered one in place.
        await second.CloseDramaturgeAsync();
        OtherSharedTest other = new();
        await other.SetUpBrowsersAsync();
        Assert.That(other.Group, Is.SameAs(this.Group));
        await other.TearDownBrowsersAsync();
    }

    private sealed class OtherSharedTest : PageTest
    {
    }
}
