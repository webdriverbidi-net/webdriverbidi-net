// <copyright file="AriaSnapshot.cs" company="WebDriverBiDi.NET Committers">
// Copyright (c) WebDriverBiDi.NET Committers. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.
// </copyright>

namespace Dramaturge;

using WebDriverBiDi.Script;

/// <summary>
/// An accessibility snapshot of part of a page: each element with a role, with its accessible name and states, and
/// the text between them, as text in a format compatible with Playwright's aria snapshots and as a tree of
/// <see cref="AriaNode"/> objects. Each node's ref turns into a locator for its element through <see cref="Locator"/>.
/// </summary>
/// <remarks>
/// The accessible names in a snapshot are computed by the library's script in the page, because a page cannot read
/// the names its browser computes. They can differ from the names the browser uses to find elements by role, as
/// <see cref="Frame.GetByRole"/> does. To act on an element from a snapshot, use its ref.
/// </remarks>
public sealed class AriaSnapshot
{
    private readonly string text;
    private readonly IReadOnlyDictionary<string, AriaSnapshotTarget> targets;

    /// <summary>
    /// Initializes a new instance of the <see cref="AriaSnapshot"/> class.
    /// </summary>
    /// <param name="root">The root of the snapshot.</param>
    /// <param name="text">The snapshot as text.</param>
    /// <param name="targets">The element each ref refers to, and its frame.</param>
    internal AriaSnapshot(AriaNode root, string text, IReadOnlyDictionary<string, AriaSnapshotTarget> targets)
    {
        this.Root = root;
        this.text = text;
        this.targets = targets;
    }

    /// <summary>
    /// Gets the root of the snapshot, with the role <see cref="AriaNode.FragmentRole"/>, whose children are the
    /// snapshot's nodes.
    /// </summary>
    public AriaNode Root { get; }

    /// <summary>
    /// Creates a locator for the element a ref in this snapshot refers to. The locator finds that element and no
    /// other, for as long as it is in its document: once it is removed, the locator finds nothing.
    /// </summary>
    /// <param name="reference">The ref, such as <c>e7</c>, or <c>f1e3</c> for an element in a frame.</param>
    /// <returns>The locator, in the element's frame.</returns>
    /// <exception cref="ArgumentException">Thrown when the snapshot has no such ref.</exception>
    /// <remarks>
    /// Using the locator after the element's document has been replaced, such as by a navigation, throws an
    /// <see cref="InvalidOperationException"/>, since the element can never be found again.
    /// </remarks>
    public ElementLocator Locator(string reference)
    {
        return this.targets.TryGetValue(reference, out AriaSnapshotTarget? target)
            ? ElementLocator.ForReference(target.Frame, target.Node, reference)
            : throw new ArgumentException($"The snapshot has no ref \"{reference}\".", nameof(reference));
    }

    /// <summary>
    /// Gets the snapshot as text, in a format compatible with Playwright's aria snapshots.
    /// </summary>
    /// <returns>The text, with a line for each node.</returns>
    public override string ToString()
    {
        return this.text;
    }
}
