// <copyright file="ElementLocator.cs" company="WebDriverBiDi.NET Committers">
// Copyright (c) WebDriverBiDi.NET Committers. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.
// </copyright>

namespace Dramaturge;

using System.Diagnostics.CodeAnalysis;
using WebDriverBiDi;
using WebDriverBiDi.BrowsingContext;
using WebDriverBiDi.Input;
using WebDriverBiDi.Script;

/// <summary>
/// A description of how to find elements in a <see cref="Dramaturge.Frame"/>: a chain of steps, each finding
/// elements within those the previous step found, or picking one of them by position. Nothing is looked up until
/// an operation runs, and each operation looks the elements up again, so a locator stays valid as the page changes.
/// A locator is immutable; chaining creates a new one.
/// </summary>
public sealed class ElementLocator
{
    // Enough to tell one match from several without serializing every match.
    private const ulong StrictMatchLimit = 2;
    private const string FocusFunction = "(element) => element.focus()";

    // Input types whose value is chosen, such as a date or a checked box, rather than typed.
    private static readonly HashSet<string> UntypeableInputTypes = new(StringComparer.OrdinalIgnoreCase)
    {
        "button", "checkbox", "color", "date", "datetime-local", "file", "hidden", "image", "month", "radio", "range", "reset", "submit", "time", "week",
    };

    private readonly IReadOnlyList<Step> steps;

