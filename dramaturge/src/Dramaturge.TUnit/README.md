# Dramaturge.TUnit

Base classes for [TUnit](https://tunit.dev/) tests that use [Dramaturge](https://www.nuget.org/packages/Dramaturge).
TUnit 1.6 and later.

> **Pre-release:** Dramaturge is at version 0.0.x, and its API may change in any release.

- **One browser launch per test session**, shared by every test class, unless a class configures a launch of its own.
- **An isolated browser and page for each test**, closed when the test ends, so no cookies or storage carry over.
- **Screenshots of a failed test's pages**, saved under `TestResults/Dramaturge` and attached to the test's result.
- **The browser chosen by the environment**: `DRAMATURGE_BROWSER` (`chrome`, `firefox`, or `edge`),
  `DRAMATURGE_CHANNEL`, and `DRAMATURGE_HEADED=1`, or in code.

## Getting Started

```bash
dotnet add package Dramaturge.TUnit
```

Derive test classes from `PageTest`:

```csharp
using Dramaturge.TUnit;
using static Dramaturge.Assertions;

public class SignInTests : PageTest
{
    [Test]
    public async Task SignsIn()
    {
        await Page.NavigateAsync("https://example.com/sign-in");
        await Page.GetByLabel("Email").FillAsync("someone@example.com");
        await Page.GetByRole("button", "Sign in").ClickAsync();
        await Expect(Page.GetByRole("heading", "Welcome")).ToBeVisibleAsync();
    }
}
```

TUnit closes the shared browser when the session ends, so it needs no registration. To configure it, register a
class derived from `DramaturgeAssemblyFixture` before any test needs it:

```csharp
[Before(HookType.TestSession)]
public static void ConfigureDramaturge() => DramaturgeAssemblyFixture.Register(new MyDramaturgeSettings());
```

`BrowserTest` gives a test its `Group` and `NewBrowserAsync`, for tests that need several isolated browsers.

The [documentation](https://webdriverbidi-net.github.io/dramaturge/) describes the rest of Dramaturge.
