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

```csharp
InputModule input = driver.Input;
```

## Performing Actions

### Mouse Click

```csharp
PerformActionsCommandParameters params = new PerformActionsCommandParameters(contextId);

// Create a pointer (mouse) action source
PointerSource mouseSource = new PointerSource("mouse", PointerType.Mouse);

// Move to element and click
mouseSource.CreatePointerMove(100, 100, TimeSpan.Zero);
mouseSource.CreatePointerDown(MouseButton.Left);
mouseSource.CreatePointerUp(MouseButton.Left);

params.Actions.Add(mouseSource);

await driver.Input.PerformActionsAsync(params);
```

### Keyboard Input

```csharp
PerformActionsCommandParameters params = new PerformActionsCommandParameters(contextId);

// Create a keyboard action source
KeySource keySource = new KeySource("keyboard");

// Type text
keySource.CreateKeyDown("H");
keySource.CreateKeyUp("H");
keySource.CreateKeyDown("i");
keySource.CreateKeyUp("i");

params.Actions.Add(keySource);

await driver.Input.PerformActionsAsync(params);
```

### Click on Element

```csharp
// First locate the element
LocateNodesCommandResult locateResult = await driver.BrowsingContext.LocateNodesAsync(
    new LocateNodesCommandParameters(contextId, new CssLocator("button")));

RemoteValue element = locateResult.Nodes[0];

// Click the element
PerformActionsCommandParameters params = new PerformActionsCommandParameters(contextId);

PointerSource mouseSource = new PointerSource("mouse", PointerType.Mouse);
mouseSource.CreatePointerMoveToElement(element.ToSharedReference(), 0, 0, TimeSpan.Zero);
mouseSource.CreatePointerDown(MouseButton.Left);
mouseSource.CreatePointerUp(MouseButton.Left);

params.Actions.Add(mouseSource);

await driver.Input.PerformActionsAsync(params);
```

### Send Keys to Element

```csharp
// Click element first to focus it
// ... (click code from above)

// Then send keys
PerformActionsCommandParameters params = new PerformActionsCommandParameters(contextId);

KeySource keySource = new KeySource("keyboard");
string text = "Hello, World!";

foreach (char c in text)
{
    keySource.CreateKeyDown(c.ToString());
    keySource.CreateKeyUp(c.ToString());
}

// Press Enter
keySource.CreateKeyDown(Keys.Enter);
keySource.CreateKeyUp(Keys.Enter);

params.Actions.Add(keySource);

await driver.Input.PerformActionsAsync(params);
```

### Modifier Keys

```csharp
PerformActionsCommandParameters params = new PerformActionsCommandParameters(contextId);

KeySource keySource = new KeySource("keyboard");

// Ctrl+A (Select All)
keySource.CreateKeyDown(Keys.Control);
keySource.CreateKeyDown("a");
keySource.CreateKeyUp("a");
keySource.CreateKeyUp(Keys.Control);

params.Actions.Add(keySource);

await driver.Input.PerformActionsAsync(params);
```

### Release Actions

```csharp
// Release all pressed keys/buttons
ReleaseActionsCommandParameters params = new ReleaseActionsCommandParameters(contextId);
await driver.Input.ReleaseActionsAsync(params);
```

## Common Key Constants

The `Keys` class provides constants for special keys:

```csharp
Keys.Enter
Keys.Tab
Keys.Backspace
Keys.Delete
Keys.Escape
Keys.Control
Keys.Shift
Keys.Alt
Keys.ArrowUp
Keys.ArrowDown
Keys.ArrowLeft
Keys.ArrowRight
// ... and more
```

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

