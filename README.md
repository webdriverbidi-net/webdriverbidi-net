# webdriverbidi-net-relaxed
A .NET client library for the WebDriver BiDi protocol

![Unit tests](https://github.com/hardkoded/webdriverbidi-net-relaxed/actions/workflows/dotnet.yml/badge.svg)
[![Coverage Status](https://coveralls.io/repos/github/hardkoded/webdriverbidi-net-relaxed/badge.svg?branch=main&kill_cache=1)](https://coveralls.io/github/hardkoded/webdriverbidi-net-relaxed?branch=main)
[![NuGet Version](https://img.shields.io/nuget/v/WebDriverBiDi-Relaxed)](https://www.nuget.org/packages/WebDriverBiDi-Relaxed)

# I DON'T NOW IF I WANT YOU TO USE THIS

This is a fork of the great [webdriverbidi-net](https://github.com/webdriverbidi-net/webdriverbidi-net) library

**WHY DID YOU CREATED THIS PROJECT?**

The webdriver spec can be quite bureaucratic, which is GOOD! But sometimes, puppeteer-sharp development can be delayed why browser and spec folks agreed on protocol stuff.
So this project is a TEMPORAL swich for puppeteer-sharp, to keep launching features, while we wait for some agreements.

**What's the difference between this project and the official one?**

Well, I will basically make changes to puppeteer-sharp can work on any browser regardless they comply with the spec or not.

This is repository contains a library that is a .NET client for the
[WebDriver BiDi protocol specification](https://w3c.github.io/webdriver-bidi/). This spec is in progress,
and features are added to the library as the specification changes. This package is also 
[published to NuGet](https://www.nuget.org/packages/WebDriverBiDi-Relaxed).

This library also includes support for other modules implementing support for the WebDriver BiDi protocol,
but not included in that specification. The other specifications which have WebDriver BiDi support that are
also included in this library are:
* [Permissions](https://www.w3.org/TR/permissions/)
* [Web Bluetooth](https://webbluetoothcg.github.io/web-bluetooth/)
* [Prefetch](https://wicg.github.io/nav-speculation/prefetch.html)

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
The library is built to support .NETStandard 2.0. This should allow the widest usage of the library across
the largest number of framework versions, including .NET Framework, .NET Core, and .NET 5 and higher.

To build the library, after cloning the repository, execute the following in a terminal window
in the root of your clone:

    dotnet build

To run the project unit tests, execute the following in a terminal window:

    dotnet test

## Development
There are five projects in this repository:
* src/WebDriverBiDi/WebDriverBiDi.csproj - The main library source code
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
* test/WebDriverBiDi.Tests/WebDriverBiDi.Tests.csproj - The unit tests for the main library

[Visual Studio Code](https://code.visualstudio.com/) is the preferred IDE for development of this library.
It can be used across multiple operating systems, and there should be nothing platform-specific in the
library or its unit tests that would require platform-specific code. For working with C# code, we recommend
using the [C# Dev Kit](https://marketplace.visualstudio.com/items?itemName=ms-dotnettools.csdevkit) extension
from Microsoft. It includes a Roslyn based code server, and enables running tests from within the test pane
of VS Code.

The project uses System.Text.Json for JSON serialization/deserialization in the main library, but also
uses [Json.NET](https://www.newtonsoft.com/json) for some unit tests. It is believed that there is some
value in testing serialization by deserializing with a different JSON serialization engine.

The project uses [NUnit](https://nunit.org/) for its unit tests.

For testing of browsing web pages and WebSocket traffic, this project uses the
[PinchHitter](https://github.com/jimevans/PinchHitter) test server library.

The project has enabled Roslyn analyzers to help with code quality, and uses the
[StyleCop analyzers](https://www.nuget.org/packages/StyleCop.Analyzers) to enforce a consistent code style.
PRs should contain no warnings from any of the analyzers. Use of warning suppression in the source code
is mostly prohibited, and will only be allowed on a very strictly reviewed case-by-case basis.

The project uses [GitHub Actions](https://github.com/hardkoded/webdriverbidi-net-relaxed/actions) for continuous
integration (CI). Code coverage statistics are generated and gathered by
[Coverlet](https://www.nuget.org/packages/coverlet.collector/), and uploaded to
[coveralls.io](https://coveralls.io/github/hardkoded/webdriverbidi-net-relaxed?branch=main). PRs for which
the code coverage drops from the current percentage on the `main` branch will need to be carefully
reviewed. For convenience, a task has been configured to collect code coverage statistics when the
tests are executed, so to run code coverage locally, you can run the test task from the Command
Palette (<kbd>Ctrl</kbd>+<kbd>Shift</kbd>+<kbd>P</kbd> or <kbd>Cmd</kbd>+<kbd>Shift</kbd>+<kbd>P</kbd>, 
choose the `Tasks: Run Task` entry, and choose the `dotnet: test with coverage` task).

Some useful plugins in your Visual Studio Code environment for this project are:
* [Coverage Gutters](https://marketplace.visualstudio.com/items?itemName=ryanluker.vscode-coverage-gutters):
This plugin allows visualization of code coverage directly within the IDE.

## Documentation
The project includes documentation, in the `docs` directory. The documentation is published
to the [GitHub Pages site for this project](https://hardkoded.github.io/webdriverbidi-net-relaxed).
Documentation is built and maintained with [DocFx](https://dotnet.github.io/docfx/), the .NET
documentation framework. To build the documentation, you will need to install the DocFx tooling
using the following command:

    dotnet tool install -g docfx

To update the DocFx tooling, you can use the following command:

    dotnet tool update -g docfx

To build the documentation, use the following commands:

    docfx metadata docs/docfx.json
    docfx build docs/docfx.json

To preview a local version of the documentation prior to publishing, you can do so with the
following command:

    docfx serve docs/_site

This will serve a local copy of the documentation at http://localhost:8080.
