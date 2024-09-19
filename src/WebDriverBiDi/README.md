# WebDriverBiDi
A .NET client library for the WebDriver BiDi protocol

This package contains a library that is a .NET client for the
[WebDriver BiDi protocol specification](https://w3c.github.io/webdriver-bidi/). It also includes support for
other modules implementing support for the WebDriver BiDi protocol, but not included in that specification,
such as the [Permissions specification](https://www.w3.org/TR/permissions/). This spec is in progress,
and features are added to the library as the specification changes.

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

To use the library, you can create and start a WebDriver BiDi session using code similar to the following:

```csharp
using WebDriverBiDi;
 
// Assumes the browser is running with a WebSocket listening for
// WebDriver BiDi traffic. Note that your URL will be different here.
string webSocketUrl = "ws://localhost:5555";
 
// Set a timeout of 10 seconds for command responses.
BiDiDriver driver = new(TimeSpan.FromSeconds(10));
await driver.StartAsync(webSocketUrl);
```
