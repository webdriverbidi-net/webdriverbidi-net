// <copyright file="SharedGroupTests.cs" company="WebDriverBiDi.NET Committers">
// Copyright (c) WebDriverBiDi.NET Committers. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.
// </copyright>

namespace Dramaturge.TUnit;

using Dramaturge.TestUtilities;
using global::TUnit.Core;

public class SharedGroupTests : PageTest
{
    [Test]
    public async Task TestHasAPageInItsOwnBrowser()
    {
        await Assert.That(this.Browser.IsDefault).IsFalse();
        await Assert.That(this.Browser.Pages).Contains(this.Page);
        await Assert.That(this.Browser.Group).IsSameReferenceAs(this.Group);
        await Assert.That(FakeBrowserSetUp.Server.SessionFor(FakeBrowserSetUp.SharedSessionName).UserContextIds).Contains(this.Browser.Id);
    }

    [Test]
    public async Task ClassesThatConfigureNothingShareTheGroup()
    {
        OtherSharedTest other = new();
        await other.SetUpBrowsersAsync();
        try
        {
            await Assert.That(other.Group).IsSameReferenceAs(this.Group);
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

        await Assert.That(second.Id).IsNotEqualTo(this.Browser.Id);
        await Assert.That(this.Group.Browsers).Contains(second);
    }

    [Test]
    public async Task BrowsersAreClosedWhenTheTestEnds()
    {
        FakeSession session = FakeBrowserSetUp.Server.SessionFor(FakeBrowserSetUp.SharedSessionName);
        OtherSharedTest other = new();
        await other.SetUpBrowsersAsync();
        Browser first = await other.NewBrowserAsync();
        Browser second = await other.NewBrowserAsync();

        await other.TearDownBrowsersAsync();

        await Assert.That(session.UserContextIds).DoesNotContain(first.Id);
        await Assert.That(session.UserContextIds).DoesNotContain(second.Id);
        await Assert.That(TestContext.Current!.Output.GetStandardOutput()).IsEmpty();
    }

    [Test]
    public async Task MembersThrowBeforeTheTestStarts()
    {
        OtherSharedTest other = new();

        await Assert.That(() => other.Group).Throws<InvalidOperationException>().WithMessage("The browser group is available once the test has started.");
        await Assert.That(() => other.Browser).Throws<InvalidOperationException>().WithMessage("The browser is available once the test has started.");
        await Assert.That(() => other.Page).Throws<InvalidOperationException>().WithMessage("The page is available once the test has started.");
    }

    [Test]
    public async Task OnlyOneAssemblyFixtureCanBeRegistered()
    {
        await Assert.That(() => DramaturgeAssemblyFixture.Register(new DramaturgeAssemblyFixture()))
            .Throws<InvalidOperationException>()
            .WithMessage("A Dramaturge assembly fixture, Dramaturge.TUnit.FakeBrowserSetUp+FakeBrowserFixture, is already registered; register only one.");
    }

    private sealed class OtherSharedTest : PageTest
    {
    }
}
