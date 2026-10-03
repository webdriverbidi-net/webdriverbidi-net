// <copyright file="TestFrameworksTUnitSamples.cs" company="WebDriverBiDi.NET Committers">
// Copyright (c) WebDriverBiDi.NET Committers. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.
// </copyright>
// Code snippets for docs/articles/test-frameworks.md and src/Dramaturge.TUnit/README.md. The namespace is outside
// Dramaturge's, as a test project's is, so that "TUnit" means TUnit's namespace.

namespace TestFrameworkSamples.TUnitTests;

using Dramaturge.TUnit;
using TUnit.Core;
using static Dramaturge.Assertions;

#region TUnitRegistration
public static class DramaturgeSetUp
{
    [Before(HookType.TestSession)]
    public static void Register() => DramaturgeAssemblyFixture.Register(new());

    [After(HookType.TestSession)]
    public static Task CloseAsync() => DramaturgeAssemblyFixture.CloseAsync();
}
#endregion

#region TUnitPageTest
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
