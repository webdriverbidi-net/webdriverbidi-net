# Working with Remote Values

Remote values represent JavaScript data returned from the browser. This guide explains how to work with them effectively.

## Overview

When you execute JavaScript in the browser, the results are returned as `RemoteValue` objects. These represent JavaScript values in a .NET-friendly way while preserving type information and references to browser objects.

## RemoteValue Class Hierarchy

Every `RemoteValue` has a `Type` property indicating the JavaScript type as a string. The library deserializes each value into a concrete subclass rather than a single generic type:

- **`RemoteValue`** (abstract base) – all remote values expose `Type`, `As<T>()`, `TryConvertTo<T>()`, and `ToLocalValue()`
- **`ValueHoldingRemoteValue<T>`** – subclass for values that carry a .NET payload; exposes a typed `Value` property
- **`ObjectReferenceRemoteValue`** – subclass for JavaScript objects that can be referenced by handle; exposes `Handle` and `InternalId`, and `ToRemoteReference()` to build a reference
- **`NodeRemoteValue`** – extends `ValueHoldingRemoteValue<NodeProperties>` and also implements `IObjectReferenceRemoteValue`, providing `SharedId` and `ToSharedReference()`

`NullRemoteValue` and `UndefinedRemoteValue` have no `Value` property — only a `Type`.

## JavaScript Type Mapping

### Primitive Types

| JavaScript Type | `Type` string | Concrete Class | `Value` Property Type |
|----------------|---------------|----------------|-----------------------|
| `string` | `"string"` | `StringRemoteValue` | `string` |
| `number` | `"number"` | `NumberRemoteValue` | `double` |
| `boolean` | `"boolean"` | `BooleanRemoteValue` | `bool` |
| `bigint` | `"bigint"` | `BigIntegerRemoteValue` | `BigInteger` |
| `undefined` | `"undefined"` | `UndefinedRemoteValue` | _(none)_ |
| `null` | `"null"` | `NullRemoteValue` | _(none)_ |

### Complex Types

| JavaScript Type | `Type` string | Concrete Class | `Value` Property Type |
|----------------|---------------|----------------|-----------------------|
| `Object` | `"object"` | `KeyValuePairCollectionRemoteValue` | `RemoteValueDictionary` |
| `Map` | `"map"` | `KeyValuePairCollectionRemoteValue` | `RemoteValueDictionary` |
| `Array` | `"array"` | `CollectionRemoteValue` | `RemoteValueList` |
| `Set` | `"set"` | `CollectionRemoteValue` | `RemoteValueList` |
| `Date` | `"date"` | `DateRemoteValue` | `DateTime` |
| `RegExp` | `"regexp"` | `RegExpRemoteValue` | `RegularExpressionValue` |
| `DOM Element` | `"node"` | `NodeRemoteValue` | `NodeProperties` |
| `Window` | `"window"` | `WindowProxyRemoteValue` | `WindowProxyProperties` |
| `Function`, `Promise`, etc. | various | `ObjectReferenceRemoteValue` | _(none; use `Handle`)_ |

`RemoteValueDictionary` is a read-only dictionary mapping keys to `RemoteValue` instances. Use `dict[key].As<SpecificType>().Value` to extract values. `RemoteValueList` is a read-only collection of `RemoteValue` instances. Use `list[index].As<SpecificType>().Value` to extract elements.

## Accessing Values

### Using As<T>() and Pattern Matching

Remote values are deserialized into their concrete types. Use C# pattern matching to check and cast in one step, or use `As<T>()` to perform a casting conversion (throws if the type is wrong) and `TryConvertTo<T>()` for a safe try-pattern:

[!code-csharp[Number Values](../code/remote-values/RemoteValuesSamples.cs#ValueAsNumber)]

### String Values

[!code-csharp[String Values](../code/remote-values/RemoteValuesSamples.cs#ValueAsString)]

### Boolean Values

[!code-csharp[Boolean Values](../code/remote-values/RemoteValuesSamples.cs#ValueAsBoolean)]

### Null and Undefined

[!code-csharp[Null and Undefined](../code/remote-values/RemoteValuesSamples.cs#NullandUndefined)]

## Working with Objects

### Simple Objects

[!code-csharp[Simple Object](../code/remote-values/RemoteValuesSamples.cs#SimpleObject)]

### Nested Objects

[!code-csharp[Nested Objects](../code/remote-values/RemoteValuesSamples.cs#NestedObjects)]

## Working with Arrays

[!code-csharp[Array with RemoteValueList](../code/remote-values/RemoteValuesSamples.cs#ArraywithRemoteValueList)]

### Array of Objects

[!code-csharp[Array of Objects](../code/remote-values/RemoteValuesSamples.cs#ArrayofObjects)]

### Converting to Dictionary or List

When you need a flattened `Dictionary<string, object>` or `List<object>` (for example, for serialization or interoperability), use a recursive helper:

[!code-csharp[ToObject Helper](../code/remote-values/RemoteValuesSamples.cs#ToObjectHelper)]

[!code-csharp[ToObject Usage](../code/remote-values/RemoteValuesSamples.cs#ToObjectUsage)]

## Working with DOM Elements

DOM elements are special remote values with type `"node"`.

### Getting Element Information

[!code-csharp[Getting Element Information](../code/remote-values/RemoteValuesSamples.cs#GettingElementInformation)]

### Using Element References

Elements can be passed to subsequent script calls:

[!code-csharp[ToSharedReference](../code/remote-values/RemoteValuesSamples.cs#ToSharedReference)]

## Creating Local Values

When passing values to JavaScript, create `LocalValue` instances.

### Primitive Local Values

[!code-csharp[LocalValue Primitives](../code/remote-values/RemoteValuesSamples.cs#LocalValuePrimitives)]

### Special Values

[!code-csharp[LocalValue Special Values](../code/remote-values/RemoteValuesSamples.cs#LocalValueSpecialValues)]

### Object Local Values

[!code-csharp[LocalValue Object](../code/remote-values/RemoteValuesSamples.cs#LocalValueObject)]

### Array Local Values

[!code-csharp[LocalValue Array](../code/remote-values/RemoteValuesSamples.cs#LocalValueArray)]

### Date Values

[!code-csharp[LocalValue Date](../code/remote-values/RemoteValuesSamples.cs#LocalValueDate)]

### Regular Expression Values

[!code-csharp[LocalValue RegExp](../code/remote-values/RemoteValuesSamples.cs#LocalValueRegExp)]

## Type Checking Patterns

### Safe Type Conversion

[!code-csharp[Safe Type Conversion](../code/remote-values/RemoteValuesSamples.cs#SafeTypeConversion)]

### Checking for Specific Types

[!code-csharp[Checking for Specific Types](../code/remote-values/RemoteValuesSamples.cs#CheckingforSpecificTypes)]

## Common Patterns

### Pattern: Extract Multiple Values

[!code-csharp[Extract Multiple Values](../code/remote-values/RemoteValuesSamples.cs#ExtractMultipleValues)]

### Pattern: Element Collection

[!code-csharp[Element Collection](../code/remote-values/RemoteValuesSamples.cs#ElementCollection)]

### Pattern: Round-trip Element

[!code-csharp[Round-trip Element](../code/remote-values/RemoteValuesSamples.cs#Round-tripElement)]

## Best Practices

1. **Always check result type**: Use pattern matching or type checks
2. **Handle null/undefined**: Check for these types explicitly
3. **Use appropriate .NET types**: `long` for integers, `double` for decimals
4. **Cache element references**: Store `SharedReference` for reuse
5. **Return structured data**: Use objects to return multiple values
6. **Convert early**: Get values into .NET types as soon as possible

## Next Steps

- [Script Module](modules/script.md): Complete script execution guide
- [Core Concepts](core-concepts.md): Understanding commands and responses
- [Examples](examples/common-scenarios.md): Practical usage examples

