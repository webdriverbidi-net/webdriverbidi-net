// <copyright file="ElementLocator.cs" company="WebDriverBiDi.NET Committers">
// Copyright (c) WebDriverBiDi.NET Committers. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.
// </copyright>

namespace WebDriverBiDi.Automation;

using WebDriverBiDi.BrowsingContext;
using WebDriverBiDi.Script;

/// <summary>
/// A description of how to find elements in a <see cref="Automation.Frame"/>. Nothing is looked up until an
/// operation runs, and each operation looks the elements up again, so a locator stays valid as the page changes.
/// </summary>
public sealed class ElementLocator
{
    // Enough to tell one match from several without serializing every match.
    private const ulong StrictMatchLimit = 2;

    private readonly Locator locator;

    /// <summary>
    /// Initializes a new instance of the <see cref="ElementLocator"/> class.
    /// </summary>
    /// <param name="frame">The frame in which elements are found.</param>
    /// <param name="locator">How the elements are found.</param>
    internal ElementLocator(Frame frame, Locator locator)
    {
        this.Frame = frame;
        this.locator = locator;
    }

    /// <summary>
    /// Gets the frame in which elements are found.
    /// </summary>
    public Frame Frame { get; }

    private BrowserGroup Group => this.Frame.Page.Browser.Group;

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
    /// Describes how the locator finds elements, such as <c>css "button.primary"</c>.
    /// </summary>
    /// <returns>The description.</returns>
    public override string ToString()
    {
        return this.locator switch
        {
            CssLocator css => $"css \"{css.Value}\"",
            XPathLocator xpath => $"xpath \"{xpath.Value}\"",
            InnerTextLocator text => $"text \"{text.Value}\"",
            AccessibilityLocator accessibility => $"accessibility role \"{accessibility.Role}\" name \"{accessibility.Name}\"",
            ContextLocator context => $"context \"{context.BrowsingContextId}\"",
            _ => this.locator.Type,
        };
    }

    private TimeBudget CreateBudget(TimeSpan? timeout, CancellationToken cancellationToken)
    {
        return new TimeBudget(timeout ?? this.Group.Options.ActionTimeout, this.Group.Options.TimeProvider, cancellationToken);
    }

    private async Task<IList<NodeRemoteValue>> ResolveAsync(ulong? maxCount, TimeBudget budget)
    {
        LocateNodesCommandParameters parameters = new(this.Frame.Id, this.locator)
        {
            MaxNodeCount = maxCount,
            SerializationOptions = new SerializationOptions() { MaxDomDepth = 0 },
        };
        LocateNodesCommandResult result = await this.Group.Driver.BrowsingContext.LocateNodesAsync(parameters, budget.Remaining, budget.CancellationToken).ConfigureAwait(false);
        return result.Nodes;
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
}
