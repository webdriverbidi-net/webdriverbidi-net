// <copyright file="TestFrameworksMSTestSamples.cs" company="WebDriverBiDi.NET Committers">
// Copyright (c) WebDriverBiDi.NET Committers. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.
// </copyright>
// Code snippets for docs/articles/test-frameworks.md and src/Dramaturge.MSTest/README.md.

namespace TestFrameworkSamples.MSTestTests;

using Dramaturge.MSTest;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using static Dramaturge.Assertions;

#region MSTestRegistration
[TestClass]
public static class DramaturgeSetUp
{
    [AssemblyInitialize]
    public static void Initialize(TestContext context) => DramaturgeAssemblyFixture.Register(new());

    [AssemblyCleanup]
    public static Task CleanupAsync() => DramaturgeAssemblyFixture.CloseAsync();
}
#endregion

#region MSTestPageTest
[TestClass]
public class SignInTests : PageTest
{
    [TestMethod]
    public async Task SignsIn()
    {
        await this.Page.NavigateAsync("https://example.com/sign-in");
        await this.Page.GetByLabel("Email").FillAsync("someone@example.com");
        await this.Page.GetByRole("button", "Sign in").ClickAsync();
        await Expect(this.Page.GetByRole("heading", "Welcome")).ToBeVisibleAsync();
    }
}
#endregion
