// <copyright file="LocatorAssertions.cs" company="WebDriverBiDi.NET Committers">
// Copyright (c) WebDriverBiDi.NET Committers. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.
// </copyright>

namespace Dramaturge;

using System.Text.RegularExpressions;
using WebDriverBiDi.Script;

/// <summary>
/// Expectations for the elements a locator finds, each checked again until it is met or its time runs out. An
/// expectation about one element throws <see cref="AmbiguousElementException"/> at once if more than one matches.
/// </summary>
public sealed class LocatorAssertions
{
    private readonly ElementLocator locator;
    private readonly bool isNot;

    /// <summary>
    /// Initializes a new instance of the <see cref="LocatorAssertions"/> class.
    /// </summary>
    /// <param name="locator">The locator.</param>
    /// <param name="isNot">A value indicating whether the expectations are negated.</param>
    internal LocatorAssertions(ElementLocator locator, bool isNot)
    {
        this.locator = locator;
        this.isNot = isNot;
    }

    /// <summary>
    /// Gets the negated expectations, each met when its condition does not hold.
    /// </summary>
    public LocatorAssertions Not => new(this.locator, !this.isNot);

    /// <summary>
    /// Expects the element to be visible.
    /// </summary>
    /// <param name="timeout">The time to retry, or <see langword="null"/> for <see cref="AutomationOptions.ExpectTimeout"/>.</param>
    /// <param name="cancellationToken">A token that cancels the expectation.</param>
    /// <returns>A task that completes when the expectation is met.</returns>
    /// <exception cref="AmbiguousElementException">Thrown when more than one element matches.</exception>
    /// <exception cref="ExpectationFailedException">Thrown when the expectation is not met in time.</exception>
    public Task ToBeVisibleAsync(TimeSpan? timeout = null, CancellationToken cancellationToken = default)
    {
        return this.ExpectElementAsync("to be visible", false, timeout, cancellationToken, this.ObserveVisibilityAsync(true));
    }

    /// <summary>
    /// Expects the element to be hidden. No matching element is hidden.
    /// </summary>
    /// <param name="timeout">The time to retry, or <see langword="null"/> for <see cref="AutomationOptions.ExpectTimeout"/>.</param>
    /// <param name="cancellationToken">A token that cancels the expectation.</param>
    /// <returns>A task that completes when the expectation is met.</returns>
    /// <exception cref="AmbiguousElementException">Thrown when more than one element matches.</exception>
    /// <exception cref="ExpectationFailedException">Thrown when the expectation is not met in time.</exception>
    public Task ToBeHiddenAsync(TimeSpan? timeout = null, CancellationToken cancellationToken = default)
    {
        return this.ExpectElementAsync("to be hidden", true, timeout, cancellationToken, this.ObserveVisibilityAsync(false));
    }

    /// <summary>
    /// Expects an element to match.
    /// </summary>
    /// <param name="timeout">The time to retry, or <see langword="null"/> for <see cref="AutomationOptions.ExpectTimeout"/>.</param>
    /// <param name="cancellationToken">A token that cancels the expectation.</param>
    /// <returns>A task that completes when the expectation is met.</returns>
    /// <exception cref="AmbiguousElementException">Thrown when more than one element matches.</exception>
    /// <exception cref="ExpectationFailedException">Thrown when the expectation is not met in time.</exception>
    public Task ToBeAttachedAsync(TimeSpan? timeout = null, CancellationToken cancellationToken = default)
    {
        return this.ExpectElementAsync("to be attached", false, timeout, cancellationToken, (_, _) => Task.FromResult<Observation?>(new Observation(true, "attached")));
    }

    /// <summary>
    /// Expects the element to be enabled.
    /// </summary>
    /// <param name="timeout">The time to retry, or <see langword="null"/> for <see cref="AutomationOptions.ExpectTimeout"/>.</param>
    /// <param name="cancellationToken">A token that cancels the expectation.</param>
    /// <returns>A task that completes when the expectation is met.</returns>
    /// <exception cref="AmbiguousElementException">Thrown when more than one element matches.</exception>
    /// <exception cref="ExpectationFailedException">Thrown when the expectation is not met in time.</exception>
    public Task ToBeEnabledAsync(TimeSpan? timeout = null, CancellationToken cancellationToken = default)
    {
        return this.ExpectStateAsync("to be enabled", "enabled", "enabled", timeout, cancellationToken);
    }

