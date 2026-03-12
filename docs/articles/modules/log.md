# Log Module

The Log module provides access to browser console logs and other logging events.

## Overview

The Log module allows you to:

- Capture console log messages
- Monitor JavaScript errors
- Track different log levels
- Access log metadata

## Accessing the Module

[!code-csharp[Accessing Module](../../code/modules/LogModuleSamples.cs#AccessingModule)]

## Monitoring Console Logs

### Basic Log Monitoring

[!code-csharp[Basic Log Monitoring](../../code/modules/LogModuleSamples.cs#BasicLogMonitoring)]

### Log Entry Properties

[!code-csharp[Log Entry Properties](../../code/modules/LogModuleSamples.cs#LogEntryProperties)]

## Filtering by Log Level

### Monitor Only Errors

[!code-csharp[Monitor Only Errors](../../code/modules/LogModuleSamples.cs#MonitorOnlyErrors)]

### Monitor Warnings and Errors

[!code-csharp[Monitor Warnings and Errors](../../code/modules/LogModuleSamples.cs#MonitorWarningsandErrors)]

## Log Types

### Console Logs

These come from `console.log()`, `console.error()`, etc.:

[!code-csharp[Console Logs](../../code/modules/LogModuleSamples.cs#ConsoleLogs)]

### JavaScript Errors

Uncaught JavaScript exceptions:

[!code-csharp[JavaScript Errors](../../code/modules/LogModuleSamples.cs#JavaScriptErrors)]

## Common Patterns

### Collect All Logs

[!code-csharp[Collect All Logs](../../code/modules/LogModuleSamples.cs#CollectAllLogs)]

### Fail on JavaScript Errors

[!code-csharp[Fail on JavaScript Errors](../../code/modules/LogModuleSamples.cs#FailonJavaScriptErrors)]

### Log to File

[!code-csharp[Log to File](../../code/modules/LogModuleSamples.cs#LogtoFile)]

## Best Practices

1. **Subscribe early**: Subscribe to log events before navigating
2. **Filter appropriately**: Don't process every log if you only need errors
3. **Handle async logging**: Use async handlers if writing to files or databases
4. **Store important logs**: Keep error logs for debugging
5. **Test for errors**: Fail tests if unexpected JavaScript errors occur

## Next Steps

- [Events and Observables](../events-observables.md): Understanding event handling
- [Examples: Console Monitoring](../examples/console-monitoring.md): Complete examples
- [API Reference](../../api/index.md): Complete API documentation
