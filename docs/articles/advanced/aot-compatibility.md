# AOT (Ahead-of-Time) Compatibility

This guide explains how to use WebDriverBiDi.NET in Native AOT compilation scenarios, including how to ensure custom modules work correctly without reflection-based serialization.

## Overview

.NET Native AOT (Ahead-of-Time) compilation produces standalone executables that don't rely on a just-in-time (JIT) compiler at runtime. This improves startup time and reduces memory usage, but it also means that reflection-based features — including `System.Text.Json`'s default serialization — may not work correctly.

WebDriverBiDi.NET supports AOT out of the box for all built-in modules. If you're using custom modules, you'll need to take one additional step to register serialization metadata for your custom types.

## How AOT Serialization Works

`System.Text.Json` supports AOT through **source-generated serializer contexts**. Instead of using reflection at runtime to inspect types, a source generator produces serialization code at compile time.

WebDriverBiDi.NET ships with a pre-built context, `WebDriverBiDiJsonSerializerContext`, that covers every built-in command, result, and event type. When reflection is unavailable (i.e., in AOT mode), the library's `Transport` automatically uses this context.

## Using Built-in Modules in AOT

If you're only using the built-in modules, no additional configuration is needed. The library handles everything automatically:

[!code-csharp[Built-in Modules in AOT](../../code/advanced/AotCompatibilitySamples.cs#Built-inModulesinAOT)]

## Custom Modules in AOT

When you create a [custom module](custom-modules.md) with your own command parameters, results, or event args, the library's source-generated context doesn't know about those types. In a JIT environment this isn't a problem — `System.Text.Json` falls back to reflection. In AOT, however, serialization of your custom types will fail unless you provide the metadata yourself.

The solution has two parts:

1. Create a source-generated `JsonSerializerContext` for your custom types
2. Register it with the driver before connecting

### Step 1: Define Your Custom Types

Suppose you have a custom module with its own command and event types:

[!code-csharp[Custom Types for AOT](../../code/core-concepts/CoreConceptsCustomModuleSamples.cs#CustomTypesforAOT)]

### Step 2: Create a Source-Generated Serializer Context

Add `[JsonSerializable]` attributes for the **protocol wrapper types** that the transport actually serializes and deserializes — not just your raw types. The transport serializes `CommandParameters` subclasses directly, but it deserializes responses as `CommandResponseMessage<T>` and events as `EventMessage<T>`:

[!code-csharp[Source-Generated Context](../../code/core-concepts/CoreConceptsCustomModuleSamples.cs#Source-GeneratedContext)]

> **Important:** You must include `CommandResponseMessage<T>` for each command result type, and `EventMessage<T>` for each event args type. These are the closed generic types that the transport deserializes at runtime.

### Step 3: Register the Context with the Driver

Call `RegisterTypeInfoResolver` **before** starting the driver:

[!code-csharp[Register and Connect](../../code/advanced/AotCompatibilitySamples.cs#RegisterandConnect)]

The library combines your resolver with its own using `JsonTypeInfoResolver.Combine()`, so both built-in and custom types are handled seamlessly.

## Multiple Custom Modules

If you have several custom modules, you can either include all types in a single `JsonSerializerContext`, or register multiple contexts separately:

[!code-csharp[Multiple Contexts](../../code/advanced/AotCompatibilitySamples.cs#MultipleCustomModulesOptionA)]

[!code-csharp[Multiple Contexts](../../code/advanced/AotCompatibilitySamples.cs#MultipleCustomModulesOptionARegistration)]

[!code-csharp[Multiple Contexts](../../code/advanced/AotCompatibilitySamples.cs#MultipleCustomModulesOptionBRegistration)]

Both approaches work. Option A produces a single source-generated context, which is slightly more efficient. Option B is better for independently packaged modules.

## Packaging AOT-Compatible Modules

When distributing a custom module as a NuGet package, include the source-generated context so consumers don't have to create their own. See [Core Concepts - Custom JSON Type Resolvers](../core-concepts.md#custom-json-type-resolvers-aot-scenarios) for the context definition pattern.

Document that consumers should register it:

[!code-csharp[Consumer Registers Extension](../../code/advanced/AotCompatibilitySamples.cs#ConsumerRegistersExtension)]

## Troubleshooting

### Serialization fails at runtime in AOT

If you see errors like `JsonSerializerOptions instance is locked` or types not being serialized correctly:

- Ensure `RegisterTypeInfoResolver` is called **before** `StartAsync`. The transport's serializer options are frozen on first use.
- Verify that you've included the `CommandResponseMessage<T>` and `EventMessage<T>` wrapper types in your context, not just the inner types.

### Types work in development but fail in AOT

This typically means reflection-based serialization was handling your types in development (JIT mode), masking the fact that they aren't in any source-generated context. Add `[JsonSerializable]` attributes for all custom types and register the context.

### Enums not serializing correctly in AOT

If your custom types use enums with a custom `JsonConverter` (such as `EnumValueJsonConverter<T>`), ensure the enum array type is rooted for AOT. Add a static constructor to your context:

[!code-csharp[AOT Enum Rooting](../../code/advanced/AotCompatibilitySamples.cs#AOTEnumRooting)]

> This pattern requires `using System.Runtime.CompilerServices;` for `RuntimeHelpers`.

## Best Practices

1. **Always create a `JsonSerializerContext` for custom modules** — even if you don't target AOT today, this future-proofs your code and avoids reflection overhead.
2. **Register resolvers before starting** — `RegisterTypeInfoResolver` must be called before `StartAsync`. Attempting to register after connecting throws an exception.
3. **Include wrapper types** — remember to include `CommandResponseMessage<T>` and `EventMessage<T>`, not just your raw parameter/result/event types.
4. **Test in AOT mode** — publish your application with `dotnet publish -p:PublishAot=true` and verify end-to-end behavior.
5. **Ship contexts with packages** — if distributing modules as NuGet packages, include the serializer context so consumers can register it.

## Next Steps

- [Custom Modules](custom-modules.md): Learn how to create custom modules
- [Architecture](../architecture.md): Understand the module and transport system
- [Error Handling](error-handling.md): Handle failures in custom modules
- [Performance Considerations](performance.md): Optimize command execution