    /// <summary>
    /// Expects the element to be disabled.
    /// </summary>
    /// <param name="timeout">The time to retry, or <see langword="null"/> for <see cref="AutomationOptions.ExpectTimeout"/>.</param>
    /// <param name="cancellationToken">A token that cancels the expectation.</param>
    /// <returns>A task that completes when the expectation is met.</returns>
    /// <exception cref="AmbiguousElementException">Thrown when more than one element matches.</exception>
    /// <exception cref="ExpectationFailedException">Thrown when the expectation is not met in time.</exception>
    public Task ToBeDisabledAsync(TimeSpan? timeout = null, CancellationToken cancellationToken = default)
    {
        return this.ExpectStateAsync("to be disabled", "enabled", "disabled", timeout, cancellationToken);
    }

    /// <summary>
    /// Expects the element to be editable, being neither disabled nor read-only.
    /// </summary>
    /// <param name="timeout">The time to retry, or <see langword="null"/> for <see cref="AutomationOptions.ExpectTimeout"/>.</param>
    /// <param name="cancellationToken">A token that cancels the expectation.</param>
    /// <returns>A task that completes when the expectation is met.</returns>
    /// <exception cref="AmbiguousElementException">Thrown when more than one element matches.</exception>
    /// <exception cref="InvalidOperationException">Thrown when the element is not an input, a text area, a select, or an editable element.</exception>
    /// <exception cref="ExpectationFailedException">Thrown when the expectation is not met in time.</exception>
    public Task ToBeEditableAsync(TimeSpan? timeout = null, CancellationToken cancellationToken = default)
    {
        return this.ExpectStateAsync("to be editable", "editable", "editable", timeout, cancellationToken);
    }

    /// <summary>
    /// Expects a checkbox or radio button to be checked. An element in a mixed state is not checked.
    /// </summary>
    /// <param name="timeout">The time to retry, or <see langword="null"/> for <see cref="AutomationOptions.ExpectTimeout"/>.</param>
    /// <param name="cancellationToken">A token that cancels the expectation.</param>
    /// <returns>A task that completes when the expectation is met.</returns>
    /// <exception cref="AmbiguousElementException">Thrown when more than one element matches.</exception>
    /// <exception cref="InvalidOperationException">Thrown when the element cannot be checked.</exception>
    /// <exception cref="ExpectationFailedException">Thrown when the expectation is not met in time.</exception>
    public Task ToBeCheckedAsync(TimeSpan? timeout = null, CancellationToken cancellationToken = default)
    {
        return this.ExpectStateAsync("to be checked", "checked", "checked", timeout, cancellationToken);
    }

    /// <summary>
    /// Expects the element to be empty: an input or a text area without a value, or another element with no child
    /// elements and only white space for text.
    /// </summary>
    /// <param name="timeout">The time to retry, or <see langword="null"/> for <see cref="AutomationOptions.ExpectTimeout"/>.</param>
    /// <param name="cancellationToken">A token that cancels the expectation.</param>
    /// <returns>A task that completes when the expectation is met.</returns>
    /// <exception cref="AmbiguousElementException">Thrown when more than one element matches.</exception>
    /// <exception cref="ExpectationFailedException">Thrown when the expectation is not met in time.</exception>
    public Task ToBeEmptyAsync(TimeSpan? timeout = null, CancellationToken cancellationToken = default)
    {
        return this.ExpectElementAsync("to be empty", false, timeout, cancellationToken, async (node, budget) =>
        {
            bool empty = (await this.locator.CallStateAsync(node, "(state, element) => state.isEmpty(element)", budget).ConfigureAwait(false)).As<BooleanRemoteValue>().Value;
            return new Observation(empty, empty ? "empty" : "not empty");
        });
    }

    /// <summary>
    /// Expects the element to have focus within its document.
    /// </summary>
    /// <param name="timeout">The time to retry, or <see langword="null"/> for <see cref="AutomationOptions.ExpectTimeout"/>.</param>
    /// <param name="cancellationToken">A token that cancels the expectation.</param>
    /// <returns>A task that completes when the expectation is met.</returns>
    /// <exception cref="AmbiguousElementException">Thrown when more than one element matches.</exception>
    /// <exception cref="ExpectationFailedException">Thrown when the expectation is not met in time.</exception>
    public Task ToBeFocusedAsync(TimeSpan? timeout = null, CancellationToken cancellationToken = default)
    {
        return this.ExpectElementAsync("to be focused", false, timeout, cancellationToken, async (node, budget) =>
        {
            bool focused = (await this.locator.CallStateAsync(node, "(state, element) => state.isFocused(element)", budget).ConfigureAwait(false)).As<BooleanRemoteValue>().Value;
            return new Observation(focused, focused ? "focused" : "not focused");
        });
    }

