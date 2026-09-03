# Custom Modules

This guide explains how to create custom modules to extend WebDriverBiDi.NET with your own commands and functionality.

> **Advanced guide:** This article is for framework authors and library extenders. If you are building a typical automation application, use `BiDiDriver` and the built-in modules directly instead of creating custom modules.

## Overview

WebDriverBiDi.NET's module system is extensible, allowing you to:
- Implement custom WebDriver BiDi commands
- Create higher-level abstractions over protocol commands
- Integrate experimental or browser-specific features
- Build reusable automation patterns

## Module Basics

### Module Structure

All modules inherit from the `Module` base class:

[!code-csharp[Module Structure](../../code/advanced/CustomModulesSamples.cs#ModuleStructure)]

### Registering a Module

[!code-csharp[Register and Use Module](../../code/advanced/CustomModulesSamples.cs#RegisterandUseModule)]

After registration, retrieve a module by name using `GetModule<T>`:

```csharp
MyCustomModule myModule = driver.GetModule<MyCustomModule>("myCustom");
```

This is useful for reaching a module by name when you hold a reference to the driver but not to the module instance you registered. `GetModule<T>` throws `InvalidCastException` if the registered module cannot be cast to `T`, and `ArgumentException` if no module with that name has been registered.

> [!NOTE]
> `GetModule<T>` is declared on `BiDiDriver`, not on `IBiDiCommandExecutor`. The `Module` base class stores the executor as `IBiDiCommandExecutor` (see the note under [Module Events](#module-events)), so calling `GetModule<T>` from inside a module requires a cast to `BiDiDriver`. Prefer passing any module a custom module depends on into its constructor instead.

## Creating Commands

### Command Parameters

Define parameters that extend `CommandParameters`:

[!code-csharp[Command Parameters](../../code/advanced/CustomModulesSamples.cs#CommandParameters)]

### Command Results

Define results that extend `CommandResult`:

[!code-csharp[Command Result](../../code/advanced/CustomModulesSamples.cs#CommandResult)]

### Command Method

Implement the command in your module:

[!code-csharp[Command Method](../../code/advanced/CustomModulesSamples.cs#CommandMethod)]

## Example: Page Utilities Module

Let's create a complete custom module for common page operations:

[!code-csharp[Page Utilities Module](../../code/advanced/CustomModulesSamples.cs#PageUtilitiesModule)]

### Using the Custom Module

[!code-csharp[Using Page Utilities Module](../../code/advanced/CustomModulesSamples.cs#UsingPageUtilitiesModule)]

## Example: Testing Utilities Module

Create a module with common testing helpers:

[!code-csharp[Test Utilities Module](../../code/advanced/CustomModulesSamples.cs#TestUtilitiesModule)]

## Example: Performance Monitoring Module

The Performance module pattern uses `Script.EvaluateAsync` to run `performance.getEntriesByType('navigation')` and `performance.getEntriesByType('resource')` in the browser. See the [Test Utilities Module](#example-testing-utilities-module) example for the pattern of wrapping `ExecuteCommandAsync` with custom methods.

## Module Events

You can also expose observable events from your custom module:

[!code-csharp[Custom Events Module](../../code/advanced/CustomModulesSamples.cs#CustomEventsModule)]

> **Why `IBiDiCommandExecutor`, not `IBiDiDriverConfiguration`?**
> The `Module` base class constructor requires `IBiDiCommandExecutor` because event registration
> goes through that interface. When your module calls `this.RegisterObservableEvent<T>(...)` in its
> constructor, the base class calls `this.Driver.RegisterEvent<T>(...)` internally.
> `RegisterEvent<T>` is defined on `IBiDiCommandExecutor`; it is not present on
> `IBiDiDriverConfiguration`, which only exposes `RegisterModule` and `RegisterTypeInfoResolverAsync`.
> Passing a `BiDiDriver` instance satisfies both interfaces, so your module constructor always
> receives a `BiDiDriver` in practice.

## Enum Wire Values

Enums used in command parameters, results, and event args serialize as JSON strings through `EnumValueJsonConverter<T>` (applied with a `JsonConverter` attribute on the enum). By default the wire value is the member name lowercased (`Enabled` becomes `"enabled"`), and deserialization is strict: an incoming string that matches no member throws a `JsonException` rather than mapping silently. Three attributes adjust this for a custom module's enums:

- `StringEnumValueAttribute` (on a member) sets the exact wire string when lowercasing the name is not enough, such as hyphenated protocol values.
- `StringEnumUnmatchedValueAttribute<T>` (on the enum) names the member to which any unmatched incoming string deserializes, opting that enum out of strict deserialization.
- `StringEnumNullSentinelValueAttribute<T>` (on the enum) names a member that serializes as JSON `null`, for protocol members where sending null means "reset to default".

[!code-csharp[Custom Enum Wire Values](../../code/advanced/CustomModulesSamples.cs#CustomEnumWireValues)]

## Best Practices

### 1. Namespace Your Commands

Use a clear module prefix for your custom commands:

[!code-csharp[Namespace Command](../../code/advanced/CustomModulesSamples.cs#NamespaceCommand)]

### 2. Provide Defaults

Make your modules easy to use with sensible defaults:

[!code-csharp[Optional Timeout Default](../../code/advanced/CustomModulesSamples.cs#OptionalTimeoutDefault)]

### 3. Document Your Module

Add XML documentation comments to your module class and methods. See the [Page Utilities Module](#example-page-utilities-module) for the structure.

### 4. Handle Errors Gracefully

[!code-csharp[Error Handling in GetElementText](../../code/advanced/CustomModulesSamples.cs#ErrorHandlinginGetElementText)]

### 5. Make Modules Testable

Extract an interface from your module (e.g., `IPageUtilities` with `WaitForElementAsync` and `GetElementTextAsync`), implement it in your module class, and create a `MockPageUtilities` for unit tests.

## Packaging Custom Modules

Create a NuGet package for reusable modules:

```xml
<Project Sdk="Microsoft.NET.Sdk">
  <PropertyGroup>
    <TargetFramework>netstandard2.0</TargetFramework>
    <PackageId>MyCompany.WebDriverBiDi.Extensions</PackageId>
    <Version>1.0.0</Version>
    <Authors>Your Name</Authors>
    <Description>Custom WebDriver BiDi modules</Description>
  </PropertyGroup>
</Project>
```

Then add the WebDriverBiDi dependency with the .NET CLI, pinning an exact version so an update stays deliberate (see [Pinning an exact version](../getting-started.md#pinning-an-exact-version)):

```bash
dotnet add package WebDriverBiDi
```

> **AOT support:** If your package will be used in AOT environments, include a source-generated `JsonSerializerContext` with `[JsonSerializable]` attributes for your custom types. See [AOT Compatibility](aot-compatibility.md) for details.

## Advanced: Implementing Protocol Extensions

For actual protocol extensions (not just helper methods):

[!code-csharp[Experimental Module](../../code/advanced/CustomModulesSamples.cs#ExperimentalModule)]

## Custom Transport

For scenarios requiring custom message processing — for example, injecting test doubles, logging
raw frames, or applying transformations to incoming messages — you can subclass `Transport` and
override `CreateIncomingMessage`:

```csharp
using WebDriverBiDi.Protocol;
using System.Buffers;

public class LoggingTransport : Transport
{
    public LoggingTransport(Connection connection) : base(connection) { }

    protected override IncomingMessage CreateIncomingMessage(IMemoryOwner<byte> owner, int length)
    {
        // Inspect or log the raw message bytes here before handing them to the base implementation.
        return base.CreateIncomingMessage(owner, length);
    }
}
```

Pass your custom transport to `BiDiDriver` via the constructor overload that accepts a `Transport`:

```csharp
WebSocketConnection connection = new();
LoggingTransport transport = new(connection);
await using BiDiDriver driver = new(TimeSpan.FromSeconds(60), transport);
await driver.StartAsync("ws://localhost:9515/session/YOUR-SESSION-ID");
```

## Next Steps

- [AOT Compatibility](aot-compatibility.md): Make custom modules work in AOT environments
- [Architecture](../architecture.md): Understand the module system
- [Core Concepts](../core-concepts.md): Learn about commands and events
- [Error Handling](error-handling.md): Implement robust error handling
- [Examples](../examples/common-scenarios.md): See modules in action

## Summary

Custom modules allow you to:
- Extend WebDriverBiDi.NET with reusable functionality
- Create domain-specific abstractions
- Implement experimental features
- Build shareable automation libraries

The module system is flexible and powerful, enabling you to build exactly the automation framework you need.

