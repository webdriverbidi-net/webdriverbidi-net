// <copyright file="AnalyzerTestHelpers.cs" company="WebDriverBiDi.NET Committers">
// Copyright (c) WebDriverBiDi.NET Committers. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.
// </copyright>

namespace WebDriverBiDi.Analyzers.Tests;

using System.IO;
using System.Reflection;
using System.Threading.Tasks;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CodeFixes;
using Microsoft.CodeAnalysis.CSharp.Testing;
using Microsoft.CodeAnalysis.Diagnostics;
using Microsoft.CodeAnalysis.Testing;
using WebDriverBiDi;

/// <summary>
/// A <see cref="CSharpCodeFixTest{TAnalyzer, TCodeFix, TVerifier}"/> that forces LF line endings
/// for the ad-hoc test project.
/// </summary>
/// <typeparam name="TAnalyzer">The type of analyzer under test.</typeparam>
/// <typeparam name="TCodeFix">The type of code fix provider under test.</typeparam>
/// <remarks>
/// Microsoft.CodeAnalysis.Testing's fix-verification pipeline reformats the span touched by a code
/// fix using the ambient newline setting, which falls back to <c>Environment.NewLine</c> when no
/// <c>end_of_line</c> EditorConfig override is in effect for the test project. That makes code fix
/// tests fail on Windows (actual output gets CRLF on the touched line) even though the checked-out
/// source and the code fix providers themselves are LF-only — this is independent of the code fix
/// provider's own logic and cannot be worked around from within a provider. Injecting an explicit
/// <c>end_of_line = lf</c> EditorConfig here makes the comparison consistent across platforms.
/// </remarks>
public sealed class LfCodeFixTest<TAnalyzer, TCodeFix> : CSharpCodeFixTest<TAnalyzer, TCodeFix, DefaultVerifier>
    where TAnalyzer : DiagnosticAnalyzer, new()
    where TCodeFix : CodeFixProvider, new()
{
    private const string LfEditorConfig = "root = true\n\n[*]\nend_of_line = lf\n";

    /// <summary>
    /// Initializes a new instance of the <see cref="LfCodeFixTest{TAnalyzer, TCodeFix}"/> class.
    /// </summary>
    public LfCodeFixTest()
    {
        // Only TestState needs this: it is the project the code fix provider actually runs
        // against, which is where the newline-sensitive cleanup pipeline reads its EditorConfig
        // from. Adding it to FixedState too is unnecessary and, for the handful of tests that
        // omit FixedCode (relying on the framework's "no explicit fixed state" skip of full output
        // comparison), it defeats that skip by giving FixedState explicit content.
        this.TestState.AnalyzerConfigFiles.Add(("/.editorconfig", LfEditorConfig));
    }
}

/// <summary>
/// Helper methods for analyzer tests.
/// </summary>
public static class AnalyzerTestHelpers
{
    /// <summary>
    /// Verifies that an analyzer produces no diagnostics for the given test code.
    /// </summary>
    /// <typeparam name="TAnalyzer">The type of analyzer to test.</typeparam>
    /// <param name="testCode">The test code to analyze.</param>
    /// <returns>A task representing the asynchronous operation.</returns>
    public static async Task VerifyAnalyzerAsync<TAnalyzer>(string testCode)
        where TAnalyzer : DiagnosticAnalyzer, new()
    {
        CSharpAnalyzerTest<TAnalyzer, DefaultVerifier> test = new()
        {
            TestCode = testCode,
            ReferenceAssemblies = ReferenceAssemblies.Net.Net80,
        };

        test.TestState.AdditionalReferences.Add(MetadataReference.CreateFromFile(GetWebDriverBiDiAssemblyPath()));

        await test.RunAsync();
    }

    /// <summary>
    /// Verifies that an analyzer produces the expected diagnostics for the given test code.
    /// </summary>
    /// <typeparam name="TAnalyzer">The type of analyzer to test.</typeparam>
    /// <param name="testCode">The test code to analyze.</param>
    /// <param name="expected">The expected diagnostic results.</param>
    /// <returns>A task representing the asynchronous operation.</returns>
    public static async Task VerifyAnalyzerAsync<TAnalyzer>(string testCode, params DiagnosticResult[] expected)
        where TAnalyzer : DiagnosticAnalyzer, new()
    {
        CSharpAnalyzerTest<TAnalyzer, DefaultVerifier> test = new()
        {
            TestCode = testCode,
            ReferenceAssemblies = ReferenceAssemblies.Net.Net80,
        };

        test.TestState.AdditionalReferences.Add(MetadataReference.CreateFromFile(GetWebDriverBiDiAssemblyPath()));
        test.ExpectedDiagnostics.AddRange(expected);

        await test.RunAsync();
    }

    /// <summary>
    /// Gets the path to the WebDriverBiDi assembly for use in code fix tests.
    /// </summary>
    /// <returns>The assembly path.</returns>
    internal static string GetWebDriverBiDiAssemblyPath()
    {
        // Get the test assembly's location
        string testAssemblyPath = Assembly.GetExecutingAssembly().Location;
        string testDirectory = Path.GetDirectoryName(testAssemblyPath) ?? string.Empty;

        // Extract configuration (Debug/Release) from path: .../bin/{Configuration}/net10.0
        string? configDir = Path.GetDirectoryName(testDirectory);
        string configuration = configDir != null ? Path.GetFileName(configDir) : "Debug";

        // Navigate up from test/WebDriverBiDi.Analyzers.Tests/bin/{Configuration}/net10.0
        // to get to the project root, then go to src/WebDriverBiDi/bin/{Configuration}/net10.0
        string? currentPath = testDirectory;

        // Go up to the project root (5 levels up: net10.0 -> Configuration -> bin -> WebDriverBiDi.Analyzers.Tests -> test)
        for (int i = 0; i < 5 && currentPath != null; i++)
        {
            currentPath = Path.GetDirectoryName(currentPath);
        }

        if (currentPath != null)
        {
            string net80AssemblyPath = Path.Combine(currentPath, "src", "WebDriverBiDi", "bin", configuration, "net8.0", "WebDriverBiDi.dll");
            if (File.Exists(net80AssemblyPath))
            {
                return net80AssemblyPath;
            }
        }

        // Fall back to the current loaded assembly
        return typeof(BiDiDriver).Assembly.Location;
    }
}
