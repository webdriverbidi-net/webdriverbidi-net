# Dramaturge.TUnit

Base classes for [TUnit](https://tunit.dev/) tests that use [Dramaturge](https://www.nuget.org/packages/Dramaturge).
TUnit 1.6 and later.

> **Pre-release:** Dramaturge is at version 0.0.x, and its API may change in any release.

- **One browser launch per test session**, shared by every test class, unless a class configures a launch of its own.
- **An isolated browser and page for each test**, closed when the test ends, so no cookies or storage carry over.
- **Screenshots of a failed test's pages**, and videos and a trace if asked for, saved under
  `TestResults/Dramaturge` and attached to the test's result.
- **The browser chosen by the environment**: `DRAMATURGE_BROWSER` (`chrome`, `firefox`, `edge`, or `safari`),
  `DRAMATURGE_CHANNEL`, and `DRAMATURGE_HEADED=1`, or in code.

## Getting Started

```bash
dotnet add package Dramaturge.TUnit
```

Register the shared browser once in the test project. TUnit runs hooks only from the assembly that declares them:

<!-- readme-csharp: docs/code/TestFrameworksTUnitSamples.cs#TUnitRegistration -->
```csharp
using Dramaturge.TUnit;
using TUnit.Core;

public static class DramaturgeSetUp
{
    [Before(HookType.TestSession)]
    public static void Register() => DramaturgeAssemblyFixture.Register(new());

    [After(HookType.TestSession)]
    public static Task CloseAsync() => DramaturgeAssemblyFixture.CloseAsync();
}
```

Then derive test classes from `PageTest`:

<!-- readme-csharp: docs/code/TestFrameworksTUnitSamples.cs#TUnitPageTest -->
```csharp
using Dramaturge.TUnit;
using TUnit.Core;
using static Dramaturge.Assertions;

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
```

`BrowserTest` gives a test its `Group` and `NewBrowserAsync`, for tests that need several isolated browsers.

The [documentation](https://webdriverbidi-net.github.io/dramaturge/) describes the rest of Dramaturge.