    /// <summary>
    /// Expects the element to be at least partly within its frame's viewport.
    /// </summary>
    /// <param name="timeout">The time to retry, or <see langword="null"/> for <see cref="AutomationOptions.ExpectTimeout"/>.</param>
    /// <param name="cancellationToken">A token that cancels the expectation.</param>
    /// <returns>A task that completes when the expectation is met.</returns>
    /// <exception cref="AmbiguousElementException">Thrown when more than one element matches.</exception>
    /// <exception cref="ExpectationFailedException">Thrown when the expectation is not met in time.</exception>
    public Task ToBeInViewportAsync(TimeSpan? timeout = null, CancellationToken cancellationToken = default)
    {
        return this.ExpectElementAsync("to be in the viewport", false, timeout, cancellationToken, async (node, budget) =>
        {
            bool inViewport = (await this.locator.CallInspectorAsync(node, "(inspector, element) => inspector.isElementInViewPort(element)", budget).ConfigureAwait(false)).As<BooleanRemoteValue>().Value;
            return new Observation(inViewport, inViewport ? "in the viewport" : "outside the viewport");
        });
    }

    /// <summary>
    /// Expects the locator to match a number of elements.
    /// </summary>
    /// <param name="count">The number of elements.</param>
    /// <param name="timeout">The time to retry, or <see langword="null"/> for <see cref="AutomationOptions.ExpectTimeout"/>.</param>
    /// <param name="cancellationToken">A token that cancels the expectation.</param>
    /// <returns>A task that completes when the expectation is met.</returns>
    /// <exception cref="ExpectationFailedException">Thrown when the expectation is not met in time.</exception>
    public Task ToHaveCountAsync(int count, TimeSpan? timeout = null, CancellationToken cancellationToken = default)
    {
        return this.locator.ExpectAsync(this.Describe($"to have count {count}"), this.isNot, timeout, cancellationToken, async budget =>
        {
            int matched = (await this.locator.FindAllAsync(budget).ConfigureAwait(false)).Count;
            return new Observation(matched == count, matched.ToString(System.Globalization.CultureInfo.InvariantCulture));
        });
    }

    /// <summary>
    /// Expects the element's text, with white space normalized, to be a string.
    /// </summary>
    /// <param name="expected">The text; its white space is normalized too.</param>
    /// <param name="ignoreCase">A value indicating whether case is ignored.</param>
    /// <param name="useInnerText">A value indicating whether to read the rendered text rather than the text content.</param>
    /// <param name="timeout">The time to retry, or <see langword="null"/> for <see cref="AutomationOptions.ExpectTimeout"/>.</param>
    /// <param name="cancellationToken">A token that cancels the expectation.</param>
    /// <returns>A task that completes when the expectation is met.</returns>
    /// <exception cref="AmbiguousElementException">Thrown when more than one element matches.</exception>
    /// <exception cref="InvalidOperationException">Thrown when <paramref name="useInnerText"/> is set and an element is not an HTML element.</exception>
    /// <exception cref="ExpectationFailedException">Thrown when the expectation is not met in time.</exception>
    public Task ToHaveTextAsync(string expected, bool ignoreCase = false, bool useInnerText = false, TimeSpan? timeout = null, CancellationToken cancellationToken = default)
    {
        return this.ExpectTextAsync("to have text", TextPattern.For(TextPattern.Normalize(expected), ignoreCase), useInnerText, timeout, cancellationToken);
    }

    /// <summary>
    /// Expects the element's text, with white space normalized, to match a regular expression.
    /// </summary>
    /// <param name="expected">The regular expression, which may match anywhere in the text.</param>
    /// <param name="useInnerText">A value indicating whether to read the rendered text rather than the text content.</param>
    /// <param name="timeout">The time to retry, or <see langword="null"/> for <see cref="AutomationOptions.ExpectTimeout"/>.</param>
    /// <param name="cancellationToken">A token that cancels the expectation.</param>
    /// <returns>A task that completes when the expectation is met.</returns>
    /// <exception cref="AmbiguousElementException">Thrown when more than one element matches.</exception>
    /// <exception cref="InvalidOperationException">Thrown when <paramref name="useInnerText"/> is set and an element is not an HTML element.</exception>
    /// <exception cref="ExpectationFailedException">Thrown when the expectation is not met in time.</exception>
    public Task ToHaveTextAsync(Regex expected, bool useInnerText = false, TimeSpan? timeout = null, CancellationToken cancellationToken = default)
    {
        return this.ExpectTextAsync("to have text", TextPattern.For(expected), useInnerText, timeout, cancellationToken);
    }

