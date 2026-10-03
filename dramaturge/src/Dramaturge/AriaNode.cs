// <copyright file="AriaNode.cs" company="WebDriverBiDi.NET Committers">
// Copyright (c) WebDriverBiDi.NET Committers. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.
// </copyright>

namespace Dramaturge;

using System.Text.Json;

/// <summary>
/// A node of an accessibility snapshot: an element with a role, a frame, a run of text, or the root of the snapshot.
/// </summary>
public sealed class AriaNode
{
    /// <summary>
    /// The role of a run of text.
    /// </summary>
    public const string TextRole = "text";

    /// <summary>
    /// The role of the root of a snapshot, whose children are the snapshot's nodes.
    /// </summary>
    public const string FragmentRole = "fragment";

    /// <summary>
    /// The role of a frame.
    /// </summary>
    public const string FrameRole = "iframe";

    private readonly List<AriaNode> children = [];

    private AriaNode(string role)
    {
        this.Role = role;
    }

    /// <summary>
    /// Gets the node's ARIA role; <see cref="FrameRole"/> for a frame, <see cref="TextRole"/> for a run of text, or
    /// <see cref="FragmentRole"/> for the root of a snapshot.
    /// </summary>
    public string Role { get; }

    /// <summary>
    /// Gets the node's accessible name, with white space collapsed; empty if it has none, and for a run of text.
    /// </summary>
    public string Name { get; private set; } = string.Empty;

    /// <summary>
    /// Gets the text of a run of text, or <see langword="null"/> for any other node.
    /// </summary>
    public string? Text { get; private set; }

    /// <summary>
    /// Gets the node's ref, which <see cref="AriaSnapshot.Locator"/> turns into a locator for its element, or
    /// <see langword="null"/> if the snapshot has no refs, and for a run of text or the root.
    /// </summary>
    public string? Ref { get; private set; }

    /// <summary>
    /// Gets whether the node is checked, or <see langword="null"/> if its role cannot be checked.
    /// </summary>
    public ToggleState? Checked { get; private set; }

    /// <summary>
    /// Gets whether the node is disabled, or <see langword="null"/> if its role cannot be disabled.
    /// </summary>
    public bool? Disabled { get; private set; }

    /// <summary>
    /// Gets whether the node is expanded, or <see langword="null"/> if it is not expandable or does not say.
    /// </summary>
    public bool? Expanded { get; private set; }

    /// <summary>
    /// Gets the node's level, such as 1 for an h1 heading, or <see langword="null"/> if it has none.
    /// </summary>
    public int? Level { get; private set; }

    /// <summary>
    /// Gets whether the node is pressed, or <see langword="null"/> if it is not a button.
    /// </summary>
    public ToggleState? Pressed { get; private set; }

    /// <summary>
    /// Gets whether the node is selected, or <see langword="null"/> if its role cannot be selected.
    /// </summary>
    public bool? Selected { get; private set; }

    /// <summary>
    /// Gets the URL of a link, or <see langword="null"/> for any other node.
    /// </summary>
    public string? Url { get; private set; }

    /// <summary>
    /// Gets the placeholder of a text box when it differs from its name, or <see langword="null"/>.
    /// </summary>
    public string? Placeholder { get; private set; }

    /// <summary>
    /// Gets the node's child nodes and runs of text, in document order.
    /// </summary>
    public IReadOnlyList<AriaNode> Children => this.children;

    /// <summary>
    /// Creates a node from the JSON the library's script returns: an object for a node, or a string for a run of text.
    /// </summary>
    /// <param name="element">The JSON.</param>
    /// <returns>The node, with its descendants.</returns>
    internal static AriaNode FromJson(JsonElement element)
    {
        if (element.ValueKind == JsonValueKind.String)
        {
            return new AriaNode(TextRole) { Text = element.GetString() };
        }

        AriaNode node = new(element.GetProperty("role").GetString()!)
        {
            Name = element.GetProperty("name").GetString()!,
            Ref = ReadString(element, "ref"),
            Checked = ReadToggle(element, "checked"),
            Disabled = ReadBoolean(element, "disabled"),
            Expanded = ReadBoolean(element, "expanded"),
            Level = element.TryGetProperty("level", out JsonElement level) ? level.GetInt32() : null,
            Pressed = ReadToggle(element, "pressed"),
            Selected = ReadBoolean(element, "selected"),
            Url = ReadString(element, "url"),
            Placeholder = ReadString(element, "placeholder"),
        };
        node.children.AddRange(element.GetProperty("children").EnumerateArray().Select(FromJson));
        return node;
    }

    /// <summary>
    /// Adds nodes to the end of this node's children, such as the content of the frame this node stands for.
    /// </summary>
    /// <param name="nodes">The nodes.</param>
    internal void AddChildren(IEnumerable<AriaNode> nodes)
    {
        this.children.AddRange(nodes);
    }

    private static string? ReadString(JsonElement element, string name)
    {
        return element.TryGetProperty(name, out JsonElement value) ? value.GetString() : null;
    }

    private static bool? ReadBoolean(JsonElement element, string name)
    {
        return element.TryGetProperty(name, out JsonElement value) ? value.GetBoolean() : null;
    }

    // A tri-state attribute is true, false, or the string "mixed".
    private static ToggleState? ReadToggle(JsonElement element, string name)
    {
        if (!element.TryGetProperty(name, out JsonElement value))
        {
            return null;
        }

        return value.ValueKind switch
        {
            JsonValueKind.True => ToggleState.On,
            JsonValueKind.False => ToggleState.Off,
            _ => ToggleState.Mixed,
        };
    }
}
