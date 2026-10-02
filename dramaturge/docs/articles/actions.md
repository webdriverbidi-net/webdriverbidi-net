# Actions

Actions on an element act the way a user would, through WebDriver BiDi's input actions, so the page sees the same events it would from a person. Each action waits until its locator finds exactly one element and that element is ready.

## Readiness

| Action | Waits until the element is |
| --- | --- |
| `ClickAsync`, `DblClickAsync`, `TapAsync`, `CheckAsync`, `DragToAsync` | Visible, stable, enabled, and not covered by another element |
| `HoverAsync` | Visible, stable, enabled, and not covered |
| `FillAsync`, `ClearAsync` | Visible, stable, enabled, editable, and not covered |
| `SelectOptionAsync` | Visible and enabled, with every option present and enabled |
| `ScrollIntoViewIfNeededAsync` | Visible and stable |
| `PressAsync`, `PressSequentiallyAsync`, `FocusAsync`, `BlurAsync`, `SetInputFilesAsync`, `DispatchEventAsync` | Found |

"Stable" means it has stopped moving. "Not covered" means the point the pointer would use is on the element, not on something drawn over it; the element is scrolled into view first if needed. An element that is removed while an action waits is found again, and an action that runs out of time throws `WebDriverBiDiTimeoutException` saying what it last saw.

## Clicking and Pointing

[!code-csharp[Pointer Actions](../code/ActionsSamples.cs#Pointer)]

`ClickOptions` sets the button, the number of clicks, the modifier keys held, and the `Offset` from the center of the element's visible part; by default the pointer goes to a point on the element that nothing covers. `TapAsync` uses a touch pointer. `DragToAsync` drags with the mouse onto another element in the same frame, and `DragOptions.TargetOffset` chooses where it lands.

> **Firefox:** a native HTML5 drag from WebDriver input actions dispatches only `dragstart` ([bug 1515879](https://bugzilla.mozilla.org/show_bug.cgi?id=1515879)), so `DragToAsync` onto a `draggable` element's drop target does not drop in Firefox.

## Typing

[!code-csharp[Text Entry](../code/ActionsSamples.cs#Text)]

- `FillAsync` replaces an input's, a text area's, or an editable element's text, selecting it and typing the new text over it. An input whose value is not typed, such as a date, a color, or a checkbox, cannot be filled and throws `InvalidOperationException`.
- `PressSequentiallyAsync` types text one key press at a time, without clearing what is there, optionally with a delay between keys.
- `PressAsync` presses one key: a character, or a `Keys` constant (in `WebDriverBiDi.Input`) such as `Keys.Enter`, holding any `KeyModifiers`.

Text is typed one user-perceived character at a time, so emoji and accented characters arrive whole.

## Form Controls

[!code-csharp[Form Controls](../code/ActionsSamples.cs#FormControls)]

- `CheckAsync`, `UncheckAsync`, and `SetCheckedAsync` click the control only if it is not already in the state asked for, then confirm that the click changed it. A radio button cannot be unchecked by clicking it, so unchecking one throws.
- `SelectOptionAsync` selects options of a `<select>` by value, label, or index, deselecting the rest, and fires the `input` and `change` events a user's choice would. It returns the values selected.
- `SetInputFilesAsync` sets a file input's files. The paths are on the machine the browser runs on.

## Acting Without Waiting

Every action takes `Force`, which skips the readiness checks. The element must still be found, exactly once:

[!code-csharp[Force](../code/ActionsSamples.cs#Force)]

Use it only when a check is wrong for a particular page; a forced click on a covered element can land on whatever covers it.

## Other Actions

[!code-csharp[Other Actions](../code/ActionsSamples.cs#Other)]

`DispatchEventAsync` creates the event with the interface a user's action would use for its type, such as `MouseEvent` for `click`. The event bubbles, is cancelable, and crosses shadow boundaries unless its init properties say otherwise. It returns `false` if a listener canceled it.

## The Page's Mouse and Keyboard

For input not aimed at one element, such as drawing on a canvas or holding a key across several actions, use the page's `Mouse` and `Keyboard`. Mouse coordinates are CSS pixels of the page's viewport; for an element's position, see `BoundingBoxAsync` and `BoundingBox.ToTopLevelAsync`:

[!code-csharp[Mouse and Keyboard](../code/ActionsSamples.cs#MouseAndKeyboard)]

A page keeps one mouse and one keyboard for its lifetime, so a key or button held down stays down until it is released.
