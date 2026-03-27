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
> `IBiDiDriverConfiguration`, which only exposes `RegisterModule` and `RegisterTypeInfoResolver`.
> Passing a `BiDiDriver` instance satisfies both interfaces, so your module constructor always
> receives a `BiDiDriver` in practice.

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

  <ItemGroup>
    <PackageReference Include="WebDriverBiDi" Version="*" />
  </ItemGroup>
</Project>
```

> **AOT support:** If your package will be used in AOT environments, include a source-generated `JsonSerializerContext` with `[JsonSerializable]` attributes for your custom types. See [AOT Compatibility](aot-compatibility.md) for details.

## Advanced: Implementing Protocol Extensions

For actual protocol extensions (not just helper methods):

[!code-csharp[Experimental Module](../../code/advanced/CustomModulesSamples.cs#ExperimentalModule)]

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

