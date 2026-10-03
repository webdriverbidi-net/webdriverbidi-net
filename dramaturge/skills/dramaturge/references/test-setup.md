# Test Setup

Use the package for the test framework. Each gives two base classes, with the same members in every package:

- `PageTest`: each test gets `Browser`, a browser of its own (its own cookies and storage), and `Page`, a page in it. Most tests use this.
- `BrowserTest`: each test gets `Group` and `NewBrowserAsync()`, for tests that need several isolated browsers, such as two users of a chat.

The browser is launched once for the whole run, shared by every class; the browsers a test opens are closed when it ends; a failed test's pages are saved as screenshots under `TestResults/Dramaturge` and attached to its result. The examples project, https://github.com/webdriverbidi-net/dramaturge/tree/main/samples/Dramaturge.Examples, is a complete xUnit project built this way.

## Registering

Each test project registers the shared browser once. Without it, the first test fails with a message giving the code to add.

xUnit v3 (`Dramaturge.Xunit`):

<!-- readme-csharp: docs/code/TestFrameworksXunitSamples.cs#XunitRegistration -->
```csharp
using Xunit;

[assembly: AssemblyFixture(typeof(Dramaturge.Xunit.DramaturgeAssemblyFixture))]
```

NUnit 4+ (`Dramaturge.NUnit`), outside any namespace, or in the namespace that holds every test:

<!-- readme-csharp: docs/code/TestFrameworksNUnitSamples.cs#NUnitRegistration -->
```csharp
using Dramaturge.NUnit;
using NUnit.Framework;

[SetUpFixture]
public class DramaturgeSetUp : DramaturgeSetUpFixture
{
}
```

MSTest 4+ (`Dramaturge.MSTest`):

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

TUnit 1.6+ (`Dramaturge.TUnit`):

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

## Writing Tests

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

The same class with NUnit's `[Test]`, MSTest's `[TestClass]` and `[TestMethod]`, or TUnit's `[Test]` works with that framework's package.

## Configuring

- **The browser:** `DRAMATURGE_BROWSER` (`chrome`, `firefox`, `edge`, `safari`), `DRAMATURGE_CHANNEL` (`stable`, `beta`, `developerpreview`, `alpha`, `extendedsupport`), and `DRAMATURGE_HEADED=1` choose it; the default is headless stable Chrome. Set `DRAMATURGE_BROWSER` from a CI matrix to run the same tests in each browser.
- **In a class:** override `BrowserOptions` (each test's browser: viewport, locale, permissions, and so on). Overriding `ConfigureLauncher(builder)` or `GroupOptions` gives the class a browser launch of its own, so do it only when a class needs a differently launched browser.
- **For every class:** register a class derived from the registration's fixture (`DramaturgeAssemblyFixture`; NUnit: `DramaturgeSetUpFixture`) that overrides `ConfigureLauncher` and `GroupOptions`; MSTest and TUnit pass it to `Register` in place of `new()`. Set `DramaturgeOptions` (timeouts, `TestIdAttribute`) here, not per test.
- `ScreenshotOnFailure = false` or another `ArtifactsDirectory`, set in a constructor or a test, changes the screenshots.
- A remote grid or running browser cannot be headless: from `ConfigureLauncher`, return `BrowserLauncher.Configure(...).LaunchUsingRemoteGrid(url)` rather than changing the environment's builder.

<!-- readme-csharp: docs/code/TestFrameworksXunitSamples.cs#SharedSettings -->
```csharp
using Dramaturge;
using Dramaturge.Browsers;
using Dramaturge.Xunit;

// Registered in place of DramaturgeAssemblyFixture:
// [assembly: AssemblyFixture(typeof(ShopFixture))]
public class ShopFixture : DramaturgeAssemblyFixture
{
    protected override DramaturgeOptions? GroupOptions => new() { ActionTimeout = TimeSpan.FromSeconds(10), TestIdAttribute = "data-test" };

    protected override BrowserLauncherBuilder ConfigureLauncher(BrowserLauncherBuilder builder)
    {
        string? grid = Environment.GetEnvironmentVariable("SELENIUM_GRID_URL");
        return grid is null ? builder : BrowserLauncher.Configure(BrowserKind.Chrome).LaunchUsingRemoteGrid(new Uri(grid));
    }
}
```

## Framework Notes

- xUnit: `InitializeAsync`/`DisposeAsync` open and close the browsers; an override must call the base method.
- NUnit: `SetUpBrowsersAsync`/`TearDownBrowsersAsync`; a fixture's tests share one object, so run them in parallel only with `[FixtureLifeCycle(LifeCycle.InstancePerTestCase)]`.
- MSTest: the base class's `[TestInitialize]` runs before the test class's own, and its `[TestCleanup]` after.
- TUnit: the classes receive TUnit's test start and end events; an override of `SetUpBrowsersAsync` must call the base method.

## Without a Package

For another framework, launch once per test class or suite and give each test a browser of its own:

<!-- readme-csharp: docs/code/skill/SkillFixtureSamples.cs#Fixture -->
```csharp
using Dramaturge;
using Dramaturge.Browsers;

/// <summary>
/// Launches one browser for a test class's tests, and closes it after them.
/// </summary>
public sealed class BrowserFixture : IAsyncDisposable
{
    private BrowserGroup? group;

    /// <summary>
    /// Gets the launched browser.
    /// </summary>
    public BrowserGroup Group => this.group ?? throw new InvalidOperationException("The browser has not been launched.");

    /// <summary>
    /// Launches the browser.
    /// </summary>
    /// <returns>A task that completes when the browser is launched.</returns>
    public async Task InitializeAsync()
    {
        this.group = await BrowserGroup.LaunchAsync(BrowserLauncher.Configure(BrowserKind.Chrome).WithHeadlessOption());
    }

    /// <inheritdoc/>
    public async ValueTask DisposeAsync()
    {
        if (this.group is not null)
        {
            await this.group.DisposeAsync();
        }
    }
}

/// <summary>
/// The base of a test class: each test gets a browser of its own, isolated from the others, and a page in it.
/// </summary>
/// <param name="fixture">The launched browser, shared by the class's tests.</param>
public abstract class PageTest(BrowserFixture fixture) : IAsyncDisposable
{
    private Browser? browser;

    /// <summary>
    /// Gets the test's page.
    /// </summary>
    protected Page Page { get; private set; } = null!;

    /// <summary>
    /// Opens the test's browser and page.
    /// </summary>
    /// <returns>A task that completes when the page is open.</returns>
    public async Task InitializeAsync()
    {
        this.browser = await fixture.Group.CreateBrowserAsync();
        this.Page = await this.browser.NewPageAsync();
    }

    /// <inheritdoc/>
    public async ValueTask DisposeAsync()
    {
        if (this.browser is not null)
        {
            await this.browser.CloseAsync();
        }
    }
}
```

Further:

- Serve fixed pages and fake APIs with routes rather than a server, so tests need no network.
- In CI, install browsers ahead with `dramaturge install chrome firefox` (`Dramaturge.Tool`) and set `DRAMATURGE_SKIP_DOWNLOAD=1`, or point `CHROME_EXECUTABLE` / `FIREFOX_EXECUTABLE` at installed browsers.
- Keep `Expect` timeouts short (the 5 s default) and action timeouts reasonable; a test should fail fast with the message `Expect` gives, not hang.
