# Test Setup

A launch is slow; a user context is cheap. Launch the browser **once per test class (or suite)**, give **each test a `Browser` of its own**, so that no cookie or storage carries over, and close it after the test. The examples project, https://github.com/webdriverbidi-net/dramaturge/tree/main/samples/Dramaturge.Examples, is a complete xUnit project built this way.

A framework-neutral pair of classes:

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

Wire them into the test framework's lifecycle:

- **xUnit v3**: implement `IAsyncLifetime` on both, and give each test class `IClassFixture<BrowserFixture>` with the fixture in its constructor.
- **NUnit**: launch in `[OneTimeSetUp]` and dispose in `[OneTimeTearDown]`; open the test's browser in `[SetUp]` and close it in `[TearDown]`.
- **MSTest**: `[ClassInitialize]`/`[ClassCleanup]` for the group, `[TestInitialize]`/`[TestCleanup]` for each test's browser.

Further:

- Serve fixed pages and fake APIs with routes rather than a server, so tests need no network.
- In CI, install browsers ahead with `dramaturge install chrome firefox` (`Dramaturge.Tool`) and set `DRAMATURGE_SKIP_DOWNLOAD=1`, or point `CHROME_EXECUTABLE` / `FIREFOX_EXECUTABLE` at installed browsers.
- Set `DramaturgeOptions` once, on the fixture's launch, not per test.
- Keep `Expect` timeouts short (the 5 s default) and action timeouts reasonable; a test should fail fast with the message `Expect` gives, not hang.
