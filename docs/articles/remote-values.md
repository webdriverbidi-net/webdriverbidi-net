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

```csharp
EvaluateResult result = await driver.Script.EvaluateAsync(
    new EvaluateCommandParameters("42", target, true));

if (result is EvaluateResultSuccess success)
{
    RemoteValue remoteValue = success.Result;
    
    // Convert to long
    long number = remoteValue.ValueAs<long>();
    Console.WriteLine(number); // 42
    
    // Can also convert to double
    double doubleNumber = remoteValue.ValueAs<double>();
    Console.WriteLine(doubleNumber); // 42.0
}
```

### String Values

```csharp
EvaluateResult result = await driver.Script.EvaluateAsync(
    new EvaluateCommandParameters("'Hello, World!'", target, true));

if (result is EvaluateResultSuccess success)
{
    string text = success.Result.ValueAs<string>();
    Console.WriteLine(text); // "Hello, World!"
}
```

### Boolean Values

```csharp
EvaluateResult result = await driver.Script.EvaluateAsync(
    new EvaluateCommandParameters("true", target, true));

if (result is EvaluateResultSuccess success)
{
    bool flag = success.Result.ValueAs<bool>();
    Console.WriteLine(flag); // True
}
```

### Null and Undefined

```csharp
EvaluateResult result = await driver.Script.EvaluateAsync(
    new EvaluateCommandParameters("null", target, true));

if (result is EvaluateResultSuccess success)
{
    if (success.Result.Type == "null" || success.Result.Type == "undefined")
    {
        Console.WriteLine("Value is null or undefined");
    }
}
```

## Working with Objects

### Simple Objects

```csharp
string script = @"
({
    name: 'John',
    age: 30,
    active: true
})";

EvaluateResult result = await driver.Script.EvaluateAsync(
    new EvaluateCommandParameters(script, target, true));

if (result is EvaluateResultSuccess success)
{
    RemoteValue obj = success.Result;
    
    // Convert to RemoteValueDictionary; extract values with ValueAs<T>()
    RemoteValueDictionary dict = obj.ValueAs<RemoteValueDictionary>();
    
    Console.WriteLine(dict["name"].ValueAs<string>());   // "John"
    Console.WriteLine(dict["age"].ValueAs<long>());    // 30
    Console.WriteLine(dict["active"].ValueAs<bool>()); // True
}
```

### Nested Objects

```csharp
string script = @"
({
    user: {
        name: 'John',
        address: {
            city: 'New York',
            zip: '10001'
        }
    }
})";

EvaluateResult result = await driver.Script.EvaluateAsync(
    new EvaluateCommandParameters(script, target, true));

if (result is EvaluateResultSuccess success)
{
    RemoteValueDictionary dict = success.Result.ValueAs<RemoteValueDictionary>();
    RemoteValueDictionary user = dict["user"].ValueAs<RemoteValueDictionary>();
    RemoteValueDictionary address = user["address"].ValueAs<RemoteValueDictionary>();
    
    Console.WriteLine(address["city"].ValueAs<string>()); // "New York"
}
```

## Working with Arrays

```csharp
string script = "[1, 2, 3, 4, 5]";

EvaluateResult result = await driver.Script.EvaluateAsync(
    new EvaluateCommandParameters(script, target, true));

if (result is EvaluateResultSuccess success)
{
    RemoteValueList list = success.Result.ValueAs<RemoteValueList>();
    
    Console.WriteLine($"Length: {list.Count}"); // 5
    
    foreach (RemoteValue item in list)
    {
        Console.WriteLine(item.ValueAs<long>());
    }
}
```

### Array of Objects

```csharp
string script = @"
[
    { name: 'Alice', age: 25 },
    { name: 'Bob', age: 30 },
    { name: 'Charlie', age: 35 }
]";

EvaluateResult result = await driver.Script.EvaluateAsync(
    new EvaluateCommandParameters(script, target, true));

if (result is EvaluateResultSuccess success)
{
    RemoteValueList list = success.Result.ValueAs<RemoteValueList>();
    
    foreach (RemoteValue item in list)
    {
        RemoteValueDictionary person = item.ValueAs<RemoteValueDictionary>();
        Console.WriteLine($"{person["name"].ValueAs<string>()}, age {person["age"].ValueAs<long>()}");
    }
}
```