    /// <summary>
    /// Initializes a new instance of the <see cref="ElementLocator"/> class.
    /// </summary>
    /// <param name="frame">The frame in which elements are found.</param>
    /// <param name="locator">How the elements are found.</param>
    internal ElementLocator(Frame frame, Locator locator)
        : this(frame, [new LocateStep(locator)])
    {
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="ElementLocator"/> class for a GetBy helper's query.
    /// </summary>
    /// <param name="frame">The frame in which elements are found.</param>
    /// <param name="query">The query.</param>
    internal ElementLocator(Frame frame, ElementQuery query)
        : this(frame, [new LocateStep(query)])
    {
    }

    private ElementLocator(Frame frame, IReadOnlyList<Step> steps)
    {
        this.Frame = frame;
        this.steps = steps;
    }

    /// <summary>
    /// Gets the frame in which elements are found.
    /// </summary>
    public Frame Frame { get; }

    private BrowserGroup Group => this.Frame.Page.Browser.Group;

    /// <summary>
    /// Creates a locator for elements within those this locator finds.
    /// </summary>
    /// <param name="locator">How the elements are found within each element this locator finds.</param>
    /// <returns>The locator.</returns>
    public ElementLocator Locate(Locator locator)
    {
        return this.Append(new LocateStep(locator));
    }

    /// <summary>
    /// Creates a locator for elements, within the elements this locator finds, by their rendered text: by default, text containing
    /// <paramref name="text"/> ignoring case; with <paramref name="exact"/>, text matching it exactly, with case.
    /// The browser compares its own rendering of the text, without collapsing whitespace.
    /// </summary>
    /// <param name="text">The text.</param>
    /// <param name="exact">Whether the whole text must match, with case.</param>
    /// <returns>The locator.</returns>
    public ElementLocator GetByText(string text, bool exact = false)
    {
        return this.Append(new LocateStep(ElementQuery.ByText(text, exact)));
    }

    /// <summary>
    /// Creates a locator for input elements by their placeholder: by default, one containing <paramref name="text"/> ignoring case; with
    /// <paramref name="exact"/>, one matching it exactly, within the elements this locator finds.
    /// </summary>
    /// <param name="text">The placeholder text.</param>
    /// <param name="exact">Whether the whole placeholder must match, with case.</param>
    /// <returns>The locator.</returns>
    public ElementLocator GetByPlaceholder(string text, bool exact = false)
    {
        return this.Append(new LocateStep(ElementQuery.ByAttribute("placeholder", "getByPlaceholder", text, exact)));
    }

    /// <summary>
    /// Creates a locator for elements, such as images, by their alternative text: by default, text containing <paramref name="text"/>
    /// ignoring case; with <paramref name="exact"/>, text matching it exactly, within the elements this locator finds.
    /// </summary>
    /// <param name="text">The alternative text.</param>
    /// <param name="exact">Whether the whole alternative text must match, with case.</param>
    /// <returns>The locator.</returns>
    public ElementLocator GetByAltText(string text, bool exact = false)
    {
        return this.Append(new LocateStep(ElementQuery.ByAttribute("alt", "getByAltText", text, exact)));
    }

    /// <summary>
    /// Creates a locator for elements by their title attribute: by default, one containing <paramref name="text"/> ignoring case; with
    /// <paramref name="exact"/>, one matching it exactly, within the elements this locator finds.
    /// </summary>
    /// <param name="text">The title text.</param>
    /// <param name="exact">Whether the whole title must match, with case.</param>
    /// <returns>The locator.</returns>
    public ElementLocator GetByTitle(string text, bool exact = false)
    {
        return this.Append(new LocateStep(ElementQuery.ByAttribute("title", "getByTitle", text, exact)));
    }

    /// <summary>
    /// Creates a locator for elements by their test ID, the value of the <see cref="DramaturgeOptions.TestIdAttribute"/> attribute, within the elements this locator finds.
    /// </summary>
    /// <param name="testId">The test ID, matched exactly.</param>
    /// <returns>The locator.</returns>
    public ElementLocator GetByTestId(string testId)
    {
        return this.Append(new LocateStep(ElementQuery.ByTestId(this.Group.Options.TestIdAttribute, testId)));
    }

    /// <summary>
    /// Creates a locator for elements by the accessibility role the browser computes for them, such as "button", and optionally the
    /// accessible name, which must match exactly, within the elements this locator finds.
    /// </summary>
    /// <param name="role">The role. img is asked for as image, the name ARIA 1.3 gives it and browsers report.</param>
    /// <param name="name">The exact accessible name, or <see langword="null"/> for any.</param>
    /// <param name="states">ARIA states the elements must have, such as checked, or <see langword="null"/> for any.</param>
    /// <returns>The locator.</returns>
    /// <remarks>
    /// The browser computes the roles and names this matches. An accessibility snapshot, such as one from
    /// <see cref="Page.AriaSnapshotAsync"/>, computes names with the library's script in the page, so a name read from
    /// a snapshot can differ from the browser's, and find nothing here; to act on an element from a snapshot, use
    /// <see cref="AriaSnapshot.Locator"/> with its ref.
    /// </remarks>
    public ElementLocator GetByRole(string role, string? name = null, RoleStates? states = null)
    {
        return new ElementLocator(this.Frame, [.. this.steps, .. RoleSteps(role, name, states)]);
    }

    /// <summary>
    /// Creates a locator for elements, within the elements this locator finds, by their labels: the elements their aria-labelledby attribute refers
    /// to; failing that, their aria-label attribute; failing that, the label elements of a form control. By default a
    /// label must contain <paramref name="text"/> ignoring case; with <paramref name="exact"/>, it must match it
    /// exactly, with case. Labels are compared with runs of whitespace collapsed.
    /// </summary>
    /// <param name="text">The label text.</param>
    /// <param name="exact">Whether the whole label must match, with case.</param>
    /// <returns>The locator.</returns>
    public ElementLocator GetByLabel(string text, bool exact = false)
    {
        return this.Append(new LabelStep(text, exact));
    }

    /// <summary>
    /// Creates a locator for the shadow roots of the elements this locator finds, open or closed, so that the next
    /// step searches within them. An element without a shadow root is dropped. Browsers do not evaluate XPath within
    /// a shadow root, so an XPath step cannot follow this one.
    /// </summary>
    /// <returns>The locator.</returns>
    public ElementLocator ShadowRoot()
    {
        return this.Append(new ShadowRootStep());
    }

    /// <summary>
    /// Creates a locator for the first element this locator finds.
    /// </summary>
    /// <returns>The locator.</returns>
    public ElementLocator First()
    {
        return this.Append(new IndexStep(0));
    }

    /// <summary>
    /// Creates a locator for the last element this locator finds.
    /// </summary>
    /// <returns>The locator.</returns>
    public ElementLocator Last()
    {
        return this.Append(new IndexStep(IndexStep.LastIndex));
    }

    /// <summary>
    /// Creates a locator for the element at a position among those this locator finds, in the order the browser
    /// reports them: document order for a single step, and grouped by the previous step's matches otherwise.
    /// A position past the last element matches nothing.
    /// </summary>
    /// <param name="index">The zero-based position.</param>
    /// <returns>The locator.</returns>
    /// <exception cref="ArgumentOutOfRangeException">Thrown when <paramref name="index"/> is negative.</exception>
    public ElementLocator Nth(int index)
    {
        return index < 0
            ? throw new ArgumentOutOfRangeException(nameof(index), index, "The position must not be negative.")
            : this.Append(new IndexStep(index));
    }

    /// <summary>
    /// Creates a locator for the elements this locator and another both find.
    /// </summary>
    /// <param name="other">The other locator, which searches the whole frame.</param>
    /// <returns>The locator.</returns>
    /// <exception cref="ArgumentException">Thrown when <paramref name="other"/> finds elements in another frame.</exception>
    public ElementLocator And(ElementLocator other)
    {
        return this.Append(new AndStep(this.RequireSameFrame(other, nameof(other))));
    }

    /// <summary>
    /// Creates a locator for the elements either this locator or another finds: this locator's matches, then the
    /// other's that this locator does not find.
    /// </summary>
    /// <param name="other">The other locator, which searches the whole frame.</param>
    /// <returns>The locator.</returns>
    /// <exception cref="ArgumentException">Thrown when <paramref name="other"/> finds elements in another frame.</exception>
    public ElementLocator Or(ElementLocator other)
    {
        return this.Append(new OrStep(this.RequireSameFrame(other, nameof(other))));
    }

    /// <summary>
    /// Creates a locator for the elements this locator finds that meet every given condition. Text conditions
    /// compare an element's rendered text ignoring case and runs of whitespace. Element conditions search within
    /// each element, so they cost a lookup for each element this locator finds.
    /// </summary>
    /// <param name="hasText">Text the element's text must contain.</param>
    /// <param name="hasNotText">Text the element's text must not contain.</param>
    /// <param name="has">A locator that must find something within the element.</param>
    /// <param name="hasNot">A locator that must find nothing within the element.</param>
    /// <returns>The locator.</returns>
    /// <exception cref="ArgumentException">Thrown when no condition is given, or a locator finds elements in another frame.</exception>
    public ElementLocator Filter(string? hasText = null, string? hasNotText = null, ElementLocator? has = null, ElementLocator? hasNot = null)
    {
        if (hasText is null && hasNotText is null && has is null && hasNot is null)
        {
            throw new ArgumentException("A filter needs at least one condition.");
        }

        return this.Append(new FilterStep(
            hasText,
            hasNotText,
            has is null ? null : this.RequireSameFrame(has, nameof(has)),
            hasNot is null ? null : this.RequireSameFrame(hasNot, nameof(hasNot))));
    }

    /// <summary>
    /// Counts the elements that match now, without waiting.
    /// </summary>
    /// <param name="cancellationToken">A token that cancels the command.</param>
    /// <returns>The number of matching elements.</returns>
    public Task<int> CountAsync(CancellationToken cancellationToken = default)
    {
        return this.TraceAsync(this.Call("Count", "count"), this.CreateBudget(null, cancellationToken), async budget => (await this.ResolveAsync(null, budget).ConfigureAwait(false)).Count);
    }

    /// <summary>
    /// Gets a value indicating whether the element is visible now, without waiting. No matching element is not
    /// visible.
    /// </summary>
    /// <param name="cancellationToken">A token that cancels the commands.</param>
    /// <returns><see langword="true"/> if the element is visible; otherwise, <see langword="false"/>.</returns>
    /// <exception cref="AmbiguousElementException">Thrown when more than one element matches.</exception>
    public Task<bool> IsVisibleAsync(CancellationToken cancellationToken = default)
    {
        return this.TraceAsync(this.Call("Is visible", "isVisible"), this.CreateBudget(null, cancellationToken), this.IsVisibleCoreAsync);
    }

    /// <summary>
    /// Waits for the element to reach a state, looking it up again until it does.
    /// </summary>
    /// <param name="state">The state.</param>
    /// <param name="timeout">The time to wait, or <see langword="null"/> for <see cref="DramaturgeOptions.ActionTimeout"/>.</param>
    /// <param name="cancellationToken">A token that cancels the wait.</param>
    /// <returns>A task that completes when the element is in the state.</returns>
    /// <exception cref="AmbiguousElementException">Thrown when more than one element matches, for any state but <see cref="ElementState.Detached"/>.</exception>
    /// <exception cref="WebDriverBiDiTimeoutException">Thrown when the element does not reach the state in time.</exception>
    public Task WaitForAsync(ElementState state = ElementState.Visible, TimeSpan? timeout = null, CancellationToken cancellationToken = default)
    {
        return this.TraceAsync(this.Call("Wait for {state}", "waitFor", ("state", state.ToString())), this.CreateBudget(timeout, cancellationToken), budget => this.PollAsync(budget, $"{this} to be {state.ToString().ToLowerInvariant()}", async () =>
        {
            (bool reached, string observed) = await this.CheckStateAsync(state, budget).ConfigureAwait(false);
            return (reached, reached, observed);
        }));
    }

    /// <summary>
    /// Gets the frame of the iframe or frame element this locator finds, waiting until exactly one element matches
    /// and its document has been loaded. The frame is found once; if the element's document is later replaced, the
    /// returned frame reports <see cref="Frame.IsDetached"/>, and this method finds the new one.
    /// </summary>
    /// <param name="timeout">The time to wait, or <see langword="null"/> for <see cref="DramaturgeOptions.ActionTimeout"/>.</param>
    /// <param name="cancellationToken">A token that cancels the wait.</param>
    /// <returns>The frame.</returns>
    /// <exception cref="AmbiguousElementException">Thrown when more than one element matches.</exception>
    /// <exception cref="InvalidOperationException">Thrown when the element is not a frame element.</exception>
    /// <exception cref="WebDriverBiDiTimeoutException">Thrown when no frame is found in time.</exception>
    public Task<Frame> ContentFrameAsync(TimeSpan? timeout = null, CancellationToken cancellationToken = default)
    {
        return this.TraceAsync(this.Call("Content frame", "contentFrame"), this.CreateBudget(timeout, cancellationToken), async budget => (await this.PollAsync(budget, $"the frame of {this}", () => this.FindContentFrameAsync(budget)).ConfigureAwait(false))!);
    }

    /// <summary>
    /// Takes an accessibility snapshot of the element and its descendants, waiting until exactly one element matches:
    /// each element with a role, with its accessible name and states, and the text between them, including the
    /// content of frames within it.
    /// </summary>
    /// <param name="options">Whether the snapshot has refs and includes frames, or <see langword="null"/> for both.</param>
    /// <param name="timeout">The time to wait, or <see langword="null"/> for <see cref="DramaturgeOptions.ActionTimeout"/>.</param>
    /// <param name="cancellationToken">A token that cancels the wait and the snapshot.</param>
    /// <returns>The snapshot, which includes the element itself if it has a role.</returns>
    /// <exception cref="AmbiguousElementException">Thrown when more than one element matches.</exception>
    /// <exception cref="WebDriverBiDiTimeoutException">Thrown when no element matches in time.</exception>
    /// <remarks>
    /// The snapshot's names are computed by the library's script in the page, and can differ from those the browser
    /// uses for <see cref="Frame.GetByRole"/>; act on an element from a snapshot through its ref, with
    /// <see cref="AriaSnapshot.Locator"/>.
    /// </remarks>
    public Task<AriaSnapshot> AriaSnapshotAsync(AriaSnapshotOptions? options = null, TimeSpan? timeout = null, CancellationToken cancellationToken = default)
    {
        AriaSnapshotOptions snapshotOptions = options ?? new AriaSnapshotOptions();
        return this.TraceAsync(this.Call("Aria snapshot", "ariaSnapshot"), this.CreateBudget(timeout, cancellationToken), async budget => (await this.PollElementAsync(budget, async node => (true, await AriaSnapshotBuilder.TakeAsync(this.Frame, node, snapshotOptions, budget).ConfigureAwait(false), string.Empty)).ConfigureAwait(false))!);
    }

    /// <summary>
    /// Clicks the element, once it is visible, stable, enabled, and not covered by another element, scrolling it
    /// into view if needed.
    /// </summary>
    /// <param name="options">The button, click count, position, modifier keys, and wait, or <see langword="null"/> for a single left click at the chosen point.</param>
    /// <param name="cancellationToken">A token that cancels the wait and the action.</param>
    /// <returns>A task that completes when the click has been performed.</returns>
    /// <exception cref="AmbiguousElementException">Thrown when more than one element matches.</exception>
    /// <exception cref="WebDriverBiDiTimeoutException">Thrown when the element is not ready in time.</exception>
    public Task ClickAsync(ClickOptions? options = null, CancellationToken cancellationToken = default)
    {
        options ??= new ClickOptions();
        return this.TraceAsync(this.Call("Click", "click"), this.CreateBudget(options.Timeout, cancellationToken), budget => this.ClickCoreAsync(options, options.ClickCount, budget));
    }

    /// <summary>
    /// Double-clicks the element, once it is ready, as <see cref="ClickAsync"/> waits.
    /// </summary>
    /// <param name="options">The button, position, modifier keys, and wait; the click count is always 2.</param>
    /// <param name="cancellationToken">A token that cancels the wait and the action.</param>
    /// <returns>A task that completes when the double click has been performed.</returns>
    /// <exception cref="AmbiguousElementException">Thrown when more than one element matches.</exception>
    /// <exception cref="WebDriverBiDiTimeoutException">Thrown when the element is not ready in time.</exception>
    public Task DblClickAsync(ClickOptions? options = null, CancellationToken cancellationToken = default)
    {
        options ??= new ClickOptions();
        return this.TraceAsync(this.Call("Double click", "dblclick"), this.CreateBudget(options.Timeout, cancellationToken), budget => this.ClickCoreAsync(options, 2, budget));
    }

    /// <summary>
    /// Moves the pointer over the element, once it is visible, stable, enabled, and not covered by another element,
    /// scrolling it into view if needed.
    /// </summary>
    /// <param name="options">The position, modifier keys, and wait, or <see langword="null"/> for the chosen point.</param>
    /// <param name="cancellationToken">A token that cancels the wait and the action.</param>
    /// <returns>A task that completes when the pointer is over the element.</returns>
    /// <exception cref="AmbiguousElementException">Thrown when more than one element matches.</exception>
    /// <exception cref="WebDriverBiDiTimeoutException">Thrown when the element is not ready in time.</exception>
    public Task HoverAsync(PointerActionOptions? options = null, CancellationToken cancellationToken = default)
    {
        options ??= new PointerActionOptions();
        return this.TraceAsync(this.Call("Hover", "hover"), this.CreateBudget(options.Timeout, cancellationToken), budget => this.PerformPointerActionAsync("hover", "hovered", options, budget, (_, _) => { }));
    }

    /// <summary>
    /// Taps the element with a touch pointer, once it is ready, as <see cref="ClickAsync"/> waits.
    /// </summary>
    /// <param name="options">The position, modifier keys, and wait, or <see langword="null"/> for a tap at the chosen point.</param>
    /// <param name="cancellationToken">A token that cancels the wait and the action.</param>
    /// <returns>A task that completes when the tap has been performed.</returns>
    /// <exception cref="AmbiguousElementException">Thrown when more than one element matches.</exception>
    /// <exception cref="WebDriverBiDiTimeoutException">Thrown when the element is not ready in time.</exception>
    public Task TapAsync(PointerActionOptions? options = null, CancellationToken cancellationToken = default)
    {
        options ??= new PointerActionOptions();
        return this.TraceAsync(this.Call("Tap", "tap"), this.CreateBudget(options.Timeout, cancellationToken), budget => this.PerformPointerActionAsync("click", "tapped", options, budget, (pointer, builder) => builder.AddAction(pointer.CreatePointerDown()).AddAction(pointer.CreatePointerUp()), PointerType.Touch));
    }

    /// <summary>
    /// Drags the element onto another with the mouse, once the element is ready to be dragged, as
    /// <see cref="ClickAsync"/> waits, and the target is visible, stable, and not covered by another element.
    /// </summary>
    /// <param name="target">The locator of the element to drop onto, in the same frame.</param>
    /// <param name="options">The points held and dropped on, modifier keys, and wait, or <see langword="null"/> for the chosen points.</param>
    /// <param name="cancellationToken">A token that cancels the wait and the action.</param>
    /// <returns>A task that completes when the element has been dropped.</returns>
    /// <exception cref="ArgumentException">Thrown when the target finds elements in another frame.</exception>
    /// <exception cref="AmbiguousElementException">Thrown when more than one element matches either locator.</exception>
    /// <exception cref="WebDriverBiDiTimeoutException">Thrown when either element is not ready in time.</exception>
    public Task DragToAsync(ElementLocator target, DragOptions? options = null, CancellationToken cancellationToken = default)
    {
        this.RequireSameFrame(target, nameof(target));
        options ??= new DragOptions();
        return this.TraceAsync(this.Call("Drag to {target}", "dragTo", ("target", target.ToString())), this.CreateBudget(options.Timeout, cancellationToken), async budget =>
        {
            ActionTarget? from = await this.PollAsync(budget, $"{this} to be ready to be dragged", () => this.FindActionTargetAsync("drag", options, budget)).ConfigureAwait(false);
            PointerActionOptions targetOptions = new() { Offset = options.TargetOffset, Force = options.Force };
            ActionTarget? to = await target.PollAsync(budget, $"{target} to be ready to be dropped on", () => target.FindActionTargetAsync("drop", targetOptions, budget)).ConfigureAwait(false);
            if (budget.Trace is ActionTrace trace)
            {
                await trace.TargetAsync(this.Frame, from!.Node, from.Offset).ConfigureAwait(false);
            }

            await this.PerformPointerInputAsync(options.Modifiers, PointerType.Mouse, budget, (pointer, builder) => builder
                .AddAction(MoveTo(pointer, from!))
                .AddAction(pointer.CreatePointerDown())
                .AddAction(MoveTo(pointer, to!))
                .AddAction(pointer.CreatePointerUp())).ConfigureAwait(false);
        });
    }

    /// <summary>
    /// Dispatches an event to the element, once one matches, created with the interface a user's action would use
    /// for its type, such as <c>MouseEvent</c> for <c>click</c>. The event bubbles, is cancelable, and crosses shadow
    /// boundaries, unless the init properties say otherwise.
    /// </summary>
    /// <param name="type">The event type, such as <c>click</c>.</param>
    /// <param name="eventInit">The event's init properties, such as <c>clientX</c>, or <see langword="null"/> for none.</param>
    /// <param name="timeout">The time to wait, or <see langword="null"/> for <see cref="DramaturgeOptions.ActionTimeout"/>.</param>
    /// <param name="cancellationToken">A token that cancels the wait and the command.</param>
    /// <returns><see langword="false"/> if a listener canceled the event; otherwise, <see langword="true"/>.</returns>
    /// <exception cref="AmbiguousElementException">Thrown when more than one element matches.</exception>
    /// <exception cref="WebDriverBiDiTimeoutException">Thrown when no element matches in time.</exception>
    public Task<bool> DispatchEventAsync(string type, IReadOnlyDictionary<string, LocalValue>? eventInit = null, TimeSpan? timeout = null, CancellationToken cancellationToken = default)
    {
        return this.TraceAsync(this.Call("Dispatch \"{type}\"", "dispatchEvent", ("type", type)), this.CreateBudget(timeout, cancellationToken), async budget =>
        {
            NodeRemoteValue? node = await this.PollAsync(budget, $"{this} to be attached", () => this.TryFindOneAsync(budget)).ConfigureAwait(false);
            await this.RecordTargetAsync(node!, budget).ConfigureAwait(false);
            LocalValue init = LocalValue.Object(eventInit?.ToDictionary(property => property.Key, property => property.Value) ?? []);
            RemoteValue dispatched = await this.Group.ScriptHost.CallActionsAsync(this.Frame.Id, "(actions, element, type, init) => actions.dispatchEvent(element, type, init)", [node!.ToSharedReference(), LocalValue.String(type), init], budget).ConfigureAwait(false);
            return dispatched.As<BooleanRemoteValue>().Value;
        });
    }

    /// <summary>
    /// Scrolls the element into view, once it is visible and stable, unless it is in view already.
    /// </summary>
    /// <param name="timeout">The time to wait, or <see langword="null"/> for <see cref="DramaturgeOptions.ActionTimeout"/>.</param>
    /// <param name="cancellationToken">A token that cancels the wait.</param>
    /// <returns>A task that completes when the element is in view.</returns>
    /// <exception cref="AmbiguousElementException">Thrown when more than one element matches.</exception>
    /// <exception cref="WebDriverBiDiTimeoutException">Thrown when the element is not in view in time.</exception>
    public Task ScrollIntoViewIfNeededAsync(TimeSpan? timeout = null, CancellationToken cancellationToken = default)
    {
        return this.TraceAsync(this.Call("Scroll into view", "scrollIntoViewIfNeeded"), this.CreateBudget(timeout, cancellationToken), budget => this.PollAsync(budget, $"{this} to be in view", () => this.TryScrollIntoViewAsync(budget)));
    }

    /// <summary>
    /// Focuses the element, once one matches.
    /// </summary>
    /// <param name="timeout">The time to wait, or <see langword="null"/> for <see cref="DramaturgeOptions.ActionTimeout"/>.</param>
    /// <param name="cancellationToken">A token that cancels the wait.</param>
    /// <returns>A task that completes when the element has been focused.</returns>
    /// <exception cref="AmbiguousElementException">Thrown when more than one element matches.</exception>
    /// <exception cref="WebDriverBiDiTimeoutException">Thrown when no element matches in time.</exception>
    public Task FocusAsync(TimeSpan? timeout = null, CancellationToken cancellationToken = default)
    {
        return this.TraceAsync(this.Call("Focus", "focus"), this.CreateBudget(timeout, cancellationToken), budget => this.PollAsync(budget, $"{this} to be focused", () => this.TryCallOnElementAsync(FocusFunction, budget)));
    }

    /// <summary>
    /// Removes focus from the element, once one matches.
    /// </summary>
    /// <param name="timeout">The time to wait, or <see langword="null"/> for <see cref="DramaturgeOptions.ActionTimeout"/>.</param>
    /// <param name="cancellationToken">A token that cancels the wait.</param>
    /// <returns>A task that completes when focus has been removed from the element.</returns>
    /// <exception cref="AmbiguousElementException">Thrown when more than one element matches.</exception>
    /// <exception cref="WebDriverBiDiTimeoutException">Thrown when no element matches in time.</exception>
    public Task BlurAsync(TimeSpan? timeout = null, CancellationToken cancellationToken = default)
    {
        return this.TraceAsync(this.Call("Blur", "blur"), this.CreateBudget(timeout, cancellationToken), budget => this.PollAsync(budget, $"{this} to be blurred", () => this.TryCallOnElementAsync("(element) => element.blur()", budget)));
    }

    /// <summary>
    /// Focuses the element, once one matches, and presses a key, holding down any modifier keys.
    /// </summary>
    /// <param name="key">The key: a special key from <see cref="Keys"/>, such as <see cref="Keys.Enter"/>, or a single character.</param>
    /// <param name="options">The modifier keys and wait, or <see langword="null"/> for the key alone.</param>
    /// <param name="cancellationToken">A token that cancels the wait and the action.</param>
    /// <returns>A task that completes when the key has been pressed and released.</returns>
    /// <exception cref="AmbiguousElementException">Thrown when more than one element matches.</exception>
    /// <exception cref="WebDriverBiDiTimeoutException">Thrown when no element matches in time.</exception>
    public Task PressAsync(string key, KeyActionOptions? options = null, CancellationToken cancellationToken = default)
    {
        options ??= new KeyActionOptions();
        return this.TraceAsync(this.Call("Press \"{key}\"", "press", ("key", key)), this.CreateBudget(options.Timeout, cancellationToken), async budget =>
        {
            await this.PollAsync(budget, $"{this} to be focused", () => this.TryCallOnElementAsync(FocusFunction, budget)).ConfigureAwait(false);
            await this.PerformKeyActionsAsync(new InputBuilder().AddKeyChordAction([.. ModifierKeyValues.For(options.Modifiers), key]), budget).ConfigureAwait(false);
        });
    }

    /// <summary>
    /// Focuses the element, once one matches, and types text into it one key at a time, as a press and a release
    /// of each character, without clearing what it holds.
    /// </summary>
    /// <param name="text">The text, which may include special keys from <see cref="Keys"/>.</param>
    /// <param name="options">The delay between keys and the wait, or <see langword="null"/> for no delay.</param>
    /// <param name="cancellationToken">A token that cancels the wait and the action.</param>
    /// <returns>A task that completes when the text has been typed.</returns>
    /// <exception cref="AmbiguousElementException">Thrown when more than one element matches.</exception>
    /// <exception cref="WebDriverBiDiTimeoutException">Thrown when no element matches in time.</exception>
    public Task PressSequentiallyAsync(string text, PressSequentiallyOptions? options = null, CancellationToken cancellationToken = default)
    {
        options ??= new PressSequentiallyOptions();
        return this.TraceAsync(this.Call("Type \"{text}\"", "pressSequentially", ("text", text)), this.CreateBudget(options.Timeout, cancellationToken), async budget =>
        {
            await this.PollAsync(budget, $"{this} to be focused", () => this.TryCallOnElementAsync(FocusFunction, budget)).ConfigureAwait(false);
            await this.PerformKeyActionsAsync(new InputBuilder().AddSendKeysToActiveElementAction(text, options.Delay), budget).ConfigureAwait(false);
        });
    }

    /// <summary>
    /// Replaces the text of an input, a text area, or an editable element, once it is visible, stable, enabled,
    /// editable, and not covered by another element: its text is selected, and the new text typed over it.
    /// </summary>
    /// <param name="value">The text; empty clears the element.</param>
    /// <param name="options">The wait, or <see langword="null"/> for the default.</param>
    /// <param name="cancellationToken">A token that cancels the wait and the action.</param>
    /// <returns>A task that completes when the text has been typed.</returns>
    /// <exception cref="AmbiguousElementException">Thrown when more than one element matches.</exception>
    /// <exception cref="InvalidOperationException">Thrown when the element cannot be edited, or is an input, such as a date or a checkbox, whose value is not typed.</exception>
    /// <exception cref="WebDriverBiDiTimeoutException">Thrown when the element is not ready in time.</exception>
    public Task FillAsync(string value, ActionOptions? options = null, CancellationToken cancellationToken = default)
    {
        options ??= new ActionOptions();
        return this.TraceAsync(this.Call("Fill \"{value}\"", "fill", ("value", value)), this.CreateBudget(options.Timeout, cancellationToken), budget => this.ReplaceTextAsync(value, "type", "filled", options.Force, budget));
    }

    /// <summary>
    /// Clears the text of an input, a text area, or an editable element, once it is ready, as
    /// <see cref="FillAsync"/> waits.
    /// </summary>
    /// <param name="options">The wait, or <see langword="null"/> for the default.</param>
    /// <param name="cancellationToken">A token that cancels the wait and the action.</param>
    /// <returns>A task that completes when the text has been deleted.</returns>
    /// <exception cref="AmbiguousElementException">Thrown when more than one element matches.</exception>
    /// <exception cref="InvalidOperationException">Thrown when the element cannot be edited, or is an input, such as a date or a checkbox, whose value is not typed.</exception>
    /// <exception cref="WebDriverBiDiTimeoutException">Thrown when the element is not ready in time.</exception>
    public Task ClearAsync(ActionOptions? options = null, CancellationToken cancellationToken = default)
    {
        options ??= new ActionOptions();
        return this.TraceAsync(this.Call("Clear", "clear"), this.CreateBudget(options.Timeout, cancellationToken), budget => this.ReplaceTextAsync(string.Empty, "clear", "cleared", options.Force, budget));
    }

    /// <summary>
    /// Checks a checkbox or radio button, unless it is checked already, by clicking it once it is ready, as
    /// <see cref="ClickAsync"/> waits, then confirms that the click checked it.
    /// </summary>
    /// <param name="options">The position, modifier keys, and wait, or <see langword="null"/> for the defaults.</param>
    /// <param name="cancellationToken">A token that cancels the wait and the action.</param>
    /// <returns>A task that completes when the element is checked.</returns>
    /// <exception cref="AmbiguousElementException">Thrown when more than one element matches.</exception>
    /// <exception cref="InvalidOperationException">Thrown when the element cannot be checked, or clicking it did not check it.</exception>
    /// <exception cref="WebDriverBiDiTimeoutException">Thrown when the element is not ready in time.</exception>
    public Task CheckAsync(PointerActionOptions? options = null, CancellationToken cancellationToken = default)
    {
        options ??= new PointerActionOptions();
        return this.TraceAsync(this.Call("Check", "check"), this.CreateBudget(options.Timeout, cancellationToken), budget => this.SetCheckedCoreAsync(true, options, budget));
    }

    /// <summary>
    /// Unchecks a checkbox, unless it is unchecked already, by clicking it once it is ready, as
    /// <see cref="ClickAsync"/> waits, then confirms that the click unchecked it.
    /// </summary>
    /// <param name="options">The position, modifier keys, and wait, or <see langword="null"/> for the defaults.</param>
    /// <param name="cancellationToken">A token that cancels the wait and the action.</param>
    /// <returns>A task that completes when the element is unchecked.</returns>
    /// <exception cref="AmbiguousElementException">Thrown when more than one element matches.</exception>
    /// <exception cref="InvalidOperationException">Thrown when the element cannot be checked, is a checked radio button, which clicking cannot uncheck, or clicking it did not uncheck it.</exception>
    /// <exception cref="WebDriverBiDiTimeoutException">Thrown when the element is not ready in time.</exception>
    public Task UncheckAsync(PointerActionOptions? options = null, CancellationToken cancellationToken = default)
    {
        options ??= new PointerActionOptions();
        return this.TraceAsync(this.Call("Uncheck", "uncheck"), this.CreateBudget(options.Timeout, cancellationToken), budget => this.SetCheckedCoreAsync(false, options, budget));
    }

    /// <summary>
    /// Checks or unchecks a checkbox or radio button, as <see cref="CheckAsync"/> and <see cref="UncheckAsync"/> do.
    /// An element in a mixed state counts as unchecked.
    /// </summary>
    /// <param name="isChecked">Whether the element is to be checked.</param>
    /// <param name="options">The position, modifier keys, and wait, or <see langword="null"/> for the defaults.</param>
    /// <param name="cancellationToken">A token that cancels the wait and the action.</param>
    /// <returns>A task that completes when the element is in the state.</returns>
    /// <exception cref="AmbiguousElementException">Thrown when more than one element matches.</exception>
    /// <exception cref="InvalidOperationException">Thrown when the element cannot be checked, is a checked radio button being unchecked, or clicking it did not change its state.</exception>
    /// <exception cref="WebDriverBiDiTimeoutException">Thrown when the element is not ready in time.</exception>
    public Task SetCheckedAsync(bool isChecked, PointerActionOptions? options = null, CancellationToken cancellationToken = default)
    {
        options ??= new PointerActionOptions();
        return this.TraceAsync(this.Call("Set checked {checked}", "setChecked", ("checked", isChecked ? "true" : "false")), this.CreateBudget(options.Timeout, cancellationToken), budget => this.SetCheckedCoreAsync(isChecked, options, budget));
    }

    /// <summary>
    /// Selects options of a <c>&lt;select&gt;</c> element, deselecting the others, once the element is visible and
    /// enabled and every option exists and is enabled, then fires the <c>input</c> and <c>change</c> events a user's
    /// choice would.
    /// </summary>
    /// <param name="selections">The options; none deselects every option.</param>
    /// <param name="options">The wait, or <see langword="null"/> for the default.</param>
    /// <param name="cancellationToken">A token that cancels the wait and the action.</param>
    /// <returns>The values of the options now selected.</returns>
    /// <exception cref="AmbiguousElementException">Thrown when more than one element matches.</exception>
    /// <exception cref="InvalidOperationException">Thrown when the element is not a <c>&lt;select&gt;</c> element, or several options are given for one that takes one.</exception>
    /// <exception cref="WebDriverBiDiTimeoutException">Thrown when the element or an option is not ready in time.</exception>
    public Task<IReadOnlyList<string>> SelectOptionAsync(IEnumerable<SelectOption> selections, ActionOptions? options = null, CancellationToken cancellationToken = default)
    {
        options ??= new ActionOptions();
        List<SelectOption> chosen = [.. selections];
        return this.TraceAsync(this.Call("Select option", "selectOption", ("options", chosen.Select(option => option.ToString()).ToList())), this.CreateBudget(options.Timeout, cancellationToken), async budget => (await this.PollAsync(budget, $"{this} to be ready to have options selected", () => this.TrySelectOptionsAsync(chosen, options.Force, budget)).ConfigureAwait(false))!);
    }

    /// <summary>
    /// Sets the files of a file input, once one matches. The paths are on the machine the browser runs on.
    /// </summary>
    /// <param name="files">The paths of the files; none clears the input.</param>
    /// <param name="timeout">The time to wait, or <see langword="null"/> for <see cref="DramaturgeOptions.ActionTimeout"/>.</param>
    /// <param name="cancellationToken">A token that cancels the wait and the command.</param>
    /// <returns>A task that completes when the files have been set.</returns>
    /// <exception cref="AmbiguousElementException">Thrown when more than one element matches.</exception>
    /// <exception cref="WebDriverBiDiCommandException">Thrown when the browser cannot set the files, such as for an element that is not a file input, or several files for an input that takes one.</exception>
    /// <exception cref="WebDriverBiDiTimeoutException">Thrown when no element matches in time.</exception>
    public Task SetInputFilesAsync(IEnumerable<string> files, TimeSpan? timeout = null, CancellationToken cancellationToken = default)
    {
        List<string> fileList = [.. files];
        return this.TraceAsync(this.Call("Set input files", "setInputFiles", ("files", fileList)), this.CreateBudget(timeout, cancellationToken), async budget =>
        {
            NodeRemoteValue? node = await this.PollAsync(budget, $"{this} to be attached", () => this.TryFindOneAsync(budget)).ConfigureAwait(false);
            await this.RecordTargetAsync(node!, budget).ConfigureAwait(false);
            SetFilesCommandParameters parameters = new(this.Frame.Id, node!.ToSharedReference());
            parameters.Files.AddRange(fileList);
            await this.Group.Driver.Input.SetFilesAsync(parameters, budget.Remaining, budget.CancellationToken).ConfigureAwait(false);
        });
    }

    /// <summary>
    /// Gets a value indicating whether the element is hidden now, without waiting. No matching element is hidden.
    /// </summary>
    /// <param name="cancellationToken">A token that cancels the commands.</param>
    /// <returns><see langword="true"/> if no element matches or the element is not visible; otherwise, <see langword="false"/>.</returns>
    /// <exception cref="AmbiguousElementException">Thrown when more than one element matches.</exception>
    public Task<bool> IsHiddenAsync(CancellationToken cancellationToken = default)
    {
        return this.TraceAsync(this.Call("Is hidden", "isHidden"), this.CreateBudget(null, cancellationToken), async budget => !await this.IsVisibleCoreAsync(budget).ConfigureAwait(false));
    }

    /// <summary>
    /// Gets a value indicating whether the element is enabled, once one matches.
    /// </summary>
    /// <param name="timeout">The time to wait, or <see langword="null"/> for <see cref="DramaturgeOptions.ActionTimeout"/>.</param>
    /// <param name="cancellationToken">A token that cancels the wait.</param>
    /// <returns><see langword="true"/> if the element is enabled; otherwise, <see langword="false"/>.</returns>
    /// <exception cref="AmbiguousElementException">Thrown when more than one element matches.</exception>
    /// <exception cref="WebDriverBiDiTimeoutException">Thrown when no element matches in time.</exception>
    public Task<bool> IsEnabledAsync(TimeSpan? timeout = null, CancellationToken cancellationToken = default)
    {
        return this.TraceAsync(this.Call("Is enabled", "isEnabled"), this.CreateBudget(timeout, cancellationToken), async budget => await this.QueryStateAsync("enabled", budget).ConfigureAwait(false) == "enabled");
    }

    /// <summary>
    /// Gets a value indicating whether the element is disabled, once one matches.
    /// </summary>
    /// <param name="timeout">The time to wait, or <see langword="null"/> for <see cref="DramaturgeOptions.ActionTimeout"/>.</param>
    /// <param name="cancellationToken">A token that cancels the wait.</param>
    /// <returns><see langword="true"/> if the element is disabled; otherwise, <see langword="false"/>.</returns>
    /// <exception cref="AmbiguousElementException">Thrown when more than one element matches.</exception>
    /// <exception cref="WebDriverBiDiTimeoutException">Thrown when no element matches in time.</exception>
    public Task<bool> IsDisabledAsync(TimeSpan? timeout = null, CancellationToken cancellationToken = default)
    {
        return this.TraceAsync(this.Call("Is disabled", "isDisabled"), this.CreateBudget(timeout, cancellationToken), async budget => await this.QueryStateAsync("enabled", budget).ConfigureAwait(false) != "enabled");
    }

    /// <summary>
    /// Gets a value indicating whether the element can be edited, being neither disabled nor read-only, once one
    /// matches.
    /// </summary>
    /// <param name="timeout">The time to wait, or <see langword="null"/> for <see cref="DramaturgeOptions.ActionTimeout"/>.</param>
    /// <param name="cancellationToken">A token that cancels the wait.</param>
    /// <returns><see langword="true"/> if the element can be edited; otherwise, <see langword="false"/>.</returns>
    /// <exception cref="AmbiguousElementException">Thrown when more than one element matches.</exception>
    /// <exception cref="InvalidOperationException">Thrown when the element is not an input, a text area, a select, or an editable element.</exception>
    /// <exception cref="WebDriverBiDiTimeoutException">Thrown when no element matches in time.</exception>
    public Task<bool> IsEditableAsync(TimeSpan? timeout = null, CancellationToken cancellationToken = default)
    {
        return this.TraceAsync(this.Call("Is editable", "isEditable"), this.CreateBudget(timeout, cancellationToken), async budget => await this.QueryStateAsync("editable", budget).ConfigureAwait(false) == "editable");
    }

    /// <summary>
    /// Gets a value indicating whether a checkbox or radio button is checked, once one matches. An element in a
    /// mixed state is not checked.
    /// </summary>
    /// <param name="timeout">The time to wait, or <see langword="null"/> for <see cref="DramaturgeOptions.ActionTimeout"/>.</param>
    /// <param name="cancellationToken">A token that cancels the wait.</param>
    /// <returns><see langword="true"/> if the element is checked; otherwise, <see langword="false"/>.</returns>
    /// <exception cref="AmbiguousElementException">Thrown when more than one element matches.</exception>
    /// <exception cref="InvalidOperationException">Thrown when the element cannot be checked.</exception>
    /// <exception cref="WebDriverBiDiTimeoutException">Thrown when no element matches in time.</exception>
    public Task<bool> IsCheckedAsync(TimeSpan? timeout = null, CancellationToken cancellationToken = default)
    {
        return this.TraceAsync(this.Call("Is checked", "isChecked"), this.CreateBudget(timeout, cancellationToken), async budget => (await this.PollAsync(budget, $"{this} to report whether it is checked", () => this.TryReadCheckedStateAsync(budget)).ConfigureAwait(false))!.IsChecked);
    }

    /// <summary>
    /// Gets the text content of the element, which includes the text of hidden descendants, once one matches.
    /// </summary>
    /// <param name="timeout">The time to wait, or <see langword="null"/> for <see cref="DramaturgeOptions.ActionTimeout"/>.</param>
    /// <param name="cancellationToken">A token that cancels the wait.</param>
    /// <returns>The text content.</returns>
    /// <exception cref="AmbiguousElementException">Thrown when more than one element matches.</exception>
    /// <exception cref="WebDriverBiDiTimeoutException">Thrown when no element matches in time.</exception>
    public Task<string> TextContentAsync(TimeSpan? timeout = null, CancellationToken cancellationToken = default)
    {
        return this.TraceAsync(this.Call("Text content", "textContent"), this.CreateBudget(timeout, cancellationToken), async budget => (await this.PollElementAsync(budget, async node => (true, await this.ReadTextAsync(node, false, budget).ConfigureAwait(false), string.Empty)).ConfigureAwait(false))!);
    }

    /// <summary>
    /// Gets the text of the element as it is rendered, once one matches.
    /// </summary>
    /// <param name="timeout">The time to wait, or <see langword="null"/> for <see cref="DramaturgeOptions.ActionTimeout"/>.</param>
    /// <param name="cancellationToken">A token that cancels the wait.</param>
    /// <returns>The rendered text.</returns>
    /// <exception cref="AmbiguousElementException">Thrown when more than one element matches.</exception>
    /// <exception cref="InvalidOperationException">Thrown when the element is not an HTML element, such as an SVG one.</exception>
    /// <exception cref="WebDriverBiDiTimeoutException">Thrown when no element matches in time.</exception>
    public Task<string> InnerTextAsync(TimeSpan? timeout = null, CancellationToken cancellationToken = default)
    {
        return this.TraceAsync(this.Call("Inner text", "innerText"), this.CreateBudget(timeout, cancellationToken), async budget => (await this.PollElementAsync(budget, async node => (true, await this.ReadTextAsync(node, true, budget).ConfigureAwait(false) ?? throw new InvalidOperationException($"{this} is not an HTML element."), string.Empty)).ConfigureAwait(false))!);
    }

    /// <summary>
    /// Gets the HTML of the element's contents, once one matches.
    /// </summary>
    /// <param name="timeout">The time to wait, or <see langword="null"/> for <see cref="DramaturgeOptions.ActionTimeout"/>.</param>
    /// <param name="cancellationToken">A token that cancels the wait.</param>
    /// <returns>The HTML.</returns>
    /// <exception cref="AmbiguousElementException">Thrown when more than one element matches.</exception>
    /// <exception cref="WebDriverBiDiTimeoutException">Thrown when no element matches in time.</exception>
    public Task<string> InnerHtmlAsync(TimeSpan? timeout = null, CancellationToken cancellationToken = default)
    {
        return this.TraceAsync(this.Call("Inner HTML", "innerHTML"), this.CreateBudget(timeout, cancellationToken), async budget => (await this.PollElementAsync(budget, async node => (true, await this.ReadStringAsync(node, "(element) => element.innerHTML", [], budget).ConfigureAwait(false), string.Empty)).ConfigureAwait(false))!);
    }

    /// <summary>
    /// Gets the value of an input, a text area, or a select, once one matches.
    /// </summary>
    /// <param name="timeout">The time to wait, or <see langword="null"/> for <see cref="DramaturgeOptions.ActionTimeout"/>.</param>
    /// <param name="cancellationToken">A token that cancels the wait.</param>
    /// <returns>The value; for a select, the value of its first selected option, or empty if none is selected.</returns>
    /// <exception cref="AmbiguousElementException">Thrown when more than one element matches.</exception>
    /// <exception cref="InvalidOperationException">Thrown when the element is not an input, a text area, or a select.</exception>
    /// <exception cref="WebDriverBiDiTimeoutException">Thrown when no element matches in time.</exception>
    public Task<string> InputValueAsync(TimeSpan? timeout = null, CancellationToken cancellationToken = default)
    {
        return this.TraceAsync(this.Call("Input value", "inputValue"), this.CreateBudget(timeout, cancellationToken), async budget => (await this.PollElementAsync(budget, async node => (true, await this.ReadValueAsync(node, budget).ConfigureAwait(false), string.Empty)).ConfigureAwait(false))!);
    }

    /// <summary>
    /// Gets the value of an attribute of the element, once one matches.
    /// </summary>
    /// <param name="name">The attribute's name.</param>
    /// <param name="timeout">The time to wait, or <see langword="null"/> for <see cref="DramaturgeOptions.ActionTimeout"/>.</param>
    /// <param name="cancellationToken">A token that cancels the wait.</param>
    /// <returns>The value, or <see langword="null"/> if the element does not have the attribute.</returns>
    /// <exception cref="AmbiguousElementException">Thrown when more than one element matches.</exception>
    /// <exception cref="WebDriverBiDiTimeoutException">Thrown when no element matches in time.</exception>
    public Task<string?> GetAttributeAsync(string name, TimeSpan? timeout = null, CancellationToken cancellationToken = default)
    {
        return this.TraceAsync(this.Call("Get attribute \"{name}\"", "getAttribute", ("name", name)), this.CreateBudget(timeout, cancellationToken), budget => this.PollElementAsync(budget, async node => (true, await this.ReadAttributeAsync(node, name, budget).ConfigureAwait(false), string.Empty)));
    }

    /// <summary>
    /// Gets the box the element occupies, in its frame's viewport, once one matches. For the box in the page's
    /// coordinates, such as for <see cref="Page.Mouse"/>, see <see cref="BoundingBox.ToTopLevelAsync"/>.
    /// </summary>
    /// <param name="timeout">The time to wait, or <see langword="null"/> for <see cref="DramaturgeOptions.ActionTimeout"/>.</param>
    /// <param name="cancellationToken">A token that cancels the wait.</param>
    /// <returns>The box, or <see langword="null"/> if the element is not visible.</returns>
    /// <exception cref="AmbiguousElementException">Thrown when more than one element matches.</exception>
    /// <exception cref="WebDriverBiDiTimeoutException">Thrown when no element matches in time.</exception>
    public Task<BoundingBox?> BoundingBoxAsync(TimeSpan? timeout = null, CancellationToken cancellationToken = default)
    {
        return this.TraceAsync(this.Call("Bounding box", "boundingBox"), this.CreateBudget(timeout, cancellationToken), budget => this.PollElementAsync(budget, async node =>
        {
            RemoteValue box = await this.Group.ScriptHost.CallAsync(this.Frame.Id, "(inspector, element) => { if (!inspector.isElementVisible(element)) { return null; } const { x, y, width, height } = element.getBoundingClientRect(); return { x, y, width, height }; }", [node.ToSharedReference()], budget).ConfigureAwait(false);
            return (true, box is NullRemoteValue ? null : new BoundingBox(this.Frame, Number(box, "x"), Number(box, "y"), Number(box, "width"), Number(box, "height")), string.Empty);
        }));
    }

    /// <summary>
    /// Captures an image of the element, once it is visible and stable, whether or not it is scrolled into view.
    /// </summary>
    /// <param name="format">The image format, or <see langword="null"/> for PNG.</param>
    /// <param name="timeout">The time to wait, or <see langword="null"/> for <see cref="DramaturgeOptions.ActionTimeout"/>.</param>
    /// <param name="cancellationToken">A token that cancels the wait and the capture.</param>
    /// <returns>The image.</returns>
    /// <exception cref="AmbiguousElementException">Thrown when more than one element matches.</exception>
    /// <exception cref="WebDriverBiDiTimeoutException">Thrown when the element is not ready in time.</exception>
    public Task<byte[]> ScreenshotAsync(ImageFormat? format = null, TimeSpan? timeout = null, CancellationToken cancellationToken = default)
    {
        return this.TraceAsync(this.Call("Screenshot", "screenshot"), this.CreateBudget(timeout, cancellationToken), async budget => (await this.PollElementAsync(budget, async node =>
        {
            string? notReady = await this.CheckStatesAsync(node, "['stable', 'visible']", budget).ConfigureAwait(false);
            if (notReady is not null)
            {
                return (false, null, notReady);
            }

            CaptureScreenshotCommandParameters parameters = new(this.Frame.Id) { Clip = new ElementClipRectangle(node.ToSharedReference()), Format = format, Origin = ScreenshotOrigin.Document };
            CaptureScreenshotCommandResult result = await this.Group.Driver.BrowsingContext.CaptureScreenshotAsync(parameters, budget.Remaining, budget.CancellationToken).ConfigureAwait(false);
            return (true, Convert.FromBase64String(result.Data), string.Empty);
        }).ConfigureAwait(false))!);
    }

    /// <summary>
    /// Calls a JavaScript function with the element as its first argument, once one matches, in the page's own
    /// script realm, awaiting a promise it returns.
    /// </summary>
    /// <param name="function">The function's declaration, such as <c>(element, name) =&gt; element.dataset[name]</c>.</param>
    /// <param name="arguments">The arguments after the element, or <see langword="null"/> for none.</param>
    /// <param name="timeout">The time to wait for the element, or <see langword="null"/> for <see cref="DramaturgeOptions.ActionTimeout"/>.</param>
    /// <param name="cancellationToken">A token that cancels the wait and the call.</param>
    /// <returns>The function's result.</returns>
    /// <exception cref="AmbiguousElementException">Thrown when more than one element matches.</exception>
    /// <exception cref="ScriptException">Thrown when the function throws.</exception>
    /// <exception cref="WebDriverBiDiTimeoutException">Thrown when no element matches in time.</exception>
    public Task<RemoteValue> EvaluateAsync(string function, IEnumerable<LocalValue>? arguments = null, TimeSpan? timeout = null, CancellationToken cancellationToken = default)
    {
        List<LocalValue> argumentList = [.. arguments ?? []];
        return this.TraceAsync(this.Call("Evaluate", "evaluate", ("function", function)), this.CreateBudget(timeout, cancellationToken), budget => this.EvaluateCoreAsync(function, argumentList, budget));
    }

    /// <summary>
    /// Calls a JavaScript function with the element as its first argument, as
    /// <see cref="EvaluateAsync(string, IEnumerable{LocalValue}?, TimeSpan?, CancellationToken)"/> does, and converts
    /// its result, as <see cref="Frame.EvaluateAsync{T}"/> does.
    /// </summary>
    /// <typeparam name="T">The type to convert the result to.</typeparam>
    /// <param name="function">The function's declaration.</param>
    /// <param name="arguments">The arguments after the element, or <see langword="null"/> for none.</param>
    /// <param name="timeout">The time to wait for the element, or <see langword="null"/> for <see cref="DramaturgeOptions.ActionTimeout"/>.</param>
    /// <param name="cancellationToken">A token that cancels the wait and the call.</param>
    /// <returns>The function's result, converted.</returns>
    public async Task<T> EvaluateAsync<[DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.PublicParameterlessConstructor)] T>(string function, IEnumerable<LocalValue>? arguments = null, TimeSpan? timeout = null, CancellationToken cancellationToken = default)
    {
        List<LocalValue> argumentList = [.. arguments ?? []];
        return await this.TraceAsync(this.Call("Evaluate", "evaluate", ("function", function)), this.CreateBudget(timeout, cancellationToken), async budget => RemoteValueConverter.Convert<T>(await this.EvaluateCoreAsync(function, argumentList, budget).ConfigureAwait(false))).ConfigureAwait(false);
    }

