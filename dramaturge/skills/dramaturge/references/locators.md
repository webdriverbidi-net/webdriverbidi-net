# Locators

Full guide: https://webdriverbidi-net.github.io/dramaturge/articles/locators.html

| Method | Finds by | Matching |
| --- | --- | --- |
| `GetByRole(role, name, states)` | Accessibility role the browser computes, and accessible name | Name exact |
| `GetByLabel(text)` | `aria-labelledby`, then `aria-label`, then a form control's `<label>` | Contains, ignoring case and runs of whitespace |
| `GetByText(text)` | Rendered text (not in Firefox) | Contains, ignoring case |
| `GetByPlaceholder`, `GetByAltText`, `GetByTitle` | That attribute | Contains, ignoring case |
| `GetByTestId(id)` | `DramaturgeOptions.TestIdAttribute`, `data-testid` by default | Exact |
| `Locate(new CssLocator(…))`, `XPathLocator` | CSS or XPath (`WebDriverBiDi.BrowsingContext`) | |

Text methods take `exact: true` for a whole match with case. `GetByRole` takes `RoleStates` (`Checked`, `Pressed`, `Expanded`, `Selected`, `Level`, `Disabled`).

- **Scope**: `Page` and `Frame` methods search the document; a locator's methods search within its matches.
- **Filter**: `Filter(hasText:, hasNotText:, has:, hasNot:)` keeps matches containing, or not, a text or element. Text compares rendered text, ignoring case and whitespace runs.
- **Combine**: `And` (both match), `Or` (either, first locator's matches first). Combined locators must be in the same frame.
- **One of many**: `First()`, `Last()`, `Nth(i)` (zero-based, document order); `CountAsync()` counts now.
- **Strict**: actions, reads, and single-element expectations throw `AmbiguousElementException` when more than one element matches.
- **Shadow DOM**: `ShadowRoot()` searches inside the matches' shadow roots, open or closed; `PierceShadowRoots = true` searches open ones automatically (not XPath).
- **Frames**: `await locator.ContentFrameAsync()` returns the frame of an `iframe`; search the frame. A frame whose document is replaced reports `IsDetached`.
- **State**: `WaitForAsync(ElementState.Detached)` waits for an element to go; no element counts as hidden.
