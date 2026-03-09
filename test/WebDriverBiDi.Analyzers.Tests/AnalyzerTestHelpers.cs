// <copyright file="AnalyzerTestHelpers.cs" company="WebDriverBiDi.NET Committers">
// Copyright (c) WebDriverBiDi.NET Committers. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.
// </copyright>

namespace WebDriverBiDi.Analyzers.Tests;

using System.IO;
using System.Reflection;
using System.Threading.Tasks;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Testing;
using Microsoft.CodeAnalysis.Diagnostics;
using Microsoft.CodeAnalysis.Testing;
using WebDriverBiDi;

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