    /// <summary>
    /// Describes how the locator finds elements, such as <c>css "form" &gt;&gt; css "input" &gt;&gt; nth=1</c>.
    /// </summary>
    /// <returns>The description.</returns>
    public override string ToString()
    {
        return string.Join(" >> ", this.steps.Select(step => step.ToString()));
    }

    /// <summary>
    /// Creates a locator for the element a ref in an accessibility snapshot refers to, for <see cref="AriaSnapshot.Locator"/>.
    /// </summary>
    /// <param name="frame">The frame the element is in.</param>
    /// <param name="node">The element.</param>
    /// <param name="reference">The ref, which describes the locator.</param>
    /// <returns>The locator.</returns>
    internal static ElementLocator ForReference(Frame frame, NodeRemoteValue node, string reference)
    {
        return new ElementLocator(frame, [new ReferenceStep(node, reference)]);
    }

    /// <summary>
    /// Creates a locator for elements by role, name, and states, for <see cref="Frame.GetByRole"/>.
    /// </summary>
    /// <param name="frame">The frame in which elements are found.</param>
    /// <param name="role">The role.</param>
    /// <param name="name">The exact accessible name, or <see langword="null"/> for any.</param>
    /// <param name="states">The states, or <see langword="null"/> for any.</param>
    /// <returns>The locator.</returns>
    internal static ElementLocator ByRole(Frame frame, string role, string? name, RoleStates? states)
    {
        return new ElementLocator(frame, RoleSteps(role, name, states));
    }