### Converting to Dictionary or List

When you need a flattened `Dictionary<string, object>` or `List<object>` (for example, for serialization or interoperability), use a recursive helper:

```csharp
using System.Linq;

static object? ToObject(RemoteValue value)
{
    return value.Type switch
    {
        "string" => value.ValueAs<string>(),
        "number" => value.Value is long l ? l : value.ValueAs<double>(),
        "boolean" => value.ValueAs<bool>(),
        "null" or "undefined" => null,
        "object" or "map" => value.ValueAs<RemoteValueDictionary>()
            .ToDictionary(kvp => kvp.Key.ToString() ?? "", kvp => ToObject(kvp.Value)),
        "array" or "set" => value.ValueAs<RemoteValueList>()
            .Select(ToObject)
            .ToList(),
        _ => value.Value
    };
}

// Usage: convert RemoteValueDictionary to Dictionary<string, object>
RemoteValueDictionary dict = success.Result.ValueAs<RemoteValueDictionary>();
Dictionary<string, object?> flat = dict.ToDictionary(
    kvp => kvp.Key.ToString() ?? "",
    kvp => ToObject(kvp.Value));
```

## Working with DOM Elements

DOM elements are special remote values with type `"node"`.

### Getting Element Information

```csharp
EvaluateResult result = await driver.Script.EvaluateAsync(
    new EvaluateCommandParameters(
        "document.querySelector('button')",
        target,
        true));

if (result is EvaluateResultSuccess success)
{
    RemoteValue element = success.Result;
    
    Console.WriteLine($"Type: {element.Type}"); // "node"
    Console.WriteLine($"SharedId: {element.SharedId}");
    
    // Get node properties
    NodeProperties? nodeProps = element.ValueAs<NodeProperties>();
    
    if (nodeProps != null)
    {
        Console.WriteLine($"Tag: {nodeProps.LocalName}");
        Console.WriteLine($"Node Type: {nodeProps.NodeType}");
        Console.WriteLine($"Child Count: {nodeProps.ChildNodeCount}");
        
        // Get attributes
        if (nodeProps.Attributes != null)
        {
            foreach (var attr in nodeProps.Attributes)
            {
                Console.WriteLine($"  {attr.Key} = {attr.Value}");
            }
        }
    }
}
```

### Using Element References

Elements can be passed to subsequent script calls:

```csharp
// Get element
EvaluateResult getResult = await driver.Script.EvaluateAsync(
    new EvaluateCommandParameters(
        "document.querySelector('button')",
        target,
        true));

if (getResult is EvaluateResultSuccess getSuccess)
{
    RemoteValue element = getSuccess.Result;
    
    // Create a reference
    SharedReference elementRef = element.ToSharedReference();
    
    // Use in another script call
    CallFunctionCommandParameters clickParams = new CallFunctionCommandParameters(
        "(element) => element.click()",
        target,
        false);
    clickParams.Arguments.Add(elementRef);
    
    await driver.Script.CallFunctionAsync(clickParams);
}
```

## Creating Local Values

When passing values to JavaScript, create `LocalValue` instances.

### Primitive Local Values

```csharp
CallFunctionCommandParameters params = new CallFunctionCommandParameters(
    "(str, num, bool) => console.log(str, num, bool)",
    target,
    false);

params.Arguments.Add(LocalValue.String("Hello"));
params.Arguments.Add(LocalValue.Number(42));
params.Arguments.Add(LocalValue.Boolean(true));

await driver.Script.CallFunctionAsync(params);
```

### Special Values

```csharp
params.Arguments.Add(LocalValue.Null());
params.Arguments.Add(LocalValue.Undefined());

// Special numbers
params.Arguments.Add(LocalValue.Number(double.PositiveInfinity));
params.Arguments.Add(LocalValue.Number(double.NegativeInfinity));
params.Arguments.Add(LocalValue.Number(double.NaN));
```

### Object Local Values

