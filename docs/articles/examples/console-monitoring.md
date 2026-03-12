# Console Monitoring Example

This example demonstrates how to capture and monitor browser console logs, JavaScript errors, and warnings using WebDriverBiDi.NET.

## Overview

This example shows:
- Capturing all console log messages
- Filtering by log level (error, warn, info, debug)
- Collecting logs for analysis
- Failing tests on JavaScript errors
- Logging to files

## Example 1: Basic Console Monitoring

[!code-csharp[Basic Console Monitoring](../../code/examples/ConsoleMonitoringSamples.cs#BasicConsoleMonitoring)]

## Example 2: Error Detection and Reporting

[!code-csharp[Error Detection and Reporting](../../code/examples/ConsoleMonitoringSamples.cs#ErrorDetectionandReporting)]

## Example 3: Detailed Log Analysis

[!code-csharp[LogAnalyzer class](../../code/examples/ConsoleMonitoringSamples.cs#LogAnalyzerclass)]

[!code-csharp[LogAnalyzer usage](../../code/examples/ConsoleMonitoringSamples.cs#LogAnalyzerusage)]

## Example 4: Logging to File

[!code-csharp[Logging to File](../../code/examples/ConsoleMonitoringSamples.cs#LoggingtoFile)]

## Example 5: Filtering Console Logs

[!code-csharp[Filter Console Logs](../../code/examples/ConsoleMonitoringSamples.cs#FilterConsoleLogs)]

[!code-csharp[Filter by Console API Calls](../../code/examples/ConsoleMonitoringSamples.cs#FilterbyConsoleAPICalls)]

[!code-csharp[Filter JavaScript Exceptions](../../code/examples/ConsoleMonitoringSamples.cs#FilterJavaScriptExceptions)]

## Example 6: Console Log Assertions

[!code-csharp[ConsoleAsserter class](../../code/examples/ConsoleMonitoringSamples.cs#ConsoleAsserterclass)]

[!code-csharp[ConsoleAsserter usage](../../code/examples/ConsoleMonitoringSamples.cs#ConsoleAsserterusage)]

## Example 7: Real-Time Console Display

[!code-csharp[Real-Time Console Display](../../code/examples/ConsoleMonitoringSamples.cs#Real-TimeConsoleDisplay)]

## Pattern: Collecting Logs Per Test

[!code-csharp[TestLogger class](../../code/examples/ConsoleMonitoringSamples.cs#TestLoggerclass)]

[!code-csharp[TestLogger usage](../../code/examples/ConsoleMonitoringSamples.cs#TestLoggerusage)]

## Best Practices

1. **Subscribe early**: Subscribe to log events before navigating
2. **Filter appropriately**: Don't process every log if you only need errors
3. **Use async handlers**: For file writing or database logging
4. **Store important logs**: Keep error logs for debugging
5. **Clean up**: Remove observers when done to prevent memory leaks
6. **Test for errors**: Fail tests if unexpected errors occur

## Common Issues

### Missing Logs

**Problem**: Not all console logs are captured.

**Solution**: 
- Ensure subscription happens before navigation
- Check that log level filtering isn't too restrictive
- Wait for async operations to complete

### Too Many Logs

**Problem**: Overwhelming amount of log output.

**Solution**:
- Filter by log level (errors/warnings only)
- Filter by log type (JavaScript errors only)
- Use conditional logging based on test phase

### Stack Traces Missing

**Problem**: Error logs don't include stack traces.

**Solution**: Stack traces are only available for some error types. Check `e.StackTrace != null` before accessing.

## Next Steps

- [Log Module](../modules/log.md): Complete log module guide
- [Events and Observables](../events-observables.md): Understanding event handling
- [Common Scenarios](common-scenarios.md): More examples