    /// <summary>
    /// Expects the texts of every matching element, with white space normalized, to be a list of strings, in
    /// order.
    /// </summary>
    /// <param name="expected">The texts; their white space is normalized too.</param>
    /// <param name="ignoreCase">A value indicating whether case is ignored.</param>
    /// <param name="useInnerText">A value indicating whether to read the rendered text rather than the text content.</param>
    /// <param name="timeout">The time to retry, or <see langword="null"/> for <see cref="AutomationOptions.ExpectTimeout"/>.</param>
    /// <param name="cancellationToken">A token that cancels the expectation.</param>
    /// <returns>A task that completes when the expectation is met.</returns>
    /// <exception cref="InvalidOperationException">Thrown when <paramref name="useInnerText"/> is set and an element is not an HTML element.</exception>
    /// <exception cref="ExpectationFailedException">Thrown when the expectation is not met in time.</exception>
    public Task ToHaveTextAsync(IEnumerable<string> expected, bool ignoreCase = false, bool useInnerText = false, TimeSpan? timeout = null, CancellationToken cancellationToken = default)
    {
        return this.ExpectTextsAsync("to have texts", [.. expected.Select(text => TextPattern.For(TextPattern.Normalize(text), ignoreCase))], false, useInnerText, timeout, cancellationToken);
    }

    /// <summary>
    /// Expects the texts of every matching element, with white space normalized, to match a list of regular
    /// expressions, in order.
    /// </summary>
    /// <param name="expected">The regular expressions, each of which may match anywhere in its text.</param>
    /// <param name="useInnerText">A value indicating whether to read the rendered text rather than the text content.</param>
    /// <param name="timeout">The time to retry, or <see langword="null"/> for <see cref="AutomationOptions.ExpectTimeout"/>.</param>
    /// <param name="cancellationToken">A token that cancels the expectation.</param>
    /// <returns>A task that completes when the expectation is met.</returns>
    /// <exception cref="InvalidOperationException">Thrown when <paramref name="useInnerText"/> is set and an element is not an HTML element.</exception>
    /// <exception cref="ExpectationFailedException">Thrown when the expectation is not met in time.</exception>
    public Task ToHaveTextAsync(IEnumerable<Regex> expected, bool useInnerText = false, TimeSpan? timeout = null, CancellationToken cancellationToken = default)
    {
        return this.ExpectTextsAsync("to have texts", [.. expected.Select(TextPattern.For)], false, useInnerText, timeout, cancellationToken);
    }

    /// <summary>
    /// Expects the element's text, with white space normalized, to contain a string.
    /// </summary>
    /// <param name="expected">The text to find; its white space is normalized too.</param>
    /// <param name="ignoreCase">A value indicating whether case is ignored.</param>
    /// <param name="useInnerText">A value indicating whether to read the rendered text rather than the text content.</param>
    /// <param name="timeout">The time to retry, or <see langword="null"/> for <see cref="AutomationOptions.ExpectTimeout"/>.</param>
    /// <param name="cancellationToken">A token that cancels the expectation.</param>
    /// <returns>A task that completes when the expectation is met.</returns>
    /// <exception cref="AmbiguousElementException">Thrown when more than one element matches.</exception>
    /// <exception cref="InvalidOperationException">Thrown when <paramref name="useInnerText"/> is set and an element is not an HTML element.</exception>
    /// <exception cref="ExpectationFailedException">Thrown when the expectation is not met in time.</exception>
    public Task ToContainTextAsync(string expected, bool ignoreCase = false, bool useInnerText = false, TimeSpan? timeout = null, CancellationToken cancellationToken = default)
    {
        return this.ExpectTextAsync("to contain text", TextPattern.For(TextPattern.Normalize(expected), ignoreCase, true), useInnerText, timeout, cancellationToken);
    }

    /// <summary>
    /// Expects the element's text, with white space normalized, to match a regular expression.
    /// </summary>
    /// <param name="expected">The regular expression, which may match anywhere in the text.</param>
    /// <param name="useInnerText">A value indicating whether to read the rendered text rather than the text content.</param>
    /// <param name="timeout">The time to retry, or <see langword="null"/> for <see cref="AutomationOptions.ExpectTimeout"/>.</param>
    /// <param name="cancellationToken">A token that cancels the expectation.</param>
    /// <returns>A task that completes when the expectation is met.</returns>
    /// <exception cref="AmbiguousElementException">Thrown when more than one element matches.</exception>
    /// <exception cref="InvalidOperationException">Thrown when <paramref name="useInnerText"/> is set and an element is not an HTML element.</exception>
    /// <exception cref="ExpectationFailedException">Thrown when the expectation is not met in time.</exception>
    public Task ToContainTextAsync(Regex expected, bool useInnerText = false, TimeSpan? timeout = null, CancellationToken cancellationToken = default)
    {
        return this.ExpectTextAsync("to contain text", TextPattern.For(expected), useInnerText, timeout, cancellationToken);
    }