    /// <summary>
    /// Creates a locator for elements by label, for <see cref="Frame.GetByLabel"/>.
    /// </summary>
    /// <param name="frame">The frame in which elements are found.</param>
    /// <param name="text">The label text.</param>
    /// <param name="exact">Whether the whole label must match, with case.</param>
    /// <returns>The locator.</returns>
    internal static ElementLocator ByLabel(Frame frame, string text, bool exact)
    {
        return new ElementLocator(frame, [new LabelStep(text, exact)]);
    }

    /// <summary>
    /// Checks an expectation until its condition, negated if asked, holds, or the time runs out.
    /// </summary>
    /// <param name="expected">What the expectation requires, negation included, such as <c>not to be visible</c>.</param>
    /// <param name="isNot">A value indicating whether the expectation is negated.</param>
    /// <param name="timeout">The time to retry, or <see langword="null"/> for <see cref="DramaturgeOptions.ExpectTimeout"/>.</param>
    /// <param name="cancellationToken">A token that cancels the expectation.</param>
    /// <param name="observe">One check, returning <see langword="null"/> when the element was removed while it was checked.</param>
    /// <param name="describeDetails">Describes what the last check saw, in lines after the failure message's first, or <see langword="null"/> for none.</param>
    /// <returns>A task that completes when the expectation is met.</returns>
    /// <exception cref="ExpectationFailedException">Thrown when the expectation is not met in time.</exception>
    internal Task ExpectAsync(string expected, bool isNot, TimeSpan? timeout, CancellationToken cancellationToken, Func<TimeBudget, Task<Observation?>> observe, Func<string, string>? describeDetails = null)
    {
        TimeBudget budget = new(timeout ?? this.Group.Options.ExpectTimeout, this.Group.Options.TimeProvider, cancellationToken);
        TracedCall call = TraceRecording.Call("Expect", $"Expect {expected}", "{locator}", "expect", ("locator", this.ToString()));
        return this.TraceAsync(call, budget, actionBudget => this.ExpectCoreAsync(expected, isNot, observe, describeDetails, actionBudget));
    }

