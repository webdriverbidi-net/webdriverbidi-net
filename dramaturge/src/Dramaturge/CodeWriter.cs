// <copyright file="CodeWriter.cs" company="WebDriverBiDi.NET Committers">
// Copyright (c) WebDriverBiDi.NET Committers. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.
// </copyright>

namespace Dramaturge;

using System.Globalization;
using System.Text;

/// <summary>
/// Writes recorded actions as C# statements, and statements as a whole file for a <see cref="CodeTarget"/>, with
/// <c>\n</c> line endings.
/// </summary>
internal static class CodeWriter
{
    /// <summary>
    /// The namespace of <see cref="WebDriverBiDi.BrowsingContext.CssLocator"/>.
    /// </summary>
    public const string BrowsingContextNamespace = "WebDriverBiDi.BrowsingContext";

    /// <summary>
    /// The namespace of <see cref="WebDriverBiDi.Input.Keys"/> and <see cref="WebDriverBiDi.Input.PointerButton"/>.
    /// </summary>
    public const string InputNamespace = "WebDriverBiDi.Input";

    /// <summary>
    /// The using directive's name that brings <see cref="Assertions.Expect(ElementLocator)"/> into scope.
    /// </summary>
    public const string AssertionsNamespace = "static Dramaturge.Assertions";

    private const string DefaultTestIdAttribute = "data-testid";

    // The key names KeyboardEvent.key gives that are also members of Keys.
    private static readonly HashSet<string> KeyMembers =
    [
        "Backspace", "Tab", "Enter", "Escape", "PageUp", "PageDown", "End", "Home", "ArrowLeft", "ArrowUp", "ArrowRight", "ArrowDown",
        "Insert", "Delete", "Pause", "Help", "Clear", "Cancel", "F1", "F2", "F3", "F4", "F5", "F6", "F7", "F8", "F9", "F10", "F11", "F12",
    ];

    /// <summary>
    /// Writes a string as a C# string literal.
    /// </summary>
    /// <param name="value">The string.</param>
    /// <returns>The literal, with its quotes.</returns>
    public static string Literal(string value)
    {
        StringBuilder builder = new("\"");
        foreach (char character in value)
        {
            builder.Append(character switch
            {
                '"' => "\\\"",
                '\\' => "\\\\",
                '\n' => "\\n",
                '\r' => "\\r",
                '\t' => "\\t",
                '\0' => "\\0",
                _ when char.IsControl(character) || character is (char)0x2028 or (char)0x2029 => $"\\u{(int)character:x4}",
                _ => character.ToString(),
            });
        }

        return builder.Append('"').ToString();
    }

    /// <summary>
    /// Writes lines of text as a raw string literal whose lines are indented one level more than the statement, and
    /// that has more quotes around it than any run of quotes in the text.
    /// </summary>
    /// <param name="text">The text, with <c>\n</c> line endings.</param>
    /// <returns>The literal.</returns>
    public static string RawLiteral(string text)
    {
        int longestQuotes = 0;
        int quotes = 0;
        foreach (char character in text)
        {
            quotes = character == '"' ? quotes + 1 : 0;
            longestQuotes = Math.Max(longestQuotes, quotes);
        }

        string delimiter = new('"', Math.Max(3, longestQuotes + 1));
        IEnumerable<string> lines = text.TrimEnd('\n').Split('\n').Select(line => line.Length == 0 ? line : $"    {line}");
        return $"{delimiter}\n{string.Join("\n", lines)}\n    {delimiter}";
    }

    /// <summary>
    /// Writes a key as the argument of <see cref="ElementLocator.PressAsync"/>: a member of
    /// <see cref="WebDriverBiDi.Input.Keys"/>, or a character's literal.
    /// </summary>
    /// <param name="key">The key, as <c>KeyboardEvent.key</c> gives it.</param>
    /// <returns>The argument, or <see langword="null"/> for a key that is neither.</returns>
    public static string? Key(string key)
    {
        if (KeyMembers.Contains(key))
        {
            return $"Keys.{key}";
        }

        return new StringInfo(key).LengthInTextElements == 1 ? Literal(key) : null;
    }

    /// <summary>
    /// Writes modifier keys as a <see cref="KeyModifiers"/> value.
    /// </summary>
    /// <param name="modifiers">The modifier keys' names.</param>
    /// <returns>The value, or <see langword="null"/> when there are none.</returns>
    public static string? Modifiers(IReadOnlyList<string> modifiers)
    {
        return modifiers.Count == 0 ? null : string.Join(" | ", modifiers.Select(modifier => $"KeyModifiers.{modifier}"));
    }

