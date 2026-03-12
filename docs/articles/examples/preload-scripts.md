# Preload Scripts Example

This example demonstrates how to use preload scripts to inject JavaScript into pages before any other scripts execute using WebDriverBiDi.NET.

## Overview

Preload scripts allow you to:
- Inject utilities available on every page
- Monitor page behavior before it starts
- Modify or intercept page functionality
- Wait for specific page conditions
- Communicate with your test code via channels

## Example 1: Basic Preload Script

[!code-csharp[Basic Preload Script](../../code/script/PreloadScriptSamples.cs#BasicPreloadScript)]

## Example 2: Preload Script with Channel Communication

[!code-csharp[Preload Script with Channel](../../code/script/PreloadScriptSamples.cs#PreloadScriptwithChannel)]

## Example 3: Wait for Element with Preload Script

[!code-csharp[Wait for Element Preload Script](../../code/script/PreloadScriptSamples.cs#WaitforElementPreloadScript)]

## Example 4: Sandbox Isolation

[!code-csharp[Sandboxed Preload Script](../../code/script/PreloadScriptSamples.cs#SandboxedPreloadScript)]

## Example 5: Intercept Page Behavior

[!code-csharp[Intercept Fetch Preload Script](../../code/script/PreloadScriptSamples.cs#InterceptFetchPreloadScript)]

## Example 6: Performance Monitoring

[!code-csharp[Performance Monitor Preload Script](../../code/script/PreloadScriptSamples.cs#PerformanceMonitorPreloadScript)]

## Example 7: Multiple Preload Scripts

[!code-csharp[Multiple Preload Scripts](../../code/script/PreloadScriptSamples.cs#MultiplePreloadScripts)]

## Pattern: Conditional Preload Scripts

[!code-csharp[Conditional Preload Scripts](../../code/script/PreloadScriptSamples.cs#ConditionalPreloadScripts)]

## Pattern: Temporary Preload Script

[!code-csharp[Temporary Preload Script](../../code/script/PreloadScriptSamples.cs#TemporaryPreloadScript)]

## Best Practices

1. **Use channels**: Communicate with test code via channels
2. **Use sandboxes**: Isolate preload script from page scripts
3. **Keep scripts simple**: Complex logic should be in test code
4. **Remove when done**: Clean up preload scripts after use
5. **Handle timing**: Use load events to ensure DOM is ready
6. **Test isolation**: Each test should manage its own preload scripts

## Common Issues

### Script Not Running

**Problem**: Preload script doesn't seem to execute.

**Solution**:
- Add the script before navigation
- Check for JavaScript errors in the script
- Use `console.log()` in the script to verify execution

### Can't Access Objects

**Problem**: Objects created by preload script are undefined.

**Solution**:
- Ensure you're using the same sandbox
- Check that script actually executed
- Verify timing (DOM might not be ready)

### Channel Messages Not Received

**Problem**: Messages from preload script aren't arriving.

**Solution**:
- Subscribe to script messages before navigation
- Use correct channel ID
- Check that channel was passed as argument

## Next Steps

- [Script Module](../modules/script.md): Complete script module guide
- [Events and Observables](../events-observables.md): Understanding event handling
- [Common Scenarios](common-scenarios.md): More examples

