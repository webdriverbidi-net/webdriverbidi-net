// <copyright file="InputBuilder.cs" company="WebDriverBidi.NET Committers">
// Copyright (c) WebDriverBidi.NET Committers. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.
// </copyright>

namespace WebDriverBidi.Client;

using WebDriverBidi.Input;

/// <summary>
/// Helper class to build a sequence of inputs suitable for use as payload to
/// the input.PerformActions command in the WebDriver BiDi protocol.
/// </summary>
public class InputBuilder
{
    private readonly Dictionary<string, SourceActions> sources = new();

    /// <summary>
    /// Creates a key-based input source, like a keyboard, primarily for entering text.
    /// </summary>
    /// <returns>The key-based input source.</returns>
    public KeyInputSource CreateKeyInputSource()
    {
        // When created, the input source action list is prepopulated with the number of
        // pause actions equal to the number of the current longest action sequence. This
        // ensures that the action sequences of all input sources are synchonized.
        KeySourceActions source = new();
        source.Actions.AddRange(this.CreatePauseActions());
        this.sources[source.Id] = source;
        return new KeyInputSource(source.Id);
    }

    /// <summary>
    /// Creates a pointer input source, like a mouse, pen, or stylus. Also used for touch actions.
    /// </summary>
    /// <param name="pointerType">The type of pointer input source to create.</param>
    /// <returns>The pointer input source.</returns>
    public PointerInputSource CreatePointerInputSource(PointerType pointerType)
    {
        // When created, the input source action list is prepopulated with the number of
        // pause actions equal to the number of the current longest action sequence. This
        // ensures that the action sequences of all input sources are synchonized.
        PointerSourceActions source = new()
        {
            Parameters = new PointerParameters()
            {
                PointerType = pointerType,
            },
        };
        source.Actions.AddRange(this.CreatePauseActions());
        this.sources[source.Id] = source;
        return new PointerInputSource(source.Id, pointerType);
    }

    /// <summary>
    /// Creates a wheel input source, like a mouse wheel, for primarily for
    /// providing discrete consecutive input values.
    /// </summary>
    /// <returns>The wheel input source.</returns>
    public WheelInputSource CreateWheelInputSource()
    {
        // When created, the input source action list is prepopulated with the number of
        // pause actions equal to the number of the current longest action sequence. This
        // ensures that the action sequences of all input sources are synchonized.
        WheelSourceActions source = new();
        source.Actions.AddRange(this.CreatePauseActions());
        this.sources[source.Id] = source;
        return new WheelInputSource(source.Id);
    }

    /// <summary>
    /// Clears the builder of all current input sources and associated actions.
    /// </summary>
    public void Clear()
    {
        this.sources.Clear();
    }

    /// <summary>
    /// Adds an action to the built set of actions. Adding an action will
    /// add a "tick" to the set of all actions to be executed.
    /// </summary>
    /// <param name="actionToAdd">The action to add to the set of actions.</param>
    /// <returns>A self reference.</returns>
    public InputBuilder AddAction(Action actionToAdd)
    {
        this.AddActions(actionToAdd);
        return this;
    }

    /// <summary>
    /// Adds an action to the built set of actions. Adding an action will
    /// add a "tick" to the set of all actions to be executed. Only one action
    /// for each input source may be added for a single tick.
    /// </summary>
    /// <param name="actionsToAdd">The set actions to add to the existing set of actions.</param>
    /// <returns>A self reference.</returns>
    public InputBuilder AddActions(params Action[] actionsToAdd)
    {
        this.ProcessTick(actionsToAdd);
        return this;
    }

    /// <summary>
    /// Gets the added actions as a list, suitable for adding to the payload of the
    /// input.PerformActions command of the WebDriver BiDi protocol.
    /// </summary>
    /// <returns>A list of actions.</returns>
    public List<SourceActions> Build()
    {
        return this.sources.Values.ToList();
    }

    private void ProcessTick(params Action[] interactionsToAdd)
    {
        List<string> unusedDevices = this.sources.Keys.ToList();
        List<string> usedDevices = new();
        foreach (Action interaction in interactionsToAdd)
        {
            if (!this.sources.ContainsKey(interaction.SourceId))
            {
                throw new ArgumentException($"Builder does not contain an input source for ID {interaction.SourceId}");
            }

            if (usedDevices.Contains(interaction.SourceId))
            {
                throw new ArgumentException("You can only add one action per input source for a single tick.");
            }

            usedDevices.Add(interaction.SourceId);
            if (unusedDevices.Contains(interaction.SourceId))
            {
                unusedDevices.Remove(interaction.SourceId);
            }

            SourceActions source = this.sources[interaction.SourceId];
            if (source is KeySourceActions keySource)
            {
                keySource.Actions.Add(interaction.AsActionType<IKeySourceAction>());
            }
            else if (source is PointerSourceActions pointerSource)
            {
                pointerSource.Actions.Add(interaction.AsActionType<IPointerSourceAction>());
            }
            else if (source is WheelSourceActions wheelSource)
            {
                wheelSource.Actions.Add(interaction.AsActionType<IWheelSourceAction>());
            }
        }

        foreach (string unusedDevice in unusedDevices)
        {
            SourceActions source = this.sources[unusedDevice];
            if (source is KeySourceActions keySource)
            {
                keySource.Actions.Add(new PauseAction());
            }
            else if (source is PointerSourceActions pointerSource)
            {
                pointerSource.Actions.Add(new PauseAction());
            }
            else if (source is WheelSourceActions wheelSource)
            {
                wheelSource.Actions.Add(new PauseAction());
            }
        }
    }

    private List<PauseAction> CreatePauseActions()
    {
        List<PauseAction> initialPauseActions = new();
        int maxActionCount = 0;
        foreach (SourceActions source in this.sources.Values)
        {
            if (source is KeySourceActions keySource)
            {
                maxActionCount = Math.Max(maxActionCount, keySource.Actions.Count);
            }
            else if (source is PointerSourceActions pointerSource)
            {
                maxActionCount = Math.Max(maxActionCount, pointerSource.Actions.Count);
            }
            else if (source is WheelSourceActions wheelSource)
            {
                maxActionCount = Math.Max(maxActionCount, wheelSource.Actions.Count);
            }
        }

        if (maxActionCount > 0)
        {
            for (int i = 0; i < maxActionCount; i++)
            {
                initialPauseActions.Add(new PauseAction());
            }
        }

        return initialPauseActions;
    }
}
