# Test Frameworks

Four packages give tests base classes that manage browsers for them, one for each of xUnit, NUnit, MSTest, and TUnit:

- **One browser launch per test run**, shared by every test class, unless a class configures a launch of its own.
- **An isolated browser and page for each test**, closed when the test ends, so that no cookie or storage carries over from another test.
- **Screenshots of a failed test's pages**, saved to files and attached to the test's result.
- **The browser chosen by environment variables**, so that a CI matrix runs the same tests in each browser, or in code.

| Package | Framework |
| --- | --- |
| `Dramaturge.Xunit` | xUnit v3, 3.2.2 and later |
| `Dramaturge.NUnit` | NUnit 4 and later |
| `Dramaturge.MSTest` | MSTest 4 and later |
| `Dramaturge.TUnit` | TUnit 1.6 and later |

The packages are for .NET 10. Each has the same two base classes, with the same members:

- **`PageTest`** gives each test a `Browser` and a `Page` in it. Most tests use it.
- **`BrowserTest`** gives each test the `Group`, and `NewBrowserAsync` to open browsers, for tests that need several, such as two users of a chat.

## Registering the Shared Browser

Each test project registers the browser its test classes share, once. The registration closes the browser when the run ends; without it, the first test that needs the browser fails, with a message giving the code to add.

# [xUnit](#tab/xunit)

```bash
dotnet add package Dramaturge.Xunit
```

