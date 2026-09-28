// <copyright file="ElementLocator.cs" company="WebDriverBiDi.NET Committers">
// Copyright (c) WebDriverBiDi.NET Committers. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.
// </copyright>

namespace WebDriverBiDi.Automation;

using WebDriverBiDi.BrowsingContext;
using WebDriverBiDi.Script;

/// <summary>
/// A description of how to find elements in a <see cref="Automation.Frame"/>: a chain of steps, each finding
/// elements within those the previous step found, or picking one of them by position. Nothing is looked up until
/// an operation runs, and each operation looks the elements up again, so a locator stays valid as the page changes.
/// A locator is immutable; chaining creates a new one.
/// </summary>
public sealed class ElementLocator
{
    // Enough to tell one match from several without serializing every match.
    private const ulong StrictMatchLimit = 2;

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
    /// Creates a locator for elements by their test ID, the value of the <see cref="AutomationOptions.TestIdAttribute"/> attribute, within the elements this locator finds.
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
    /// <param name="role">The role.</param>
    /// <param name="name">The exact accessible name, or <see langword="null"/> for any.</param>
    /// <param name="states">ARIA states the elements must have, such as checked, or <see langword="null"/> for any.</param>
    /// <returns>The locator.</returns>
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
    public async Task<int> CountAsync(CancellationToken cancellationToken = default)
    {
        TimeBudget budget = this.CreateBudget(null, cancellationToken);
        return (await this.ResolveAsync(null, budget).ConfigureAwait(false)).Count;
    }

    /// <summary>
    /// Gets a value indicating whether the element is visible now, without waiting. No matching element is not
    /// visible.
    /// </summary>
    /// <param name="cancellationToken">A token that cancels the commands.</param>
    /// <returns><see langword="true"/> if the element is visible; otherwise, <see langword="false"/>.</returns>
    /// <exception cref="AmbiguousElementException">Thrown when more than one element matches.</exception>
    public async Task<bool> IsVisibleAsync(CancellationToken cancellationToken = default)
    {
        TimeBudget budget = this.CreateBudget(null, cancellationToken);
        IList<NodeRemoteValue> nodes = await this.ResolveAsync(StrictMatchLimit, budget).ConfigureAwait(false);
        this.ThrowIfAmbiguous(nodes);
        return nodes.Count == 1 && await this.IsVisibleAsync(nodes[0], budget).ConfigureAwait(false);
    }

    /// <summary>
    /// Waits for the element to reach a state, looking it up again until it does.
    /// </summary>
    /// <param name="state">The state.</param>
    /// <param name="timeout">The time to wait, or <see langword="null"/> for <see cref="AutomationOptions.ActionTimeout"/>.</param>
    /// <param name="cancellationToken">A token that cancels the wait.</param>
    /// <returns>A task that completes when the element is in the state.</returns>
    /// <exception cref="AmbiguousElementException">Thrown when more than one element matches, for any state but <see cref="ElementState.Detached"/>.</exception>
    /// <exception cref="WebDriverBiDiTimeoutException">Thrown when the element does not reach the state in time.</exception>
    public async Task WaitForAsync(ElementState state = ElementState.Visible, TimeSpan? timeout = null, CancellationToken cancellationToken = default)
    {
        TimeBudget budget = this.CreateBudget(timeout, cancellationToken);
        string? observed = null;
        while (true)
        {
            try
            {
                bool reached;
                (reached, observed) = await this.CheckStateAsync(state, budget).ConfigureAwait(false);
                if (reached)
                {
                    return;
                }
            }
            catch (WebDriverBiDiCommandException ex) when (ex.ErrorCode == ErrorCode.NoSuchNode)
            {
                observed = "the element was removed while it was checked";
            }
            catch (WebDriverBiDiTimeoutException) when (budget.IsExhausted)
            {
                // A command cut short by the end of the budget observed nothing; the attempt before it did.
                observed ??= "a command was still running";
            }

            if (budget.IsExhausted)
            {
                break;
            }

            await budget.DelayAsync(this.Group.Options.PollInterval).ConfigureAwait(false);
            if (budget.IsExhausted)
            {
                break;
            }
        }

        throw new WebDriverBiDiTimeoutException($"Timed out after {budget.Duration.TotalSeconds} seconds waiting for {this} to be {state.ToString().ToLowerInvariant()}; {observed}.");
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

    // The protocol cannot find elements by label, so the library's script does, within the start nodes or the
    // whole document.
    private async Task<List<NodeRemoteValue>> LocateByLabelAsync(LabelStep label, List<NodeRemoteValue>? startNodes, TimeBudget budget)
    {
        if (startNodes is not null && startNodes.Count == 0)
        {
            return startNodes;
        }

        LocalValue[] arguments = [LocalValue.String(label.Text), LocalValue.Boolean(label.Exact), .. (startNodes ?? []).Select(node => node.ToSharedReference())];
        RemoteValue result = await this.Group.ScriptHost.CallAsync(this.Frame.Id, "(inspector, text, exact, ...scopes) => inspector.findElementsByLabel(scopes.length > 0 ? scopes : [document], text, exact)", arguments, budget).ConfigureAwait(false);
        return DistinctNodes(result.As<CollectionRemoteValue>().Value!.Select(node => node.As<NodeRemoteValue>()));
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

    private async Task<bool> IsVisibleAsync(NodeRemoteValue node, TimeBudget budget)
    {
        RemoteValue visible = await this.Group.ScriptHost.CallAsync(this.Frame.Id, "(inspector, element) => inspector.isElementVisible(element)", [node.ToSharedReference()], budget).ConfigureAwait(false);
        return visible.As<BooleanRemoteValue>().Value;
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
}