    /// <summary>
    /// Expects matching elements to contain each of a list of strings, each in a later element than the one
    /// before it, with white space normalized; other elements may come between them.
    /// </summary>
    /// <param name="expected">The texts to find; their white space is normalized too.</param>
    /// <param name="ignoreCase">A value indicating whether case is ignored.</param>
    /// <param name="useInnerText">A value indicating whether to read the rendered text rather than the text content.</param>
    /// <param name="timeout">The time to retry, or <see langword="null"/> for <see cref="AutomationOptions.ExpectTimeout"/>.</param>
    /// <param name="cancellationToken">A token that cancels the expectation.</param>
    /// <returns>A task that completes when the expectation is met.</returns>
    /// <exception cref="InvalidOperationException">Thrown when <paramref name="useInnerText"/> is set and an element is not an HTML element.</exception>
    /// <exception cref="ExpectationFailedException">Thrown when the expectation is not met in time.</exception>
    public Task ToContainTextAsync(IEnumerable<string> expected, bool ignoreCase = false, bool useInnerText = false, TimeSpan? timeout = null, CancellationToken cancellationToken = default)
    {
        return this.ExpectTextsAsync("to contain texts", [.. expected.Select(text => TextPattern.For(TextPattern.Normalize(text), ignoreCase, true))], true, useInnerText, timeout, cancellationToken);
    }

    /// <summary>
    /// Expects matching elements to match each of a list of regular expressions, each in a later element than
    /// the one before it, with white space normalized; other elements may come between them.
    /// </summary>
    /// <param name="expected">The regular expressions.</param>
    /// <param name="useInnerText">A value indicating whether to read the rendered text rather than the text content.</param>
    /// <param name="timeout">The time to retry, or <see langword="null"/> for <see cref="AutomationOptions.ExpectTimeout"/>.</param>
    /// <param name="cancellationToken">A token that cancels the expectation.</param>
    /// <returns>A task that completes when the expectation is met.</returns>
    /// <exception cref="InvalidOperationException">Thrown when <paramref name="useInnerText"/> is set and an element is not an HTML element.</exception>
    /// <exception cref="ExpectationFailedException">Thrown when the expectation is not met in time.</exception>
    public Task ToContainTextAsync(IEnumerable<Regex> expected, bool useInnerText = false, TimeSpan? timeout = null, CancellationToken cancellationToken = default)
    {
        return this.ExpectTextsAsync("to contain texts", [.. expected.Select(TextPattern.For)], true, useInnerText, timeout, cancellationToken);
    }

    /// <summary>
    /// Expects the value of an input, a text area, or a select to be a string.
    /// </summary>
    /// <param name="expected">The value.</param>
    /// <param name="timeout">The time to retry, or <see langword="null"/> for <see cref="AutomationOptions.ExpectTimeout"/>.</param>
    /// <param name="cancellationToken">A token that cancels the expectation.</param>
    /// <returns>A task that completes when the expectation is met.</returns>
    /// <exception cref="AmbiguousElementException">Thrown when more than one element matches.</exception>
    /// <exception cref="InvalidOperationException">Thrown when the element is not an input, a text area, or a select.</exception>
    /// <exception cref="ExpectationFailedException">Thrown when the expectation is not met in time.</exception>
    public Task ToHaveValueAsync(string expected, TimeSpan? timeout = null, CancellationToken cancellationToken = default)
    {
        return this.ExpectValueAsync(TextPattern.For(expected), timeout, cancellationToken);
    }

    /// <summary>
    /// Expects the value of an input, a text area, or a select to match a regular expression.
    /// </summary>
    /// <param name="expected">The regular expression, which may match anywhere in the value.</param>
    /// <param name="timeout">The time to retry, or <see langword="null"/> for <see cref="AutomationOptions.ExpectTimeout"/>.</param>
    /// <param name="cancellationToken">A token that cancels the expectation.</param>
    /// <returns>A task that completes when the expectation is met.</returns>
    /// <exception cref="AmbiguousElementException">Thrown when more than one element matches.</exception>
    /// <exception cref="InvalidOperationException">Thrown when the element is not an input, a text area, or a select.</exception>
    /// <exception cref="ExpectationFailedException">Thrown when the expectation is not met in time.</exception>
    public Task ToHaveValueAsync(Regex expected, TimeSpan? timeout = null, CancellationToken cancellationToken = default)
    {
        return this.ExpectValueAsync(TextPattern.For(expected), timeout, cancellationToken);
    }