    /// <summary>
    /// Checks an accessibility snapshot template with the library's script in the locator's frame.
    /// </summary>
    /// <param name="template">The template.</param>
    /// <param name="budget">The time the call may take.</param>
    /// <returns>What is wrong with the template, with its line and column, or <see langword="null"/> if it is valid.</returns>
    internal async Task<string?> ValidateAriaTemplateAsync(string template, TimeBudget budget)
    {
        RemoteValue error = await this.Group.ScriptHost.CallSnapshotAsync(this.Frame.Id, "(snapshots, template) => snapshots.validate(template)", [LocalValue.String(template)], budget).ConfigureAwait(false);
        return error is StringRemoteValue message ? message.Value : null;
    }

    /// <summary>
    /// Matches the accessibility snapshot of an element against a template with the library's script.
    /// </summary>
    /// <param name="node">The element.</param>
    /// <param name="template">The template, which must be valid.</param>
    /// <param name="budget">The time the call may take.</param>
    /// <returns>Whether the snapshot matches, and the snapshot as text without refs.</returns>
    internal async Task<(bool Matches, string Actual)> MatchAriaSnapshotAsync(NodeRemoteValue node, string template, TimeBudget budget)
    {
        RemoteValue result = await this.Group.ScriptHost.CallSnapshotAsync(this.Frame.Id, "(snapshots, root, template) => snapshots.match(root, template)", [node.ToSharedReference(), LocalValue.String(template)], budget).ConfigureAwait(false);
        return (Property(result, "matches").As<BooleanRemoteValue>().Value, Property(result, "actual").As<StringRemoteValue>().Value);
    }

    /// <summary>
    /// Finds the one element the locator matches now.
    /// </summary>
    /// <param name="budget">The time the commands may take.</param>
    /// <returns>The element, or <see langword="null"/> if none matches.</returns>
    /// <exception cref="AmbiguousElementException">Thrown when more than one element matches.</exception>
    internal async Task<NodeRemoteValue?> FindOneAsync(TimeBudget budget)
    {
        IList<NodeRemoteValue> nodes = await this.ResolveAsync(StrictMatchLimit, budget).ConfigureAwait(false);
        this.ThrowIfAmbiguous(nodes);
        return nodes.Count == 0 ? null : nodes[0];
    }

    /// <summary>
    /// Finds every element the locator matches now.
    /// </summary>
    /// <param name="budget">The time the commands may take.</param>
    /// <returns>The matching elements, in document order.</returns>
    internal Task<IList<NodeRemoteValue>> FindAllAsync(TimeBudget budget)
    {
        return this.ResolveAsync(null, budget);
    }

    /// <summary>
    /// Reads the text of elements in one call.
    /// </summary>
    /// <param name="nodes">The elements.</param>
    /// <param name="useInnerText">A value indicating whether to read the rendered text rather than the text content.</param>
    /// <param name="budget">The time the command may take.</param>
    /// <returns>Each element's text; for rendered text, <see langword="null"/> for an element that is not HTML.</returns>
    internal async Task<IReadOnlyList<string?>> ReadTextsAsync(IList<NodeRemoteValue> nodes, bool useInnerText, TimeBudget budget)
    {
        LocalValue[] arguments = [LocalValue.Boolean(useInnerText), .. nodes.Select(node => node.ToSharedReference())];
        RemoteValue texts = await this.Group.ScriptHost.CallStateAsync(this.Frame.Id, "(state, useInnerText, ...elements) => state.readTexts(elements, useInnerText)", arguments, budget).ConfigureAwait(false);
        return [.. texts.As<CollectionRemoteValue>().Value!.Select(AsString)];
    }

    /// <summary>
    /// Reads the value of an input, a text area, or a select.
    /// </summary>
    /// <param name="node">The element.</param>
    /// <param name="budget">The time the command may take.</param>
    /// <returns>The value.</returns>
    /// <exception cref="InvalidOperationException">Thrown when the element is not an input, a text area, or a select.</exception>
    internal async Task<string> ReadValueAsync(NodeRemoteValue node, TimeBudget budget)
    {
        RemoteValue value = await this.Group.ScriptHost.CallStateAsync(this.Frame.Id, "(state, element) => state.readValue(element)", [node.ToSharedReference()], budget).ConfigureAwait(false);
        return AsString(value) ?? throw new InvalidOperationException($"{this} is not an input, a text area, or a select.");
    }

    /// <summary>
    /// Reads an attribute of an element.
    /// </summary>
    /// <param name="node">The element.</param>
    /// <param name="name">The attribute's name.</param>
    /// <param name="budget">The time the command may take.</param>
    /// <returns>The value, or <see langword="null"/> if the element does not have the attribute.</returns>
    internal async Task<string?> ReadAttributeAsync(NodeRemoteValue node, string name, TimeBudget budget)
    {
        RemoteValue value = await this.Group.ScriptHost.CallStateAsync(this.Frame.Id, "(state, element, name) => state.readAttribute(element, name)", [node.ToSharedReference(), LocalValue.String(name)], budget).ConfigureAwait(false);
        return AsString(value);
    }

    /// <summary>
    /// Reads the computed value of a CSS property of an element.
    /// </summary>
    /// <param name="node">The element.</param>
    /// <param name="property">The property's name.</param>
    /// <param name="budget">The time the command may take.</param>
    /// <returns>The computed value.</returns>
    internal async Task<string> ReadCssAsync(NodeRemoteValue node, string property, TimeBudget budget)
    {
        RemoteValue value = await this.Group.ScriptHost.CallStateAsync(this.Frame.Id, "(state, element, property) => state.readCss(element, property)", [node.ToSharedReference(), LocalValue.String(property)], budget).ConfigureAwait(false);
        return value.As<StringRemoteValue>().Value;
    }

    /// <summary>
    /// Gets a value indicating whether an element is visible.
    /// </summary>
    /// <param name="node">The element.</param>
    /// <param name="budget">The time the command may take.</param>
    /// <returns><see langword="true"/> if the element is visible; otherwise, <see langword="false"/>.</returns>
    internal async Task<bool> IsVisibleAsync(NodeRemoteValue node, TimeBudget budget)
    {
        RemoteValue visible = await this.CallInspectorAsync(node, "(inspector, element) => inspector.isElementVisible(element)", budget).ConfigureAwait(false);
        return visible.As<BooleanRemoteValue>().Value;
    }

    /// <summary>
    /// Reads the state the element state library reports for an element, such as <c>enabled</c>.
    /// </summary>
    /// <param name="node">The element.</param>
    /// <param name="state">The state to query, such as <c>enabled</c>.</param>
    /// <param name="budget">The time the command may take.</param>
    /// <returns>The reported state, or <see langword="null"/> if the element was removed while it was checked.</returns>
    /// <exception cref="InvalidOperationException">Thrown when the element cannot have the state.</exception>
    internal async Task<string?> ReadElementStateAsync(NodeRemoteValue node, string state, TimeBudget budget)
    {
        RemoteValue? result = await this.QueryElementStateAsync(node, state, budget).ConfigureAwait(false);
        return result is null ? null : Property(result, "received").As<StringRemoteValue>().Value;
    }

    /// <summary>
    /// Calls a function in the sandbox with the element state library and an element.
    /// </summary>
    /// <param name="node">The element.</param>
    /// <param name="functionDeclaration">The function, taking the library and the element.</param>
    /// <param name="budget">The time the command may take.</param>
    /// <returns>The function's result.</returns>
    internal Task<RemoteValue> CallInspectorAsync(NodeRemoteValue node, string functionDeclaration, TimeBudget budget)
    {
        return this.Group.ScriptHost.CallAsync(this.Frame.Id, functionDeclaration, [node.ToSharedReference()], budget);
    }

    /// <summary>
    /// Calls a function in the sandbox with the element state reads and an element.
    /// </summary>
    /// <param name="node">The element.</param>
    /// <param name="functionDeclaration">The function, taking the reads and the element.</param>
    /// <param name="budget">The time the command may take.</param>
    /// <returns>The function's result.</returns>
    internal Task<RemoteValue> CallStateAsync(NodeRemoteValue node, string functionDeclaration, TimeBudget budget)
    {
        return this.Group.ScriptHost.CallStateAsync(this.Frame.Id, functionDeclaration, [node.ToSharedReference()], budget);
    }

    // The browser finds elements by role and name; the states, which the protocol cannot express, filter them.
    private static Step[] RoleSteps(string role, string? name, RoleStates? states)
    {
        LocateStep roleStep = new(ElementQuery.ByRole(role, name));
        return states is null || states.IsEmpty ? [roleStep] : [roleStep, new RoleStatesStep(states)];
    }

    // A node met again may come back as a bare reference to its earlier entry, with no shared ID; it is dropped
    // with the other repeats.
    private static List<NodeRemoteValue> DistinctNodes(IEnumerable<NodeRemoteValue> nodes)
    {
        HashSet<string> seen = [];
        return [.. nodes.Where(node => !string.IsNullOrEmpty(node.SharedId) && seen.Add(node.SharedId!))];
    }

    private static string? AsString(RemoteValue value)
    {
        return value is StringRemoteValue text ? text.Value : null;
    }

    private static RemoteValue Property(RemoteValue value, string name)
    {
        return value.As<KeyValuePairCollectionRemoteValue>().Value!.First(property => property.Key is string key && key == name).Value;
    }

    // The library's names for the checks an element failed, in the terms of a timeout message.
    private static string DescribeNotReady(string reason)
    {
        return reason switch
        {
            "hidden" => "the element was not visible",
            "disabled" => "the element was disabled",
            "readOnly" => "the element was read-only",
            "stable" => "the element was still moving",
            "unviewable" => "the element could not be scrolled into view",
            "notconnected" => "the element was removed from the document",
            _ => $"the element was {reason}",
        };
    }

    private static InputAction MoveTo(PointerInputSource pointer, ActionTarget target)
    {
        return pointer.CreatePointerMove(target.Offset.X, target.Offset.Y, Origin.Element(target.Node.ToSharedReference()));
    }

    private static double Number(RemoteValue value, string name)
    {
        return Property(value, name).As<NumberRemoteValue>().Value;
    }

    private static int OptionIndex(RemoteValue result)
    {
        return (int)Number(result, "index");
    }

