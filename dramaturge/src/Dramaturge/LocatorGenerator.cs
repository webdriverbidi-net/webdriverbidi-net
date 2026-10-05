// <copyright file="LocatorGenerator.cs" company="WebDriverBiDi.NET Committers">
// Copyright (c) WebDriverBiDi.NET Committers. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.
// </copyright>

namespace Dramaturge;

using System.Text.Json;
using WebDriverBiDi.BrowsingContext;
using WebDriverBiDi.Script;

/// <summary>
/// Chooses the locator a recording writes for an element, from the facts the page gave about it and its nameable
/// ancestors: the first candidate that Dramaturge's own resolution finds the element alone with, in the order test
/// ID, role and name, label, placeholder, alt text, title, text, and role; then one of those within an ancestor
/// found alone by its test ID, role and name, or ID; then the first that finds the element among others, with its
/// index; then the element's CSS path.
/// </summary>
internal static class LocatorGenerator
{
    private const int MaxTextLength = 80;

    private static readonly HashSet<string> UnnamedRoles = ["generic", "none", "presentation"];

    /// <summary>
    /// Chooses a locator for an element.
    /// </summary>
    /// <param name="frame">The frame the element is in.</param>
    /// <param name="target">The element's facts.</param>
    /// <param name="ancestors">The facts of the element's nameable ancestors, nearest first.</param>
    /// <param name="elements">The element, then its ancestors, as the page sent them.</param>
    /// <param name="budget">The time resolving the candidates may take.</param>
    /// <returns>The locator, and its C# after the frame's variable, such as <c>GetByRole("button", "Save")</c>.</returns>
    public static async Task<(string Code, ElementLocator Locator)> GenerateAsync(Frame frame, JsonElement target, IReadOnlyList<JsonElement> ancestors, IReadOnlyList<NodeRemoteValue> elements, TimeBudget budget)
    {
        string? targetId = elements[0].SharedId;
        List<Candidate> candidates = [.. TargetCandidates(target)];
        Candidate? containing = null;
        int index = 0;
        foreach (Candidate candidate in candidates)
        {
            ElementLocator locator = candidate.OnFrame(frame);
            List<string?> matches = await ResolveAsync(locator, budget).ConfigureAwait(false);
            if (matches.Count == 1 && matches[0] == targetId)
            {
                return (candidate.Code, locator);
            }

            if (containing is null && matches.Contains(targetId))
            {
                (containing, index) = (candidate, matches.IndexOf(targetId));
            }
        }

        // An element gone from its document, as when its click navigated, can no longer be found; its first candidate is the likeliest.
        if (containing is null && candidates.Count > 0 && !await IsConnectedAsync(frame, elements[0], budget).ConfigureAwait(false))
        {
            return (candidates[0].Code, candidates[0].OnFrame(frame));
        }

        for (int i = 0; i < ancestors.Count && i + 1 < elements.Count; i++)
        {
            if (await FindAloneAsync(frame, AncestorCandidates(ancestors[i]), elements[i + 1].SharedId, budget).ConfigureAwait(false) is not (string scopeCode, ElementLocator scope))
            {
                continue;
            }

            foreach (Candidate candidate in candidates)
            {
                ElementLocator locator = candidate.Within!(scope);
                List<string?> matches = await ResolveAsync(locator, budget).ConfigureAwait(false);
                if (matches.Count == 1 && matches[0] == targetId)
                {
                    return ($"{scopeCode}.{candidate.Code}", locator);
                }
            }
        }

        if (containing is not null)
        {
            return ($"{containing.Code}.Nth({index})", containing.OnFrame(frame).Nth(index));
        }

        Candidate path = Css(target.GetProperty("cssPath").GetString()!);
        return (path.Code, path.OnFrame(frame));
    }

    private static IEnumerable<Candidate> TargetCandidates(JsonElement facts)
    {
        if (Text(facts, "testId") is string testId)
        {
            yield return TestId(testId);
        }

        string? role = Text(facts, "role");
        bool namedRole = role is not null && !UnnamedRoles.Contains(role);
        string name = Text(facts, "name") ?? string.Empty;
        if (namedRole && name.Length > 0)
        {
            yield return Role(role!, name);
        }

        foreach (string label in facts.GetProperty("labels").EnumerateArray().Select(label => label.GetString()!).Where(label => label.Length > 0))
        {
            yield return Partial("GetByLabel", label, (frame, text) => frame.GetByLabel(text), (scope, text) => scope.GetByLabel(text));
            yield return Exact("GetByLabel", label, (frame, text) => frame.GetByLabel(text, true), (scope, text) => scope.GetByLabel(text, true));
        }

        foreach ((string property, string method) in new[] { ("placeholder", "GetByPlaceholder"), ("alt", "GetByAltText"), ("title", "GetByTitle") })
        {
            if (Text(facts, property) is string value)
            {
                yield return Partial(method, value, (frame, text) => AttributeQuery(frame, property, text, false), (scope, text) => AttributeQuery(scope, property, text, false));
                yield return Exact(method, value, (frame, text) => AttributeQuery(frame, property, text, true), (scope, text) => AttributeQuery(scope, property, text, true));
            }
        }

        string shown = Text(facts, "text") ?? string.Empty;
        if (shown.Length > 0)
        {
            string shortened = Shorten(shown);
            yield return Partial("GetByText", shortened, (frame, text) => frame.GetByText(text), (scope, text) => scope.GetByText(text));
            if (shortened == shown)
            {
                yield return Exact("GetByText", shown, (frame, text) => frame.GetByText(text, true), (scope, text) => scope.GetByText(text, true));
            }
        }

        if (namedRole)
        {
            yield return Role(role!, null);
        }
    }