    /// <summary>
    /// Expects the element to have an attribute, with any value.
    /// </summary>
    /// <param name="name">The attribute's name.</param>
    /// <param name="timeout">The time to retry, or <see langword="null"/> for <see cref="AutomationOptions.ExpectTimeout"/>.</param>
    /// <param name="cancellationToken">A token that cancels the expectation.</param>
    /// <returns>A task that completes when the expectation is met.</returns>
    /// <exception cref="AmbiguousElementException">Thrown when more than one element matches.</exception>
    /// <exception cref="ExpectationFailedException">Thrown when the expectation is not met in time.</exception>
    public Task ToHaveAttributeAsync(string name, TimeSpan? timeout = null, CancellationToken cancellationToken = default)
    {
        return this.ExpectAttributeAsync($"to have attribute {TextPattern.Quote(name)}", name, null, timeout, cancellationToken);
    }

    /// <summary>
    /// Expects the element to have an attribute whose value is a string.
    /// </summary>
    /// <param name="name">The attribute's name.</param>
    /// <param name="value">The value.</param>
    /// <param name="ignoreCase">A value indicating whether case is ignored.</param>
    /// <param name="timeout">The time to retry, or <see langword="null"/> for <see cref="AutomationOptions.ExpectTimeout"/>.</param>
    /// <param name="cancellationToken">A token that cancels the expectation.</param>
    /// <returns>A task that completes when the expectation is met.</returns>
    /// <exception cref="AmbiguousElementException">Thrown when more than one element matches.</exception>
    /// <exception cref="ExpectationFailedException">Thrown when the expectation is not met in time.</exception>
    public Task ToHaveAttributeAsync(string name, string value, bool ignoreCase = false, TimeSpan? timeout = null, CancellationToken cancellationToken = default)
    {
        TextPattern pattern = TextPattern.For(value, ignoreCase);
        return this.ExpectAttributeAsync($"to have attribute {TextPattern.Quote(name)} with value {pattern.Description}", name, pattern, timeout, cancellationToken);
    }

    /// <summary>
    /// Expects the element to have an attribute whose value matches a regular expression.
    /// </summary>
    /// <param name="name">The attribute's name.</param>
    /// <param name="value">The regular expression, which may match anywhere in the value.</param>
    /// <param name="timeout">The time to retry, or <see langword="null"/> for <see cref="AutomationOptions.ExpectTimeout"/>.</param>
    /// <param name="cancellationToken">A token that cancels the expectation.</param>
    /// <returns>A task that completes when the expectation is met.</returns>
    /// <exception cref="AmbiguousElementException">Thrown when more than one element matches.</exception>
    /// <exception cref="ExpectationFailedException">Thrown when the expectation is not met in time.</exception>
    public Task ToHaveAttributeAsync(string name, Regex value, TimeSpan? timeout = null, CancellationToken cancellationToken = default)
    {
        TextPattern pattern = TextPattern.For(value);
        return this.ExpectAttributeAsync($"to have attribute {TextPattern.Quote(name)} with value {pattern.Description}", name, pattern, timeout, cancellationToken);
    }

    /// <summary>
    /// Expects the element's id attribute to be a string.
    /// </summary>
    /// <param name="expected">The value.</param>
    /// <param name="timeout">The time to retry, or <see langword="null"/> for <see cref="AutomationOptions.ExpectTimeout"/>.</param>
    /// <param name="cancellationToken">A token that cancels the expectation.</param>
    /// <returns>A task that completes when the expectation is met.</returns>
    /// <exception cref="AmbiguousElementException">Thrown when more than one element matches.</exception>
    /// <exception cref="ExpectationFailedException">Thrown when the expectation is not met in time.</exception>
    public Task ToHaveIdAsync(string expected, TimeSpan? timeout = null, CancellationToken cancellationToken = default)
    {
        TextPattern pattern = TextPattern.For(expected);
        return this.ExpectAttributeAsync($"to have id {pattern.Description}", "id", pattern, timeout, cancellationToken);
    }

    /// <summary>
    /// Expects the element's id attribute to match a regular expression.
    /// </summary>
    /// <param name="expected">The regular expression, which may match anywhere in the value.</param>
    /// <param name="timeout">The time to retry, or <see langword="null"/> for <see cref="AutomationOptions.ExpectTimeout"/>.</param>
    /// <param name="cancellationToken">A token that cancels the expectation.</param>
    /// <returns>A task that completes when the expectation is met.</returns>
    /// <exception cref="AmbiguousElementException">Thrown when more than one element matches.</exception>
    /// <exception cref="ExpectationFailedException">Thrown when the expectation is not met in time.</exception>
    public Task ToHaveIdAsync(Regex expected, TimeSpan? timeout = null, CancellationToken cancellationToken = default)
    {
        TextPattern pattern = TextPattern.For(expected);
        return this.ExpectAttributeAsync($"to have id {pattern.Description}", "id", pattern, timeout, cancellationToken);
    }

