# WebDriverBiDi.NET Library Analysis

Analysis date: 2026-10-20. Commit analyzed: `649f57ba` (branch `acquiescence-update`, latest tag `v0.0.63`).

This analysis was produced from a fresh read of the repository, following `prompts/LIBRARY_ANALYSIS.md`. No prior analysis, memory or existing report was used as input. No source code was modified.

## Scorecards

Scores are derived solely from the verified Action Plan items below. A category with no action item scores 100.

### Main library (`src/WebDriverBiDi`)

| Category | Score | Deductions |
|---|---|---|
| Code quality | 100 | none |
| API design | 100 | none |
| Concurrency / thread-safety | 100 | none |
| Performance | 100 | none |
| Documentation | 98 | LIB-DOC-1 |
| Testing | 99 | LIB-TEST-1 |

### Logging library (`src/WebDriverBiDi.Logging`)

| Category | Score | Deductions |
|---|---|---|
| Code quality | 100 | none |
| API design | 100 | none |
| Concurrency / thread-safety | 100 | none |
| Performance | 100 | none |
| Documentation | 100 | none |
| Testing | 100 | none |

### Analyzers (`src/WebDriverBiDi.Analyzers`, including the code-fix providers)

| Category | Score | Deductions |
|---|---|---|
| Code quality | 100 | none |
| API design | 100 | none |
| Concurrency / thread-safety | 100 | none |
| Performance | 100 | none |
| Documentation | 98 | ANA-DOC-1 |
| Testing | 97 | ANA-TEST-1 |

## What the project is

WebDriverBiDi.NET is a protocol-level .NET client for the W3C WebDriver BiDi specification. The main library targets `netstandard2.0`, `net8.0`, `net9.0` and `net10.0`, uses System.Text.Json with a source-generated serializer context so the modern targets are AOT-compatible, and exposes the protocol as it is: fifteen modules (the nine core spec modules plus Bluetooth, Digital Credentials, Permissions, Speculation, User Agent Client Hints and WebExtension), each with one strongly-typed command method per protocol command and one observable event per protocol event.

The runtime shape is `BiDiDriver` (a facade that owns the modules and command execution) over `Transport` (sessions, pending-command tracking with a canceled-command window, an unbounded incoming-message channel, a three-state connection lifecycle, unhandled-error collection with configurable Ignore/Collect/Terminate behaviors) over an abstract `Connection` with WebSocket and anonymous-pipe implementations. Events use a custom observer pattern (`ObservableEvent<T>`, `EventObserver<T>`, capture sessions, an `IObservable<T>` adapter) with a reporter pipeline that routes observer failures to the driver's error behaviors. Diagnostics are exposed through a `WebDriverBiDiEventSource` with 22 events.

Two companion artifacts are scored alongside it: `WebDriverBiDi.Logging`, which bridges the EventSource into `Microsoft.Extensions.Logging`, and `WebDriverBiDi.Analyzers`, a set of 36 Roslyn analyzers (BIDI001 through BIDI039, with 011, 018 and 019 retired) and 18 code-fix providers shipped for Roslyn 4.8 and 5.9. `WebDriverBiDi.Extensions`, `WebDriverBiDi.Client` and `WebDriverBiDi.Demo` are outside the scored scope.

The project enforces its own conventions with tests: `WebDriverBiDiConventionTests` (list-property shapes, the optional-parameter rule, enum converters, event naming, received-type immutability, `SpecRange` usage) and `AnalyzerConventionTests` (every analyzer concurrent, generated-code-aware, documented and listed in the shipped-releases file). CI runs the suites in Release and Debug with a 95% coverage gate, a Windows job with the `net48` test project, an analyzer compatibility job, browser integration jobs and a benchmark job compared against committed baselines.

## Main library

### Findings

**LIB-DOC-1 (Documentation).** The XML documentation of `ObservableEventInvocable<T>.InvokeNotifyObserversAsync` and two passages of the custom-modules guide describe observer failures as propagating to the caller unconditionally, but when an observer-error reporter is installed, which is the case for every event registered through `Module` while hosted by `BiDiDriver`, a synchronously-run observer's failure is reported through the reporter and the method does not throw.

Evidence:

