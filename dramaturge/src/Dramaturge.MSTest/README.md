# Dramaturge.MSTest

Base classes for [MSTest](https://learn.microsoft.com/dotnet/core/testing/unit-testing-mstest-intro) tests that use
[Dramaturge](https://www.nuget.org/packages/Dramaturge). MSTest 4.0 and later.

> **Pre-release:** Dramaturge is at version 0.0.x, and its API may change in any release.

- **One browser launch per test assembly**, shared by every test class, unless a class configures a launch of its own.
- **An isolated browser and page for each test**, closed when the test ends, so no cookies or storage carry over.
- **Screenshots of a failed test's pages**, saved under `TestResults/Dramaturge` and attached to the test's result.
- **The browser chosen by the environment**: `DRAMATURGE_BROWSER` (`chrome`, `firefox`, `edge`, or `safari`),
  `DRAMATURGE_CHANNEL`, and `DRAMATURGE_HEADED=1`, or in code.

## Getting Started

```bash
dotnet add package Dramaturge.MSTest
```

Register the shared browser once in the test project. MSTest runs assembly hooks only from the project's own classes:

<!-- readme-csharp: docs/code/TestFrameworksMSTestSamples.cs#MSTestRegistration -->
```csharp
using Dramaturge.MSTest;
using Microsoft.VisualStudio.TestTools.UnitTesting;

[TestClass]
public static class DramaturgeSetUp
{
    [AssemblyInitialize]
    public static void Initialize(TestContext context) => DramaturgeAssemblyFixture.Register(new());

    [AssemblyCleanup]
    public static Task CleanupAsync() => DramaturgeAssemblyFixture.CloseAsync();
}
```

Then derive test classes from `PageTest`:

<!-- readme-csharp: docs/code/TestFrameworksMSTestSamples.cs#MSTestPageTest -->
```csharp
using Dramaturge.MSTest;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using static Dramaturge.Assertions;

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
```

`BrowserTest` gives a test its `Group` and `NewBrowserAsync`, for tests that need several isolated browsers.

The [documentation](https://webdriverbidi-net.github.io/dramaturge/) describes the rest of Dramaturge.
