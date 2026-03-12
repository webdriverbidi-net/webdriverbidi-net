# Input Module

The Input module provides functionality for simulating user input including mouse, keyboard, touch, and wheel events.

## Overview

The Input module allows you to:

- Simulate mouse movements and clicks
- Send keyboard input
- Perform touch gestures
- Simulate wheel scrolling
- Chain multiple actions together

## Accessing the Module

[!code-csharp[Accessing Module](../../code/modules/InputModuleSamples.cs#AccessingModule)]

## Timeout and Cancellation

All commands in this module accept optional `timeoutOverride` and `CancellationToken` parameters. Use `timeoutOverride` to set a per-command timeout (defaults to `BiDiDriver.DefaultCommandTimeout` when omitted). Use `CancellationToken` for cooperative cancellation. See the [API Design Guide](../advanced/api-design.md#timeout-and-cancellation) for details and examples.

## Performing Actions

### Mouse Click

[!code-csharp[Mouse Click](../../code/modules/InputModuleSamples.cs#MouseClick)]

### Keyboard Input

[!code-csharp[Keyboard Input](../../code/modules/InputModuleSamples.cs#KeyboardInput)]

### Click on Element

[!code-csharp[Click on Element](../../code/modules/InputModuleSamples.cs#ClickonElement)]

### Send Keys to Element

[!code-csharp[Send Keys to Element](../../code/modules/InputModuleSamples.cs#SendKeystoElement)]

### Modifier Keys

[!code-csharp[Modifier Keys](../../code/modules/InputModuleSamples.cs#ModifierKeys)]

### Release Actions

[!code-csharp[Release Actions](../../code/modules/InputModuleSamples.cs#ReleaseActions)]

## Common Key Constants

Use Unicode values for special keys: Enter `\uE007`, Tab `\uE004`, Control `\uE009`, etc. A partial list is in the table below.

| Key         | Unicode Value |
|-------------|---------------|
| Enter       | `\uE007`      |
| Tab         | `\uE004`      |
| Backspace   | `\uE003`      |
| Delete      | `\uE017`      |
| Escape      | `\uE00C`      |
| Control     | `\uE009`      |
| Shift       | `\uE008`      |
| Alt         | `\uE00A`      |
| Arrow Up    | `\uE013`      |
| Arrow Down  | `\uE015`      |
| Arrow Left  | `\uE012`      |
| Arrow Right | `\uE014`      |

## Best Practices

1. **Release actions**: Call `ReleaseActionsAsync` between test scenarios
2. **Use delays**: Add small delays between actions for reliability
3. **Focus elements**: Click or tab to elements before sending keys
4. **Check element state**: Ensure elements are visible and enabled

## Helper Libraries

The demo project includes an `InputBuilder` helper class that simplifies common input patterns. Consider creating similar helpers for your projects.

## Next Steps

- [Browsing Context Module](browsing-context.md): Locating elements to interact with
- [Examples](../examples/form-submission.md): Form interaction examples
- [API Reference](../../api/index.md): Complete API documentation

