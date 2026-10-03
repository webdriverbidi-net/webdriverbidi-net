# Dramaturge.Xunit

Base classes for [xUnit v3](https://xunit.net/) tests that use [Dramaturge](https://www.nuget.org/packages/Dramaturge).

> **Pre-release:** Dramaturge is at version 0.0.x, and its API may change in any release.

- **One browser launch per test assembly**, shared by every test class, unless a class configures a launch of its own.
- **An isolated browser and page for each test**, closed when the test ends, so no cookies or storage carry over.
- **Screenshots of a failed test's pages**, saved under `TestResults/Dramaturge` and attached to the test's result.
- **The browser chosen by the environment**: `DRAMATURGE_BROWSER` (`chrome`, `firefox`, `edge`, or `safari`),
  `DRAMATURGE_CHANNEL`, and `DRAMATURGE_HEADED=1`, or in code.

## Getting Started

```bash
dotnet add package Dramaturge.Xunit
```

Register the shared browser once in the test project:

<!-- readme-csharp: docs/code/TestFrameworksXunitSamples.cs#XunitRegistration -->
```csharp
using Xunit;

[assembly: AssemblyFixture(typeof(Dramaturge.Xunit.DramaturgeAssemblyFixture))]
```

Then derive test classes from `PageTest`:

<!-- readme-csharp: docs/code/TestFrameworksXunitSamples.cs#XunitPageTest -->
```csharp
using Dramaturge.Xunit;
using Xunit;
using static Dramaturge.Assertions;

public class SignInTests : PageTest
{
    [Fact]
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
