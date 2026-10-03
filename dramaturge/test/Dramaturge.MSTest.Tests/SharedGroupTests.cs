// <copyright file="SharedGroupTests.cs" company="WebDriverBiDi.NET Committers">
// Copyright (c) WebDriverBiDi.NET Committers. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.
// </copyright>

namespace Dramaturge.MSTest;

using Dramaturge.TestUtilities;

[TestClass]
public class SharedGroupTests : PageTest
{
    [TestMethod]
    public void TestHasAPageInItsOwnBrowser()
    {
        Assert.IsFalse(this.Browser.IsDefault);
        Assert.Contains(this.Page, this.Browser.Pages);
        Assert.AreSame(this.Group, this.Browser.Group);
        Assert.Contains(this.Browser.Id, FakeBrowserSetUp.Server.SessionFor(FakeBrowserSetUp.SharedSessionName).UserContextIds);
    }

    [TestMethod]
    public async Task ClassesThatConfigureNothingShareTheGroup()
    {
        OtherSharedTest other = new() { TestContext = this.TestContext };
        await other.SetUpBrowsersAsync();
        await other.OpenPageAsync();
        try
        {
            Assert.AreSame(this.Group, other.Group);
        }
        finally
        {
            await other.TearDownBrowsersAsync();
        }
    }

    [TestMethod]
    public async Task NewBrowserIsIsolated()
    {
        Browser second = await this.NewBrowserAsync();

        Assert.AreNotEqual(this.Browser.Id, second.Id);
        Assert.Contains(second, this.Group.Browsers);
    }

    [TestMethod]
    public async Task BrowsersAreClosedWhenTheTestEnds()
    {
        FakeSession session = FakeBrowserSetUp.Server.SessionFor(FakeBrowserSetUp.SharedSessionName);
        OtherSharedTest other = new() { TestContext = this.TestContext };
        await other.SetUpBrowsersAsync();
        Browser first = await other.NewBrowserAsync();
        Browser second = await other.NewBrowserAsync();

        EndingContext ending = await EndingContext.EndAsync(other, this.TestContext, UnitTestOutcome.Passed);

        Assert.DoesNotContain(first.Id, session.UserContextIds);
        Assert.DoesNotContain(second.Id, session.UserContextIds);
        Assert.AreEqual(string.Empty, ending.Output);
    }

    [TestMethod]
    public void MembersThrowBeforeTheTestStarts()
    {
        OtherSharedTest other = new();

        Assert.AreEqual("The browser group is available once the test has started.", Assert.ThrowsExactly<InvalidOperationException>(() => other.Group).Message);
        Assert.AreEqual("The browser is available once the test has started.", Assert.ThrowsExactly<InvalidOperationException>(() => other.Browser).Message);
        Assert.AreEqual("The page is available once the test has started.", Assert.ThrowsExactly<InvalidOperationException>(() => other.Page).Message);
    }

    [TestMethod]
    public void OnlyOneAssemblyFixtureCanBeRegistered()
    {
        InvalidOperationException exception = Assert.ThrowsExactly<InvalidOperationException>(() => DramaturgeAssemblyFixture.Register(new DramaturgeAssemblyFixture()));

        Assert.AreEqual("A Dramaturge assembly fixture, Dramaturge.MSTest.FakeBrowserSetUp+FakeBrowserFixture, is already registered; register only one.", exception.Message);
    }

    private sealed class OtherSharedTest : PageTest
    {
    }
}
