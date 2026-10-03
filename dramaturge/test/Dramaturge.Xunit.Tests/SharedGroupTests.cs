// <copyright file="SharedGroupTests.cs" company="WebDriverBiDi.NET Committers">
// Copyright (c) WebDriverBiDi.NET Committers. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.
// </copyright>

namespace Dramaturge.Xunit;

using Dramaturge.TestUtilities;

public class SharedGroupTests(FakeBrowserFixture fixture) : PageTest
{
    [Fact]
    public void TestHasAPageInItsOwnBrowser()
    {
        Assert.False(this.Browser.IsDefault);
        Assert.Contains(this.Page, this.Browser.Pages);
        Assert.Same(this.Group, this.Browser.Group);
        Assert.Contains(this.Browser.Id, fixture.Server.SessionFor(FakeBrowserFixture.SharedSessionName).UserContextIds);
    }

    [Fact]
    public async Task ClassesThatConfigureNothingShareTheGroup()
    {
        OtherSharedTest other = new();
        await other.InitializeAsync();
        try
        {
            Assert.Same(this.Group, other.Group);
        }
        finally
        {
            await other.DisposeAsync();
        }
    }

    [Fact]
    public async Task NewBrowserIsIsolated()
    {
        Browser second = await this.NewBrowserAsync();

        Assert.NotEqual(this.Browser.Id, second.Id);
        Assert.Contains(second, this.Group.Browsers);
    }

    [Fact]
    public async Task BrowsersAreClosedWhenTheTestEnds()
    {
        FakeSession session = fixture.Server.SessionFor(FakeBrowserFixture.SharedSessionName);
        OtherSharedTest other = new();
        await other.InitializeAsync();
        Browser first = await other.NewBrowserAsync();
        Browser second = await other.NewBrowserAsync();

        await other.DisposeAsync();

        Assert.DoesNotContain(first.Id, session.UserContextIds);
        Assert.DoesNotContain(second.Id, session.UserContextIds);
        Assert.Empty(TestContext.Current.Warnings ?? []);
    }

    [Fact]
    public void MembersThrowBeforeTheTestStarts()
    {
        OtherSharedTest other = new();

        Assert.Equal("The browser group is available once the test has started.", Assert.Throws<InvalidOperationException>(() => other.Group).Message);
        Assert.Equal("The browser is available once the test has started.", Assert.Throws<InvalidOperationException>(() => other.Browser).Message);
        Assert.Equal("The page is available once the test has started.", Assert.Throws<InvalidOperationException>(() => other.Page).Message);
    }

    [Fact]
    public async Task OnlyOneAssemblyFixtureCanBeRegistered()
    {
        DramaturgeAssemblyFixture second = new();

        InvalidOperationException exception = await Assert.ThrowsAsync<InvalidOperationException>(async () => await second.InitializeAsync());

        Assert.Equal("A Dramaturge assembly fixture, Dramaturge.Xunit.FakeBrowserFixture, is already registered; register only one.", exception.Message);

        // Disposing an unregistered fixture leaves the registered one in place.
        await second.DisposeAsync();
        OtherSharedTest other = new();
        await other.InitializeAsync();
        Assert.Same(this.Group, other.Group);
        await other.DisposeAsync();
    }

    private sealed class OtherSharedTest : PageTest
    {
    }
}
