# Accessibility Snapshots

An accessibility snapshot describes a page the way assistive technology sees it: each element with a role, its accessible name and states, and the text between them. A snapshot is compact enough to read at a glance, which makes it useful for asserting the structure of a page, and for giving a coding agent or other tool a view of a page it can act on.

Snapshots are written in a format compatible with [Playwright's aria snapshots](https://playwright.dev/docs/aria-snapshots). Dramaturge takes them with the [Acquiescence](https://github.com/jimevans/acquiescence) library in the page; its [Accessibility Snapshots guide](https://jimevans.github.io/acquiescence/guide/accessibility-snapshots) describes the format and the template syntax in full.

## Taking a Snapshot

[!code-csharp[Page Snapshot](../code/AriaSnapshotSamples.cs#PageSnapshot)]

For a page with a sign-in form and a consent frame, the snapshot reads:

```yaml
- navigation "Main" [ref=e1]:
  - link "Home" [ref=e2]:
    - /url: index.html
- main [ref=e3]:
  - heading "Sign in" [level=1] [ref=e4]
  - text: Email
  - textbox "Email" [ref=e5]:
    - /placeholder: you@example.com
  - button "Sign in" [ref=e6]
  - iframe [ref=e7]:
    - paragraph [ref=f1e1]: Cookies?
    - button "Accept" [ref=f1e2]
```

Each line is an element with a role, other than `generic`, `none`, or `presentation`; the content of other elements moves up to the nearest element that has a line. Hidden content is left out. A node's states follow its name, in the order `[checked]`, `[disabled]`, `[expanded]`, `[level=N]`, `[pressed]`, `[selected]`, and `[ref=…]`; a link shows its URL and a text box its placeholder.

`Page.AriaSnapshotAsync` takes a snapshot of the page's body. `AriaSnapshotAsync` on a locator takes a snapshot of one element and its descendants, waiting until exactly one element matches:

[!code-csharp[Part of a Page](../code/AriaSnapshotSamples.cs#PartOfAPage)]

| Option | Default | Effect |
| --- | --- | --- |
| `IncludeRefs` | `true` | Gives each node a ref. Without refs, the text is easier to read and compare, and the snapshot has no locators. |
| `IncludeFrames` | `true` | Puts the content of each frame beneath its `iframe` node. Without it, a frame is an `iframe` node with nothing beneath it. |

## Refs

Every node in a snapshot has a ref, such as `e4`, and `Locator` turns a ref into a locator for that element:

[!code-csharp[Acting on a Ref](../code/AriaSnapshotSamples.cs#ActOnRef)]

- **A ref finds exactly the element its line describes**, even when another element has the same role and name.
- **An element keeps its ref** across snapshots of its document, so successive snapshots can be compared line by line: a line with a new ref describes a new element.
- **The refs of a frame's elements begin with the frame's prefix**, such as `f1`, which the frame keeps for the life of the page. The main frame's refs have none.
- **A removed element is not found.** Its locator finds nothing, as any locator does when nothing matches, so `CountAsync` returns 0 and actions wait for it until they time out.
- **An element whose document has gone**, because its frame navigated or closed, can never be found again, so its locator throws `InvalidOperationException` at once.

> **Firefox:** Firefox still finds an element of a document its frame has navigated away from, where the protocol requires an error, so a ref from before a navigation finds the old element instead of throwing.

`Locator` throws `ArgumentException` for a ref the snapshot does not have.

## The Tree

`Root` holds the same content as the text, as `AriaNode` objects: a node with the role `fragment` whose children are the snapshot's nodes.

[!code-csharp[Tree](../code/AriaSnapshotSamples.cs#Tree)]

| Property | Holds |
| --- | --- |
| `Role` | The ARIA role; `iframe` for a frame, `text` for a run of text, or `fragment` for the root |
| `Name` | The accessible name, with white space collapsed; empty if there is none |
| `Text` | The text of a run of text; `null` for other nodes |
| `Ref` | The ref, or `null` without refs |
| `Checked`, `Pressed` | `ToggleState.On`, `Off`, or `Mixed`; `null` if the role does not have the state |
| `Disabled`, `Expanded`, `Selected`, `Level` | The state; `null` if the role does not have it |
| `Url`, `Placeholder` | A link's URL, or a text box's placeholder when it differs from its name |
| `Children` | Child nodes and runs of text, in document order |

A state is present when the node's role supports it, even when it is off, so a plain button has `Pressed` of `ToggleState.Off`; the text shows only states that are on.

## Asserting a Snapshot

`ToMatchAriaSnapshotAsync` waits until an element's snapshot matches a template written in the snapshot format:

[!code-csharp[Matching a Snapshot](../code/AriaSnapshotSamples.cs#MatchSnapshot)]

- **A template lists only what matters.** A template of one node matches if any node of the snapshot matches it. Several nodes must appear in order beneath one node. Children must appear in order, with others between them, unless the template's `/children` property is `equal` or `deep-equal`.
- **A node matches** if it has the template's role, and the name, states, URL, and placeholder the template gives.
- **Names and text** match exactly, after white space is collapsed, or as regular expressions written between slashes, such as `/Orders \(\d+\)/`. Regular expressions follow JavaScript's syntax.
- **Refs are ignored**, so a snapshot's text can be used as a template as it is.
- **Indentation** shared by every line of the template, and blank lines, are ignored.
- **Frames** are not matched: the snapshot covers the element's own document.

A template that is not valid throws `ArgumentException` at once, naming the line and column. An expectation that is not met throws `ExpectationFailedException`, whose message shows how the snapshot differs from the template, and whose `Actual` is the snapshot:

```text
Expected css "nav" to match aria snapshot; the snapshot did not match after 5 seconds.
- expected
+ received

  - navigation "Main":
-   - link "About"
+   - link "Home":
+     - /url: index.html
```

## Snapshot Names and GetByRole

Browsers compute accessible names inside their accessibility engines, which pages cannot read. `GetByRole` asks the browser to find elements by role and name, so it uses the browser's names; a snapshot is taken by a script in the page, which computes names itself, following the [Accessible Name and Description Computation](https://w3c.github.io/accname/) and the [HTML Accessibility API Mappings](https://w3c.github.io/html-aam/). The two agree for common markup, but not always, and a role and name read from a snapshot can find nothing, or another element, with `GetByRole`.

**To act on an element from a snapshot, use its ref**, not `GetByRole` with the role and name the snapshot shows.

Dramaturge's tests check that `GetByRole` finds each named element of a page of common patterns with the role and name its snapshot shows. These are the known differences:

| Pattern | Snapshot | Chrome | Firefox |
| --- | --- | --- | --- |
| A table row | Named from its cells, such as `row "Tea 2"` | Not named from its cells | Not named from its cells |
| A `<figure>` with a `<figcaption>` | Named from the caption | Not named from the caption | Agrees |
| An `<input type="submit">` without a value | `button "Submit"` | Agrees | `"Submit Query"`, as the specification says |
| An `<svg>` without a role | `img`, named from its `<title>` | Agrees | Another role |

Names can also differ for CSS generated content, controls embedded in labels, chains of `aria-labelledby` references, and roles browsers assign differently.

Snapshots call the image role `img`, as Playwright's do; browsers call it `image`, its name since ARIA 1.3. `GetByRole` asks for `img` as `image`, so either name finds images.
