// <copyright file="AnalyzerConventionTests.cs" company="WebDriverBiDi.NET Committers">
// Copyright (c) WebDriverBiDi.NET Committers. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.
// </copyright>

namespace WebDriverBiDi.Analyzers.Tests;

using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;

/// <summary>
/// Conventions the analyzer and code fix sources must follow, checked against the sources themselves
/// rather than against behaviour, because a violation costs a silently missing diagnostic that no
/// individual analyzer test would think to look for.
/// </summary>
public class AnalyzerConventionTests
{
    /// <summary>
    /// A comparison of a syntax token's text against a member name, in either operand order, and the
    /// same comparison expressed as a membership test over a set of names.
    /// </summary>
    private static readonly Regex NameComparisonUsingText = new(
        """Identifier\.Text\s*(==|!=)\s*"|"\s*(==|!=)\s*[A-Za-z_.]*Identifier\.Text|\.Contains\([^)]*Identifier\.Text\)""",
        RegexOptions.Compiled);

    /// <summary>
    /// Asserts that no analyzer or code fix source compares an identifier's <c>Text</c> against a
    /// member name.
    /// </summary>
    /// <remarks>
    /// <para>
    /// <c>SyntaxToken.Text</c> is the identifier exactly as written, so for the verbatim identifier
    /// <c>@AddObserver</c> — legal C# meaning precisely <c>AddObserver</c> — it is <c>"@AddObserver"</c>
    /// and no name comparison matches. Every such comparison gates an analyzer's real work, so a
    /// mismatch is a silently lost diagnostic. <c>ValueText</c> strips the escape and is the value to
    /// compare.
    /// </para>
    /// <para>
    /// This deliberately does not ban <c>Identifier.Text</c> outright. Where an identifier is emitted
    /// into generated code, <c>Text</c> is the correct choice and <c>ValueText</c> would be a defect:
    /// dropping the <c>@</c> from a variable named <c>@event</c> produces source that does not compile.
    /// Only the comparisons are constrained.
    /// </para>
    /// </remarks>
    [Fact]
    public void NameComparisonsUseValueTextNotText()
    {
        List<string> violations = [];
        foreach (string file in EnumerateAnalyzerSources())
        {
            string[] lines = File.ReadAllLines(file);
            for (int i = 0; i < lines.Length; i++)
            {
                if (NameComparisonUsingText.IsMatch(lines[i]))
                {
                    violations.Add($"{Path.GetFileName(file)}:{i + 1}: {lines[i].Trim()}");
                }
            }
        }

        Assert.Empty(violations);
    }

    /// <summary>
    /// Confirms the convention check is actually reading the analyzer sources, so that a broken path
    /// cannot make <see cref="NameComparisonsUseValueTextNotText"/> pass by finding nothing to check.
    /// </summary>
    [Fact]
    public void AnalyzerSourcesAreFound()
    {
        List<string> sources = EnumerateAnalyzerSources().ToList();
        Assert.Contains(sources, file => Path.GetFileName(file) == "AnalyzerSymbolHelpers.cs");
        Assert.True(sources.Count > 25, $"Expected the analyzer and code fix sources, found {sources.Count}.");
    }

    private static IEnumerable<string> EnumerateAnalyzerSources()
    {
        string repositoryRoot = AnalyzerTestHelpers.FindRepositoryRoot();
        foreach (string projectDirectory in new[] { "WebDriverBiDi.Analyzers", "WebDriverBiDi.Analyzers.CodeFixProviders" })
        {
            string directory = Path.Combine(repositoryRoot, "src", projectDirectory);
            foreach (string file in Directory.EnumerateFiles(directory, "*.cs", SearchOption.AllDirectories))
            {
                // Skip build intermediates, which contain generated copies of these sources.
                if (file.Contains($"{Path.DirectorySeparatorChar}obj{Path.DirectorySeparatorChar}") ||
                    file.Contains($"{Path.DirectorySeparatorChar}bin{Path.DirectorySeparatorChar}"))
                {
                    continue;
                }

                yield return file;
            }
        }
    }
}
