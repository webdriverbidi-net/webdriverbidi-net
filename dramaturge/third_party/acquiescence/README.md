# Acquiescence

> A powerful TypeScript library for querying and waiting for element states in the browser

[![npm version](https://img.shields.io/npm/v/acquiescence.svg)](https://www.npmjs.com/package/acquiescence)
[![License](https://img.shields.io/badge/License-Apache%202.0-blue.svg)](https://opensource.org/licenses/Apache-2.0)
[![CI](https://github.com/jimevans/acquiescence/actions/workflows/ci.yml/badge.svg)](https://github.com/jimevans/acquiescence/actions/workflows/ci.yml)
[![codecov](https://codecov.io/gh/jimevans/acquiescence/graph/badge.svg?token=8S7FYOCILA)](https://codecov.io/gh/jimevans/acquiescence)

## Overview

**Acquiescence** provides sophisticated element state querying and interaction readiness detection for web applications. It goes far beyond simple existence checks to determine if elements are truly ready for user interaction.

Perfect for:
- 🧪 End-to-end testing frameworks
- 🤖 Browser automation tools
- 🎨 Interactive UI frameworks
- ♿ Accessibility testing tools
- ✅ Any scenario requiring reliable element state detection

## Features

- **🔍 Query Element States** - Check visibility, enabled/disabled state, ability to enter text, viewport position, and more with a simple, intuitive API
- **⏱️ Wait for Interactions** - Automatically wait for elements to be ready for interaction, with built-in stability detection and smart scrolling
- **🎯 Precise Hit Testing** - Determine exact click points and detect element obstruction with accurate hit testing that respects Shadow DOM
- **🚀 TypeScript First** - Built with TypeScript for excellent type safety and IntelliSense support in your IDE
- **♿ Accessibility Snapshots** - Describe a page as assistive technology sees it, in a format compatible with Playwright's aria snapshots, with refs to act on and templates to match
- **📸 DOM Snapshots** - Record a document, with the state of its inputs, scroll positions, and shadow roots, in the format of the snapshots in Playwright's traces
- **🏷️ Element Descriptions** - Describe the element a user acts on by the facts a tool can name it by: role, accessible name, labels, attributes, text, and a CSS path
- **⏺️ Action Recording** - Record the clicks, typing, key presses, choices, and files of a user's real input as actions, for a tool to write down
- **🎯 Element Picking** - Let a user pick an element with the mouse, highlighting the element under the pointer and keeping the page from seeing the click
- **🔎 Find Elements by Text, Label, and ARIA State** - Check which elements contain a text, find elements by their labels, match elements against ARIA states such as checked or expanded, and enumerate the open shadow roots to search
- **🌐 Shadow DOM Support** - Composed tree traversal of open shadow roots; hit testing also works for elements in closed shadow roots
- **⚡ Performance Optimized** - Smart caching of computed styles and efficient polling strategies for minimal performance impact

## Installation

```bash
npm install acquiescence
```

```bash
yarn add acquiescence
```

```bash
pnpm add acquiescence
```

## Quick Start

```typescript
import { ElementStateInspector } from 'acquiescence';

const inspector = new ElementStateInspector();
const button = document.querySelector('#submit-button');

// Check if an element is visible and enabled
const result = await inspector.queryElementStates(button, ['visible', 'enabled']);

if (result.status === 'success') {
  console.log('Button is ready!');
} else {
  console.log(`Button is ${result.missingState}`);
}

// Wait for an element to be ready for interaction
try {
  const hitPoint = await inspector.waitForInteractionReady(
    button,
    'click',
    5000 // 5 second timeout
  );
  console.log(`Element ready at point (${hitPoint.x}, ${hitPoint.y})`);
} catch (error) {
  console.error('Element not ready within timeout');
}
```

## Usage Examples

### Querying Element States

Check various states of DOM elements:

```typescript
const input = document.querySelector('input');

// Check a single state
const visibleResult = await inspector.queryElementState(input, 'visible');
console.log(visibleResult.matches); // true or false

// Check multiple states
const result = await inspector.queryElementStates(
  input,
  ['visible', 'enabled', 'editable']
);

if (result.status === 'success') {
  console.log('Input is ready for typing!');
} else if (result.status === 'failure') {
  console.log(`Input is not ready: ${result.missingState}`);
}
```

### Available Element States

| State | Description |
|-------|-------------|
| `visible` | Element is visible (has positive size, not hidden by CSS) |
| `hidden` | Element is not visible |
| `enabled` | Element is not disabled |
| `disabled` | Element is disabled (via disabled attribute or aria-disabled) |
| `editable` | Element can accept text input (not disabled or readonly) |
| `checked` | Checkbox, radio button, or element with a role allowing aria-checked is checked |
| `unchecked` | Such an element is not checked |
| `indeterminate` | Such an element is in a mixed state |
| `inview` | Element is currently visible in the viewport |
| `notinview` | Element is not in viewport but could be scrolled into view |
| `unviewable` | Element cannot be scrolled into view (hidden by overflow) |
| `stable` | Element's position hasn't changed for at least one animation frame |

### Checking Interaction Readiness

```typescript
const button = document.querySelector('button');
const result = await inspector.isInteractionReady(button, 'click');

if (result.status === 'ready') {
  console.log('Ready to click at:', result.interactionPoint);
  // The same point, as an offset from the element's in-view center
  console.log('Offset from center:', result.interactionOffset);
} else if (result.status === 'needsscroll') {
  console.log('Element needs to be scrolled into view');
} else {
  // For example 'hidden', 'disabled', 'noteditable', or 'obscured by <div class="overlay">'
  console.log('Element is not ready for interaction:', result.reason);
}
```

### Waiting for Interaction Readiness

```typescript
const button = document.querySelector('button');

try {
  const hitPoint = await inspector.waitForInteractionReady(
    button,
    'click',
    5000 // 5 second timeout
  );
  
  console.log(`Ready to click at (${hitPoint.x}, ${hitPoint.y})`);
  // Perform your click action here
} catch (error) {
  console.error('Element not ready within timeout:', error.message);
}
```

**Note:** `waitForInteractionReady()` automatically scrolls elements into view if they're not currently visible in the viewport.

### Supported Interaction Types

- `click` - Single click
- `doubleclick` - Double click
- `hover` - Mouse hover
- `drag` - Drag operation
- `drop` - Drop operation
- `type` - Text input
- `clear` - Clear input field
- `screenshot` - Screenshot capture

### Helper Methods

For simple checks, use the helper methods:

```typescript
const element = document.querySelector('.my-element');

// Check visibility
if (inspector.isElementVisible(element)) {
  console.log('Element is visible');
}

// Check if disabled
if (inspector.isElementDisabled(element)) {
  console.log('Element is disabled');
}

// Check if read-only
const readOnly = inspector.isElementReadOnly(element);
if (readOnly === true) {
  console.log('Element is read-only');
} else if (readOnly === false) {
  console.log('Element is editable');
}
```

### Accessibility Snapshots

Take a snapshot of part of a page: each element with a role, its accessible name and states, and the text between them, in a format compatible with [Playwright's aria snapshots](https://playwright.dev/docs/aria-snapshots).

```typescript
import { AriaSnapshotGenerator, AriaSnapshotMatcher } from 'acquiescence';

const generator = new AriaSnapshotGenerator();
const snapshot = generator.generate(document.body);
console.log(snapshot.text);
// - navigation "Main" [ref=e3]:
//   - link "Home" [ref=e4]:
//     - /url: /
// ...
// - button "Sign in" [ref=e10]

// Each ref leads back to its element.
const button = snapshot.references.find((reference) => reference.ref === 'e10')?.element;

// Match a snapshot against a template.
const matcher = new AriaSnapshotMatcher();
const result = matcher.match(document.body, `
  - navigation "Main":
    - link "Home"
`);
console.log(result.matches); // true
```

Acquiescence computes accessible names itself, following the Accessible Name and Description Computation and the HTML Accessibility API Mappings. Browsers compute names in their own accessibility engines, which pages cannot read, so a snapshot's names can differ from what the browser reports, for example with CSS generated content or controls embedded in labels. To act on an element from a snapshot, use its ref.

See the [Accessibility Snapshots guide](https://jimevans.github.io/acquiescence/guide/accessibility-snapshots) for the format, refs, and template syntax.

### DOM Snapshots

Record a document for a trace viewer to show later, in the format of the frame snapshots in [Playwright's traces](https://playwright.dev/docs/trace-viewer): its elements and text without scripts or event handlers, with the values of inputs, scroll positions, open shadow roots, and the stylesheets script made.

```typescript
import { DomSnapshotGenerator } from 'acquiescence';

const generator = new DomSnapshotGenerator();
const snapshot = generator.generate(document, { target: document.querySelector('#save') ?? undefined });
console.log(snapshot.html); // ['HTML', {}, ['HEAD', ...], ['BODY', ...]]
```

See the [DOM Snapshots guide](https://jimevans.github.io/acquiescence/guide/dom-snapshots) for the format and the state recorded.

### Element Descriptions

Describe the element a user acts on, such as for writing a locator for it: its role, accessible name, labels, attributes (including `name` and `type`), text, and a CSS path, and the same for its nameable ancestors.

```typescript
import { ElementDescriber } from 'acquiescence';

const description = new ElementDescriber().describe(clickedElement);
console.log(description.target.role, description.target.name, description.target.cssPath);
```

See the [Element Descriptions guide](https://jimevans.github.io/acquiescence/guide/element-descriptions).

### Action Recording

Record the actions a user takes in a document from the browser's events for real input: clicks, checkboxes checked, text entered, keys pressed, options selected, and files chosen.

```typescript
import { ActionRecorder } from 'acquiescence';

const recorder = new ActionRecorder((action) => console.log(action.kind, action.element));
recorder.start(document);
```

See the [Action Recording guide](https://jimevans.github.io/acquiescence/guide/action-recording).

### Element Picking

Let a user pick an element with the mouse: the element under the pointer is highlighted, and the one clicked is reported without the page seeing the click.

```typescript
import { ElementPicker } from 'acquiescence';

const picker = new ElementPicker((element) => console.log(element));
picker.start(document);
```

See the [Element Picking guide](https://jimevans.github.io/acquiescence/guide/element-picking).

## Browser Support

Acquiescence can be used in both Node.js environments (with jsdom) and directly in the browser.

### Browser Bundle

For direct browser usage, a bundled version is available:

```html
<script src="node_modules/acquiescence/dist/acquiescence.browser.js"></script>
<script>
  const inspector = new Acquiescence.ElementStateInspector();
  // Use the inspector
</script>
```

Every export is a property of the `Acquiescence` global: `ElementStateInspector`, `AriaSnapshotGenerator`, `AriaSnapshotMatcher`, `DomSnapshotGenerator`, `ElementDescriber`, `ActionRecorder`, `ElementPicker`, `TimeoutWaiter`, and `RequestAnimationFrameWaiter`.

## API Reference

### `ElementStateInspector`

The main class for querying element states and waiting for interactions.

#### Methods

- `queryElementState(element, state)` - Check a single element state
- `queryElementStates(element, states)` - Check multiple element states
- `isInteractionReady(element, interactionType, hitPointOffset?)` - Check if element is ready for interaction
- `waitForInteractionReady(element, interactionType, timeoutInMilliseconds, hitPointOffset?)` - Wait for element to be ready
- `isElementVisible(element)` - Helper to check visibility
- `isElementDisabled(element)` - Helper to check disabled state
- `isElementReadOnly(element)` - Helper to check read-only state
- `getElementInViewPortRect(element)` - Get the element's bounding rectangle in the viewport, or `undefined` if it is not in the viewport
- `isElementInViewPort(element)` - Check if the element is in the viewport
- `isElementScrollable(element)` - Check if the element can be scrolled into view
- `getElementClickPoint(element, offset?)` - Get a click point of the element, or an error message if it is not in the viewport or is obscured
- `elementsContainText(elements, text)` - Check, for each element, whether its rendered text contains a string
- `findOpenShadowRoots(scopes)` - Find the open shadow roots within some scopes, including nested ones
- `elementsMatchAriaStates(elements, states)` - Check, for each element, whether it has every given ARIA state
- `findElementsByLabel(scopes, text, exact)` - Find the elements within some scopes whose labels match a text
- `getElementLabels(element)` - Get the texts of an element's labels

### `AriaSnapshotGenerator`

- `generate(element, options?)` - Take an accessibility snapshot of an element and its descendants; returns its `text`, its tree (`root`), and its `references`

### `AriaSnapshotMatcher`

- `match(element, template)` - Match the snapshot of an element against a template; returns `{ matches, actual }`

### `DomSnapshotGenerator`

- `generate(document, options?)` - Take a snapshot of a document for a trace viewer; returns its `html` tree, `doctype`, `viewport`, `url`, and timings

### `ElementDescriber`

- `describe(element, options?)` - Describe the element a user acting on an element acts on, and its nameable ancestors
- `getActionTarget(element)` - Get the element a user acting on an element acts on

### `ActionRecorder`

- `constructor(report, options?)` - Create a recorder that reports each action to `report`; `options.ignore` leaves out events aimed at elements it returns true for
- `start(document)` - Start recording a document's actions
- `stop()` - Stop recording

### `ElementPicker`

- `constructor(pick, options?)` - Create a picker that reports each element picked to `pick`; `options.resolve` gives the element highlighted and picked for the element under the pointer, and `options.ignore` leaves out elements it returns true for
- `start(document)` - Start picking in a document, adding the highlight
- `stop()` - Stop picking, removing the highlight

### `TimeoutWaiter` and `RequestAnimationFrameWaiter`

Waiters that poll a condition until it returns a truthy result: `TimeoutWaiter` on a timer, `RequestAnimationFrameWaiter` on every animation frame.

- `new TimeoutWaiter(condition, timeoutInMilliseconds = 0, pollIntervalsInMilliseconds = [100])` - Create a waiter; checks after the first are spaced by the intervals in turn, and the last interval repeats
- `new RequestAnimationFrameWaiter(condition, timeoutInMilliseconds = 0)` - Create a waiter
- `waitForCondition()` - Resolve with the first truthy result of the condition; reject with `Timeout after Nms` when the timeout passes, or `Wait cancelled` when cancelled. An exception from the condition is ignored, and the condition is checked again. A timeout of 0 checks the condition once.
- `cancel()` - Cancel the wait

For complete API documentation, see the [full API reference](https://jimevans.github.io/acquiescence/api/).

## Documentation

For more detailed documentation, guides, and examples, visit:

📚 **[Full Documentation](https://jimevans.github.io/acquiescence/)**

- [Getting Started Guide](https://jimevans.github.io/acquiescence/guide/getting-started)
- [Element States Guide](https://jimevans.github.io/acquiescence/guide/element-states)
- [Interaction Types Guide](https://jimevans.github.io/acquiescence/guide/interactions)
- [Stability Detection Guide](https://jimevans.github.io/acquiescence/guide/stability)
- [Accessibility Snapshots Guide](https://jimevans.github.io/acquiescence/guide/accessibility-snapshots)
- [DOM Snapshots Guide](https://jimevans.github.io/acquiescence/guide/dom-snapshots)
- [Element Descriptions Guide](https://jimevans.github.io/acquiescence/guide/element-descriptions)
- [Action Recording Guide](https://jimevans.github.io/acquiescence/guide/action-recording)
- [Element Picking Guide](https://jimevans.github.io/acquiescence/guide/element-picking)
- [Best Practices](https://jimevans.github.io/acquiescence/guide/best-practices)
- [API Reference](https://jimevans.github.io/acquiescence/api/)

## Development

### Setup

```bash
# Clone the repository
git clone https://github.com/jimevans/acquiescence.git
cd acquiescence

# Install dependencies
npm install

# Build the project
npm run build

# Run tests
npm test

# Run tests with coverage
npm run test:coverage

# Build documentation
npm run docs:build

# Serve documentation locally
npm run docs:dev
```

### Scripts

- `npm run build` - Build the TypeScript project
- `npm run build:browser` - Build the browser bundle
- `npm test` - Run tests
- `npm run test:watch` - Run tests in watch mode
- `npm run test:coverage` - Run tests with coverage
- `npm run lint` - Run linter
- `npm run lint:fix` - Fix linting issues
- `npm run docs:dev` - Start documentation dev server
- `npm run docs:build` - Build documentation

## Contributing

Contributions are welcome! Please feel free to submit a Pull Request.

1. Fork the repository
2. Create your feature branch (`git checkout -b feature/amazing-feature`)
3. Commit your changes (`git commit -m 'Add some amazing feature'`)
4. Push to the branch (`git push origin feature/amazing-feature`)
5. Open a Pull Request

## License

This project is licensed under the Apache License 2.0 - see the [LICENSE](LICENSE) file for details.

## Why "Acquiescence"?

**Acquiescence** means "the reluctant acceptance of something without protest" - which perfectly describes what this library helps you do: wait patiently (but efficiently!) for elements to reach the state you need them to be in, without constantly polling or throwing errors.

## Related Projects

- [Selenium](https://selenium.dev/) - Browser automation framework
- [WebdriverIO](https://webdriver.io/) - Browser and mobile automation
- [Puppeteer](https://pptr.dev/) - Chrome DevTools Protocol
- [Testing Library](https://testing-library.com/) - Simple and complete testing utilities

---

Made with ❤️ by the Acquiescence team

