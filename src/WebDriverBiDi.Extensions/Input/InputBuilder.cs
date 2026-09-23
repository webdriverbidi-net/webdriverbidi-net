// <copyright file="InputBuilder.cs" company="WebDriverBiDi.NET Committers">
// Copyright (c) WebDriverBiDi.NET Committers. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.
// </copyright>

namespace WebDriverBiDi.Input;

/// <summary>
/// Helper class to build a sequence of inputs suitable for use as payload to
/// the input.PerformActions command in the WebDriver BiDi protocol.
/// </summary>
public class InputBuilder
{
    private readonly Dictionary<string, SourceActions> sources = [];
    private KeyInputSource? defaultKeyInputSource;
    private PointerInputSource? defaultPointerInputSource;
    private WheelInputSource? defaultWheelInputSource;

    /// <summary>
    /// Gets the default key input source, which is created the first time it is used.
    /// </summary>
    public KeyInputSource DefaultKeyInputSource => this.defaultKeyInputSource ??= this.CreateKeyInputSource();

    /// <summary>
    /// Gets the default pointer input source, a mouse, which is created the first time it is used.
    /// Pointer sources created with <see cref="CreatePointerInputSource"/> never become the default.
    /// </summary>
    public PointerInputSource DefaultPointerInputSource => this.defaultPointerInputSource ??= this.CreatePointerInputSource(PointerType.Mouse);

    /// <summary>
    /// Gets the default wheel input source, which is created the first time it is used.
    /// </summary>
    public WheelInputSource DefaultWheelInputSource => this.defaultWheelInputSource ??= this.CreateWheelInputSource();

    /// <summary>
    /// Creates a key input source.
    /// </summary>
    /// <returns>The input source.</returns>
    public KeyInputSource CreateKeyInputSource()
    {
        KeySourceActions source = new();
        this.AddSource(source);
        return new KeyInputSource(source.Id);
    }

    /// <summary>
    /// Creates a pointer input source.
    /// </summary>
    /// <param name="pointerType">The kind of pointer: a mouse, a pen, or a touch.</param>
    /// <returns>The input source.</returns>
    public PointerInputSource CreatePointerInputSource(PointerType pointerType)
    {
        PointerSourceActions source = new()
        {
            Parameters = new PointerParameters()
            {
                PointerType = pointerType,
            },
        };
        this.AddSource(source);
        return new PointerInputSource(source.Id, pointerType);
    }

    /// <summary>
    /// Creates a wheel input source.
    /// </summary>
    /// <returns>The input source.</returns>
    public WheelInputSource CreateWheelInputSource()
    {
        WheelSourceActions source = new();
        this.AddSource(source);
        return new WheelInputSource(source.Id);
    }

    /// <summary>
    /// Creates an input source that performs only pauses, for timing the other sources' actions.
    /// </summary>
    /// <returns>The input source.</returns>
    public NoneInputSource CreateNoneInputSource()
    {
        NoneSourceActions source = new();
        this.AddSource(source);
        return new NoneInputSource(source.Id);
    }

    /// <summary>
    /// Removes every input source and action, including the default input sources.
    /// </summary>
    public void Clear()
    {
        this.sources.Clear();
        this.defaultKeyInputSource = null;
        this.defaultPointerInputSource = null;
        this.defaultWheelInputSource = null;
    }

    /// <summary>
    /// Adds an action as a tick of its own, in which every other input source pauses.
    /// </summary>
    /// <param name="actionToAdd">The action.</param>
    /// <returns>This builder, for chaining.</returns>
    /// <exception cref="ArgumentException">Thrown when the action's input source was not created by this builder.</exception>
    public InputBuilder AddAction(InputAction actionToAdd)
    {
        return this.AddActions(actionToAdd);
    }

    /// <summary>
    /// Adds actions performed together, as one tick, in which every input source without an action pauses.
    /// </summary>
    /// <param name="actionsToAdd">The actions, at most one for each input source.</param>
    /// <returns>This builder, for chaining.</returns>
    /// <exception cref="ArgumentException">Thrown when an action's input source was not created by this builder, or two actions share one.</exception>
    public InputBuilder AddActions(params InputAction[] actionsToAdd)
    {
        HashSet<string> usedSources = [];
        foreach (InputAction inputAction in actionsToAdd)
        {
            if (!this.sources.ContainsKey(inputAction.SourceId))
            {
                throw new ArgumentException($"Builder does not contain an input source for ID {inputAction.SourceId}", nameof(actionsToAdd));
            }

            if (!usedSources.Add(inputAction.SourceId))
            {
                throw new ArgumentException("You can only add one action per input source for a single tick.", nameof(actionsToAdd));
            }
        }

        foreach (InputAction inputAction in actionsToAdd)
        {
            Append(this.sources[inputAction.SourceId], inputAction);
        }

        foreach (SourceActions idleSource in this.sources.Values.Where(source => !usedSources.Contains(source.Id)))
        {
            Append(idleSource, null);
        }

        return this;
    }

    /// <summary>
    /// Builds the action sequences of every input source, for <see cref="PerformActionsCommandParameters.Actions"/>.
    /// </summary>
    /// <returns>The action sequences.</returns>
    public List<SourceActions> Build()
    {
        return [.. this.sources.Values];
    }

    // Appends the action, or a pause when there is none.
    private static void Append(SourceActions source, InputAction? inputAction)
    {
        switch (source)
        {
            case KeySourceActions keySource:
                keySource.Actions.Add(inputAction?.AsActionType<IKeySourceAction>() ?? new PauseAction());
                break;
            case PointerSourceActions pointerSource:
                pointerSource.Actions.Add(inputAction?.AsActionType<IPointerSourceAction>() ?? new PauseAction());
                break;
            case WheelSourceActions wheelSource:
                wheelSource.Actions.Add(inputAction?.AsActionType<IWheelSourceAction>() ?? new PauseAction());
                break;
            default:
                ((NoneSourceActions)source).Actions.Add(inputAction?.AsActionType<INoneSourceAction>() ?? new PauseAction());
                break;
        }
    }

    private static int CountActions(SourceActions source)
    {
        return source switch
        {
            KeySourceActions keySource => keySource.Actions.Count,
            PointerSourceActions pointerSource => pointerSource.Actions.Count,
            WheelSourceActions wheelSource => wheelSource.Actions.Count,
            _ => ((NoneSourceActions)source).Actions.Count,
        };
    }

    // A new source pauses through every tick already added, so that its first action lines up with the next tick.
    private void AddSource(SourceActions source)
    {
        int tickCount = this.sources.Values.Select(CountActions).DefaultIfEmpty(0).Max();
        for (int tick = 0; tick < tickCount; tick++)
        {
            Append(source, null);
        }

        this.sources[source.Id] = source;
    }
}