    private static LocalValue ToggleValue(ToggleState state)
    {
        return state switch
        {
            ToggleState.On => LocalValue.Boolean(true),
            ToggleState.Off => LocalValue.Boolean(false),
            _ => LocalValue.String("mixed"),
        };
    }

    private static async Task<List<NodeRemoteValue>> KeepByContentAsync(List<NodeRemoteValue> nodes, ElementLocator content, bool keepContaining, TimeBudget budget)
    {
        List<NodeRemoteValue> kept = [];
        foreach (NodeRemoteValue node in nodes)
        {
            List<NodeRemoteValue> matches = await content.ResolveWithinAsync([node], null, budget).ConfigureAwait(false);
            if ((matches.Count > 0) == keepContaining)
            {
                kept.Add(node);
            }
        }

        return kept;
    }

    private TracedCall Call(string title, string method, params (string Name, object Value)[] parameters)
    {
        return TraceRecording.Call("Locator", title, "{locator}", method, [("locator", this.ToString()), .. parameters]);
    }

    private Task<T> TraceAsync<T>(TracedCall call, TimeBudget budget, Func<TimeBudget, Task<T>> action)
    {
        return TraceRecording.RunAsync(this.Frame.Page.Browser, this.Frame.Page, budget, call, action);
    }

    private Task TraceAsync(TracedCall call, TimeBudget budget, Func<TimeBudget, Task> action)
    {
        return TraceRecording.RunAsync(this.Frame.Page.Browser, this.Frame.Page, budget, call, action);
    }

    private Task RecordTargetAsync(NodeRemoteValue node, TimeBudget budget)
    {
        return budget.Trace is ActionTrace trace ? trace.TargetAsync(this.Frame, node, null) : Task.CompletedTask;
    }

    private async Task ExpectCoreAsync(string expected, bool isNot, Func<TimeBudget, Task<Observation?>> observe, Func<string, string>? describeDetails, TimeBudget budget)
    {
        string? actual = null;
        (bool met, bool _, string? observed) = await this.TryPollAsync(budget, async () =>
        {
            Observation? observation = await observe(budget).ConfigureAwait(false);
            if (observation is null)
            {
                return (false, false, DescribeNotReady("notconnected"));
            }

            actual = observation.Actual;
            bool holds = observation.Holds != isNot;
            return (holds, holds, actual is null ? "no element matched" : observation.Summary ?? $"received {actual}");
        }).ConfigureAwait(false);
        if (!met)
        {
            string details = actual is not null && describeDetails is not null ? describeDetails(actual) : string.Empty;
            throw new ExpectationFailedException($"Expected {this} {expected}; {observed} after {budget.Duration.TotalSeconds} seconds.{details}", expected, actual, budget.Duration);
        }
    }

    private async Task<bool> IsVisibleCoreAsync(TimeBudget budget)
    {
        IList<NodeRemoteValue> nodes = await this.ResolveAsync(StrictMatchLimit, budget).ConfigureAwait(false);
        this.ThrowIfAmbiguous(nodes);
        return nodes.Count == 1 && await this.IsVisibleAsync(nodes[0], budget).ConfigureAwait(false);
    }

    private Task ClickCoreAsync(ClickOptions options, int clickCount, TimeBudget budget)
    {
        return this.PerformPointerActionAsync(clickCount > 1 ? "doubleclick" : "click", "clicked", options, budget, (pointer, builder) =>
        {
            for (int index = 0; index < clickCount; index++)
            {
                builder.AddAction(pointer.CreatePointerDown(options.Button)).AddAction(pointer.CreatePointerUp(options.Button));
            }
        });
    }

    private async Task SetCheckedCoreAsync(bool isChecked, PointerActionOptions options, TimeBudget budget)
    {
        string awaited = $"{this} to report whether it is checked";
        CheckedState? state = await this.PollAsync(budget, awaited, () => this.TryReadCheckedStateAsync(budget)).ConfigureAwait(false);
        if (state!.IsChecked == isChecked)
        {
            return;
        }

        if (!isChecked && state.IsRadio)
        {
            throw new InvalidOperationException($"{this} is a radio button, which clicking cannot uncheck.");
        }

        await this.PerformPointerActionAsync("click", "clicked", options, budget, (pointer, builder) => builder.AddAction(pointer.CreatePointerDown()).AddAction(pointer.CreatePointerUp())).ConfigureAwait(false);
        state = await this.PollAsync(budget, awaited, () => this.TryReadCheckedStateAsync(budget)).ConfigureAwait(false);
        if (state!.IsChecked != isChecked)
        {
            throw new InvalidOperationException($"Clicking {this} did not {(isChecked ? "check" : "uncheck")} it.");
        }
    }

    private async Task<RemoteValue> EvaluateCoreAsync(string function, List<LocalValue> argumentList, TimeBudget budget)
    {
        RemoteValue? result = await this.PollElementAsync(budget, async node =>
        {
            RemoteValue value = await this.Group.Driver.Script.CallFunctionAsync(this.Frame.Id, function, [node.ToSharedReference(), .. argumentList], null, budget.Remaining, budget.CancellationToken).ConfigureAwait(false);
            return (true, value, string.Empty);
        }).ConfigureAwait(false);
        return result!;
    }

    private TimeBudget CreateBudget(TimeSpan? timeout, CancellationToken cancellationToken)
    {
        return new TimeBudget(timeout ?? this.Group.Options.ActionTimeout, this.Group.Options.TimeProvider, cancellationToken);
    }

    private ElementLocator Append(Step step)
    {
        return new ElementLocator(this.Frame, [.. this.steps, step]);
    }

    private ElementLocator RequireSameFrame(ElementLocator other, string parameterName)
    {
        return other.Frame == this.Frame ? other : throw new ArgumentException($"{other} finds elements in another frame than {this}.", parameterName);
    }

    private async Task<IList<NodeRemoteValue>> ResolveAsync(ulong? maxCount, TimeBudget budget)
    {
        return await this.ResolveWithinAsync(null, maxCount, budget).ConfigureAwait(false);
    }

    // Applies each step to the previous step's matches. The first step searches within the start nodes, or the
    // whole frame when there are none; the match limit applies only to the last step.
    private async Task<List<NodeRemoteValue>> ResolveWithinAsync(List<NodeRemoteValue>? startNodes, ulong? maxCount, TimeBudget budget)
    {
        List<NodeRemoteValue>? nodes = startNodes;
        for (int i = 0; i < this.steps.Count; i++)
        {
            nodes = await this.steps[i].ApplyAsync(this, nodes, i == this.steps.Count - 1 ? maxCount : null, budget).ConfigureAwait(false);
        }

        return nodes!;
    }

    // Nested start nodes can find an element twice, so matches are de-duplicated, and the match limit is only
    // sent where a duplicate cannot use it up.
    private async Task<List<NodeRemoteValue>> LocateAsync(Locator locator, List<NodeRemoteValue>? startNodes, ulong? maxCount, TimeBudget budget)
    {
        if (startNodes is not null && startNodes.Count == 0)
        {
            return startNodes;
        }

        if (this.Group.Options.PierceShadowRoots && locator is not XPathLocator)
        {
            startNodes = await this.AddOpenShadowRootsAsync(startNodes, budget).ConfigureAwait(false);
        }

        LocateNodesCommandParameters parameters = new(this.Frame.Id, locator)
        {
            MaxNodeCount = startNodes is null || startNodes.Count == 1 ? maxCount : null,
            SerializationOptions = new SerializationOptions() { MaxDomDepth = 0 },
        };
        if (startNodes is not null)
        {
            parameters.StartNodes.AddRange(startNodes.Select(node => node.ToSharedReference()));
        }

        LocateNodesCommandResult result = await this.Group.Driver.BrowsingContext.LocateNodesAsync(parameters, budget.Remaining, budget.CancellationToken).ConfigureAwait(false);
        return DistinctNodes(result.Nodes);
    }

    // A snapshot's element is found while it is in its document. Once its document is gone, the browser no longer
    // knows it, which no wait can change, so that fails at once rather than being retried as a removed element is.
    private async Task<List<NodeRemoteValue>> FindReferencedAsync(ReferenceStep reference, TimeBudget budget)
    {
        try
        {
            RemoteValue connected = await this.Group.Driver.Script.CallFunctionAsync(this.Frame.Id, "(element) => element.isConnected ? element : null", [reference.Node.ToSharedReference()], this.Group.Options.SandboxName, budget.Remaining, budget.CancellationToken).ConfigureAwait(false);
            return connected is NodeRemoteValue node ? [node] : [];
        }
        catch (WebDriverBiDiCommandException ex) when (ex.ErrorCode == ErrorCode.NoSuchNode)
        {
            throw new InvalidOperationException($"The element of ref {reference.Reference} is no longer in its document, which has been replaced or discarded.", ex);
        }
    }

    // The protocol cannot find elements by label, so the library's script does, within the start nodes or the
    // whole document.
    private async Task<List<NodeRemoteValue>> LocateByLabelAsync(LabelStep label, List<NodeRemoteValue>? startNodes, TimeBudget budget)
    {
        if (startNodes is not null && startNodes.Count == 0)
        {
            return startNodes;
        }

        if (this.Group.Options.PierceShadowRoots)
        {
            startNodes = await this.AddOpenShadowRootsAsync(startNodes, budget).ConfigureAwait(false);
        }

        LocalValue[] arguments = [LocalValue.String(label.Text), LocalValue.Boolean(label.Exact), .. (startNodes ?? []).Select(node => node.ToSharedReference())];
        RemoteValue result = await this.Group.ScriptHost.CallAsync(this.Frame.Id, "(inspector, text, exact, ...scopes) => inspector.findElementsByLabel(scopes.length > 0 ? scopes : [document], text, exact)", arguments, budget).ConfigureAwait(false);
        return DistinctNodes(result.As<CollectionRemoteValue>().Value!.Select(node => node.As<NodeRemoteValue>()));
    }

    // The scopes (the start nodes, or the document), followed by the open shadow roots within them; the scopes
    // are left as they were when there are none.
    private async Task<List<NodeRemoteValue>?> AddOpenShadowRootsAsync(List<NodeRemoteValue>? startNodes, TimeBudget budget)
    {
        LocalValue[] arguments = [.. (startNodes ?? []).Select(node => node.ToSharedReference())];
        RemoteValue result = await this.Group.ScriptHost.CallAsync(this.Frame.Id, "(inspector, ...scopes) => { const searched = scopes.length > 0 ? scopes : [document]; const roots = inspector.findOpenShadowRoots(searched); return roots.length > 0 ? [...searched, ...roots] : null; }", arguments, budget).ConfigureAwait(false);
        return result is CollectionRemoteValue scopes ? DistinctNodes(scopes.Value!.Select(node => node.As<NodeRemoteValue>())) : startNodes;
    }

    // The browser serializes each element with its shadow root, open or closed, which script alone could not reach.
    private async Task<List<NodeRemoteValue>> EnterShadowRootsAsync(List<NodeRemoteValue> nodes, TimeBudget budget)
    {
        if (nodes.Count == 0)
        {
            return nodes;
        }

        CallFunctionCommandParameters parameters = new("(...elements) => elements", new ContextTarget(this.Frame.Id) { Sandbox = this.Group.Options.SandboxName }, false)
        {
            SerializationOptions = new SerializationOptions() { MaxDomDepth = 0, IncludeShadowTree = IncludeShadowTreeSerializationOption.All },
        };
        parameters.Arguments.AddRange(nodes.Select(node => node.ToSharedReference()));
        EvaluateResult result = await this.Group.Driver.Script.CallFunctionAsync(parameters, budget.Remaining, budget.CancellationToken).ConfigureAwait(false);
        IEnumerable<NodeRemoteValue> elements = result.As<EvaluateResultSuccess>().Result.As<CollectionRemoteValue>().Value!.Select(node => node.As<NodeRemoteValue>());
        return DistinctNodes(elements.Select(element => element.Value?.ShadowRoot).OfType<NodeRemoteValue>());
    }

    private async Task<List<NodeRemoteValue>> KeepByRoleStatesAsync(RoleStates states, List<NodeRemoteValue> nodes, TimeBudget budget)
    {
        if (nodes.Count == 0)
        {
            return nodes;
        }

        Dictionary<string, LocalValue> expected = [];
        if (states.Checked is not null)
        {
            expected["checked"] = ToggleValue(states.Checked.Value);
        }

        if (states.Pressed is not null)
        {
            expected["pressed"] = ToggleValue(states.Pressed.Value);
        }

        if (states.Expanded is not null)
        {
            expected["expanded"] = LocalValue.Boolean(states.Expanded.Value);
        }

        if (states.Selected is not null)
        {
            expected["selected"] = LocalValue.Boolean(states.Selected.Value);
        }

        if (states.Level is not null)
        {
            expected["level"] = LocalValue.Number(states.Level.Value);
        }

        if (states.Disabled is not null)
        {
            expected["disabled"] = LocalValue.Boolean(states.Disabled.Value);
        }

        LocalValue[] arguments = [LocalValue.Object(expected), .. nodes.Select(node => node.ToSharedReference())];
        RemoteValue result = await this.Group.ScriptHost.CallAsync(this.Frame.Id, "(inspector, states, ...elements) => inspector.elementsMatchAriaStates(elements, states)", arguments, budget).ConfigureAwait(false);
        RemoteValueList matches = result.As<CollectionRemoteValue>().Value!;
        return [.. nodes.Where((node, index) => matches[index].As<BooleanRemoteValue>().Value)];
    }