- `src/WebDriverBiDi/ObservableEventInvocable{T}.cs` lines 43–53: the summary says "If exactly one observer throws, the original exception is rethrown. If multiple observers throw, an `AggregateException` containing all caught exceptions is thrown", and line 52 documents `<exception cref="AggregateException">` with no qualification.
- `src/WebDriverBiDi/ObservableEvent{T}.cs` lines 255–261 (the `NotifyObserversAsync` that the invocable forwards to) document the actual contract: "When an observer-error reporter is installed, as it is for every event the library creates, a failing observer reports its own failure through it ... and this method does not throw for it. Without a reporter, a failure of a synchronously-run observer propagates ...".
- `src/WebDriverBiDi/EventObserver{T}.cs` lines 675–691: `catch (Exception ex) when (observerErrorReporter is not null)` reports the synchronous failure and returns instead of throwing.
- `src/WebDriverBiDi/Module.cs` lines 77–84: `ConfigureObserverErrorReporting` installs the reporter on every registered event when the host implements `IEventObserverErrorReporter`, which `BiDiDriver` does.
- `docs/articles/advanced/custom-modules.md` lines 90–91 ("connect the invocable to the driver's reporting of asynchronous observer failures") and 94–95 ("the callback that receives the failures of asynchronous observers") restrict the reporter to asynchronous observers; the reporter also receives synchronous observers' failures, as the `IsAsynchronousHandler = false` report in `EventObserver<T>` shows. The callout at lines 107–114 is scoped to the `RunHandlerAsynchronously` case and is accurate as written.

**LIB-TEST-1 (Testing).** The benchmark baselines folder carries a naming convention and copy instructions that contradict the convention the benchmark job and the benchmark reporting guide actually use, so a contributor following the file would place a baseline where CI never reads it.

Evidence:

- `test/WebDriverBiDi.Benchmarks/baselines/.gitkeep` says the convention is `v{VERSION}-baseline.json`, shows `v1.0.0-baseline.json` examples and a `cp ... v1.0.0-baseline.json` command, and says the files "can optionally be committed".
- The folder contains five committed `ci-baseline-<BenchmarkClass>.json` files and nothing in the `v*-baseline.json` shape.
- `.github/workflows/ci.yml` line 139 onward ("Compare against baseline") reads `test/WebDriverBiDi.Benchmarks/baselines`, and `test/WebDriverBiDi.Benchmarks/REPORTING.md` lines 63–70 document the baseline as "a set of `ci-baseline-*.json` files committed to" that folder.

### Action plan

1. **LIB-DOC-1**
   - Artifact: `src/WebDriverBiDi/ObservableEventInvocable{T}.cs`, `docs/articles/advanced/custom-modules.md`.
   - Recommendation: replace the summary, remarks and `<exception>` element of `InvokeNotifyObserversAsync` with the contract stated on `ObservableEvent<T>.NotifyObserversAsync` (an `<inheritdoc cref="ObservableEvent{T}.NotifyObserversAsync(T)"/>` plus a one-line summary is sufficient), so the `AggregateException` and rethrow wording is conditioned on no reporter being installed. In `custom-modules.md`, change the two passages at lines 90–91 and 94–95 from "asynchronous observer failures" / "failures of asynchronous observers" to "observer failures", since the reporter receives synchronous and asynchronous failures alike. Leave the `RunHandlerAsynchronously` callout at lines 107–114 unchanged.
   - Evidence: listed under the finding.
   - Verification: build the docs project and the solution with `-warnaserror --no-incremental` (the doc-region validation runs as part of the build); read the regenerated API page for `InvokeNotifyObserversAsync` and confirm it no longer promises an unconditional throw. No test change is required; the behavior is already covered by the `EventObserver<T>` reporter tests.

2. **LIB-TEST-1**
   - Artifact: `test/WebDriverBiDi.Benchmarks/baselines/.gitkeep`.
   - Recommendation: rewrite the file to state the real convention: one committed `ci-baseline-<BenchmarkClass>.json` per benchmark class, produced by the `benchmark-baseline.yml` workflow and compared by the `ci.yml` benchmark job, with a pointer to `REPORTING.md` for how to refresh them. Remove the `v{VERSION}-baseline.json` naming, the `cp` example and the "optionally committed" sentence. Alternatively delete the file, since the folder is no longer empty and the `.gitkeep` serves no purpose.
   - Evidence: listed under the finding.
   - Verification: `ls test/WebDriverBiDi.Benchmarks/baselines` shows only `ci-baseline-*.json` files and, if kept, a `.gitkeep` whose text matches `REPORTING.md` and the `ci.yml` baseline step.

## Logging library

