# Actions

Full guide: https://webdriverbidi-net.github.io/dramaturge/articles/actions.html

| Action | Waits until the element is |
| --- | --- |
| `ClickAsync`, `DblClickAsync`, `TapAsync`, `CheckAsync`, `DragToAsync`, `HoverAsync` | Visible, stable, enabled, not covered (scrolled into view if needed) |
| `FillAsync`, `ClearAsync` | Also editable |
| `SelectOptionAsync` | Visible and enabled, with every option present and enabled |
| `PressAsync`, `PressSequentiallyAsync`, `FocusAsync`, `SetInputFilesAsync`, `DispatchEventAsync` | Found |

- `ClickOptions`: `Button` (`PointerButton`, `WebDriverBiDi.Input`), `ClickCount`, `Modifiers` (`KeyModifiers`), `Offset` from the center, `Force`, `Timeout`.
- `FillAsync` replaces text; date, color, checkbox, and other untyped inputs throw `InvalidOperationException`. `PressSequentiallyAsync` types key by key, for pages that react to each key. `PressAsync` presses one key, a character or a `Keys` constant (`WebDriverBiDi.Input`), with `KeyActionOptions.Modifiers`.
- `CheckAsync`/`UncheckAsync`/`SetCheckedAsync` click only when needed, then confirm; a radio button cannot be unchecked.
- `SelectOptionAsync([SelectOption.ByLabel("Norway")])`, `ByValue`, `ByIndex`; fires `input` and `change`.
- `SetInputFilesAsync(paths)`: paths on the browser's machine.
- `Force = true` skips readiness checks; the element must still match once. A forced click on a covered element may hit what covers it.
- `page.Mouse` (CSS pixels of the viewport) and `page.Keyboard` act without an element; keys held stay held until released.
- Timeouts throw `WebDriverBiDiTimeoutException` with what the last check saw; a wrong element kind throws `InvalidOperationException` at once.