    /// <summary>
    /// Writes an options argument, as a target-typed object creation.
    /// </summary>
    /// <param name="settings">The settings, such as <c>Button = PointerButton.Right</c>, each omitted when <see langword="null"/>.</param>
    /// <returns>The argument, or empty when there are no settings.</returns>
    public static string Options(params string?[] settings)
    {
        string[] given = [.. settings.OfType<string>()];
        return given.Length == 0 ? string.Empty : $"new() {{ {string.Join(", ", given)} }}";
    }

    /// <summary>
    /// Writes a collection expression.
    /// </summary>
    /// <param name="items">The items, each already C#.</param>
    /// <returns>The collection expression.</returns>
    public static string Collection(IEnumerable<string> items)
    {
        return $"[{string.Join(", ", items)}]";
    }

    /// <summary>
    /// Writes a whole file for a target.
    /// </summary>
    /// <param name="target">The target.</param>
    /// <param name="browserName">The recorded browser's name, as its session reported it, or empty when unknown.</param>
    /// <param name="testIdAttribute">The test ID attribute the recording's locators use.</param>
    /// <param name="statements">The statements, in order.</param>
    /// <param name="namespaces">The namespaces the statements use.</param>
    /// <returns>The file.</returns>
    public static string File(CodeTarget target, string browserName, string testIdAttribute, IReadOnlyList<string> statements, IEnumerable<string> namespaces)
    {
        bool customTestId = testIdAttribute != DefaultTestIdAttribute;
        (string? package, string? framework, string? classAttribute, string? testAttribute) = target switch
        {
            CodeTarget.Xunit => ("Dramaturge.Xunit", "Xunit", null, "[Fact]"),
            CodeTarget.NUnit => ("Dramaturge.NUnit", "NUnit.Framework", null, "[Test]"),
            CodeTarget.MSTest => ("Dramaturge.MSTest", "Microsoft.VisualStudio.TestTools.UnitTesting", "[TestClass]", "[TestMethod]"),
            CodeTarget.TUnit => ("Dramaturge.TUnit", "TUnit.Core", null, "[Test]"),
            _ => ((string?)null, (string?)null, (string?)null, (string?)null),
        };
        SortedSet<string> usings = new(StringComparer.Ordinal) { "Dramaturge" };
        usings.UnionWith(namespaces);
        usings.UnionWith(package is null ? ["Dramaturge.Browsers"] : [package, framework!]);

        List<string> lines = [.. usings.Select(name => $"using {name};"), string.Empty];
        if (package is null)
        {
            string options = customTestId ? $", new DramaturgeOptions() {{ TestIdAttribute = {Literal(testIdAttribute)} }}" : string.Empty;
            lines.Add($"await using BrowserGroup group = await BrowserGroup.LaunchAsync({Launcher(browserName)}.WithHeadlessOption(false){options});");
            lines.Add("Page page = await group.DefaultBrowser.NewPageAsync();");
            lines.AddRange(Indent(statements, string.Empty));
        }
        else
        {
            if (classAttribute is not null)
            {
                lines.Add(classAttribute);
            }

            lines.AddRange(["public class RecordedTests : PageTest", "{"]);
            if (customTestId)
            {
                lines.AddRange([$"    protected override DramaturgeOptions? GroupOptions => new() {{ TestIdAttribute = {Literal(testIdAttribute)} }};", string.Empty]);
            }

            lines.AddRange([$"    {testAttribute}", "    public async Task Recorded()", "    {", "        Page page = this.Page;"]);
            lines.AddRange(Indent(statements, "        "));
            lines.AddRange(["    }", "}"]);
        }

        return string.Join("\n", lines) + "\n";
    }

    // The launcher of the recorded browser, or the one the environment chooses when the browser is not one Dramaturge launches.
    private static string Launcher(string browserName)
    {
        return browserName switch
        {
            "firefox" => "BrowserLauncher.Configure(BrowserKind.Firefox)",
            "chrome" => "BrowserLauncher.Configure(BrowserKind.Chrome)",
            "msedge" => "BrowserLauncher.Configure(BrowserKind.Edge)",
            _ => "BrowserLauncher.ConfigureFromEnvironment()",
        };
    }

    private static IEnumerable<string> Indent(IReadOnlyList<string> statements, string indent)
    {
        return statements.SelectMany(statement => statement.Split('\n')).Select(line => line.Length == 0 ? line : indent + line);
    }
}
