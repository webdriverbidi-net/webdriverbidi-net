# webdriverbidi-net
A .NET client library for the WebDriver Bidi protocol

![Unit tests](https://github.com/jimevans/webdriverbidi-net/actions/workflows/dotnet.yml/badge.svg)
[![Coverage Status](https://coveralls.io/repos/github/jimevans/webdriverbidi-net/badge.svg?branch=main&kill_cache=1)](https://coveralls.io/github/jimevans/webdriverbidi-net?branch=main)

This is repository contains a library that is a .NET client for the
[WebDriver Bidi protocol specification](https://w3c.github.io/webdriver-bidi/). This spec is in progress,
and features are added to the library as the specification changes. It is not yet published as a consumable
artifact to NuGet, but will be once the specification becomes more stable.

The library allows a user to automate a browser using WebDriver Bidi, standard protocol developed and maintained
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

WebDriver Bidi uses JSON payloads across a websocket connection to communicate with the browser to execute
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
aforementioned could conceiveably use this library as a mechanism for driving the browser using .NET.
* This library does not manage browser launching and/or profile information. It expects a browser to already
be launched, and for the WebDriver Bidi websocket to already be open and available for communication. Moreover,
it is the user's responsibility to know what the URL of the websocket connection is to initiate a session.

## Getting Started
The library is built using .NET 6. There are no plans at present to support earlier versions of .NET.
Future versions of .NET will be supported when [a new LTS version](https://dotnet.microsoft.com/en-us/platform/support/policy/dotnet-core) is released. The next LTS version of .NET is scheduled to be .NET 8, released in November, 2023.

To build the library, after cloning the repository, execute the following in a terminal window
in the root of your clone:

    dotnet build

To run the project unit tests, execute the following in a terminal window:

    dotnet test

## Development
There are four projects in this repository:
* src/WebDriverBidi/WebDriverBidi.csproj - The main library source code
* src/WebDriverBidi.Client/WebDriverBidi.Client.csproj - A console application used as a "playground"
for practice using the library. Changes to this project are not canonical at this time, and this
project should not be viewed as having desirable coding practices.
* test/WebDriverBidi.Tests/WebDriverBidi.Tests.csproj - The unit tests for the main library
* test/PinchHitter/PinchHitter.csproj - A class library implementing an in-memory simple web server
capable of serving HTTP content as well as acting as a server for WebSocket traffic. The project
uses a`System.Net.Sockets.TcpListener` to avoid the necessity of registering URL prefixes on Windows,
which using `System.Net.Sockets.HttpListener` requires, and which needs administrative access. This
code is separated into a separate project, so as to facilitate the possibility of its extraction to
a separate repository in the future.

[Visual Studio Code](https://code.visualstudio.com/) is the preferred IDE for development of this library.
It can be used across multiple operating systems, and there should be nothing platform-specific in the
library or its unit tests that would require platform-specific code. For working with C# code, we recommend
using the [C# for Visual Studio Code plugin](https://marketplace.visualstudio.com/items?itemName=ms-dotnettools.csharp).

The project currently uses [Json.NET](https://www.newtonsoft.com/json) for JSON serialization/deserialization.

The project uses [NUnit](https://nunit.org/) for its unit tests.

The project has enabled Roslyn analyzers to help with code quality, and uses the
[StyleCop analyzers](https://www.nuget.org/packages/StyleCop.Analyzers) to enforce a consistent code style.
PRs should contain no warnings from any of the analyzers. Use of warning suppression in the source code
is mostly prohibited, and will only be allowed on a very strictly reviewed case-by-case basis.

The project uses [GitHub Actions](https://github.com/jimevans/webdriverbidi-net/actions) for continuous
integration (CI). Code coverage statistics are generated and gathered by
[Coverlet](https://www.nuget.org/packages/coverlet.collector/), and uploaded to
[coveralls.io](https://coveralls.io/github/jimevans/webdriverbidi-net?branch=main). PRs for which
the code coverage drops from the current percentage on the `main` branch will need to be carefully
reviewed.

Some useful plugins in your Visual Studio Code environment for this project are:
* [.NET Core Test Explorer](https://marketplace.visualstudio.com/items?itemName=formulahendry.dotnet-test-explorer):
This plugin allows one to execute any or all of the unit tests from within the IDE. By changing the settings
of the plugin to add `/p:CollectCoverage=true /p:CoverletOutputFormat=lcov /p:CoverletOutput=../coverage/lcov` to
the test arguments, code coverage data can be collected locally when the tests are executed using the explorer.
* [Coverage Gutters](https://marketplace.visualstudio.com/items?itemName=ryanluker.vscode-coverage-gutters):
This plugin allows visualization of code coverage directly within the IDE.