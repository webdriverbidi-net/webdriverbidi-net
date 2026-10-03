// <copyright file="TestFrameworksNUnitSamples.cs" company="WebDriverBiDi.NET Committers">
// Copyright (c) WebDriverBiDi.NET Committers. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.
// </copyright>
// Code snippets for docs/articles/test-frameworks.md and src/Dramaturge.NUnit/README.md. The namespace is outside
// Dramaturge's, as a test project's is, so that "NUnit" means NUnit's namespace.

namespace TestFrameworkSamples.NUnitTests;

using Dramaturge.NUnit;
using NUnit.Framework;
using static Dramaturge.Assertions;

#region NUnitRegistration
[SetUpFixture]
public class DramaturgeSetUp : DramaturgeSetUpFixture
{
}
#endregion

#region NUnitPageTest
public class SignInTests : PageTest
{
    [Test]
    public async Task SignsIn()
    {
        await this.Page.NavigateAsync("https://example.com/sign-in");
        await this.Page.GetByLabel("Email").FillAsync("someone@example.com");
        await this.Page.GetByRole("button", "Sign in").ClickAsync();
        await Expect(this.Page.GetByRole("heading", "Welcome")).ToBeVisibleAsync();
    }
}
#endregion