    private async Task<List<NodeRemoteValue>> FilterAsync(FilterStep filter, List<NodeRemoteValue> nodes, TimeBudget budget)
    {
        if (filter.HasText is not null)
        {
            nodes = await this.KeepByTextAsync(nodes, filter.HasText, true, budget).ConfigureAwait(false);
        }

        if (filter.HasNotText is not null)
        {
            nodes = await this.KeepByTextAsync(nodes, filter.HasNotText, false, budget).ConfigureAwait(false);
        }

        if (filter.Has is not null)
        {
            nodes = await KeepByContentAsync(nodes, filter.Has, true, budget).ConfigureAwait(false);
        }

        if (filter.HasNot is not null)
        {
            nodes = await KeepByContentAsync(nodes, filter.HasNot, false, budget).ConfigureAwait(false);
        }

        return nodes;
    }

    // One call checks every element's text.
    private async Task<List<NodeRemoteValue>> KeepByTextAsync(List<NodeRemoteValue> nodes, string text, bool keepContaining, TimeBudget budget)
    {
        if (nodes.Count == 0)
        {
            return nodes;
        }

        LocalValue[] arguments = [LocalValue.String(text), .. nodes.Select(node => node.ToSharedReference())];
        RemoteValue result = await this.Group.ScriptHost.CallAsync(this.Frame.Id, "(inspector, text, ...elements) => inspector.elementsContainText(elements, text)", arguments, budget).ConfigureAwait(false);
        RemoteValueList contains = result.As<CollectionRemoteValue>().Value!;
        return [.. nodes.Where((node, index) => contains[index].As<BooleanRemoteValue>().Value == keepContaining)];
    }

    // Tries an attempt until it is done or the budget runs out, reporting what the last attempt saw. An element
    // removed while it is checked is looked up again, and so is one whose document was being replaced: a command
    // failing while a navigation starts in the frame is retried whatever the error, as browsers do not all report
    // that case with the protocol's own errors.
    private async Task<T> PollAsync<T>(TimeBudget budget, string awaited, Func<Task<(bool Done, T Result, string Observed)>> attempt)
    {
        budget.Trace?.Log($"waiting for {awaited}");
        (bool done, T result, string? observed) = await this.TryPollAsync(budget, attempt).ConfigureAwait(false);
        return done ? result : throw new WebDriverBiDiTimeoutException($"Timed out after {budget.Duration.TotalSeconds} seconds waiting for {awaited}; {observed}.");
    }

    private async Task<(bool Done, T Result, string? Observed)> TryPollAsync<T>(TimeBudget budget, Func<Task<(bool Done, T Result, string Observed)>> attempt)
    {
        string? observed = null;
        while (true)
        {
            int navigationsStarted = this.Frame.NavigationsStarted;
            try
            {
                (bool done, T result, string seen) = await attempt().ConfigureAwait(false);
                if (done)
                {
                    return (true, result, null);
                }

                observed = seen;
                budget.Trace?.Log(seen);
            }
            catch (WebDriverBiDiCommandException ex) when (ex.ErrorCode == ErrorCode.NoSuchNode)
            {
                observed = "the element was removed while it was checked";
            }
            catch (WebDriverBiDiCommandException) when (this.Frame.NavigationsStarted != navigationsStarted)
            {
                observed = "the frame navigated while the element was checked";
            }
            catch (WebDriverBiDiTimeoutException)
            {
                // Each command is given the rest of the budget, so one that times out has spent it, whatever the
                // budget's own clock says; it observed nothing, and the attempt before it did.
                observed ??= "a command was still running";
                break;
            }

            // The delay ends with the budget, at once if none is left.
            await budget.DelayAsync(this.Group.Options.PollInterval).ConfigureAwait(false);
            if (budget.IsExhausted)
            {
                break;
            }
        }

        return (false, default!, observed);
    }

    // The browser serializes an element's content window as its browsing context ID, which names the frame.
    private async Task<(bool Found, Frame? Frame, string Observed)> FindContentFrameAsync(TimeBudget budget)
    {
        IList<NodeRemoteValue> nodes = await this.ResolveAsync(StrictMatchLimit, budget).ConfigureAwait(false);
        this.ThrowIfAmbiguous(nodes);
        if (nodes.Count == 0)
        {
            return (false, null, "no element matched");
        }

        RemoteValue window = await this.Group.Driver.Script.CallFunctionAsync(this.Frame.Id, "(element) => element.contentWindow", [nodes[0].ToSharedReference()], this.Group.Options.SandboxName, budget.Remaining, budget.CancellationToken).ConfigureAwait(false);
        return window switch
        {
            WindowProxyRemoteValue contentWindow => this.Group.FindFrame(contentWindow.Value.BrowsingContextId) is Frame frame
                ? (true, frame, string.Empty)
                : (false, null, "the frame was not tracked yet"),
            NullRemoteValue => (false, null, "the frame had no document"),
            _ => throw new InvalidOperationException($"{this} is not a frame element."),
        };
    }

    // Waits for the element to be ready for the interaction, then aims the pointer at the point the readiness check
    // chose (or was asked for), relative to the element, which the browser places correctly within frames.
    private async Task PerformPointerActionAsync(string interactionType, string pastTense, PointerActionOptions options, TimeBudget budget, Action<PointerInputSource, InputBuilder> addActions, PointerType pointerType = PointerType.Mouse)
    {
        ActionTarget? target = await this.PollAsync(budget, $"{this} to be ready to be {pastTense}", () => this.FindActionTargetAsync(interactionType, options, budget)).ConfigureAwait(false);
        if (budget.Trace is ActionTrace trace)
        {
            await trace.TargetAsync(this.Frame, target!.Node, target.Offset).ConfigureAwait(false);
        }

        await this.PerformPointerInputAsync(options.Modifiers, pointerType, budget, (pointer, builder) =>
        {
            builder.AddAction(MoveTo(pointer, target!));
            addActions(pointer, builder);
        }).ConfigureAwait(false);
    }

    // Holds the modifier keys down around the pointer's actions.
    private Task PerformPointerInputAsync(KeyModifiers modifiers, PointerType pointerType, TimeBudget budget, Action<PointerInputSource, InputBuilder> addActions)
    {
        InputBuilder builder = new();
        string[] modifierKeys = ModifierKeyValues.For(modifiers);
        foreach (string key in modifierKeys)
        {
            builder.AddAction(builder.DefaultKeyInputSource.CreateKeyDown(key));
        }

        addActions(builder.CreatePointerInputSource(pointerType), builder);
        foreach (string key in Enumerable.Reverse(modifierKeys))
        {
            builder.AddAction(builder.DefaultKeyInputSource.CreateKeyUp(key));
        }

        return this.Group.Driver.Input.PerformActionsAsync(this.Frame.Id, builder, budget.Remaining, budget.CancellationToken);
    }

    private async Task<(bool Found, ActionTarget? Target, string Observed)> FindActionTargetAsync(string interactionType, PointerActionOptions options, TimeBudget budget)
    {
        IList<NodeRemoteValue> nodes = await this.ResolveAsync(StrictMatchLimit, budget).ConfigureAwait(false);
        this.ThrowIfAmbiguous(nodes);
        if (nodes.Count == 0)
        {
            return (false, null, "no element matched");
        }

        NodeRemoteValue node = nodes[0];
        PointerOffset requested = options.Offset ?? default;
        if (options.Force)
        {
            await this.ScrollIntoViewAsync(node, onlyIfOutOfView: true, budget).ConfigureAwait(false);
            return (true, new ActionTarget(node, requested), string.Empty);
        }

        LocalValue offset = LocalValue.Object(new Dictionary<string, LocalValue>() { ["x"] = LocalValue.Number(requested.X), ["y"] = LocalValue.Number(requested.Y) });
        RemoteValue readiness = await this.Group.ScriptHost.CallAsync(this.Frame.Id, "(inspector, element, type, offset) => inspector.isInteractionReady(element, type, offset)", [node.ToSharedReference(), LocalValue.String(interactionType), offset], budget).ConfigureAwait(false);
        switch (Property(readiness, "status").As<StringRemoteValue>().Value)
        {
            case "ready":
                RemoteValue actual = Property(readiness, "interactionOffset");
                return (true, new ActionTarget(node, new PointerOffset(Property(actual, "x").As<NumberRemoteValue>().Value, Property(actual, "y").As<NumberRemoteValue>().Value)), string.Empty);
            case "needsscroll":
                await this.ScrollIntoViewAsync(node, onlyIfOutOfView: false, budget).ConfigureAwait(false);
                return (false, null, "the element was scrolled into view");
            default:
                return (false, null, DescribeNotReady(Property(readiness, "reason").As<StringRemoteValue>().Value));
        }
    }

    private async Task<(bool Found, NodeRemoteValue? Node, string Observed)> TryFindOneAsync(TimeBudget budget)
    {
        NodeRemoteValue? node = await this.FindOneAsync(budget).ConfigureAwait(false);
        return node is null ? (false, null, "no element matched") : (true, node, string.Empty);
    }

    // Waits for exactly one element to match, then makes an attempt on it, until the attempt is done.
    private Task<T?> PollElementAsync<T>(TimeBudget budget, Func<NodeRemoteValue, Task<(bool Done, T? Result, string Observed)>> attempt)
    {
        return this.PollAsync(budget, $"{this} to be attached", async () =>
        {
            (bool found, NodeRemoteValue? node, string observed) = await this.TryFindOneAsync(budget).ConfigureAwait(false);
            return found ? await attempt(node!).ConfigureAwait(false) : (false, default, observed);
        });
    }

    private Task<RemoteValue> CallOnElementAsync(NodeRemoteValue node, string functionDeclaration, IEnumerable<LocalValue> arguments, TimeBudget budget)
    {
        return this.Group.Driver.Script.CallFunctionAsync(this.Frame.Id, functionDeclaration, [node.ToSharedReference(), .. arguments], this.Group.Options.SandboxName, budget.Remaining, budget.CancellationToken);
    }

    private async Task<string> ReadStringAsync(NodeRemoteValue node, string functionDeclaration, IEnumerable<LocalValue> arguments, TimeBudget budget)
    {
        RemoteValue value = await this.CallOnElementAsync(node, functionDeclaration, arguments, budget).ConfigureAwait(false);
        return value.As<StringRemoteValue>().Value;
    }

    // The element's text, or null when its rendered text is asked for and it is not an HTML element.
    private async Task<string?> ReadTextAsync(NodeRemoteValue node, bool useInnerText, TimeBudget budget)
    {
        return (await this.ReadTextsAsync([node], useInnerText, budget).ConfigureAwait(false))[0];
    }

    // Null when the element was removed while it was checked.
    private async Task<RemoteValue?> QueryElementStateAsync(NodeRemoteValue node, string state, TimeBudget budget)
    {
        RemoteValue result = await this.Group.ScriptHost.CallAsync(this.Frame.Id, $"(inspector, element) => inspector.queryElementState(element, '{state}')", [node.ToSharedReference()], budget).ConfigureAwait(false);
        return Property(result, "received").As<StringRemoteValue>().Value switch
        {
            "error:notconnected" => null,
            "error:noteditable" => throw new InvalidOperationException($"{this} is not an editable element."),
            "error:notcheckable" => throw new InvalidOperationException($"{this} is not a checkbox or radio button."),
            _ => result,
        };
    }

    // The state the library reports for the element, such as "enabled"; an element removed while it is checked is
    // looked up again.
    private async Task<string> QueryStateAsync(string state, TimeBudget budget)
    {
        string? received = await this.PollElementAsync(budget, async node =>
        {
            string? received = await this.ReadElementStateAsync(node, state, budget).ConfigureAwait(false);
            return received is null ? (false, null, DescribeNotReady("notconnected")) : (true, received, string.Empty);
        }).ConfigureAwait(false);
        return received!;
    }

    // Null when the element has every state; otherwise, the first missing one, in the terms of a timeout message.
    private async Task<string?> CheckStatesAsync(NodeRemoteValue node, string states, TimeBudget budget)
    {
        RemoteValue result = await this.Group.ScriptHost.CallAsync(this.Frame.Id, $"(inspector, element) => inspector.queryElementStates(element, {states})", [node.ToSharedReference()], budget).ConfigureAwait(false);
        return Property(result, "status").As<StringRemoteValue>().Value switch
        {
            "success" => null,
            "failure" => DescribeNotReady(Property(result, "missingState").As<StringRemoteValue>().Value),
            _ => DescribeNotReady(Property(result, "message").As<StringRemoteValue>().Value),
        };
    }

    private async Task<(bool Done, bool Unused, string Observed)> TryCallOnElementAsync(string functionDeclaration, TimeBudget budget)
    {
        (bool found, NodeRemoteValue? node, string observed) = await this.TryFindOneAsync(budget).ConfigureAwait(false);
        if (!found)
        {
            return (false, false, observed);
        }

        await this.Group.Driver.Script.CallFunctionAsync(this.Frame.Id, functionDeclaration, [node!.ToSharedReference()], this.Group.Options.SandboxName, budget.Remaining, budget.CancellationToken).ConfigureAwait(false);
        return (true, true, string.Empty);
    }

    private async Task<(bool Read, CheckedState? State, string Observed)> TryReadCheckedStateAsync(TimeBudget budget)
    {
        (bool found, NodeRemoteValue? node, string observed) = await this.TryFindOneAsync(budget).ConfigureAwait(false);
        if (!found)
        {
            return (false, null, observed);
        }

        RemoteValue? result = await this.QueryElementStateAsync(node!, "checked", budget).ConfigureAwait(false);
        return result is null
            ? (false, null, DescribeNotReady("notconnected"))
            : (true, new CheckedState(Property(result, "received").As<StringRemoteValue>().Value == "checked", Property(result, "isRadio").As<BooleanRemoteValue>().Value), string.Empty);
    }

