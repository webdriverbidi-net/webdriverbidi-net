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

    private TimeBudget CreateBudget(TimeSpan? timeout, CancellationToken cancellationToken)
    {
        return new TimeBudget(timeout ?? this.Group.Options.ActionTimeout, this.Group.Options.TimeProvider, cancellationToken);
    }

    private ElementLocator Append(Step step)
    {
        return new ElementLocator(this.Frame, [.. this.steps, step]);
    }

    // Each step searches within the previous step's matches. Nested start nodes can find an element twice, so
    // matches are de-duplicated, and the match limit is only sent where a duplicate cannot use it up. A repeat
    // may come back as a bare reference to the earlier entry, with no shared ID; it is dropped with the others.
    private async Task<IList<NodeRemoteValue>> ResolveAsync(ulong? maxCount, TimeBudget budget)
    {
        List<NodeRemoteValue>? nodes = null;
        for (int i = 0; i < this.steps.Count; i++)
        {
            if (this.steps[i] is IndexStep indexStep)
            {
                nodes = indexStep.Select(nodes!);
                continue;
            }

            if (nodes is not null && nodes.Count == 0)
            {
                return nodes;
            }

            LocateNodesCommandParameters parameters = new(this.Frame.Id, ((LocateStep)this.steps[i]).Locator)
            {
                MaxNodeCount = i == this.steps.Count - 1 && (nodes is null || nodes.Count == 1) ? maxCount : null,
                SerializationOptions = new SerializationOptions() { MaxDomDepth = 0 },
            };
            if (nodes is not null)
            {
                parameters.StartNodes.AddRange(nodes.Select(node => node.ToSharedReference()));
            }

            LocateNodesCommandResult result = await this.Group.Driver.BrowsingContext.LocateNodesAsync(parameters, budget.Remaining, budget.CancellationToken).ConfigureAwait(false);
            HashSet<string> seen = [];
            nodes = [.. result.Nodes.Where(node => !string.IsNullOrEmpty(node.SharedId) && seen.Add(node.SharedId!))];
        }

        return nodes!;
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

    private abstract record Step;

    private sealed record LocateStep(Locator Locator) : Step
    {
        public override string ToString()
        {
            return this.Locator switch
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

        public List<NodeRemoteValue> Select(List<NodeRemoteValue> nodes)
        {
            int position = this.Index == LastIndex ? nodes.Count - 1 : this.Index;
            return position >= 0 && position < nodes.Count ? [nodes[position]] : [];
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
}