### Findings

None. `WebDriverBiDiEventSourceLogger`, `WebDriverBiDiLoggerActivator` and `WebDriverBiDiLoggingExtensions` were read in full; the event-name-to-log-level mapping covers all 22 EventSource events; the activator's lifetime handling (dispose on provider disposal, no double activation) is correct; XML documentation and `docs/articles/advanced/webdriverbidi-logging.md` match the code, including the documented limitation that the listener must be activated by resolving the provider; the tests pass with 100% line, branch and function coverage in both configurations.

### Action plan

None.

## Analyzers

### Findings

**ANA-DOC-1 (Documentation).** The analyzers guide pins an out-of-date package version in its installation snippet, and the release preparation scripts do not update it, so the drift recurs on every release.

Evidence:

- `docs/articles/advanced/analyzers.md` line 16: `<PackageReference Include="WebDriverBiDi.Analyzers" Version="0.0.61">`.
- `git describe --tags --abbrev=0` reports `v0.0.63`, and `docs/articles/getting-started.md` line 33 pins `Version="0.0.63"` for the main package.
- `scripts/prep-release.sh` (lines 17 and 94) and `scripts/prep-release.ps1` (lines 17, 109 and 114) rewrite only `docs/articles/getting-started.md`; neither script touches `analyzers.md` except to emit a reminder about rule removals (lines 267 and 361).

**ANA-TEST-1 (Testing).** One branch of `BiDiDriver017_NullableListAddAnalyzer` is never executed by the test suite in either configuration, so the analyzers artifact does not meet the project's 100% branch-coverage bar.

Evidence:

- `src/WebDriverBiDi.Analyzers/BiDiDriver017_NullableListAddAnalyzer.cs` line 173, the predicate of `GetNullableListElementType`:
  ```csharp
  return type is INamedTypeSymbol namedTypeSymbol
      && CollectionTypeNames.Contains(namedTypeSymbol.OriginalDefinition.Name)
      && namedTypeSymbol.TypeArguments.Length == 1
      && IsNullableType(namedTypeSymbol)
      ? namedTypeSymbol.TypeArguments[0]
      : null;
  ```
- Release lcov (`lcov-WebDriverBiDi.Analyzers.Tests.coverage.201026141546168.info`) branch records for line 173: `BRDA:173,8,{50,1}`, `BRDA:173,31,{49,1}`, `BRDA:173,48,{49,0}`, `BRDA:173,56,{4,45}`. Debug lcov (`...201026141609492.info`) is identical with blocks 9, 32, 49 and 57. The four branch points are the four `&&` operands in source order; the third, `namedTypeSymbol.TypeArguments.Length == 1` evaluating to false, has zero hits in both runs. Every other line, branch and function of the analyzers assembly is covered in both configurations.
- The existing BIDI017 tests reach the first operand false and the second operand false (`AddToNonGenericCollectionProperty_NoDiagnostic` with `ArrayList?`, `AddOnNullableValueTypeProperty_NoDiagnostic`, the unresolved-type tests, `AddToIEnumerableProperty_NoDiagnostic`) and the fourth operand false (the non-nullable `List<T>` tests), but no test uses a type whose name is `List`, `IList` or `ICollection` with a type-argument count other than one.

### Action plan