[!code-csharp[Registration](../code/TestFrameworksXunitSamples.cs#XunitRegistration)]

# [NUnit](#tab/nunit)

```bash
dotnet add package Dramaturge.NUnit
```

A set-up fixture applies to the tests of its namespace and the namespaces within it, so put it outside any namespace, or in the namespace that holds all the tests:

[!code-csharp[Registration](../code/TestFrameworksNUnitSamples.cs#NUnitRegistration)]

# [MSTest](#tab/mstest)

```bash
dotnet add package Dramaturge.MSTest
```

MSTest runs assembly hooks only from the test project's own classes, so the registration is a class of two hooks:

[!code-csharp[Registration](../code/TestFrameworksMSTestSamples.cs#MSTestRegistration)]

# [TUnit](#tab/tunit)

```bash
dotnet add package Dramaturge.TUnit
```

TUnit runs hooks only from the assembly that declares them, so the registration is a class of two hooks:

[!code-csharp[Registration](../code/TestFrameworksTUnitSamples.cs#TUnitRegistration)]

---

## Writing Tests

Derive the test class from `PageTest`, and use `Page`. `Expect` is a static method of `Assertions`, brought in with `using static Dramaturge.Assertions;`.

# [xUnit](#tab/xunit)

[!code-csharp[Page Test](../code/TestFrameworksXunitSamples.cs#XunitPageTest)]

# [NUnit](#tab/nunit)

[!code-csharp[Page Test](../code/TestFrameworksNUnitSamples.cs#NUnitPageTest)]

# [MSTest](#tab/mstest)

[!code-csharp[Page Test](../code/TestFrameworksMSTestSamples.cs#MSTestPageTest)]

# [TUnit](#tab/tunit)

[!code-csharp[Page Test](../code/TestFrameworksTUnitSamples.cs#TUnitPageTest)]

---

A test that needs several isolated browsers derives from `BrowserTest`, and opens each with `NewBrowserAsync`. The examples in the rest of this article use xUnit; the members are the same in every package.

[!code-csharp[Several Browsers](../code/TestFrameworksXunitSamples.cs#SeveralBrowsers)]

The browsers a test opens are closed when it ends, whether it passes or fails. `Group`, `Browser`, and `Page` are available from when the test starts, and throw `InvalidOperationException` if read before.

## Choosing the Browser

The shared browser is launched from `BrowserLauncher.ConfigureFromEnvironment`: Chrome, stable, and headless, unless environment variables say otherwise. Browsers are downloaded on first use, as they are anywhere else, and the `CHROME_EXECUTABLE` and other executable variables still pick an installed browser, as [Browser Setup](browser-setup.md#downloads-and-the-cache) describes.

| Variable | Values | Default |
| --- | --- | --- |
| `DRAMATURGE_BROWSER` | `Chrome`, `Firefox`, `Edge`, `Safari` | `Chrome` |
| `DRAMATURGE_CHANNEL` | `Stable`, `Beta`, `DeveloperPreview`, `Alpha`, `ExtendedSupport` | `Stable` |
| `DRAMATURGE_HEADED` | `1` or `true` shows the browser, for debugging | Headless |

A CI matrix sets `DRAMATURGE_BROWSER` to run the same tests in each browser:

```yaml
strategy:
  matrix:
    browser: [chrome, firefox]
steps:
  - run: dotnet test
    env:
      DRAMATURGE_BROWSER: ${{ matrix.browser }}
```

## Configuring in Code

A test class overrides members of its base class:

| Member | Sets | For |
| --- | --- | --- |
| `BrowserOptions` | The [browser options](configuration.md#browser-options) of each browser a test opens without options of its own | Each test |
| `ConfigureLauncher` | The launcher, given the one the environment chooses; return it changed, or another | A class's own browser launch |
| `GroupOptions` | The [group's options](configuration.md#group-options), such as timeouts | A class's own browser launch |

[!code-csharp[Class Settings](../code/TestFrameworksXunitSamples.cs#ClassSettings)]

A class that overrides `ConfigureLauncher` or `GroupOptions` has a browser launch of its own, shared by its tests and closed when the class finishes, because its tests need a browser launched differently from the others. To configure the shared browser, register a class derived from the shared browser's fixture in place of it, overriding the same members:

[!code-csharp[Shared Settings](../code/TestFrameworksXunitSamples.cs#SharedSettings)]

With NUnit, derive the set-up fixture from `DramaturgeSetUpFixture` as the registration does; with MSTest and TUnit, pass an object of the derived class to `DramaturgeAssemblyFixture.Register` in place of `new()`.

A remote grid or a running browser cannot be launched headless, so a launcher from the environment changed to connect to one needs `WithHeadlessOption(false)`; or start from `BrowserLauncher.Configure`, as the example does.

## Screenshots of Failed Tests

When a test fails, every open page of every browser it opened is captured, before the browsers are closed, to `TestResults/Dramaturge` in the test assembly's directory: `page-1.png`, `page-2.png`, and so on, in a directory named for the test, which replaces any earlier run's. The directory's name keeps the letters, digits, `.`, `-`, and `_` of the test's name, with others replaced by `_`, and at most 120 characters of it.

Each screenshot is also attached to the test's result, where the framework's reports and IDE show it:

| Framework | Attached with |
| --- | --- |
| xUnit | `TestContext.AddAttachment`, as a `image/png` attachment |
| NUnit | `TestContext.AddTestAttachment` |
| MSTest | `TestContext.AddResultFile` |
| TUnit | `TestContext.Output.AttachArtifact` |

A page that cannot be captured, and a browser that cannot be closed, are reported as a warning (xUnit) or in the test's output (the others), and never change the test's result.

`ScreenshotOnFailure` turns capturing off, and `ArtifactsDirectory` changes the directory, for a class in its constructor or for one test:

[!code-csharp[Screenshots](../code/TestFrameworksXunitSamples.cs#Screenshots)]

## Lifecycle and Parallel Tests

Each package hooks into its framework's own lifecycle. A test class can add its own set-up and clean-up as usual:

| Framework | Browsers are opened and closed in | Notes |
| --- | --- | --- |
| xUnit | `InitializeAsync` and `DisposeAsync` (`IAsyncLifetime`) | An override must call the base method. |
| NUnit | `SetUpBrowsersAsync` and `TearDownBrowsersAsync` (`[SetUp]`, `[TearDown]`) | An override must call the base method. |
| MSTest | `SetUpBrowsersAsync`, then `PageTest.OpenPageAsync` (`[TestInitialize]`), and `TearDownBrowsersAsync` (`[TestCleanup]`) | MSTest runs these before the test class's own `[TestInitialize]` methods and after its `[TestCleanup]` methods. |
| TUnit | `SetUpBrowsersAsync` and `TearDownBrowsersAsync`, on TUnit's test start and end events | An override of `SetUpBrowsersAsync` must call the base method. |

Tests can run in parallel, each with its own browser. xUnit, MSTest, and TUnit create a test class object for each test. NUnit creates one for a fixture's tests, and a test's browsers are kept on it, so run the tests of one fixture in parallel only with `[FixtureLifeCycle(LifeCycle.InstancePerTestCase)]`.