    /// <summary>
    /// Expects the element's whole class attribute to be a string.
    /// </summary>
    /// <param name="expected">The value.</param>
    /// <param name="timeout">The time to retry, or <see langword="null"/> for <see cref="AutomationOptions.ExpectTimeout"/>.</param>
    /// <param name="cancellationToken">A token that cancels the expectation.</param>
    /// <returns>A task that completes when the expectation is met.</returns>
    /// <exception cref="AmbiguousElementException">Thrown when more than one element matches.</exception>
    /// <exception cref="ExpectationFailedException">Thrown when the expectation is not met in time.</exception>
    public Task ToHaveClassAsync(string expected, TimeSpan? timeout = null, CancellationToken cancellationToken = default)
    {
        TextPattern pattern = TextPattern.For(expected);
        return this.ExpectAttributeAsync($"to have class {pattern.Description}", "class", pattern, timeout, cancellationToken);
    }

    /// <summary>
    /// Expects the element's class attribute to match a regular expression.
    /// </summary>
    /// <param name="expected">The regular expression, which may match anywhere in the value.</param>
    /// <param name="timeout">The time to retry, or <see langword="null"/> for <see cref="AutomationOptions.ExpectTimeout"/>.</param>
    /// <param name="cancellationToken">A token that cancels the expectation.</param>
    /// <returns>A task that completes when the expectation is met.</returns>
    /// <exception cref="AmbiguousElementException">Thrown when more than one element matches.</exception>
    /// <exception cref="ExpectationFailedException">Thrown when the expectation is not met in time.</exception>
    public Task ToHaveClassAsync(Regex expected, TimeSpan? timeout = null, CancellationToken cancellationToken = default)
    {
        TextPattern pattern = TextPattern.For(expected);
        return this.ExpectAttributeAsync($"to have class {pattern.Description}", "class", pattern, timeout, cancellationToken);
    }

    /// <summary>
    /// Expects the computed value of a CSS property of the element to be a string.
    /// </summary>
    /// <param name="name">The property's name, such as <c>color</c>.</param>
    /// <param name="value">The computed value, such as <c>rgb(0, 0, 0)</c>.</param>
    /// <param name="timeout">The time to retry, or <see langword="null"/> for <see cref="AutomationOptions.ExpectTimeout"/>.</param>
    /// <param name="cancellationToken">A token that cancels the expectation.</param>
    /// <returns>A task that completes when the expectation is met.</returns>
    /// <exception cref="AmbiguousElementException">Thrown when more than one element matches.</exception>
    /// <exception cref="ExpectationFailedException">Thrown when the expectation is not met in time.</exception>
    public Task ToHaveCssAsync(string name, string value, TimeSpan? timeout = null, CancellationToken cancellationToken = default)
    {
        return this.ExpectCssAsync(name, TextPattern.For(value), timeout, cancellationToken);
    }

    /// <summary>
    /// Expects the computed value of a CSS property of the element to match a regular expression.
    /// </summary>
    /// <param name="name">The property's name, such as <c>color</c>.</param>
    /// <param name="value">The regular expression, which may match anywhere in the value.</param>
    /// <param name="timeout">The time to retry, or <see langword="null"/> for <see cref="AutomationOptions.ExpectTimeout"/>.</param>
    /// <param name="cancellationToken">A token that cancels the expectation.</param>
    /// <returns>A task that completes when the expectation is met.</returns>
    /// <exception cref="AmbiguousElementException">Thrown when more than one element matches.</exception>
    /// <exception cref="ExpectationFailedException">Thrown when the expectation is not met in time.</exception>
    public Task ToHaveCssAsync(string name, Regex value, TimeSpan? timeout = null, CancellationToken cancellationToken = default)
    {
        return this.ExpectCssAsync(name, TextPattern.For(value), timeout, cancellationToken);
    }

    // Each pattern must match a later text than the one before it.
    private static bool ContainsInOrder(IReadOnlyList<string> texts, List<TextPattern> patterns)
    {
        int matched = 0;
        foreach (string text in texts)
        {
            if (matched < patterns.Count && patterns[matched].Matches(text))
            {
                matched++;
            }
        }

        return matched == patterns.Count;
    }

    private string Describe(string expected)
    {
        return this.isNot ? $"not {expected}" : expected;
    }

    private Task ExpectElementAsync(string expected, bool holdsWithoutElement, TimeSpan? timeout, CancellationToken cancellationToken, Func<NodeRemoteValue, TimeBudget, Task<Observation?>> observe)
    {
        return this.locator.ExpectAsync(this.Describe(expected), this.isNot, timeout, cancellationToken, async budget =>
        {
            NodeRemoteValue? node = await this.locator.FindOneAsync(budget).ConfigureAwait(false);
            return node is null ? new Observation(holdsWithoutElement, null) : await observe(node, budget).ConfigureAwait(false);
        });
    }

