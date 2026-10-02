# Assertions

`Expect` makes an assertion that is checked again until it holds, or until its time runs out. Use it for anything the page may still be changing: after a click, the result may take a moment to appear, and an assertion that reads the page once would fail, or pass, by chance.

`Expect` is a static method of `Assertions`; bring it in with `using static Dramaturge.Assertions;`.

## Elements

[!code-csharp[Element Expectations](../code/AssertionsSamples.cs#ElementExpectations)]

| Matcher | Holds when the element |
| --- | --- |
| `ToBeVisibleAsync`, `ToBeHiddenAsync` | Is visible, or hidden; no element at all is hidden |
| `ToBeAttachedAsync` | Exists in the document |
| `ToBeEnabledAsync`, `ToBeDisabledAsync` | Is enabled, or disabled |
| `ToBeEditableAsync` | Can be edited: neither disabled nor read-only |
| `ToBeCheckedAsync` | Is a checked checkbox or radio button; a mixed checkbox is not checked |
| `ToBeEmptyAsync` | Is an input or text area without a value, or an element with no child elements and only white space |
| `ToBeFocusedAsync` | Has focus in its document or shadow root |
| `ToBeInViewportAsync` | Is at least partly within its frame's viewport |
| `ToHaveTextAsync`, `ToContainTextAsync` | Has, or contains, a text; see [Text](#text) |
| `ToHaveValueAsync` | Is an input, text area, or select with a value |
| `ToHaveAttributeAsync` | Has an attribute, optionally with a value |
| `ToHaveIdAsync`, `ToHaveClassAsync` | Has an `id`, or a whole `class` attribute |
| `ToHaveCssAsync` | Has a computed CSS property value |
| `ToHaveCountAsync` | Is one of exactly the given number of elements the locator finds |

Each matcher that compares a value takes a string, for an exact match, or a `Regex`, which may match anywhere in the value. A matcher about one element throws `AmbiguousElementException` at once if the locator finds several; `ToHaveCountAsync` and the list forms of the text matchers work with every match. A matcher that cannot apply to the element, such as `ToHaveValueAsync` on a paragraph, throws `InvalidOperationException` at once.

## Negation

`Not` waits until the condition stops holding:

[!code-csharp[Negation](../code/AssertionsSamples.cs#Negation)]

No element at all satisfies the negation of a matcher about an element's state or text, so `Not.ToBeVisibleAsync()` passes once the element is gone.

## Text

[!code-csharp[Text Expectations](../code/AssertionsSamples.cs#TextExpectations)]

- **White space** is collapsed and trimmed, in the element's text and in an expected string, so line breaks and indentation in the HTML do not matter. A `Regex` is matched against the collapsed text, unchanged.
- **Case** is compared exactly unless `ignoreCase` is set. For a `Regex`, use `RegexOptions.IgnoreCase`.
- **Hidden text** is included: the matchers read `textContent`. `useInnerText: true` reads the rendered text instead, without what CSS hides.
- **A list of texts** checks every matching element, in order. `ToHaveTextAsync` requires exactly those texts; `ToContainTextAsync` requires each text in a later element than the one before, with other elements allowed between.

`ToHaveValueAsync`, `ToHaveAttributeAsync`, and the other value matchers compare the value as it is, because a value's white space can matter.

## Pages

[!code-csharp[Page Expectations](../code/AssertionsSamples.cs#PageExpectations)]

`ToHaveUrlAsync` follows the URL the browser reports for each navigation, fragment change, and `history` update, without sending a command. `ToHaveTitleAsync` reads the title, with white space collapsed, again after each navigation.

## Timeouts and Failures

An expectation retries for `ExpectTimeout`, 5 seconds unless [configured](configuration.md), or the timeout given to the matcher. When it runs out, it throws `ExpectationFailedException`, which works with any test framework:

[!code-csharp[Failure](../code/AssertionsSamples.cs#Failure)]

`Expected` says what was required, `Actual` what the last check saw (or `null` if no element ever matched), and `Timeout` how long it retried.
