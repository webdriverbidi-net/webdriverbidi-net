// <copyright file="AriaSnapshotBuilder.cs" company="WebDriverBiDi.NET Committers">
// Copyright (c) WebDriverBiDi.NET Committers. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.
// </copyright>

namespace Dramaturge;

using System.Text.Json;
using System.Text.RegularExpressions;
using WebDriverBiDi;
using WebDriverBiDi.Script;

/// <summary>
/// Takes accessibility snapshots with the library's script, one frame at a time, and puts each frame's snapshot
/// beneath the iframe node that stands for it.
/// </summary>
internal static class AriaSnapshotBuilder
{
    private const string SnapshotFunction = "(snapshots, root, includeRefs, refPrefix) => snapshots.snapshot(root, includeRefs, refPrefix)";

    // A frame's line has no name, states, or text, only a ref, so no other line can look like it.
    private static readonly Regex FrameLine = new(@"^( *)- iframe( \[ref=[^\]]*\])?$", RegexOptions.CultureInvariant);

    /// <summary>
    /// Takes a snapshot of an element, or of a frame's document, and of the frames within it.
    /// </summary>
    /// <param name="frame">The frame.</param>
    /// <param name="root">The element, or <see langword="null"/> for the document.</param>
    /// <param name="options">What the snapshot includes.</param>
    /// <param name="budget">The time the snapshot may take.</param>
    /// <returns>The snapshot.</returns>
    public static async Task<AriaSnapshot> TakeAsync(Frame frame, NodeRemoteValue? root, AriaSnapshotOptions options, TimeBudget budget)
    {
        Dictionary<string, AriaSnapshotTarget> targets = [];
        (AriaNode tree, string text) = await TakeFrameAsync(frame, root, options, targets, budget).ConfigureAwait(false);
        return new AriaSnapshot(tree, text, targets);
    }

    private static async Task<(AriaNode Tree, string Text)> TakeFrameAsync(Frame frame, NodeRemoteValue? root, AriaSnapshotOptions options, Dictionary<string, AriaSnapshotTarget> targets, TimeBudget budget)
    {
        BrowserGroup group = frame.Page.Browser.Group;
        LocalValue rootArgument = root is null ? LocalValue.Null : root.ToSharedReference();
        RemoteValue result = await group.ScriptHost.CallSnapshotAsync(frame.Id, SnapshotFunction, [rootArgument, LocalValue.Boolean(options.IncludeRefs), LocalValue.String(frame.Page.GetSnapshotRefPrefix(frame))], budget).ConfigureAwait(false);

        AriaNode tree;
        using (JsonDocument document = JsonDocument.Parse(Property(result, "tree").As<StringRemoteValue>().Value))
        {
            tree = AriaNode.FromJson(document.RootElement);
        }

        string text = Property(result, "text").As<StringRemoteValue>().Value;
        IList<RemoteValue> refs = Property(result, "refs").As<CollectionRemoteValue>().Value!;
        IList<RemoteValue> elements = Property(result, "elements").As<CollectionRemoteValue>().Value!;
        for (int i = 0; i < refs.Count; i++)
        {
            targets[refs[i].As<StringRemoteValue>().Value] = new AriaSnapshotTarget(frame, elements[i].As<NodeRemoteValue>());
        }

        if (!options.IncludeFrames)
        {
            return (tree, text);
        }

        // A frame with no document stays an empty iframe node.
        List<AriaNode> frameNodes = [.. Descendants(tree).Where(node => node.Role == AriaNode.FrameRole)];
        List<string?> frameTexts = [];
        IList<RemoteValue> windows = Property(result, "frames").As<CollectionRemoteValue>().Value!;
        for (int i = 0; i < windows.Count; i++)
        {
            if (windows[i] is not WindowProxyRemoteValue window)
            {
                frameTexts.Add(null);
                continue;
            }

            Frame childFrame = await FindTrackedFrameAsync(group, window.Value.BrowsingContextId, budget).ConfigureAwait(false);
            (AriaNode childTree, string childText) = await TakeFrameAsync(childFrame, null, options, targets, budget).ConfigureAwait(false);
            frameNodes[i].AddChildren(childTree.Children);
            frameTexts.Add(childText);
        }

        return (tree, InsertFrameTexts(text, frameTexts));
    }

    // The group tracks a frame once the browser reports it, which can be after the page can see it.
    private static async Task<Frame> FindTrackedFrameAsync(BrowserGroup group, string contextId, TimeBudget budget)
    {
        while (true)
        {
            if (group.FindFrame(contextId) is Frame frame)
            {
                return frame;
            }

            if (budget.IsExhausted)
            {
                throw new WebDriverBiDiTimeoutException($"Timed out after {budget.Duration.TotalSeconds} seconds waiting for the frame {contextId} to be tracked.");
            }

            await budget.DelayAsync(group.Options.PollInterval).ConfigureAwait(false);
        }
    }

    // Each frame's text goes beneath its frame's line, one level deeper, and that line gains a colon.
    private static string InsertFrameTexts(string text, List<string?> frameTexts)
    {
        List<string> lines = [];
        int frameIndex = 0;
        foreach (string line in text.Split('\n'))
        {
            Match match = FrameLine.Match(line);
            string? frameText = match.Success ? frameTexts[frameIndex++] : null;
            if (string.IsNullOrEmpty(frameText))
            {
                lines.Add(line);
                continue;
            }

            lines.Add($"{line}:");
            string indent = match.Groups[1].Value + "  ";
            lines.AddRange(frameText!.Split('\n').Select(frameLine => indent + frameLine));
        }

        return string.Join("\n", lines);
    }

    private static IEnumerable<AriaNode> Descendants(AriaNode node)
    {
        foreach (AriaNode child in node.Children)
        {
            yield return child;
            foreach (AriaNode descendant in Descendants(child))
            {
                yield return descendant;
            }
        }
    }

    private static RemoteValue Property(RemoteValue value, string name)
    {
        return value.As<KeyValuePairCollectionRemoteValue>().Value!.First(property => property.Key is string key && key == name).Value;
    }
}
