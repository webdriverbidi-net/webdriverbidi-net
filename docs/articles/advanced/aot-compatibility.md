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

Add a `[JsonSerializable]` attribute for each of **your own types**: every `CommandParameters` subclass, every `CommandResult` subclass, and every event args type:

[!code-csharp[Source-Generated Context](../../code/core-concepts/CoreConceptsCustomModuleSamples.cs#Source-GeneratedContext)]

> **Note:** Do not register the library's envelope types (`CommandResponseMessage<T>`, `EventMessage<T>`). Their members are internal to the library, so a context in your assembly cannot generate working metadata for them; the transport reads the envelopes itself and asks the serializer only for your result and event args types. The [BIDI034](analyzers.md#bidi034) analyzer reports an envelope type named in a `[JsonSerializable]` attribute.

### Step 3: Register the Context with the Driver

Call `RegisterTypeInfoResolverAsync` **before** starting the driver:

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

If types are not being serialized correctly:

- Ensure `RegisterTypeInfoResolverAsync` is called **before** `StartAsync`. Registering after the transport
  has connected throws `InvalidOperationException` with the message "Cannot register a type info resolver
  after the transport is connected". The transport rebuilds its serializer options around each new resolver,
  so this is a lifecycle restriction rather than a frozen-options one.
- Verify that every custom `CommandParameters`, `CommandResult` and event args type is listed in your context; the library's envelope types (`CommandResponseMessage<T>`, `EventMessage<T>`) must **not** be.

### Types work in development but fail in AOT

This typically means reflection-based serialization was handling your types in development (JIT mode), masking the fact that they aren't in any source-generated context. Add `[JsonSerializable]` attributes for all custom types and register the context.

### Enums with a custom converter

Enums that use `EnumValueJsonConverter<T>` need nothing extra under AOT. The converter reads the
enum's members through `Enum.GetValues<T>()`, whose specialization roots the `T[]` array type, so the
compiler generates it without being asked. Earlier versions of this guide recommended rooting that
array type from a static constructor on your context; that is no longer necessary, and the library no
longer does it either.

### Diagnostic events and logging produce nothing under AOT

The ILCompiler sets `EventSourceSupport` to `false` by default, so in a Native AOT application
`EventSource.IsEnabled()` is permanently false. The library's `WebDriverBiDiEventSource` emits nothing, no
`EventListener` is ever called, and the `WebDriverBiDi.Logging` bridge forwards no entries. Nothing throws
and nothing is written to say why, so it looks simply like an absence of events. Opt back in:

```xml
<PropertyGroup>
  <PublishAot>true</PublishAot>
  <EventSourceSupport>true</EventSourceSupport>
</PropertyGroup>
```

See [Observability](observability.md) for the events this restores.

### Values in `AdditionalData` are not serialized through your context

`AdditionalData`, `CapabilityRequest.AdditionalCapabilities` and `Command.AdditionalCommandProperties` hold
`object?` values, so the serializer must look their runtime types up when the command is sent, rather than knowing
them from a generated contract. Under Native AOT, a type nothing has registered fails the send with
`WebDriverBiDiSerializationException` wrapping a `NotSupportedException` naming the type. The
[BIDI022](analyzers.md#available-analyzers) analyzer flags every write to these dictionaries as a reminder.

The library's own context already registers the types these dictionaries usually hold: `string`, `bool`, `int`,
`long`, `uint`, `ulong`, `double`, `decimal`, `DateTime`, `object`, `List<object?>` and `Dictionary<string, object?>`.
Values of those types need nothing from you. A value of any other type — your own record, an enum, an array of a
custom type — needs a context that includes it, registered exactly as a custom module's types are:

[!code-csharp[AdditionalData Under AOT](../../code/advanced/AotCompatibilitySamples.cs#AdditionalDataUnderAot)]

Register it before `StartAsync`, exactly as a custom module's context is registered (see
[Step 3](#step-3-register-the-context-with-the-driver)). Registering the value's own type is enough; the transport
serializes the command envelope itself.

## Best Practices

1. **Always create a `JsonSerializerContext` for custom modules** — even if you don't target AOT today, this future-proofs your code and avoids reflection overhead.
2. **Register resolvers before starting** — `RegisterTypeInfoResolverAsync` must be called before `StartAsync`. Attempting to register after connecting throws an exception.
3. **Register your payload types only** — your parameters, results and event args. The transport handles the response and event envelopes itself.
4. **Test in AOT mode** — publish your application with `dotnet publish -p:PublishAot=true` and verify end-to-end behavior.
5. **Ship contexts with packages** — if distributing modules as NuGet packages, include the serializer context so consumers can register it.

## Next Steps

- [Custom Modules](custom-modules.md): Learn how to create custom modules
- [Architecture](../architecture.md): Understand the module and transport system
- [Error Handling](error-handling.md): Handle failures in custom modules
- [Performance Considerations](performance.md): Optimize command execution