    private Func<NodeRemoteValue, TimeBudget, Task<Observation?>> ObserveVisibilityAsync(bool visible)
    {
        return async (node, budget) =>
        {
            bool isVisible = await this.locator.IsVisibleAsync(node, budget).ConfigureAwait(false);
            return new Observation(isVisible == visible, isVisible ? "visible" : "hidden");
        };
    }

    // An element removed while it is checked is checked again.
    private Task ExpectStateAsync(string expected, string state, string holdsWhen, TimeSpan? timeout, CancellationToken cancellationToken)
    {
        return this.ExpectElementAsync(expected, false, timeout, cancellationToken, async (node, budget) =>
        {
            string? received = await this.locator.ReadElementStateAsync(node, state, budget).ConfigureAwait(false);
            return received is null ? null : new Observation(received == holdsWhen, received == "readOnly" ? "read-only" : received);
        });
    }

    private Task ExpectTextAsync(string expected, TextPattern pattern, bool useInnerText, TimeSpan? timeout, CancellationToken cancellationToken)
    {
        return this.ExpectElementAsync($"{expected} {pattern.Description}", false, timeout, cancellationToken, async (node, budget) =>
        {
            string text = (await this.ReadTextsAsync([node], useInnerText, budget).ConfigureAwait(false))[0];
            return new Observation(pattern.Matches(text), TextPattern.Quote(text));
        });
    }

    // Not strict: every matching element is read, in one call.
    private Task ExpectTextsAsync(string expected, List<TextPattern> patterns, bool contain, bool useInnerText, TimeSpan? timeout, CancellationToken cancellationToken)
    {
        string description = $"{expected} [{string.Join(", ", patterns.Select(pattern => pattern.Description))}]";
        return this.locator.ExpectAsync(this.Describe(description), this.isNot, timeout, cancellationToken, async budget =>
        {
            IList<NodeRemoteValue> nodes = await this.locator.FindAllAsync(budget).ConfigureAwait(false);
            IReadOnlyList<string> texts = nodes.Count == 0 ? [] : await this.ReadTextsAsync(nodes, useInnerText, budget).ConfigureAwait(false);
            bool holds = contain ? ContainsInOrder(texts, patterns) : texts.Count == patterns.Count && patterns.Select((pattern, index) => pattern.Matches(texts[index])).All(matches => matches);
            return new Observation(holds, $"[{string.Join(", ", texts.Select(TextPattern.Quote))}]");
        });
    }

    private async Task<IReadOnlyList<string>> ReadTextsAsync(IList<NodeRemoteValue> nodes, bool useInnerText, TimeBudget budget)
    {
        IReadOnlyList<string?> texts = await this.locator.ReadTextsAsync(nodes, useInnerText, budget).ConfigureAwait(false);
        return [.. texts.Select(text => TextPattern.Normalize(text ?? throw new InvalidOperationException($"{this.locator} matches an element that is not an HTML element.")))];
    }

    private Task ExpectValueAsync(TextPattern pattern, TimeSpan? timeout, CancellationToken cancellationToken)
    {
        return this.ExpectElementAsync($"to have value {pattern.Description}", false, timeout, cancellationToken, async (node, budget) =>
        {
            string value = await this.locator.ReadValueAsync(node, budget).ConfigureAwait(false);
            return new Observation(pattern.Matches(value), TextPattern.Quote(value));
        });
    }

    // Without a pattern, any value of the attribute holds.
    private Task ExpectAttributeAsync(string expected, string name, TextPattern? pattern, TimeSpan? timeout, CancellationToken cancellationToken)
    {
        return this.ExpectElementAsync(expected, false, timeout, cancellationToken, async (node, budget) =>
        {
            string? value = await this.locator.ReadAttributeAsync(node, name, budget).ConfigureAwait(false);
            return value is null
                ? new Observation(false, $"no {TextPattern.Quote(name)} attribute")
                : new Observation(pattern?.Matches(value) ?? true, $"{name}={TextPattern.Quote(value)}");
        });
    }

    private Task ExpectCssAsync(string name, TextPattern pattern, TimeSpan? timeout, CancellationToken cancellationToken)
    {
        return this.ExpectElementAsync($"to have CSS {TextPattern.Quote(name)} with value {pattern.Description}", false, timeout, cancellationToken, async (node, budget) =>
        {
            string value = await this.locator.ReadCssAsync(node, name, budget).ConfigureAwait(false);
            return new Observation(pattern.Matches(value), TextPattern.Quote(value));
        });
    }
}
