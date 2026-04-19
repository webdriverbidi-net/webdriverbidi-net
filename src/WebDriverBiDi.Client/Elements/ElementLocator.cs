// <copyright file="ElementLocator.cs" company="WebDriverBiDi.NET Committers">
// Copyright (c) WebDriverBiDi.NET Committers. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.
// </copyright>

namespace WebDriverBiDi.Client.Elements;

using WebDriverBiDi.BrowsingContext;
using WebDriverBiDi.Client.Inputs;
using WebDriverBiDi.Input;
using WebDriverBiDi.Script;

/// <summary>
/// Provides a Playwright-inspired API for locating and interacting with web elements in the WebDriver BiDi protocol.
/// </summary>
public class ElementLocator
{
    private readonly BiDiDriver driver;
    private readonly string browsingContextId;
    private readonly Locator locator;
    private readonly ElementLocator? parent;
    private readonly ElementStateInspector inspector;
    private readonly ElementLocatorSettings settings;
    private readonly int? specificIndex;

    /// <summary>
    /// Initializes a new instance of the <see cref="ElementLocator"/> class.
    /// </summary>
    /// <param name="driver">The <see cref="BiDiDriver"/> instance used for executing commands.</param>
    /// <param name="browsingContextId">The ID of the browsing context containing the element(s).</param>
    /// <param name="locator">The <see cref="Locate"/> strategy to find element(s).</param>
    public ElementLocator(BiDiDriver driver, string browsingContextId, Locator locator)
        : this(driver, browsingContextId, locator, null, null, null, null)
    {
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="ElementLocator"/> class.
    /// </summary>
    /// <param name="driver">The <see cref="BiDiDriver"/> instance used for executing commands.</param>
    /// <param name="browsingContextId">The ID of the browsing context containing the element(s).</param>
    /// <param name="locator">The <see cref="Locate"/> strategy to find element(s).</param>
    /// <param name="settings">The <see cref="ElementLocatorSettings"/> for this locator.</param>
    public ElementLocator(BiDiDriver driver, string browsingContextId, Locator locator, ElementLocatorSettings settings)
        : this(driver, browsingContextId, locator, null, null, settings, null)
    {
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="ElementLocator"/> class for internal chaining.
    /// </summary>
    /// <param name="driver">The <see cref="BiDiDriver"/> instance used for executing commands.</param>
    /// <param name="browsingContextId">The ID of the browsing context containing the element(s).</param>
    /// <param name="locator">The <see cref="Locate"/> strategy to find element(s).</param>
    /// <param name="parent">The parent <see cref="ElementLocator"/> in the chain.</param>
    /// <param name="inspector">The <see cref="ElementStateInspector"/> for auto-wait functionality.</param>
    /// <param name="settings">The <see cref="ElementLocatorSettings"/> for this locator.</param>
    /// <param name="specificIndex">The specific index to select from multiple matches, if any.</param>
    internal ElementLocator(BiDiDriver driver, string browsingContextId, Locator locator, ElementLocator? parent, ElementStateInspector? inspector, ElementLocatorSettings? settings, int? specificIndex)
    {
        this.driver = driver;
        this.browsingContextId = browsingContextId;
        this.locator = locator;
        this.parent = parent;
        this.inspector = inspector ?? new ElementStateInspector(driver);
        this.settings = settings ?? new ElementLocatorSettings();
        this.specificIndex = specificIndex;
    }

    /// <summary>
    /// Creates a new <see cref="ElementLocator"/> that finds elements matching the specified locator within the context of this locator's matched elements.
    /// </summary>
    /// <param name="childLocator">The <see cref="Locate"/> strategy for the child elements.</param>
    /// <returns>A new <see cref="ElementLocator"/> instance representing the child locator.</returns>
    public ElementLocator Locate(Locator childLocator)
    {
        return new ElementLocator(this.driver, this.browsingContextId, childLocator, this, this.inspector, this.settings, null);
    }

    /// <summary>
    /// Returns a locator for the first matching element.
    /// </summary>
    /// <param name="timeout">Optional timeout override. If null, uses global timeout.</param>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation, containing an <see cref="ElementLocator"/> for the first element.</returns>
    public async Task<ElementLocator> FirstAsync(TimeSpan? timeout = null)
    {
        TimeSpan effectiveTimeout = this.GetTimeout(timeout);
        using CancellationTokenSource cts = new(effectiveTimeout);

        try
        {
            Task timeoutTask = Task.Delay(Timeout.InfiniteTimeSpan, cts.Token);
            IList<NodeRemoteValue> elements = await this.ResolveElementsAsync(timeoutTask).ConfigureAwait(false);
            if (elements.Count == 0)
            {
                throw new ElementNotFoundException($"No elements found matching locator: {this.GetLocatorDescription()}");
            }

            return new ElementLocator(this.driver, this.browsingContextId, this.locator, this.parent, this.inspector, this.settings, 0);
        }
        finally
        {
            cts.Cancel();
        }
    }

    /// <summary>
    /// Returns a locator for the nth matching element (0-indexed).
    /// </summary>
    /// <param name="index">The zero-based index of the element to select.</param>
    /// <param name="timeout">Optional timeout override. If null, uses global timeout.</param>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation, containing an <see cref="ElementLocator"/> for the nth element.</returns>
    public async Task<ElementLocator> NthAsync(int index, TimeSpan? timeout = null)
    {
        TimeSpan effectiveTimeout = this.GetTimeout(timeout);
        using CancellationTokenSource cts = new(effectiveTimeout);

        try
        {
            Task timeoutTask = Task.Delay(Timeout.InfiniteTimeSpan, cts.Token);
            IList<NodeRemoteValue> elements = await this.ResolveElementsAsync(timeoutTask).ConfigureAwait(false);
            if (elements.Count == 0)
            {
                throw new ElementNotFoundException($"No elements found matching locator: {this.GetLocatorDescription()}");
            }

            if (index < 0 || index >= elements.Count)
            {
                throw new ArgumentOutOfRangeException(nameof(index), $"Index {index} is out of range for {elements.Count} elements");
            }

            return new ElementLocator(this.driver, this.browsingContextId, this.locator, this.parent, this.inspector, this.settings, index);
        }
        finally
        {
            cts.Cancel();
        }
    }

    /// <summary>
    /// Returns a locator for the last matching element.
    /// </summary>
    /// <param name="timeout">Optional timeout override. If null, uses global timeout.</param>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation, containing an <see cref="ElementLocator"/> for the last element.</returns>
    public async Task<ElementLocator> LastAsync(TimeSpan? timeout = null)
    {
        TimeSpan effectiveTimeout = this.GetTimeout(timeout);
        using CancellationTokenSource cts = new(effectiveTimeout);

        try
        {
            Task timeoutTask = Task.Delay(Timeout.InfiniteTimeSpan, cts.Token);
            IList<NodeRemoteValue> elements = await this.ResolveElementsAsync(timeoutTask).ConfigureAwait(false);
            if (elements.Count == 0)
            {
                throw new ElementNotFoundException($"No elements found matching locator: {this.GetLocatorDescription()}");
            }

            return new ElementLocator(this.driver, this.browsingContextId, this.locator, this.parent, this.inspector, this.settings, elements.Count - 1);
        }
        finally
        {
            cts.Cancel();
        }
    }

    /// <summary>
    /// Returns a list of locators, one for each matching element.
    /// </summary>
    /// <param name="timeout">Optional timeout override. If null, uses global timeout.</param>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation, containing a read-only list of <see cref="ElementLocator"/> instances.</returns>
    public async Task<IReadOnlyList<ElementLocator>> AllAsync(TimeSpan? timeout = null)
    {
        TimeSpan effectiveTimeout = this.GetTimeout(timeout);
        using CancellationTokenSource cts = new(effectiveTimeout);

        try
        {
            Task timeoutTask = Task.Delay(Timeout.InfiniteTimeSpan, cts.Token);
            IList<NodeRemoteValue> elements = await this.ResolveElementsAsync(timeoutTask).ConfigureAwait(false);
            List<ElementLocator> locators = new();
            for (int i = 0; i < elements.Count; i++)
            {
                locators.Add(new ElementLocator(this.driver, this.browsingContextId, this.locator, this.parent, this.inspector, this.settings, i));
            }

            return locators.AsReadOnly();
        }
        finally
        {
            cts.Cancel();
        }
    }

    /// <summary>
    /// Clicks the element. Requires exactly one matching element.
    /// </summary>
    /// <param name="navigationBehavior">The navigation behavior expected after clicking.</param>
    /// <param name="timeout">Optional timeout override. If null, uses global timeout.</param>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
    public async Task ClickAsync(ClickNavigationBehavior navigationBehavior = ClickNavigationBehavior.None, TimeSpan? timeout = null)
    {
        TimeSpan effectiveTimeout = this.GetTimeout(timeout);
        using CancellationTokenSource cts = new(effectiveTimeout);
        EventObserver<NavigationEventArgs>? observer = null;

        try
        {
            Task timeoutTask = Task.Delay(Timeout.InfiniteTimeSpan, cts.Token);

            SharedReference element = await this.ResolveSingleElementAsync(timeoutTask).ConfigureAwait(false);
            await this.inspector.WaitForInteractionReadyAsync(this.browsingContextId, element, InteractionType.Click, timeoutTask).ConfigureAwait(false);

            TaskCompletionSource<bool> navigationCompletionSource = new();

            // Set up the appropriate event handler based on navigation behavior
            Action<NavigationEventArgs> navigationHandler = (args) =>
            {
                if (args.BrowsingContextId == this.browsingContextId)
                {
                    navigationCompletionSource.TrySetResult(true);
                }
            };

            switch (navigationBehavior)
            {
                case ClickNavigationBehavior.WaitForNavigationStart:
                    observer = this.driver.BrowsingContext.OnNavigationStarted.AddObserver(navigationHandler);
                    break;

                case ClickNavigationBehavior.WaitForDomContentLoadedEvent:
                    observer = this.driver.BrowsingContext.OnDomContentLoaded.AddObserver(navigationHandler);
                    break;

                case ClickNavigationBehavior.WaitForLoadEvent:
                    observer = this.driver.BrowsingContext.OnLoad.AddObserver(navigationHandler);
                    break;
            }

            // Perform the click
            InputBuilder builder = new();
            builder.AddClickOnElementAction(element);
            PerformActionsCommandParameters performParams = new(this.browsingContextId);
            performParams.Actions.AddRange(builder.Build());
            await this.driver.Input.PerformActionsAsync(performParams).ConfigureAwait(false);

            if (observer is not null)
            {
                // Wait for navigation to complete
                Task navigationTask = navigationCompletionSource.Task;
                Task completedTask = await Task.WhenAny(navigationTask, timeoutTask).ConfigureAwait(false);

                if (completedTask == timeoutTask)
                {
                    throw new WebDriverBiDiTimeoutException($"Timed out waiting for navigation to complete after clicking element. Timeout: {effectiveTimeout.TotalSeconds} seconds");
                }
            }
        }
        finally
        {
            cts.Cancel();

            // Clean up event handler
            if (observer != null)
            {
                await observer.DisposeAsync();
            }
        }
    }

    /// <summary>
    /// Double-clicks the element. Requires exactly one matching element.
    /// </summary>
    /// <param name="timeout">Optional timeout override. If null, uses global timeout.</param>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
    public async Task DoubleClickAsync(TimeSpan? timeout = null)
    {
        TimeSpan effectiveTimeout = this.GetTimeout(timeout);
        using CancellationTokenSource cts = new(effectiveTimeout);

        try
        {
            Task timeoutTask = Task.Delay(Timeout.InfiniteTimeSpan, cts.Token);
            SharedReference element = await this.ResolveSingleElementAsync(timeoutTask).ConfigureAwait(false);
            await this.inspector.WaitForInteractionReadyAsync(this.browsingContextId, element, InteractionType.DoubleClick, timeoutTask).ConfigureAwait(false);

            InputBuilder builder = new();
            PointerInputSource pointer = builder.DefaultPointerInputSource;
            builder.AddAction(pointer.CreatePointerMove(0, 0, Origin.Element(new ElementOrigin(element))))
                .AddAction(pointer.CreatePointerDown(PointerButton.Left))
                .AddAction(pointer.CreatePointerUp())
                .AddAction(pointer.CreatePointerDown(PointerButton.Left))
                .AddAction(pointer.CreatePointerUp());

            PerformActionsCommandParameters performParams = new(this.browsingContextId);
            performParams.Actions.AddRange(builder.Build());
            await this.driver.Input.PerformActionsAsync(performParams).ConfigureAwait(false);
        }
        finally
        {
            cts.Cancel();
        }
    }

    /// <summary>
    /// Right-clicks the element (opens context menu). Requires exactly one matching element.
    /// </summary>
    /// <param name="timeout">Optional timeout override. If null, uses global timeout.</param>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
    public async Task ContextClickAsync(TimeSpan? timeout = null)
    {
        TimeSpan effectiveTimeout = this.GetTimeout(timeout);
        using CancellationTokenSource cts = new(effectiveTimeout);

        try
        {
            Task timeoutTask = Task.Delay(Timeout.InfiniteTimeSpan, cts.Token);
            SharedReference element = await this.ResolveSingleElementAsync(timeoutTask).ConfigureAwait(false);
            await this.inspector.WaitForInteractionReadyAsync(this.browsingContextId, element, InteractionType.Click, timeoutTask).ConfigureAwait(false);

            InputBuilder builder = new();
            PointerInputSource pointer = builder.DefaultPointerInputSource;
            builder.AddAction(pointer.CreatePointerMove(0, 0, Origin.Element(new ElementOrigin(element))))
                .AddAction(pointer.CreatePointerDown(PointerButton.Right))
                .AddAction(pointer.CreatePointerUp(PointerButton.Right));

            PerformActionsCommandParameters performParams = new(this.browsingContextId);
            performParams.Actions.AddRange(builder.Build());
            await this.driver.Input.PerformActionsAsync(performParams).ConfigureAwait(false);
        }
        finally
        {
            cts.Cancel();
        }
    }

    /// <summary>
    /// Types text into the element. Requires exactly one matching element.
    /// </summary>
    /// <param name="text">The text to type. Supports special keys from the <see cref="Keys"/> class.</param>
    /// <param name="timeout">Optional timeout override. If null, uses global timeout.</param>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
    public async Task TypeAsync(string text, TimeSpan? timeout = null)
    {
        TimeSpan effectiveTimeout = this.GetTimeout(timeout);
        using CancellationTokenSource cts = new(effectiveTimeout);

        try
        {
            Task timeoutTask = Task.Delay(Timeout.InfiniteTimeSpan, cts.Token);
            SharedReference element = await this.ResolveSingleElementAsync(timeoutTask).ConfigureAwait(false);
            await this.inspector.WaitForInteractionReadyAsync(this.browsingContextId, element, InteractionType.Type, timeoutTask).ConfigureAwait(false);

            // Focus the element first
            await this.ExecuteScriptOnElementAsync(element, "(e) => e.focus()").ConfigureAwait(false);

            // Type the text
            InputBuilder builder = new();
            builder.AddSendKeysToActiveElementAction(text);
            PerformActionsCommandParameters performParams = new(this.browsingContextId);
            performParams.Actions.AddRange(builder.Build());
            await this.driver.Input.PerformActionsAsync(performParams).ConfigureAwait(false);
        }
        finally
        {
            cts.Cancel();
        }
    }

    /// <summary>
    /// Clears the text content of the element. Requires exactly one matching element.
    /// </summary>
    /// <param name="timeout">Optional timeout override. If null, uses global timeout.</param>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
    public async Task ClearAsync(TimeSpan? timeout = null)
    {
        TimeSpan effectiveTimeout = this.GetTimeout(timeout);
        using CancellationTokenSource cts = new(effectiveTimeout);

        try
        {
            Task timeoutTask = Task.Delay(Timeout.InfiniteTimeSpan, cts.Token);
            SharedReference element = await this.ResolveSingleElementAsync(timeoutTask).ConfigureAwait(false);
            await this.inspector.WaitForInteractionReadyAsync(this.browsingContextId, element, InteractionType.Clear, timeoutTask).ConfigureAwait(false);

            // Clear using script execution
            await this.ExecuteScriptOnElementAsync(element, "(e) => { e.value = ''; e.dispatchEvent(new Event('input', { bubbles: true })); e.dispatchEvent(new Event('change', { bubbles: true })); }").ConfigureAwait(false);
        }
        finally
        {
            cts.Cancel();
        }
    }

    /// <summary>
    /// Hovers the mouse pointer over the element. Requires exactly one matching element.
    /// </summary>
    /// <param name="timeout">Optional timeout override. If null, uses global timeout.</param>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
    public async Task HoverAsync(TimeSpan? timeout = null)
    {
        TimeSpan effectiveTimeout = this.GetTimeout(timeout);
        using CancellationTokenSource cts = new(effectiveTimeout);

        try
        {
            Task timeoutTask = Task.Delay(Timeout.InfiniteTimeSpan, cts.Token);
            SharedReference element = await this.ResolveSingleElementAsync(timeoutTask).ConfigureAwait(false);
            await this.inspector.WaitForInteractionReadyAsync(this.browsingContextId, element, InteractionType.Hover, timeoutTask).ConfigureAwait(false);

            InputBuilder builder = new();
            builder.AddAction(builder.DefaultPointerInputSource.CreatePointerMove(0, 0, Origin.Element(new ElementOrigin(element))));
            PerformActionsCommandParameters performParams = new(this.browsingContextId);
            performParams.Actions.AddRange(builder.Build());
            await this.driver.Input.PerformActionsAsync(performParams).ConfigureAwait(false);
        }
        finally
        {
            cts.Cancel();
        }
    }

    /// <summary>
    /// Drags this element to the target element. Requires exactly one matching element for both source and target.
    /// </summary>
    /// <param name="target">The target <see cref="ElementLocator"/> to drag to.</param>
    /// <param name="xOffset">Optional horizontal offset from target's in-view center point.</param>
    /// <param name="yOffset">Optional vertical offset from target's in-view center point.</param>
    /// <param name="timeout">Optional timeout override. If null, uses global timeout.</param>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
    public async Task DragToAsync(ElementLocator target, int? xOffset = null, int? yOffset = null, TimeSpan? timeout = null)
    {
        TimeSpan effectiveTimeout = this.GetTimeout(timeout);
        using CancellationTokenSource cts = new(effectiveTimeout);

        try
        {
            Task timeoutTask = Task.Delay(Timeout.InfiniteTimeSpan, cts.Token);
            SharedReference sourceElement = await this.ResolveSingleElementAsync(timeoutTask).ConfigureAwait(false);
            SharedReference targetElement = await target.ResolveSingleElementAsync(timeoutTask).ConfigureAwait(false);

            await this.inspector.WaitForInteractionReadyAsync(this.browsingContextId, sourceElement, InteractionType.Drag, timeoutTask).ConfigureAwait(false);
            await this.inspector.WaitForInteractionReadyAsync(this.browsingContextId, targetElement, InteractionType.Hover, timeoutTask).ConfigureAwait(false);

            InputBuilder builder = new();
            PointerInputSource pointer = builder.DefaultPointerInputSource;
            builder.AddAction(pointer.CreatePointerMove(0, 0, Origin.Element(new ElementOrigin(sourceElement))))
                .AddAction(pointer.CreatePointerDown(PointerButton.Left))
                .AddAction(pointer.CreatePointerMove(xOffset ?? 0, yOffset ?? 0, Origin.Element(new ElementOrigin(targetElement))))
                .AddAction(pointer.CreatePointerUp());

            PerformActionsCommandParameters performParams = new(this.browsingContextId);
            performParams.Actions.AddRange(builder.Build());
            await this.driver.Input.PerformActionsAsync(performParams).ConfigureAwait(false);
        }
        finally
        {
            cts.Cancel();
        }
    }

    /// <summary>
    /// Selects an option by its value attribute. Requires exactly one matching element.
    /// </summary>
    /// <param name="optionValue">The value attribute of the option to select.</param>
    /// <param name="timeout">Optional timeout override. If null, uses global timeout.</param>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
    public async Task SelectOptionAsync(string optionValue, TimeSpan? timeout = null)
    {
        await this.SelectOptionsAsync([optionValue], timeout).ConfigureAwait(false);
    }

    /// <summary>
    /// Selects an option by its visible text. Requires exactly one matching element.
    /// </summary>
    /// <param name="optionText">The visible text of the option to select.</param>
    /// <param name="timeout">Optional timeout override. If null, uses global timeout.</param>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
    public async Task SelectOptionByTextAsync(string optionText, TimeSpan? timeout = null)
    {
        await this.SelectOptionsByTextAsync([optionText], timeout).ConfigureAwait(false);
    }

    /// <summary>
    /// Selects an option by its index (0-based). Requires exactly one matching element.
    /// </summary>
    /// <param name="index">The zero-based index of the option to select.</param>
    /// <param name="timeout">Optional timeout override. If null, uses global timeout.</param>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
    public async Task SelectOptionByIndexAsync(int index, TimeSpan? timeout = null)
    {
        await this.SelectOptionsByIndexAsync([index], timeout).ConfigureAwait(false);
    }

    /// <summary>
    /// Selects multiple options by their value attributes. Requires exactly one matching element.
    /// </summary>
    /// <param name="optionValues">The value attributes of the options to select.</param>
    /// <param name="timeout">Optional timeout override. If null, uses global timeout.</param>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
    /// <exception cref="InvalidElementTypeException">Thrown when the element is not a SELECT element or does not have the multiple attribute.</exception>
    public async Task SelectOptionsAsync(IEnumerable<string> optionValues, TimeSpan? timeout = null)
    {
        TimeSpan effectiveTimeout = this.GetTimeout(timeout);
        using CancellationTokenSource cts = new(effectiveTimeout);

        try
        {
            Task timeoutTask = Task.Delay(Timeout.InfiniteTimeSpan, cts.Token);
            SharedReference element = await this.ResolveSingleElementAsync(timeoutTask).ConfigureAwait(false);
            List<string> valuesList = optionValues.ToList();

            string script = """
                (select, values) => {
                    if (select.tagName !== 'SELECT') {
                        throw new Error('Element is not a SELECT element');
                    }
                    if (values.length > 1 && !select.hasAttribute('multiple')) {
                        throw new Error("Operation 'SelectOptions' (selecting multiple options) requires a SELECT element with the 'multiple' attribute");
                    }
                    const options = Array.from(select.options);
                    let found = false;
                    options.forEach(opt => {
                        if (values.includes(opt.value)) {
                            opt.selected = true;
                            found = true;
                        } else if (!select.multiple) {
                            opt.selected = false;
                        }
                    });
                    if (!found) {
                        throw new Error('No options found with specified values');
                    }
                    select.dispatchEvent(new Event('input', { bubbles: true }));
                    select.dispatchEvent(new Event('change', { bubbles: true }));
                }
                """;

            await this.ExecuteScriptOnElementAsync(element, script, LocalValue.Array(valuesList.Select(LocalValue.String).ToList())).ConfigureAwait(false);
        }
        finally
        {
            cts.Cancel();
        }
    }

    /// <summary>
    /// Selects multiple options by their visible text. Requires exactly one matching element.
    /// </summary>
    /// <param name="optionTexts">The visible text of the options to select.</param>
    /// <param name="timeout">Optional timeout override. If null, uses global timeout.</param>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
    /// <exception cref="InvalidElementTypeException">Thrown when the element is not a SELECT element or does not have the multiple attribute.</exception>
    public async Task SelectOptionsByTextAsync(IEnumerable<string> optionTexts, TimeSpan? timeout = null)
    {
        TimeSpan effectiveTimeout = this.GetTimeout(timeout);
        using CancellationTokenSource cts = new(effectiveTimeout);

        try
        {
            Task timeoutTask = Task.Delay(Timeout.InfiniteTimeSpan, cts.Token);
            SharedReference element = await this.ResolveSingleElementAsync(timeoutTask).ConfigureAwait(false);
            List<string> textsList = optionTexts.ToList();

            string script = """
                (select, texts) => {
                    if (select.tagName !== 'SELECT') {
                        throw new Error('Element is not a SELECT element');
                    }
                    if (texts.length > 1 && !select.hasAttribute('multiple')) {
                        throw new Error("Operation 'SelectOptions' (selecting multiple options) requires a SELECT element with the 'multiple' attribute");
                    }
                    const options = Array.from(select.options);
                    let found = false;
                    options.forEach(opt => {
                        if (texts.includes(opt.text.trim())) {
                            opt.selected = true;
                            found = true;
                        } else if (!select.multiple) {
                            opt.selected = false;
                        }
                    });
                    if (!found) {
                        throw new Error('No options found with specified text');
                    }
                    select.dispatchEvent(new Event('input', { bubbles: true }));
                    select.dispatchEvent(new Event('change', { bubbles: true }));
                }
                """;

            await this.ExecuteScriptOnElementAsync(element, script, LocalValue.Array(textsList.Select(LocalValue.String).ToList())).ConfigureAwait(false);
        }
        finally
        {
            cts.Cancel();
        }
    }

    /// <summary>
    /// Selects multiple options by their indexes (0-based). Requires exactly one matching element.
    /// </summary>
    /// <param name="indexes">The zero-based indexes of the options to select.</param>
    /// <param name="timeout">Optional timeout override. If null, uses global timeout.</param>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
    /// <exception cref="InvalidElementTypeException">Thrown when the element is not a SELECT element or does not have the multiple attribute.</exception>
    public async Task SelectOptionsByIndexAsync(IEnumerable<int> indexes, TimeSpan? timeout = null)
    {
        TimeSpan effectiveTimeout = this.GetTimeout(timeout);
        using CancellationTokenSource cts = new(effectiveTimeout);

        try
        {
            Task timeoutTask = Task.Delay(Timeout.InfiniteTimeSpan, cts.Token);
            SharedReference element = await this.ResolveSingleElementAsync(timeoutTask).ConfigureAwait(false);
            List<int> indexesList = indexes.ToList();

            string script = """
                (select, indexes) => {
                    if (select.tagName !== 'SELECT') {
                        throw new Error('Element is not a SELECT element');
                    }
                    if (indexes.length > 1 && !select.hasAttribute('multiple')) {
                        throw new Error("Operation 'SelectOptions' (selecting multiple options) requires a SELECT element with the 'multiple' attribute");
                    }
                    const options = Array.from(select.options);
                    let found = false;
                    indexes.forEach(idx => {
                        if (idx >= 0 && idx < options.length) {
                            options[idx].selected = true;
                            found = true;
                        }
                    });
                    if (!found) {
                        throw new Error('No valid indexes provided');
                    }
                    if (!select.multiple) {
                        options.forEach((opt, i) => {
                            if (!indexes.includes(i)) {
                                opt.selected = false;
                            }
                        });
                    }
                    select.dispatchEvent(new Event('input', { bubbles: true }));
                    select.dispatchEvent(new Event('change', { bubbles: true }));
                }
                """;

            await this.ExecuteScriptOnElementAsync(element, script, LocalValue.Array(indexesList.Select(LocalValue.Number).ToList())).ConfigureAwait(false);
        }
        finally
        {
            cts.Cancel();
        }
    }

    /// <summary>
    /// Checks the element if not already checked. Idempotent. Requires exactly one matching element.
    /// </summary>
    /// <param name="timeout">Optional timeout override. If null, uses global timeout.</param>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
    public async Task CheckAsync(TimeSpan? timeout = null)
    {
        TimeSpan effectiveTimeout = this.GetTimeout(timeout);
        using CancellationTokenSource cts = new(effectiveTimeout);

        try
        {
            Task timeoutTask = Task.Delay(Timeout.InfiniteTimeSpan, cts.Token);
            SharedReference element = await this.ResolveSingleElementAsync(timeoutTask).ConfigureAwait(false);
            await this.ValidateCheckableElementAsync(element, false).ConfigureAwait(false);

            bool isChecked = await this.IsCheckedAsync(timeout).ConfigureAwait(false);
            if (!isChecked)
            {
                await this.ClickAsync(ClickNavigationBehavior.None, timeout).ConfigureAwait(false);
            }
        }
        finally
        {
            cts.Cancel();
        }
    }

    /// <summary>
    /// Unchecks the element if not already unchecked. Idempotent. Requires exactly one matching element.
    /// </summary>
    /// <param name="timeout">Optional timeout override. If null, uses global timeout.</param>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
    public async Task UncheckAsync(TimeSpan? timeout = null)
    {
        TimeSpan effectiveTimeout = this.GetTimeout(timeout);
        using CancellationTokenSource cts = new(effectiveTimeout);

        try
        {
            Task timeoutTask = Task.Delay(Timeout.InfiniteTimeSpan, cts.Token);
            SharedReference element = await this.ResolveSingleElementAsync(timeoutTask).ConfigureAwait(false);
            await this.ValidateCheckableElementAsync(element, true).ConfigureAwait(false);

            bool isChecked = await this.IsCheckedAsync(timeout).ConfigureAwait(false);
            if (isChecked)
            {
                await this.ClickAsync(ClickNavigationBehavior.None, timeout).ConfigureAwait(false);
            }
        }
        finally
        {
            cts.Cancel();
        }
    }

    /// <summary>
    /// Gives focus to the element. Requires exactly one matching element.
    /// </summary>
    /// <param name="timeout">Optional timeout override. If null, uses global timeout.</param>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
    public async Task FocusAsync(TimeSpan? timeout = null)
    {
        TimeSpan effectiveTimeout = this.GetTimeout(timeout);
        using CancellationTokenSource cts = new(effectiveTimeout);

        try
        {
            Task timeoutTask = Task.Delay(Timeout.InfiniteTimeSpan, cts.Token);
            SharedReference element = await this.ResolveSingleElementAsync(timeoutTask).ConfigureAwait(false);
            await this.ExecuteScriptOnElementAsync(element, "(e) => e.focus()").ConfigureAwait(false);
        }
        finally
        {
            cts.Cancel();
        }
    }

    /// <summary>
    /// Removes focus from the element. Requires exactly one matching element.
    /// </summary>
    /// <param name="timeout">Optional timeout override. If null, uses global timeout.</param>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
    public async Task BlurAsync(TimeSpan? timeout = null)
    {
        TimeSpan effectiveTimeout = this.GetTimeout(timeout);
        using CancellationTokenSource cts = new(effectiveTimeout);

        try
        {
            Task timeoutTask = Task.Delay(Timeout.InfiniteTimeSpan, cts.Token);
            SharedReference element = await this.ResolveSingleElementAsync(timeoutTask).ConfigureAwait(false);
            await this.ExecuteScriptOnElementAsync(element, "(e) => e.blur()").ConfigureAwait(false);
        }
        finally
        {
            cts.Cancel();
        }
    }

    /// <summary>
    /// Captures a screenshot of the element. Requires exactly one matching element.
    /// </summary>
    /// <param name="timeout">Optional timeout override. If null, uses global timeout.</param>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation, containing PNG image data as a byte array.</returns>
    public async Task<byte[]> ScreenshotAsync(TimeSpan? timeout = null)
    {
        TimeSpan effectiveTimeout = this.GetTimeout(timeout);
        using CancellationTokenSource cts = new(effectiveTimeout);

        try
        {
            Task timeoutTask = Task.Delay(Timeout.InfiniteTimeSpan, cts.Token);
            SharedReference element = await this.ResolveSingleElementAsync(timeoutTask).ConfigureAwait(false);

            // Wait for element to be visible
            bool isVisible = await this.inspector.IsElementVisibleAsync(this.browsingContextId, element).ConfigureAwait(false);
            if (!isVisible)
            {
                throw new WebDriverBiDiException("Element is not visible and cannot be captured in a screenshot");
            }

            // Capture screenshot with element clip
            CaptureScreenshotCommandParameters captureParams = new(this.browsingContextId)
            {
                Clip = new ElementClipRectangle(element),
            };

            CaptureScreenshotCommandResult result = await this.driver.BrowsingContext.CaptureScreenshotAsync(captureParams).ConfigureAwait(false);
            return Convert.FromBase64String(result.Data);
        }
        finally
        {
            cts.Cancel();
        }
    }

    /// <summary>
    /// Returns true if the element is visible. Requires exactly one matching element.
    /// </summary>
    /// <param name="timeout">Optional timeout override. If null, uses global timeout.</param>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation, containing true if the element is visible; otherwise, false.</returns>
    public async Task<bool> IsVisibleAsync(TimeSpan? timeout = null)
    {
        TimeSpan effectiveTimeout = this.GetTimeout(timeout);
        using CancellationTokenSource cts = new(effectiveTimeout);

        try
        {
            Task timeoutTask = Task.Delay(Timeout.InfiniteTimeSpan, cts.Token);
            SharedReference element = await this.ResolveSingleElementAsync(timeoutTask).ConfigureAwait(false);
            return await this.inspector.IsElementVisibleAsync(this.browsingContextId, element).ConfigureAwait(false);
        }
        finally
        {
            cts.Cancel();
        }
    }

    /// <summary>
    /// Returns true if the element is enabled. Requires exactly one matching element.
    /// </summary>
    /// <param name="timeout">Optional timeout override. If null, uses global timeout.</param>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation, containing true if the element is enabled; otherwise, false.</returns>
    public async Task<bool> IsEnabledAsync(TimeSpan? timeout = null)
    {
        TimeSpan effectiveTimeout = this.GetTimeout(timeout);
        using CancellationTokenSource cts = new(effectiveTimeout);

        try
        {
            Task timeoutTask = Task.Delay(Timeout.InfiniteTimeSpan, cts.Token);
            SharedReference element = await this.ResolveSingleElementAsync(timeoutTask).ConfigureAwait(false);
            return await this.inspector.IsElementEnabledAsync(this.browsingContextId, element).ConfigureAwait(false);
        }
        finally
        {
            cts.Cancel();
        }
    }

    /// <summary>
    /// Returns true if the element is checked. Requires exactly one matching element.
    /// </summary>
    /// <param name="timeout">Optional timeout override. If null, uses global timeout.</param>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation, containing true if the element is checked; otherwise, false.</returns>
    public async Task<bool> IsCheckedAsync(TimeSpan? timeout = null)
    {
        TimeSpan effectiveTimeout = this.GetTimeout(timeout);
        using CancellationTokenSource cts = new(effectiveTimeout);

        try
        {
            Task timeoutTask = Task.Delay(Timeout.InfiniteTimeSpan, cts.Token);
            SharedReference element = await this.ResolveSingleElementAsync(timeoutTask).ConfigureAwait(false);

            string script = """
                (e) => {
                    if (e.checked !== undefined) {
                        return e.checked;
                    }
                    const ariaChecked = e.getAttribute('aria-checked');
                    if (ariaChecked === 'true') return true;
                    if (ariaChecked === 'false') return false;
                    return false;
                }
                """;

            EvaluateResultSuccess result = await this.ExecuteScriptOnElementAsync(element, script).ConfigureAwait(false);
            return result.Result.ConvertTo<BooleanRemoteValue>().Value;
        }
        finally
        {
            cts.Cancel();
        }
    }

    /// <summary>
    /// Returns the visible text content of the element. Requires exactly one matching element.
    /// </summary>
    /// <param name="timeout">Optional timeout override. If null, uses global timeout.</param>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation, containing the visible text content.</returns>
    public async Task<string> GetTextAsync(TimeSpan? timeout = null)
    {
        TimeSpan effectiveTimeout = this.GetTimeout(timeout);
        using CancellationTokenSource cts = new(effectiveTimeout);

        try
        {
            Task timeoutTask = Task.Delay(Timeout.InfiniteTimeSpan, cts.Token);
            SharedReference element = await this.ResolveSingleElementAsync(timeoutTask).ConfigureAwait(false);
            EvaluateResultSuccess result = await this.ExecuteScriptOnElementAsync(element, "(e) => e.innerText").ConfigureAwait(false);
            return result.Result.ConvertTo<StringRemoteValue>().Value;
        }
        finally
        {
            cts.Cancel();
        }
    }

    /// <summary>
    /// Returns the value of the specified attribute, or null if not present. Requires exactly one matching element.
    /// </summary>
    /// <param name="attributeName">The name of the attribute to retrieve.</param>
    /// <param name="timeout">Optional timeout override. If null, uses global timeout.</param>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation, containing the attribute value or null if not present.</returns>
    public async Task<string?> GetAttributeAsync(string attributeName, TimeSpan? timeout = null)
    {
        TimeSpan effectiveTimeout = this.GetTimeout(timeout);
        using CancellationTokenSource cts = new(effectiveTimeout);

        try
        {
            Task timeoutTask = Task.Delay(Timeout.InfiniteTimeSpan, cts.Token);
            SharedReference element = await this.ResolveSingleElementAsync(timeoutTask).ConfigureAwait(false);
            EvaluateResultSuccess result = await this.ExecuteScriptOnElementAsync(element, "(e, name) => e.getAttribute(name)", LocalValue.String(attributeName)).ConfigureAwait(false);
            if (result.Result.Type == RemoteValueType.Null)
            {
                return null;
            }

            return result.Result.ConvertTo<StringRemoteValue>().Value;
        }
        finally
        {
            cts.Cancel();
        }
    }

    private async Task<SharedReference> ResolveSingleElementAsync(Task timeoutTask)
    {
        IList<NodeRemoteValue> elements = await this.ResolveElementsAsync(timeoutTask).ConfigureAwait(false);
        if (elements.Count == 0)
        {
            throw new ElementNotFoundException($"No elements found matching locator: {this.GetLocatorDescription()}");
        }

        if (elements.Count > 1 && this.specificIndex is null)
        {
            throw new AmbiguousElementException($"Expected exactly one element but found {elements.Count} matching locator: {this.GetLocatorDescription()}");
        }

        int index = this.specificIndex ?? 0;
        return new SharedReference(elements[index].SharedId!);
    }

    private async Task<IList<NodeRemoteValue>> ResolveElementsAsync(Task timeoutTask)
    {
        // Build the chain of locators from root to this locator
        List<LocatorChainEntry> locatorChain = new();
        ElementLocator? current = this;
        while (current is not null)
        {
            locatorChain.Insert(0, new LocatorChainEntry(current.locator, current.specificIndex));
            current = current.parent;
        }

        // Resolve the first locator
        LocatorChainEntry firstEntry = locatorChain[0];
        LocateNodesCommandParameters locateParams = new(this.browsingContextId, firstEntry.Locator);
        Task<LocateNodesCommandResult> locateTask = this.driver.BrowsingContext.LocateNodesAsync(locateParams);
        Task completedTask = await Task.WhenAny(locateTask, timeoutTask).ConfigureAwait(false);
        if (completedTask == timeoutTask)
        {
            throw new WebDriverBiDiTimeoutException("Timed out while resolving elements");
        }

        LocateNodesCommandResult locateResult = await locateTask.ConfigureAwait(false);
        IList<NodeRemoteValue> currentNodes = locateResult.Nodes;

        if (firstEntry.SpecificIndex.HasValue && currentNodes.Count > firstEntry.SpecificIndex.Value)
        {
            currentNodes = new List<NodeRemoteValue> { currentNodes[firstEntry.SpecificIndex.Value] };
        }

        // Resolve subsequent locators using startNodes
        for (int i = 1; i < locatorChain.Count; i++)
        {
            LocatorChainEntry entry = locatorChain[i];
            List<NodeRemoteValue> allMatches = new();

            foreach (NodeRemoteValue node in currentNodes)
            {
                LocateNodesCommandParameters childLocateParams = new(this.browsingContextId, entry.Locator);
                childLocateParams.StartNodes.Add(new SharedReference(node.SharedId!));
                Task<LocateNodesCommandResult> childLocateTask = this.driver.BrowsingContext.LocateNodesAsync(childLocateParams);
                Task childCompletedTask = await Task.WhenAny(childLocateTask, timeoutTask).ConfigureAwait(false);
                if (childCompletedTask == timeoutTask)
                {
                    throw new WebDriverBiDiTimeoutException("Timed out while resolving elements");
                }

                LocateNodesCommandResult childResult = await childLocateTask.ConfigureAwait(false);
                foreach (NodeRemoteValue childNode in childResult.Nodes)
                {
                    allMatches.Add(childNode);
                }
            }

            currentNodes = allMatches;

            if (entry.SpecificIndex.HasValue && currentNodes.Count > entry.SpecificIndex.Value)
            {
                currentNodes = new List<NodeRemoteValue> { currentNodes[entry.SpecificIndex.Value] };
            }
        }

        return currentNodes;
    }

    private async Task<EvaluateResultSuccess> ExecuteScriptOnElementAsync(SharedReference element, string functionDefinition, params LocalValue[] additionalArguments)
    {
        ContextTarget contextTarget = new(this.browsingContextId);
        CallFunctionCommandParameters callParams = new(functionDefinition, contextTarget, false);
        callParams.Arguments.Add(element);
        foreach (LocalValue arg in additionalArguments)
        {
            callParams.Arguments.Add(arg);
        }

        EvaluateResult result = await this.driver.Script.CallFunctionAsync(callParams).ConfigureAwait(false);
        if (result.ResultType == EvaluateResultType.Exception)
        {
            throw new WebDriverBiDiException(((EvaluateResultException)result).ExceptionDetails.Text);
        }

        return (EvaluateResultSuccess)result;
    }

    private async Task ValidateCheckableElementAsync(SharedReference element, bool disallowRadio)
    {
        string script = """
            (e) => {
                const type = e.type ? e.type.toLowerCase() : '';
                const role = e.getAttribute('role');
                const checkableRoles = ['checkbox', 'switch', 'menuitemcheckbox', 'menuitemradio', 'radio'];
                const isCheckableInput = (e.tagName === 'INPUT' && (type === 'checkbox' || type === 'radio'));
                const isCheckableRole = checkableRoles.includes(role);
                return { isCheckable: isCheckableInput || isCheckableRole, isRadio: type === 'radio' || role === 'radio' || role === 'menuitemradio' };
            }
            """;

        EvaluateResultSuccess result = await this.ExecuteScriptOnElementAsync(element, script).ConfigureAwait(false);
        KeyValuePairCollectionRemoteValue obj = result.Result.ConvertTo<KeyValuePairCollectionRemoteValue>();

        bool isCheckable = false;
        bool isRadio = false;

        if (obj.Value is not null)
        {
            foreach (KeyValuePair<object, RemoteValue> prop in obj.Value)
            {
                if (prop.Key is string keyStr)
                {
                    if (string.Equals(keyStr, "isCheckable", StringComparison.Ordinal) && prop.Value is BooleanRemoteValue boolVal)
                    {
                        isCheckable = boolVal.Value;
                    }
                    else if (string.Equals(keyStr, "isRadio", StringComparison.Ordinal) && prop.Value is BooleanRemoteValue radioVal)
                    {
                        isRadio = radioVal.Value;
                    }
                }
            }
        }

        if (!isCheckable)
        {
            throw new InvalidElementTypeException("Operation 'Check/Uncheck' requires a checkable element (checkbox, radio, or ARIA checkable role)");
        }

        if (disallowRadio && isRadio)
        {
            throw new InvalidOperationException("Cannot uncheck a radio button via user interaction");
        }
    }

    private TimeSpan GetTimeout(TimeSpan? timeout)
    {
        return timeout ?? this.settings.DefaultTimeout;
    }

    private string GetLocatorDescription()
    {
        return $"{this.locator.Type}={this.locator.Value}";
    }
}
