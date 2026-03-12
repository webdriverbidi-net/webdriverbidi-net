# Script Module

The Script module provides functionality for executing JavaScript in the browser, managing preload scripts, and working with JavaScript values and objects.

## Overview

The Script module allows you to:

- Execute JavaScript code in browsing contexts
- Call JavaScript functions with arguments
- Add preload scripts that run before page scripts
- Manage script execution realms (sandboxes)
- Send messages from scripts back to your code
- Work with JavaScript objects and DOM elements

## Accessing the Module

[!code-csharp[Accessing the Module](../../code/script/ScriptSamples.cs#AccessingtheModule)]

## Timeout and Cancellation

All commands in this module accept optional `timeoutOverride` and `CancellationToken` parameters. Use `timeoutOverride` to set a per-command timeout (defaults to `BiDiDriver.DefaultCommandTimeout` when omitted). Use `CancellationToken` for cooperative cancellation. See the [API Design Guide](../advanced/api-design.md#timeout-and-cancellation) for details and examples.

## Evaluating JavaScript

### Evaluate Expression

[!code-csharp[Evaluate Expression](../../code/script/ScriptSamples.cs#EvaluateExpression)]

### Evaluate with Complex Expression

[!code-csharp[Evaluate Complex Expression](../../code/script/ScriptSamples.cs#EvaluateComplexExpression)]

## Calling Functions

### Call Function with Arguments

[!code-csharp[Call Function with Arguments](../../code/script/ScriptSamples.cs#CallFunctionwithArguments)]

### Call Function with DOM Element

[!code-csharp[Call Function with DOM Element](../../code/script/ScriptSamples.cs#CallFunctionwithDOMElement)]

### Call Method on Object

[!code-csharp[Call Method on Object](../../code/script/ScriptSamples.cs#CallMethodonObject)]

## Execution Targets

Scripts can be executed in different contexts:

### Context Target

Execute in a specific browsing context:

[!code-csharp[Context Target](../../code/script/ScriptSamples.cs#ContextTarget)]

### Realm Target

Execute in a specific execution realm:

[!code-csharp[Realm Target](../../code/script/ScriptSamples.cs#RealmTarget)]

### Sandboxed Execution

Execute in a sandbox to isolate from page scripts:

[!code-csharp[Sandboxed Execution](../../code/script/ScriptSamples.cs#SandboxedExecution)]

## Preload Scripts

Preload scripts run before any page scripts, allowing you to:

- Inject utilities into every page
- Monitor page behavior
- Modify page behavior before it starts

### Add Preload Script

[!code-csharp[Add Preload Script](../../code/script/ScriptSamples.cs#AddPreloadScript)]

### Preload Script with Arguments

Preload script arguments in WebDriver BiDi use channels to pass values. Use `ChannelValue` with `ChannelProperties`:

[!code-csharp[Preload Script with Channel](../../code/script/ScriptSamples.cs#PreloadScriptwithChannel)]

### Sandboxed Preload Script

[!code-csharp[Sandboxed Preload Script](../../code/script/ScriptSamples.cs#SandboxedPreloadScript)]

### Remove Preload Script

[!code-csharp[Remove Preload Script](../../code/script/ScriptSamples.cs#RemovePreloadScript)]

## Working with Remote Values

### Accessing Primitive Values

[!code-csharp[Accessing Primitive Values](../../code/script/ScriptSamples.cs#AccessingPrimitiveValues)]

### Accessing Objects

[!code-csharp[Accessing Objects](../../code/script/ScriptSamples.cs#AccessingObjects)]

### Accessing Arrays

[!code-csharp[Accessing Arrays](../../code/script/ScriptSamples.cs#AccessingArrays)]

### Working with DOM Elements

[!code-csharp[Working with DOM Elements](../../code/script/ScriptSamples.cs#WorkingwithDOMElements)]

## Creating Local Values

When passing values to JavaScript:

### Primitive Values

[!code-csharp[LocalValue Primitives](../../code/script/ScriptSamples.cs#LocalValuePrimitives)]

### Special Values

[!code-csharp[LocalValue Special Values](../../code/script/ScriptSamples.cs#LocalValueSpecialValues)]

### Objects

[!code-csharp[LocalValue Object](../../code/script/ScriptSamples.cs#LocalValueObject)]

### Arrays

[!code-csharp[LocalValue Array](../../code/script/ScriptSamples.cs#LocalValueArray)]

### Dates

[!code-csharp[LocalValue Date](../../code/script/ScriptSamples.cs#LocalValueDate)]

### Regular Expressions

[!code-csharp[LocalValue RegExp](../../code/script/ScriptSamples.cs#LocalValueRegExp)]

## Handling Script Errors

### Try-Catch Pattern

[!code-csharp[Try-Catch Pattern](../../code/script/ScriptSamples.cs#Try-CatchPattern)]

### Catching JavaScript Errors

[!code-csharp[Catching JavaScript Errors](../../code/script/ScriptSamples.cs#CatchingJavaScriptErrors)]

## Awaiting Promises

### Automatic Promise Resolution

[!code-csharp[Automatic Promise Resolution](../../code/script/ScriptSamples.cs#AutomaticPromiseResolution)]

### Async Functions

[!code-csharp[Async Functions](../../code/script/ScriptSamples.cs#AsyncFunctions)]

## Events

### Script Message Event

[!code-csharp[Script Message Event](../../code/script/ScriptSamples.cs#ScriptMessageEvent)]

### Realm Created/Destroyed

[!code-csharp[Realm Created/Destroyed](../../code/script/ScriptSamples.cs#RealmCreated/Destroyed)]

## Common Patterns

### Element Interaction Pattern

[!code-csharp[Element Interaction Pattern](../../code/script/ScriptSamples.cs#ElementInteractionPattern)]

### Get Element Properties Pattern

[!code-csharp[Get Element Properties Pattern](../../code/script/ScriptSamples.cs#GetElementPropertiesPattern)]

### Wait for Condition Pattern

[!code-csharp[Wait for Condition Pattern](../../code/script/ScriptSamples.cs#WaitforConditionPattern)]

## Best Practices

1. **Use `awaitPromise`**: Set to `true` for async operations
2. **Handle exceptions**: Check for `EvaluateResultException`
3. **Use sandboxes**: Isolate your scripts from page scripts
4. **Cache element references**: Store `SharedReference` for reuse
5. **Prefer functions over eval**: Use `CallFunctionAsync` for better isolation
6. **Remove preload scripts**: Clean up when no longer needed

## Next Steps

- [Remote Values Guide](../remote-values.md): Deep dive into JavaScript value handling
- [Browsing Context Module](browsing-context.md): Finding elements
- [Preload Scripts Example](../examples/preload-scripts.md): Complete examples
- [API Reference](../../api/index.md): Complete API documentation

## API Reference

See the [API documentation](../../api/index.md) for complete details on all classes and methods in the Script module.