```csharp
Dictionary<string, LocalValue> obj = new Dictionary<string, LocalValue>
{
    { "name", LocalValue.String("John") },
    { "age", LocalValue.Number(30) },
    { "active", LocalValue.Boolean(true) },
    { "metadata", LocalValue.Object(new Dictionary<string, LocalValue>
        {
            { "created", LocalValue.Date(DateTime.Now) }
        })
    }
};

params.Arguments.Add(LocalValue.Object(obj));
```

### Array Local Values

```csharp
List<LocalValue> array = new List<LocalValue>
{
    LocalValue.Number(1),
    LocalValue.Number(2),
    LocalValue.Number(3)
};

params.Arguments.Add(LocalValue.Array(array));
```

### Date Values

```csharp
params.Arguments.Add(LocalValue.Date(DateTime.Now));
params.Arguments.Add(LocalValue.Date(new DateTime(2024, 1, 1)));
```

### Regular Expression Values

```csharp
// Pattern and flags
params.Arguments.Add(LocalValue.RegularExpression("\\d+", "g"));
params.Arguments.Add(LocalValue.RegularExpression("[a-z]+", "i"));
```

## Type Checking Patterns

### Safe Type Conversion

```csharp
RemoteValue value = success.Result;

switch (value.Type)
{
    case "string":
        string str = value.ValueAs<string>();
        break;
    
    case "number":
        // Try long first, then double
        try
        {
            long longNum = value.ValueAs<long>();
        }
        catch
        {
            double doubleNum = value.ValueAs<double>();
        }
        break;
    
    case "boolean":
        bool flag = value.ValueAs<bool>();
        break;
    
    case "object":
        RemoteValueDictionary obj = value.ValueAs<RemoteValueDictionary>();
        break;
    
    case "array":
        RemoteValueList list = value.ValueAs<RemoteValueList>();
        break;
    
    case "node":
        NodeProperties? node = value.ValueAs<NodeProperties>();
        break;
    
    case "null":
    case "undefined":
        // Handle null/undefined
        break;
}
```

### Checking for Specific Types

```csharp
if (value.Type == "node")
{
    // It's a DOM element
    SharedReference elementRef = value.ToSharedReference();
}
else if (value.Type == "array")
{
    // It's an array
    RemoteValueList list = value.ValueAs<RemoteValueList>();
}
```

## Common Patterns

### Pattern: Extract Multiple Values

```csharp
string script = @"
({
    title: document.title,
    url: window.location.href,
    linkCount: document.querySelectorAll('a').length,
    ready: document.readyState === 'complete'
})";

EvaluateResult result = await driver.Script.EvaluateAsync(
    new EvaluateCommandParameters(script, target, true));

if (result is EvaluateResultSuccess success)
{
    RemoteValueDictionary data = success.Result.ValueAs<RemoteValueDictionary>();
    
    string title = data["title"].ValueAs<string>();
    string url = data["url"].ValueAs<string>();
    long linkCount = data["linkCount"].ValueAs<long>();
    bool ready = data["ready"].ValueAs<bool>();
}
```

### Pattern: Element Collection

```csharp
string script = "Array.from(document.querySelectorAll('a')).map(a => a.href)";

EvaluateResult result = await driver.Script.EvaluateAsync(
    new EvaluateCommandParameters(script, target, true));

if (result is EvaluateResultSuccess success)
{
    RemoteValueList links = success.Result.ValueAs<RemoteValueList>();
    
    foreach (RemoteValue link in links)
    {
        Console.WriteLine(link.ValueAs<string>());
    }
}
```

### Pattern: Round-trip Element

```csharp
// Get element
EvaluateResult getResult = await driver.Script.EvaluateAsync(
    new EvaluateCommandParameters(
        "document.querySelector('.target')",
        target,
        true));

RemoteValue element = ((EvaluateResultSuccess)getResult).Result;

// Get properties from element
CallFunctionCommandParameters propsParams = new CallFunctionCommandParameters(
    @"(element) => ({
        tag: element.tagName,
        text: element.textContent,
        visible: element.offsetParent !== null
    })",
    target,
    false);
propsParams.Arguments.Add(element.ToSharedReference());

EvaluateResult propsResult = await driver.Script.CallFunctionAsync(propsParams);
RemoteValueDictionary props = ((EvaluateResultSuccess)propsResult).Result
    .ValueAs<RemoteValueDictionary>();
```

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

