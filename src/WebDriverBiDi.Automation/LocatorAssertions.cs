// <copyright file="LocatorAssertions.cs" company="WebDriverBiDi.NET Committers">
// Copyright (c) WebDriverBiDi.NET Committers. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.
// </copyright>

namespace WebDriverBiDi.Automation;

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
            int matched = await this.locator.CountMatchesAsync(budget).ConfigureAwait(false);
            return new Observation(matched == count, matched.ToString(System.Globalization.CultureInfo.InvariantCulture));
        });
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
}