1. **ANA-DOC-1**
   - Artifact: `docs/articles/advanced/analyzers.md`, `scripts/prep-release.sh`, `scripts/prep-release.ps1`.
   - Recommendation: update the snippet at line 16 to `Version="0.0.63"`, and extend both release scripts so the step that rewrites the pinned version in `getting-started.md` also rewrites the `WebDriverBiDi.Analyzers` reference in `analyzers.md` (the scripts' header comments at line 17 should list both files).
   - Evidence: listed under the finding.
   - Verification: `grep -n 'Version="' docs/articles/getting-started.md docs/articles/advanced/analyzers.md` shows the same version in both; a dry run of `scripts/prep-release.sh` against a scratch copy rewrites both files.

2. **ANA-TEST-1**
   - Artifact: `test/WebDriverBiDi.Analyzers.Tests/BiDiDriver017AnalyzerTests.cs`.
   - Recommendation: add a no-diagnostic test in which a type in a `WebDriverBiDi` namespace declares a settable property of the non-generic `System.Collections.IList?` type and the test method calls `.Add(...)` on it. `System.Collections.IList` has the name `IList` and zero type arguments, so the first two operands are true and the third is false, which is the untaken branch. A user-declared non-generic class named `List` would work equally well; the framework interface avoids declaring a shadowing type.
   - Evidence: listed under the finding.
   - Verification: run the analyzer tests with coverage in both configurations:
     ```bash
     dotnet test --project test/WebDriverBiDi.Analyzers.Tests --configuration Release --coverlet --coverlet-output-format lcov
     dotnet test --project test/WebDriverBiDi.Analyzers.Tests --configuration Debug --coverlet --coverlet-output-format lcov
     ```
     and confirm the `BRDA:173,*` records for `BiDiDriver017_NullableListAddAnalyzer.cs` all show non-zero counts on both branches in both lcov files.

### Proposed new analyzers

None. Every misuse pattern considered during the read either already has a rule (nullable list initialization, driver start state, disposal, two-step subscription, custom module registration, parameter construction, capture-session lifetime) or is confined to a single API and does not meet the library-wide reach bar the prompt sets for a new rule.

## Non-issues and observations

Items examined and deliberately not counted against any score. They are recorded so a future pass does not re-raise them.

### Main library

- **Unbounded incoming channel, 60-second default timeout, `Ignore` default error behavior, strict enum deserialization, implicit `NumberRemoteValue` conversions, record non-structural equality, no builder pattern, no convenience methods, custom observer pattern, two-step subscription, `AdditionalData` as `Dictionary<string, object?>` under AOT.** All are design invariants named in the analysis prompt and all were verified to be implemented and documented as described (`architecture.md`, `api-design.md`, `events-observables.md`, `error-handling.md`, `aot-compatibility.md`).
- **List-property shapes and the optional-`CommandParameters` rule.** The nine settable-nullable lists and the eleven optional-parameter commands (`Browser.Close/CreateUserContext/GetClientWindows/GetUserContexts`, `BrowsingContext.GetTree`, `Script.GetRealms`, `Session.End/NewSession/Status`, `Storage.DeleteCookies/GetCookies`) match the allow lists in `WebDriverBiDiConventionTests`, which is the mechanism the project uses to govern them. All nineteen `CommandParameters` types with `Reset*` members have required parameters.
- **Spec coverage.** All 68 commands and 24 events in the current `index.bs` of the specification are implemented; the library additionally implements the Bluetooth, Digital Credentials, Permissions, Speculation, User Agent Client Hints and WebExtension extension specifications.
- **Transport.cs line 178** carries a stray comment, `// Message/event sent/received statistics`, above fields that are not statistics. Cosmetic; no reader is misled about behavior, so it is not scored.
- **`IBiDiDriverEvents` summary** describes the interface in historical terms ("were previously"). Accurate and harmless.
- **Test timing.** Every `Task.Delay` in `test/WebDriverBiDi.Tests` uses `Timeout.InfiniteTimeSpan` with a cancellation token or is a deadlock guard (`TransportTests` lines 4711 and 4878); no sleep-based synchronization exists.
- **`.editorconfig`** has a glob section listing `src/WebDriverBiDi.Automation` and `src/WebDriverBiDi.Browsers`, directories that no longer exist (they were extracted to the Dramaturge repository). The rules in that section still apply to the existing directories it names. Harmless.
- **Docs table of contents.** Every article under `docs/articles` is present in `docs/articles/toc.yml`; all `[!code-csharp[...]]` regions resolve, and CI compiles the samples and validates the regions (`scripts/validate-doc-regions.sh`, the `DocSnippets` project).
- **`Session.NewSessionAsync` taking optional parameters** is on the convention test's allow list and matches the prompt's optional-parameter rule.

### Logging library

- **Activation requires resolving the provider.** `AddWebDriverBiDi` registers the listener through a provider activator; nothing is captured until the provider is constructed. This is documented in `webdriverbidi-logging.md` and the package README and is a consequence of the `ILoggerProvider` lifetime model, not a defect.

### Analyzers

- **BIDI009 keeps its own start-state walk** rather than reusing `DriverStartStateWalker`. The two were compared and no diverging verdict was found for any construct in the test corpus. A structural refactor without a demonstrated benefit is excluded by the prompt.
- **Descriptor hygiene.** Every analyzer calls `EnableConcurrentExecution` and `ConfigureGeneratedCodeAnalysis`, every descriptor carries a `helpLinkUri` (BIDI007 and BIDI023 through a shared constant), no analyzer holds mutable static state, and severities and categories agree with `AnalyzerReleases.Shipped.md`; `AnalyzerReleases.Unshipped.md` is empty, which is correct with no rule changes since 0.0.62.
- **Extensions package** (`src/WebDriverBiDi.Extensions`) is not one of the three scored artifacts and was not analyzed.

## Verification log

All commands were run from the repository root at commit `649f57ba` on macOS (arm64), .NET SDK 10, with the outputs persisted to the session scratchpad (`/private/tmp/claude-501/-Users-jimevans-projects-webdriverbidi-net/b48d2bfe-f40c-4db4-a6e2-6240c13761b0/scratchpad/cov/`).

Build, both configurations, warnings as errors, non-incremental:

```bash
dotnet build WebDriverBiDi.sln --configuration Release --no-incremental -warnaserror
dotnet build WebDriverBiDi.sln --configuration Debug   --no-incremental -warnaserror
```

Result: `Build succeeded. 0 Warning(s) 0 Error(s)` in both.

Tests with coverage, both configurations, using the Microsoft Testing Platform runner and coverlet.MTP (the same flags `.github/workflows/_tests.yml` uses):

```bash
for c in Release Debug; do
  for p in WebDriverBiDi.Tests WebDriverBiDi.Analyzers.Tests WebDriverBiDi.Logging.Tests; do
    dotnet test --project test/$p --configuration $c --no-build \
      --coverlet --coverlet-output-format lcov --results-directory <scratchpad>/cov/$c
  done
done
```

| Suite | Release | Debug |
|---|---|---|
| WebDriverBiDi.Tests | 3544 passed, 0 failed | 3544 passed, 0 failed |
| WebDriverBiDi.Analyzers.Tests | 1489 passed, 0 failed | 1489 passed, 0 failed |
| WebDriverBiDi.Logging.Tests | 50 passed, 0 failed | 50 passed, 0 failed |

Coverage, computed from the `SF:` records of each artifact's own `src/` path in its suite's lcov file:

| Artifact | Config | Lines | Branches | Functions |
|---|---|---|---|---|
| WebDriverBiDi | Release | 4481/4481 (100%) | 1473/1473 (100%) | 1381/1381 (100%) |
| WebDriverBiDi | Debug | 6811/6811 (100%) | 1481/1481 (100%) | 1381/1381 (100%) |
| WebDriverBiDi.Logging | Release | 100% | 100% | 100% |
| WebDriverBiDi.Logging | Debug | 100% | 100% | 100% |
| WebDriverBiDi.Analyzers | Release | 100% | all but 1 (line 173 of BIDI017) | 100% |
| WebDriverBiDi.Analyzers | Debug | 100% | all but 1 (line 173 of BIDI017) | 100% |

Other checks:

- Specification comparison: the raw `index.bs` of the W3C WebDriver BiDi specification was downloaded and every `command` and `event` CDDL production was matched against the module command methods and observable events (68 commands, 24 events; all present).
- Analyzer conventions: `AnalyzerConventionTests` pass; a manual sweep confirmed concurrent execution, generated-code configuration, help links and `Shipped.md` agreement for all 36 rules.
- Documentation: every article under `docs/articles` and `docs/index.md`, the package READMEs, and `AnalyzerReleases.*.md` were read; cross-referenced claims (timeouts, error behaviors, subscription model, AOT limits, pipe semantics, Firefox and Chromium connection paths, logging activation) were checked against the code.
- Version pins: `git describe --tags --abbrev=0` → `v0.0.63`; `getting-started.md` pins 0.0.63; `analyzers.md` pins 0.0.61 (ANA-DOC-1).

## Limitations

- The main library's roughly five hundred protocol data-type files (command parameters, results, event args, value types) were not each read line by line. Their shape is governed by `WebDriverBiDiConventionTests` and by the source-generated serializer context, both of which were read and run, and a sample across every module was read directly. A defect confined to a single data-type file that the convention tests do not constrain could have been missed.
- `AnalyzerSymbolHelpers.cs`, `DriverStartStateWalker.cs`, `DisposalStateWalker.cs` and the longer analyzers were read in part, with the remainder verified through their tests and the coverage data rather than by direct reading.
- No browser integration tests were run locally; the CI definitions for them were reviewed instead.
- The benchmark job was not executed; its baseline-comparison logic was read, which is how LIB-TEST-1 was found.
- Per the prompt, suspicions that could not be verified were omitted rather than reported.