    private static IEnumerable<Candidate> AncestorCandidates(JsonElement facts)
    {
        if (Text(facts, "testId") is string testId)
        {
            yield return TestId(testId);
        }

        if (Text(facts, "role") is string role && !UnnamedRoles.Contains(role) && Text(facts, "name") is string name)
        {
            yield return Role(role, name);
        }

        // A CSS path that starts at the element is its unique ID.
        string cssPath = facts.GetProperty("cssPath").GetString()!;
        if (cssPath.StartsWith("#", StringComparison.Ordinal) && !cssPath.Contains(' '))
        {
            yield return Css(cssPath);
        }
    }

    private static async Task<(string Code, ElementLocator Locator)?> FindAloneAsync(Frame frame, IEnumerable<Candidate> candidates, string? elementId, TimeBudget budget)
    {
        foreach (Candidate candidate in candidates)
        {
            ElementLocator locator = candidate.OnFrame(frame);
            List<string?> matches = await ResolveAsync(locator, budget).ConfigureAwait(false);
            if (matches.Count == 1 && matches[0] == elementId)
            {
                return (candidate.Code, locator);
            }
        }

        return null;
    }

    // The shared IDs of the elements a locator finds now, or none when it cannot be resolved, as for a text locator
    // in a browser without one.
    private static async Task<List<string?>> ResolveAsync(ElementLocator locator, TimeBudget budget)
    {
        try
        {
            return [.. (await locator.FindAllAsync(budget).ConfigureAwait(false)).Select(node => node.SharedId)];
        }
        catch (Exception)
        {
            return [];
        }
    }

    private static async Task<bool> IsConnectedAsync(Frame frame, NodeRemoteValue element, TimeBudget budget)
    {
        try
        {
            RemoteValue connected = await frame.Page.Browser.Group.ScriptHost.CallAsync(frame.Id, "(inspector, element) => element.isConnected", [element.ToSharedReference()], budget).ConfigureAwait(false);
            return connected.As<BooleanRemoteValue>().Value;
        }
        catch (Exception)
        {
            return false;
        }
    }

    private static string? Text(JsonElement facts, string property)
    {
        return facts.TryGetProperty(property, out JsonElement value) && value.ValueKind == JsonValueKind.String && value.GetString()!.Length > 0 ? value.GetString() : null;
    }

    // Long text is cut at a word, which a partial match still finds.
    private static string Shorten(string text)
    {
        if (text.Length <= MaxTextLength)
        {
            return text;
        }

        int space = text.LastIndexOf(' ', MaxTextLength);
        return text.Substring(0, space > 0 ? space : MaxTextLength);
    }

    private static ElementLocator AttributeQuery(Frame frame, string property, string text, bool exact) => property switch
    {
        "placeholder" => frame.GetByPlaceholder(text, exact),
        "alt" => frame.GetByAltText(text, exact),
        _ => frame.GetByTitle(text, exact),
    };

    private static ElementLocator AttributeQuery(ElementLocator scope, string property, string text, bool exact) => property switch
    {
        "placeholder" => scope.GetByPlaceholder(text, exact),
        "alt" => scope.GetByAltText(text, exact),
        _ => scope.GetByTitle(text, exact),
    };

    private static Candidate TestId(string testId) => new($"GetByTestId({CodeWriter.Literal(testId)})", frame => frame.GetByTestId(testId), scope => scope.GetByTestId(testId));

    private static Candidate Role(string role, string? name) => name is null
        ? new($"GetByRole({CodeWriter.Literal(role)})", frame => frame.GetByRole(role), scope => scope.GetByRole(role))
        : new($"GetByRole({CodeWriter.Literal(role)}, {CodeWriter.Literal(name)})", frame => frame.GetByRole(role, name), scope => scope.GetByRole(role, name));

    private static Candidate Css(string selector) => new($"Locate(new CssLocator({CodeWriter.Literal(selector)}))", frame => frame.Locate(new CssLocator(selector)), null);

    private static Candidate Partial(string method, string text, Func<Frame, string, ElementLocator> onFrame, Func<ElementLocator, string, ElementLocator> within) => new($"{method}({CodeWriter.Literal(text)})", frame => onFrame(frame, text), scope => within(scope, text));

    private static Candidate Exact(string method, string text, Func<Frame, string, ElementLocator> onFrame, Func<ElementLocator, string, ElementLocator> within) => new($"{method}({CodeWriter.Literal(text)}, exact: true)", frame => onFrame(frame, text), scope => within(scope, text));

    // A way to find the element: its C#, and its locator in a frame or, for the element's own candidates, within another locator.
    private sealed record Candidate(string Code, Func<Frame, ElementLocator> OnFrame, Func<ElementLocator, ElementLocator>? Within);
}
