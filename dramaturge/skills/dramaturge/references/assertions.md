# Assertions

Full guide: https://webdriverbidi-net.github.io/dramaturge/articles/assertions.html

`Expect(locator)` and `Expect(page)` (`using static Dramaturge.Assertions;`) retry until the condition holds, for `ExpectTimeout` (5 s) or the matcher's `timeout`, then throw `ExpectationFailedException` with `Expected`, `Actual`, and `Timeout`.

- Element states: `ToBeVisibleAsync`, `ToBeHiddenAsync` (no element is hidden), `ToBeAttachedAsync`, `ToBeEnabledAsync`, `ToBeDisabledAsync`, `ToBeEditableAsync`, `ToBeCheckedAsync`, `ToBeEmptyAsync`, `ToBeFocusedAsync`, `ToBeInViewportAsync`.
- Values: `ToHaveTextAsync`, `ToContainTextAsync`, `ToHaveValueAsync`, `ToHaveAttributeAsync(name[, value])`, `ToHaveIdAsync`, `ToHaveClassAsync` (whole attribute), `ToHaveCssAsync(name, value)`, `ToHaveCountAsync(n)`.
- Page: `ToHaveUrlAsync`, `ToHaveTitleAsync`.
- Accessibility snapshot: `ToMatchAriaSnapshotAsync(template)` on a locator; the template lists nodes in the snapshot format, matched in order among others (`- /children: equal` or `deep-equal` for exact), names and text exactly or as `/regex/`, refs ignored, frames not matched; an invalid template throws `ArgumentException` at once; the failure message shows a line diff, and `Actual` is the snapshot. Guide: https://webdriverbidi-net.github.io/dramaturge/articles/accessibility-snapshots.html
- `Not` waits until the condition stops holding; a missing element satisfies a negated state or text matcher.
- Strings match exactly, `Regex` anywhere (`RegexOptions.IgnoreCase` for case); `ignoreCase: true` on strings.
- Text matchers collapse and trim whitespace on both sides and read `textContent` (hidden text included); `useInnerText: true` reads rendered text. Lists check every match in order: `ToHaveTextAsync([...])` exactly, `ToContainTextAsync([...])` as an ordered subsequence.
- A single-element matcher throws `AmbiguousElementException` on several matches; an inapplicable one (`ToHaveValueAsync` on a paragraph) throws `InvalidOperationException`.
