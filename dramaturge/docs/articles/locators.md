# Locators

An `ElementLocator` describes how to find elements. Creating one finds nothing: each action, read, or expectation looks the element up again when it runs, so a locator stays correct after the page changes, and an element that is replaced while an action waits for it is found again.

`Page`, `Frame`, and `ElementLocator` all have the methods below. On a page they search its main frame; on a locator they search within the elements it finds.

## Finding Elements as a User Sees Them

Prefer locators that describe what a user sees, which change less often than a page's structure:

[!code-csharp[User-Facing Locators](../code/LocatorsSamples.cs#UserFacing)]

| Method | Finds elements by | Matching |
| --- | --- | --- |
| `GetByRole(role, name, states)` | The accessibility role the browser computes, such as `button`, `link`, `heading`, or `checkbox`, and optionally the accessible name | The name must match exactly |
| `GetByLabel(text)` | The element `aria-labelledby` names; else `aria-label`; else a form control's `<label>` | Contains the text, ignoring case and runs of whitespace |
| `GetByText(text)` | Rendered text | Contains the text, ignoring case |
| `GetByPlaceholder(text)` | The `placeholder` attribute | Contains the text, ignoring case |
| `GetByAltText(text)` | The `alt` attribute | Contains the text, ignoring case |
| `GetByTitle(text)` | The `title` attribute | Contains the text, ignoring case |
| `GetByTestId(id)` | The test ID attribute, `data-testid` unless [configured](configuration.md) | Exact |

Each method that takes text also takes `exact: true`, for a whole match with case.

`GetByRole` also takes the ARIA states an element must have:

[!code-csharp[Role States](../code/LocatorsSamples.cs#RoleStates)]

> **Firefox:** `GetByText` uses WebDriver BiDi's `innerText` locator, which Firefox does not yet support ([bug 1869538](https://bugzilla.mozilla.org/show_bug.cgi?id=1869538)). In Firefox, find the element another way, such as by its role and name.

## CSS and XPath

WebDriver BiDi's own locators can be used directly:

[!code-csharp[Protocol Locators](../code/LocatorsSamples.cs#ProtocolLocators)]

`CssLocator`, `XPathLocator`, `InnerTextLocator`, and `AccessibilityLocator` are in the `WebDriverBiDi.BrowsingContext` namespace.

## Chaining and Filtering

A locator's methods create a new locator that searches within what it finds. `Filter` keeps the elements that contain, or do not contain, some text or another element. `And` and `Or` combine two locators:

[!code-csharp[Chaining](../code/LocatorsSamples.cs#Chaining)]

`Filter`'s text conditions compare rendered text, ignoring case and runs of whitespace. Its element conditions search within each element found, so they cost a lookup for each. The locators given to `Filter`, `And`, and `Or` must search the same frame.

## One Element or Many

An action, a read, or an expectation about one element requires its locator to find exactly one, and throws `AmbiguousElementException` at once if it finds several. Narrow the locator, or choose an element:

[!code-csharp[Choosing an Element](../code/LocatorsSamples.cs#Choosing)]

`Nth` counts from zero, in document order. `CountAsync`, `ToHaveCountAsync`, and the expectations about a list of texts work with every element a locator finds.

## Shadow DOM and Frames

`ShadowRoot()` continues a search inside the shadow roots, open or closed, of the elements found. With `PierceShadowRoots` [set](configuration.md), lookups also search open shadow roots automatically. Browsers do not evaluate XPath within a shadow root, and Chrome's role lookups reach every shadow root, closed ones included.

Each frame is searched separately. `ContentFrameAsync` returns the frame of an `iframe` or `frame` element, once its document has loaded:

[!code-csharp[Shadow DOM and Frames](../code/LocatorsSamples.cs#ShadowAndFrames)]

A `Frame` is fixed to one document: if the `iframe`'s document is replaced, the frame reports `IsDetached`, and `ContentFrameAsync` finds the new one.

## Waiting for a State

Actions wait for their element. To wait for an element itself, such as for a spinner to go away, use `WaitForAsync`, or an expectation with `Expect`:

[!code-csharp[Waiting](../code/LocatorsSamples.cs#Waiting)]

`ElementState` is `Attached`, `Detached`, `Visible` (the default), or `Hidden`; an element that does not exist is hidden.