    private async Task<(bool Selected, IReadOnlyList<string>? Values, string Observed)> TrySelectOptionsAsync(List<SelectOption> selections, bool force, TimeBudget budget)
    {
        (bool found, NodeRemoteValue? node, string observed) = await this.TryFindOneAsync(budget).ConfigureAwait(false);
        if (!found)
        {
            return (false, null, observed);
        }

        string? notReady = force ? null : await this.CheckStatesAsync(node!, "['visible', 'enabled']", budget).ConfigureAwait(false);
        if (notReady is not null)
        {
            return (false, null, notReady);
        }

        RemoteValue result = await this.Group.ScriptHost.CallActionsAsync(this.Frame.Id, "(actions, element, options) => actions.selectOptions(element, options)", [node!.ToSharedReference(), LocalValue.Array([.. selections.Select(selection => selection.ToLocalValue())])], budget).ConfigureAwait(false);
        switch (Property(result, "status").As<StringRemoteValue>().Value)
        {
            case "selected":
                return (true, [.. Property(result, "values").As<CollectionRemoteValue>().Value!.Select(value => value.As<StringRemoteValue>().Value)], string.Empty);
            case "notselect":
                throw new InvalidOperationException($"{this} is not a <select> element.");
            case "notmultiple":
                throw new InvalidOperationException($"{this} is a <select> element that takes one option, but {selections.Count} were given.");
            case "missing":
                return (false, null, $"no option matched {selections[OptionIndex(result)]}");
            case "disabled":
                return (false, null, $"the option matching {selections[OptionIndex(result)]} was disabled");
            default:
                return (false, null, DescribeNotReady("notconnected"));
        }
    }

    private Task PerformKeyActionsAsync(InputBuilder builder, TimeBudget budget)
    {
        return this.Group.Driver.Input.PerformActionsAsync(this.Frame.Id, builder, budget.Remaining, budget.CancellationToken);
    }

    // Selecting the element's text, then typing, replaces the text as a user would, with the events a user's typing
    // causes; an empty value deletes the selection.
    private async Task ReplaceTextAsync(string value, string interactionType, string pastTense, bool force, TimeBudget budget)
    {
        await this.PollAsync(budget, $"{this} to be ready to be {pastTense}", () => this.TrySelectTextAsync(interactionType, force, budget)).ConfigureAwait(false);
        InputBuilder builder = new();
        await this.PerformKeyActionsAsync(value.Length == 0 ? builder.AddKeyChordAction(Keys.Delete) : builder.AddSendKeysToActiveElementAction(value), budget).ConfigureAwait(false);
    }

    private async Task<(bool Selected, bool Unused, string Observed)> TrySelectTextAsync(string interactionType, bool force, TimeBudget budget)
    {
        IList<NodeRemoteValue> nodes = await this.ResolveAsync(StrictMatchLimit, budget).ConfigureAwait(false);
        this.ThrowIfAmbiguous(nodes);
        if (nodes.Count == 0)
        {
            return (false, false, "no element matched");
        }

        NodeRemoteValue node = nodes[0];
        NodeProperties properties = node.GetNodeProperties();
        if (properties.LocalName == "input" && properties.Attributes is not null && properties.Attributes.TryGetValue("type", out string? type) && UntypeableInputTypes.Contains(type.Trim()))
        {
            throw new InvalidOperationException($"{this} is an input of type \"{type}\", whose value cannot be typed.");
        }

        if (!force)
        {
            RemoteValue readiness = await this.Group.ScriptHost.CallAsync(this.Frame.Id, "(inspector, element, type) => inspector.isInteractionReady(element, type)", [node.ToSharedReference(), LocalValue.String(interactionType)], budget).ConfigureAwait(false);
            switch (Property(readiness, "status").As<StringRemoteValue>().Value)
            {
                case "ready":
                    break;
                case "needsscroll":
                    await this.ScrollIntoViewAsync(node, onlyIfOutOfView: false, budget).ConfigureAwait(false);
                    return (false, false, "the element was scrolled into view");
                default:
                    string reason = Property(readiness, "reason").As<StringRemoteValue>().Value;
                    return reason == "noteditable"
                        ? throw new InvalidOperationException($"{this} is not an editable element.")
                        : (false, false, DescribeNotReady(reason));
            }
        }

        RemoteValue selected = await this.Group.ScriptHost.CallActionsAsync(this.Frame.Id, "(actions, element) => actions.selectText(element)", [node.ToSharedReference()], budget).ConfigureAwait(false);
        return selected.As<BooleanRemoteValue>().Value ? (true, true, string.Empty) : (false, false, DescribeNotReady("notconnected"));
    }

    private async Task<(bool InView, bool Unused, string Observed)> TryScrollIntoViewAsync(TimeBudget budget)
    {
        IList<NodeRemoteValue> nodes = await this.ResolveAsync(StrictMatchLimit, budget).ConfigureAwait(false);
        this.ThrowIfAmbiguous(nodes);
        if (nodes.Count == 0)
        {
            return (false, false, "no element matched");
        }

        RemoteValue states = await this.Group.ScriptHost.CallAsync(this.Frame.Id, "(inspector, element) => inspector.queryElementStates(element, ['stable', 'visible', 'inview'])", [nodes[0].ToSharedReference()], budget).ConfigureAwait(false);
        switch (Property(states, "status").As<StringRemoteValue>().Value)
        {
            case "success":
                return (true, true, string.Empty);
            case "failure" when Property(states, "missingState").As<StringRemoteValue>().Value == "notinview":
                await this.ScrollIntoViewAsync(nodes[0], onlyIfOutOfView: false, budget).ConfigureAwait(false);
                return (false, false, "the element was scrolled into view");
            case "failure":
                return (false, false, DescribeNotReady(Property(states, "missingState").As<StringRemoteValue>().Value));
            default:
                return (false, false, DescribeNotReady(Property(states, "message").As<StringRemoteValue>().Value));
        }
    }

    private Task ScrollIntoViewAsync(NodeRemoteValue node, bool onlyIfOutOfView, TimeBudget budget)
    {
        return this.Group.ScriptHost.CallAsync(this.Frame.Id, "async (inspector, element, onlyIfOutOfView) => { if (!onlyIfOutOfView || !(await inspector.isElementInViewPort(element))) { element.scrollIntoView({ block: 'center', inline: 'center', behavior: 'instant' }); } }", [node.ToSharedReference(), LocalValue.Boolean(onlyIfOutOfView)], budget);
    }

    private async Task<(bool Reached, string Observed)> CheckStateAsync(ElementState state, TimeBudget budget)
    {
        IList<NodeRemoteValue> nodes = await this.ResolveAsync(StrictMatchLimit, budget).ConfigureAwait(false);
        if (state == ElementState.Detached)
        {
            return (nodes.Count == 0, "an element still matched");
        }

        this.ThrowIfAmbiguous(nodes);
        if (nodes.Count == 0)
        {
            return (state == ElementState.Hidden, "no element matched");
        }

        if (state == ElementState.Attached)
        {
            return (true, string.Empty);
        }

        bool visible = await this.IsVisibleAsync(nodes[0], budget).ConfigureAwait(false);
        return (visible == (state == ElementState.Visible), visible ? "the element was visible" : "the element was hidden");
    }

    private void ThrowIfAmbiguous(IList<NodeRemoteValue> nodes)
    {
        if (nodes.Count > 1)
        {
            throw new AmbiguousElementException($"{this} matched more than one element, where one was expected.");
        }
    }

    private abstract record Step
    {
        public abstract Task<List<NodeRemoteValue>> ApplyAsync(ElementLocator owner, List<NodeRemoteValue>? nodes, ulong? maxCount, TimeBudget budget);
    }

    private sealed record LocateStep(Locator Locator, string? Description = null) : Step
    {
        public LocateStep(ElementQuery query)
            : this(query.Locator, query.Description)
        {
        }

        public override Task<List<NodeRemoteValue>> ApplyAsync(ElementLocator owner, List<NodeRemoteValue>? nodes, ulong? maxCount, TimeBudget budget)
        {
            return owner.LocateAsync(this.Locator, nodes, maxCount, budget);
        }

        public override string ToString()
        {
            return this.Description ?? this.Locator switch
            {
                CssLocator css => $"css \"{css.Value}\"",
                XPathLocator xpath => $"xpath \"{xpath.Value}\"",
                InnerTextLocator text => $"text \"{text.Value}\"",
                AccessibilityLocator accessibility => $"accessibility role \"{accessibility.Role}\" name \"{accessibility.Name}\"",
                ContextLocator context => $"context \"{context.BrowsingContextId}\"",
                _ => this.Locator.Type,
            };
        }
    }

    private sealed record IndexStep(int Index) : Step
    {
        public const int LastIndex = -1;

        public override Task<List<NodeRemoteValue>> ApplyAsync(ElementLocator owner, List<NodeRemoteValue>? nodes, ulong? maxCount, TimeBudget budget)
        {
            int position = this.Index == LastIndex ? nodes!.Count - 1 : this.Index;
            return Task.FromResult<List<NodeRemoteValue>>(position >= 0 && position < nodes!.Count ? [nodes[position]] : []);
        }

        public override string ToString()
        {
            return this.Index switch
            {
                0 => "first",
                LastIndex => "last",
                _ => $"nth={this.Index}",
            };
        }
    }

    private sealed record AndStep(ElementLocator Other) : Step
    {
        public override async Task<List<NodeRemoteValue>> ApplyAsync(ElementLocator owner, List<NodeRemoteValue>? nodes, ulong? maxCount, TimeBudget budget)
        {
            if (nodes!.Count == 0)
            {
                return nodes;
            }

            HashSet<string?> others = [.. (await this.Other.ResolveWithinAsync(null, null, budget).ConfigureAwait(false)).Select(node => node.SharedId)];
            return [.. nodes.Where(node => others.Contains(node.SharedId))];
        }

        public override string ToString()
        {
            return $"and({this.Other})";
        }
    }

    private sealed record OrStep(ElementLocator Other) : Step
    {
        public override async Task<List<NodeRemoteValue>> ApplyAsync(ElementLocator owner, List<NodeRemoteValue>? nodes, ulong? maxCount, TimeBudget budget)
        {
            List<NodeRemoteValue> current = nodes!;
            HashSet<string?> found = [.. current.Select(node => node.SharedId)];
            List<NodeRemoteValue> others = await this.Other.ResolveWithinAsync(null, null, budget).ConfigureAwait(false);
            return [.. current, .. others.Where(node => found.Add(node.SharedId))];
        }

        public override string ToString()
        {
            return $"or({this.Other})";
        }
    }

    private sealed record FilterStep(string? HasText, string? HasNotText, ElementLocator? Has, ElementLocator? HasNot) : Step
    {
        public override Task<List<NodeRemoteValue>> ApplyAsync(ElementLocator owner, List<NodeRemoteValue>? nodes, ulong? maxCount, TimeBudget budget)
        {
            return owner.FilterAsync(this, nodes!, budget);
        }

        public override string ToString()
        {
            List<string> conditions = [];
            if (this.HasText is not null)
            {
                conditions.Add($"hasText \"{this.HasText}\"");
            }

            if (this.HasNotText is not null)
            {
                conditions.Add($"hasNotText \"{this.HasNotText}\"");
            }

            if (this.Has is not null)
            {
                conditions.Add($"has({this.Has})");
            }

            if (this.HasNot is not null)
            {
                conditions.Add($"hasNot({this.HasNot})");
            }

            return $"filter({string.Join(", ", conditions)})";
        }
    }

    private sealed record RoleStatesStep(RoleStates States) : Step
    {
        public override Task<List<NodeRemoteValue>> ApplyAsync(ElementLocator owner, List<NodeRemoteValue>? nodes, ulong? maxCount, TimeBudget budget)
        {
            return owner.KeepByRoleStatesAsync(this.States, nodes!, budget);
        }

        public override string ToString()
        {
            List<string> states = [];
            if (this.States.Checked is not null)
            {
                states.Add($"checked={this.States.Checked.Value.ToString().ToLowerInvariant()}");
            }

            if (this.States.Pressed is not null)
            {
                states.Add($"pressed={this.States.Pressed.Value.ToString().ToLowerInvariant()}");
            }

            if (this.States.Expanded is not null)
            {
                states.Add($"expanded={this.States.Expanded.Value.ToString().ToLowerInvariant()}");
            }

            if (this.States.Selected is not null)
            {
                states.Add($"selected={this.States.Selected.Value.ToString().ToLowerInvariant()}");
            }

            if (this.States.Level is not null)
            {
                states.Add($"level={this.States.Level.Value}");
            }

            if (this.States.Disabled is not null)
            {
                states.Add($"disabled={this.States.Disabled.Value.ToString().ToLowerInvariant()}");
            }

            return $"states({string.Join(", ", states)})";
        }
    }

    private sealed record LabelStep(string Text, bool Exact) : Step
    {
        public override Task<List<NodeRemoteValue>> ApplyAsync(ElementLocator owner, List<NodeRemoteValue>? nodes, ulong? maxCount, TimeBudget budget)
        {
            return owner.LocateByLabelAsync(this, nodes, budget);
        }

        public override string ToString()
        {
            return this.Exact ? $"getByLabel \"{this.Text}\" exact" : $"getByLabel \"{this.Text}\"";
        }
    }

    private sealed record ReferenceStep(NodeRemoteValue Node, string Reference) : Step
    {
        public override Task<List<NodeRemoteValue>> ApplyAsync(ElementLocator owner, List<NodeRemoteValue>? nodes, ulong? maxCount, TimeBudget budget)
        {
            return owner.FindReferencedAsync(this, budget);
        }

        public override string ToString()
        {
            return $"ref {this.Reference}";
        }
    }

    private sealed record ShadowRootStep : Step
    {
        public override Task<List<NodeRemoteValue>> ApplyAsync(ElementLocator owner, List<NodeRemoteValue>? nodes, ulong? maxCount, TimeBudget budget)
        {
            return owner.EnterShadowRootsAsync(nodes!, budget);
        }

        public override string ToString()
        {
            return "shadowRoot";
        }
    }

    private sealed record ActionTarget(NodeRemoteValue Node, PointerOffset Offset);

    private sealed record CheckedState(bool IsChecked, bool IsRadio);
}
