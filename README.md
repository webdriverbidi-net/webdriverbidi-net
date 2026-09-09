# webdriverbidi-net
A .NET client library for the WebDriver BiDi protocol

![CI](https://github.com/webdriverbidi-net/webdriverbidi-net/actions/workflows/ci.yml/badge.svg)
[![Coverage Status](https://coveralls.io/repos/github/webdriverbidi-net/webdriverbidi-net/badge.svg?branch=main&kill_cache=1)](https://coveralls.io/github/webdriverbidi-net/webdriverbidi-net?branch=main)
[![NuGet Version](https://img.shields.io/nuget/v/WebDriverBiDi)](https://www.nuget.org/packages/WebDriverBiDi)


This repository contains a library that is a .NET client for the
[WebDriver BiDi protocol specification](https://w3c.github.io/webdriver-bidi/). This spec is in progress,
and features are added to the library as the specification changes. This package is also 
[published to NuGet](https://www.nuget.org/packages/WebDriverBiDi).

This library also includes support for other modules implementing support for the WebDriver BiDi protocol,
but not included in that specification. The other specifications which have WebDriver BiDi support that are
also included in this library are:
* [Permissions](https://www.w3.org/TR/permissions/)
* [Web Bluetooth](https://webbluetoothcg.github.io/web-bluetooth/)
* [Prefetch](https://wicg.github.io/nav-speculation/prefetch.html)
* [User Agent Client Hints](https://wicg.github.io/ua-client-hints/)
* [Digital Credentials](https://www.w3.org/TR/digital-credentials/)

The library allows a user to automate a browser using WebDriver BiDi, standard protocol developed and maintained
under the auspices of the World Wide Web Consortium (W3C). The protocol is implemented by browser vendors as part
of the Web Platform as a direct alternative to proprietary options like the Chrome DevTools Protocol. It allows
scenarios like:
* Capturing log messages written to the JavaScript console
* Receiving notifications when new browser windows or tabs are opened
* Receiving notifications when navigation events occur
* Adding JavaScript to each page before any other JavaScript is loaded, and have that so-called "preload"
script available to the page being automated.
* Other scenarios to be added as features become documented in the specification and implemented by browser
vendors

WebDriver BiDi uses JSON payloads across a websocket connection to communicate with the browser to execute
commands and receive responses and events. This library manages the communication across the websocket and
serializing and deserializing the JSON payloads. Consumers of this library should note that a general
principle about the .NET API contained herein that objects received from the remote end (browser) of the
connection are immutable; the data contained within cannot be modified. Objects being sent from the local
end to the remote end are intended to have settable properties to shape the proper values sent across the
connection.

It is important to note some of the things this library is explicitly _not_ intended for.
* This library is not itself a replacement for [Selenium](https://selenium.dev), [Puppeteer](https://pptr.dev)
or [Playwright](https://playwright.dev). It does not provide a user-friendly automation API. This is intentional,
as the library is a low-level implementation of a client for the protocol. However, any project like those
aforementioned could conceivably use this library as a mechanism for driving the browser using .NET.
* This library does not manage browser launching and/or profile information. It expects a browser to already
be launched, and for the WebDriver BiDi websocket to already be open and available for communication. Moreover,
it is the user's responsibility to know what the URL of the websocket connection is to initiate a session.

## Getting Started
The library is built to support .NET Standard 2.0. This should allow the widest usage of the library across
the largest number of framework versions, including .NET Framework, .NET Core, and .NET 5 and higher. For
convenience, the library also builds assemblies targeting the current and immediately previous Long Term
Support (LTS) versions of .NET, as well as the most recent Standard Term Support (STS) version of .NET.
At present, that includes .NET 8 (previous LTS), .NET 9 (current STS), and .NET 10 (current LTS).

Building the repository itself requires the .NET 10 SDK: the projects use C# 14
(`<LangVersion>14</LangVersion>`) and the test projects target `net10.0`. `global.json` does not pin
an SDK version, so any .NET 10 SDK will do. Consumers of the published package need only one of the
runtimes listed above; see the
[Getting Started guide](https://webdriverbidi-net.github.io/webdriverbidi-net/articles/getting-started.html)
for consumer prerequisites.

To build the library, after cloning the repository, execute the following in a terminal window
in the root of your clone:

    dotnet build

To run the project unit tests, execute the following in a terminal window:

    dotnet test

## Development
There are 18 projects in this repository:
* src/WebDriverBiDi/WebDriverBiDi.csproj - The main library source code.
* src/WebDriverBiDi.Analyzers/WebDriverBiDi.Analyzers.csproj - Source code for Roslyn analyzers
to help users avoid antipatterns when using the main library.
* src/WebDriverBiDi.Analyzers.CodeFixProviders/WebDriverBiDi.Analyzers.CodeFixProviders.csproj - Source code
for Roslyn code fixers to help modify users' code in response to analysis performed by the analyzers.
* src/WebDriverBiDi.Client/WebDriverBiDi.Client.csproj - A library containing helper methods to
demonstrate scaffolding required to make the main library useful. This code is not unit tested,
and should be viewed as a demonstration library only.
* src/WebDriverBiDi.Demo/WebDriverBiDi.Demo.csproj - A console application used as a "playground"
for practice using the library. Changes to this project are not canonical at this time, and this
project should not be viewed as having desirable coding practices.
* src/WebDriverBiDi.DemoWebSite/WebDriverBiDi.DemoWebSite.csproj - A project that instantiates
an in-memory web server hosting content against which to test. The default code in the WebDriverBidi.Demo
project will start this server and use it to demonstrate the use of the library against a site
running on localhost. This server can be used programmatically, or as a standalone console application,
but is designed as a demonstration and is explicitly recommended against production use.
* src/WebDriverBiDi.Logging/WebDriverBiDi.Logging.csproj - A library that provides support for
structured logging by providing integration with `Microsoft.Extensions.Logging.ILogger`.
* test/WebDriverBiDi.Analyzers.Tests/WebDriverBiDi.Analyzers.Tests.csproj - Tests for the Roslyn analyzers
and associated code fix providers.
* test/WebDriverBiDi.AotTestApplication/WebDriverBiDi.AotTestApplication.csproj - A console application
used to smoke test proper JSON serialization in ahead-of-time (AOT) compilation scenarios.
* test/WebDriverBiDi.Benchmarks/WebDriverBiDi.Benchmarks.csproj - Performance benchmarks for the library.
* test/WebDriverBiDi.Compatibility.Tests/WebDriverBiDi.Compatibility.Tests.csproj - Tests that verify
the main library works when consumed from a particular build configuration. Each test builds a
separate console application that pins its reference to the configuration under test, then runs that
application out of process against a scripted WebSocket server. No browser is involved, which is what
separates these from the integration tests.
* test/WebDriverBiDi.Integration.Tests/WebDriverBiDi.Integration.Tests.csproj - Integration tests for
the main library. These tests use actual browsers to test WebDriver BiDi functionality.
* test/WebDriverBiDi.Logging.Tests/WebDriverBiDi.Logging.Tests.csproj - Tests for the structured logging extension project.
* test/WebDriverBiDi.NamedPipeTestApplication/WebDriverBiDi.NamedPipeTestApplication.csproj - A console application
that acts as a test server for named pipe communication, used by unit tests to validate pipe-based connections.
* test/WebDriverBiDi.NetStandardTestApplication/WebDriverBiDi.NetStandardTestApplication.csproj - A console
application whose main library reference is pinned to its netstandard2.0 build, allowing proper
smoke testing of netstandard2.0-specific code paths in the main library. It is driven by the
compatibility tests.
* test/WebDriverBiDi.Tests/WebDriverBiDi.Tests.csproj - The unit tests for the main library.
* test/WebDriverBiDi.TestUtilities/WebDriverBiDi.TestUtilities.csproj - A small library of helpers
shared between test projects, rather than a test project itself. It currently holds the plumbing for
launching a console application as a child process and collecting its exit code and console output,
which the compatibility and integration tests both rely on.
* docs/code/WebDriverBiDi.DocSnippets.csproj - The compilable code samples included in the documentation
articles. Building this project verifies that every documented sample compiles against the current library.

[Visual Studio Code](https://code.visualstudio.com/) is the preferred IDE for development of this library.
It can be used across multiple operating systems, and there should be nothing platform-specific in the
library or its unit tests that would require platform-specific code. For working with C# code, we recommend
using the [C# Dev Kit](https://marketplace.visualstudio.com/items?itemName=ms-dotnettools.csdevkit) extension
from Microsoft. It includes a Roslyn based code server, and enables running tests from within the test pane
of VS Code.

The project uses System.Text.Json for JSON serialization/deserialization in the main library, but also
uses [Json.NET](https://www.newtonsoft.com/json) for some unit tests. It is believed that there is some
value in testing serialization by deserializing with a different JSON serialization engine.

The project uses [xUnit.net](https://xunit.net/) for its unit tests.

For testing of browsing web pages and WebSocket traffic, this project uses the
[PinchHitter](https://github.com/jimevans/PinchHitter) test server library.

The project has enabled Roslyn analyzers to help with code quality, and uses the
[StyleCop analyzers](https://www.nuget.org/packages/StyleCop.Analyzers) along with
a `.editorconfig` file to enforce a consistent code style.
PRs should contain no warnings from any of the analyzers. Use of warning suppression in the source code
is mostly prohibited, and will only be allowed on a very strictly reviewed case-by-case basis.

The project has some performance benchmarks, using [BenchmarkDotNet](https://benchmarkdotnet.org/)
to provide baseline tracking of performance metrics.

## Continuous Integration (CI)

The project uses [GitHub Actions](https://github.com/webdriverbidi-net/webdriverbidi-net/actions) for CI.
Code coverage statistics are generated and gathered by [Coverlet](https://www.nuget.org/packages/coverlet.MTP/)
(the `coverlet.MTP` package, which integrates with the Microsoft.Testing.Platform runner the test projects use),
and uploaded to [coveralls.io](https://coveralls.io/github/webdriverbidi-net/webdriverbidi-net?branch=main).
PRs for which the code coverage drops from the current percentage on the `main` branch will need to be carefully
reviewed. For convenience, a task has been configured to collect code coverage statistics when the
tests are executed, so to run code coverage locally, you can run the test task from the Command
Palette (<kbd>Ctrl</kbd>+<kbd>Shift</kbd>+<kbd>P</kbd> or <kbd>Cmd</kbd>+<kbd>Shift</kbd>+<kbd>P</kbd>, 
choose the `Tasks: Run Task` entry, and choose the `dotnet: test with coverage` task).

Coverage is measured in CI twice, in Release by the `unit-tests` job and in Debug by a separate
`unit-tests-debug` job, because the project's standard is 100% line, branch and method coverage in
**both** configurations. The two configurations do not instrument the same branches: a gap has been
observed that a Release run reported and a Debug run did not, so measuring only one of them can miss
a real regression in the other.

**The two jobs are deliberately separate, and should not be collapsed into a `configuration` matrix
on `unit-tests`.** They already run concurrently on the same `needs: build` fan-out, so the second
job costs runner minutes rather than elapsed time, and it finishes well inside the shadow of the
integration tests, which set the wall-clock critical path. A matrix would save about forty lines of
setup, but `unit-tests` also packs the analyzer package, verifies its shipped layout, and uploads
coverage to coveralls; none of those may run twice, so each would need an `if:` guard on the matrix
value. Those are precisely the steps that exist to catch a packaging mistake before a release, and a
guard that is wrong fails by silently not checking. The duplication is the cheaper error to make.

## Benchmarks
The library tracks performance across five suites covering command object
creation, JSON serialization, pending-command bookkeeping, observable-event
dispatch, and end-to-end command execution against an in-memory echo
connection. The benchmark suite lives in
[`test/WebDriverBiDi.Benchmarks`](test/WebDriverBiDi.Benchmarks), and a CI
workflow runs it on every PR that touches the main library or the
benchmarks themselves, posting a per-benchmark delta table as a PR comment
relative to a committed baseline.

The numbers below are a sample taken on 2026-09-08 on one Apple Silicon
development machine. They are **not representative**: absolute figures move with
hardware, OS, runtime version and machine load, and the CI runner
(`ubuntu-latest`, x64) is the reference hardware for the committed baseline. Use
them to see the shape of the costs, not to predict your own. See
[REPORTING.md](test/WebDriverBiDi.Benchmarks/REPORTING.md) for how to interpret
results and operate the baseline workflow.

**Runtime environment**
- BenchmarkDotNet 0.15.8
- macOS Tahoe 26.6.2 (Darwin 25.6.0)
- Apple M5 Max, 18 physical / 18 logical cores
- .NET SDK 10.0.400, .NET 10.0.11 runtime, Arm64 RyuJIT (armv8.0-a)

**CommandProcessingBenchmarks** — command object creation overhead

| Method                          | Mean       | Allocated |
|-------------------------------- |-----------:|----------:|
| CreateSimpleCommand             |    7.10 ns |     144 B |
| CreateComplexCommand            |   13.15 ns |     232 B |
| CreateNetworkInterceptCommand   |   38.02 ns |     480 B |
| CreateScriptEvaluateCommand     |    9.82 ns |     176 B |
| CreateScriptCallFunctionCommand |   36.11 ns |     424 B |

**SerializationBenchmarks** — JSON serialization/deserialization of protocol messages

| Method                         | Mean         | Allocated |
|------------------------------- |-------------:|----------:|
| SerializeCommandParameters     |    170.12 ns |     704 B |
| DeserializeCommandResult       |     96.04 ns |     320 B |
| DeserializeNetworkEvent        |  1,998.92 ns |   3,088 B |
| DeserializeSimpleRemoteValue   |    271.84 ns |     208 B |
| DeserializeComplexRemoteValue  |  1,949.32 ns |   1,024 B |

**PendingCommandCollectionBenchmarks** — transport pending-command bookkeeping

| Method                  | Mean      | Allocated |
|------------------------ |----------:|----------:|
| AddRemovePendingCommand |  47.25 ns |      48 B |

**EventDispatchBenchmarks** — `ObservableEvent<T>.NotifyObserversAsync` dispatch cost

| Method               | ObserverCount | Mean         | Allocated |
|--------------------- |-------------- |-------------:|----------:|
| NotifyObservers      | 1             |     19.44 ns |       0 B |
| NotifyAsyncObservers | 1             |    312.73 ns |     304 B |
| NotifyObservers      | 4             |     72.87 ns |       0 B |
| NotifyAsyncObservers | 4             |  1,217.97 ns |   1,216 B |
| NotifyObservers      | 16            |    241.06 ns |       0 B |
| NotifyAsyncObservers | 16            |  4,952.73 ns |   4,864 B |

**CommandExecutionBenchmarks** — end-to-end `BiDiDriver.ExecuteCommandAsync` round trip via echo connection

| Method                                       | Mean        | Allocated |
|--------------------------------------------- |------------:|----------:|
| ExecuteCommandRoundTrip                      | 4,077.99 ns |   2,984 B |
| ExecuteCommandRoundTripWithCancellationToken | 4,314.89 ns |   3,064 B |

The second method makes the same call with a `CancellationToken`, which is the
shape [BIDI004 and BIDI013](docs/articles/advanced/analyzers.md) ask callers to
write. The difference between the two is what passing a token costs on a round
trip: the connection builds a linked `CancellationTokenSource` per send only
when a token is supplied, and the one the pending command builds registers a
callback on the caller's source rather than none.

To run the suite yourself:

    dotnet run --project test/WebDriverBiDi.Benchmarks -c Release

## Documentation
The project includes documentation, in the `docs` directory. The documentation is published
to the [GitHub Pages site for this project](https://webdriverbidi-net.github.io/webdriverbidi-net).
Documentation is built and maintained with [DocFx](https://dotnet.github.io/docfx/), the .NET
documentation framework. To build the documentation, you will need to install the DocFx tooling
using the following command:

    dotnet tool install -g docfx

To update the DocFx tooling, you can use the following command:

    dotnet tool update -g docfx

To build the documentation, use the following commands. The first two steps are required because
`docfx metadata` reads the API surface from the Release `netstandard2.0` builds of the main library
and of the `WebDriverBiDi.Logging` package (see `docs/docfx.json`), which the snippets project does
not produce by itself (it builds only the `net10.0` flavour of each):

    dotnet build src/WebDriverBiDi/WebDriverBiDi.csproj --configuration Release
    dotnet build src/WebDriverBiDi.Logging/WebDriverBiDi.Logging.csproj --configuration Release
    dotnet build docs/code/WebDriverBiDi.DocSnippets.csproj
    docfx metadata docs/docfx.json
    docfx build docs/docfx.json

To preview a local version of the documentation prior to publishing, you can do so with the
following command:

    docfx serve docs/_site

This will serve a local copy of the documentation at http://localhost:8080.

## Prompts
The `prompts` directory contains prompts that one can use to prompt a large language
model (LLM), colloquially known as "AI," to aid in development of this library.
This directory is _not_ intended as a place for prompts to use while _using_ the
library at this time.
