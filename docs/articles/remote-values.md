# Working with Remote Values

Remote values represent JavaScript data returned from the browser. This guide explains how to work with them effectively.

## Overview

When you execute JavaScript in the browser, the results are returned as `RemoteValue` objects. These represent JavaScript values in a .NET-friendly way while preserving type information and references to browser objects.

## RemoteValue Structure

Every `RemoteValue` has:

- `Type` - The JavaScript type as a string
- `Value` - The .NET representation (may be null for non-primitive types)
- `SharedId` - Unique identifier for objects that can be referenced later
- `Handle` - Internal handle (less commonly used than SharedId)

## JavaScript Type Mapping

### Primitive Types

| JavaScript Type | RemoteValue.Type | .NET Type | Example |
|----------------|------------------|-----------|---------|
| `string` | `"string"` | `string` | `"hello"` |
| `number` | `"number"` | `long` or `double` | `42`, `3.14` |
| `boolean` | `"boolean"` | `bool` | `true` |
| `undefined` | `"undefined"` | `null` | - |
| `null` | `"null"` | `null` | - |

### Complex Types

| JavaScript Type | RemoteValue.Type | .NET Type |
|----------------|------------------|-----------|
| `Object` | `"object"` | `RemoteValueDictionary` |
| `Array` | `"array"` | `RemoteValueList` |
| `Function` | `"function"` | - |
| `Promise` | `"promise"` | - |
| `DOM Element` | `"node"` | `NodeProperties` |
| `Date` | `"date"` | `DateTime` |
| `RegExp` | `"regexp"` | - |
| `Map` | `"map"` | `RemoteValueDictionary` |
| `Set` | `"set"` | `RemoteValueList` |

`RemoteValueDictionary` is a read-only dictionary mapping keys to `RemoteValue` instances. Use `dict[key].ValueAs<T>()` to extract values. `RemoteValueList` is a read-only collection of `RemoteValue` instances. Use `list[index].ValueAs<T>()` to extract elements.

## Accessing Values

### Using ValueAs<T>()

The `ValueAs<T>()` method converts the remote value to the specified .NET type:

[!code-csharp[ValueAs Number](../code/remote-values/RemoteValuesSamples.cs#ValueAsNumber)]

### String Values

[!code-csharp[ValueAs String](../code/remote-values/RemoteValuesSamples.cs#ValueAsString)]

### Boolean Values

[!code-csharp[ValueAs Boolean](../code/remote-values/RemoteValuesSamples.cs#ValueAsBoolean)]

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

