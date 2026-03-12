# WebDriverBiDi.Logging Package

Microsoft.Extensions.Logging integration for the WebDriver BiDi .NET client library.

## Overview

This package provides `ILogger` support for WebDriver BiDi diagnostic events, enabling you to capture WebDriver BiDi EventSource events through the standard .NET logging infrastructure.

## Installation

```bash
dotnet add package WebDriverBiDi.Logging
```

## Quick Start

```csharp
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.DependencyInjection;
using WebDriverBiDi;

var services = new ServiceCollection();
services.AddLogging(builder =>
{
    builder.AddConsole();
    builder.AddWebDriverBiDi();
});

var serviceProvider = services.BuildServiceProvider();

await using var driver = new BiDiDriver();
await driver.StartAsync("ws://localhost:9222");
```

For comprehensive examples including Serilog, Application Insights, custom filtering, and performance monitoring, see [Observability and Diagnostics](observability.md).

## See Also

- [WebDriverBiDi Package](https://www.nuget.org/packages/WebDriverBiDi)
- [WebDriverBiDi.Logging Package](https://www.nuget.org/packages/WebDriverBiDi.Logging)
- [Observability and Diagnostics](observability.md)
