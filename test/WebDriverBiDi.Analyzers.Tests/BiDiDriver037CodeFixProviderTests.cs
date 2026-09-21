// <copyright file="BiDiDriver037CodeFixProviderTests.cs" company="WebDriverBiDi.NET Committers">
// Copyright (c) WebDriverBiDi.NET Committers. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.
// </copyright>

namespace WebDriverBiDi.Analyzers.Tests;

using System.Collections.Immutable;
using System.Threading.Tasks;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CodeFixes;
using Microsoft.CodeAnalysis.Testing;

/// <summary>
/// Tests for the BiDiDriver037 code fix provider.
/// </summary>
public class BiDiDriver037CodeFixProviderTests
{
    [Fact]
    public async Task CodeFix_AddsJsonIncludeAttribute()
    {
        string testCode = """
            using System.Text.Json.Serialization;
            using WebDriverBiDi;

            namespace TestApp
            {
                public record MyResult : CommandResult
                {
                    [JsonPropertyName("value")]
                    public string {|#0:Value|} { get; internal set; } = string.Empty;
                }
            }
            """;

        string fixedCode = """
            using System.Text.Json.Serialization;
            using WebDriverBiDi;

            namespace TestApp
            {
                public record MyResult : CommandResult
                {
                    [JsonPropertyName("value")]
                    [JsonInclude]
                    public string Value { get; internal set; } = string.Empty;
                }
            }
            """;

        await VerifyFixAsync(testCode, fixedCode, "Value", "value", "its setter is not public", "MyResult");
    }

    [Fact]
    public async Task CodeFix_QualifiesTheAttributeWhenTheNamespaceIsNotImported()
    {
        // The name attribute is written out in full, which is the one sign available here that the
        // attribute namespace may not be in scope; the fix writes its own attribute the same way
        // rather than adding a using directive the file may not want.
        string testCode = """
            using WebDriverBiDi;

            namespace TestApp
            {
                public record MyResult : CommandResult
                {
                    [System.Text.Json.Serialization.JsonPropertyName("value")]
                    public string {|#0:Value|} { get; internal set; } = string.Empty;
                }
            }
            """;

        string fixedCode = """
            using WebDriverBiDi;

            namespace TestApp
            {
                public record MyResult : CommandResult
                {
                    [System.Text.Json.Serialization.JsonPropertyName("value")]
                    [System.Text.Json.Serialization.JsonInclude]
                    public string Value { get; internal set; } = string.Empty;
                }
            }
            """;

        await VerifyFixAsync(testCode, fixedCode, "Value", "value", "its setter is not public", "MyResult");
    }

    [Fact]
    public async Task CodeFix_KeepsTheDocumentationCommentAboveTheProperty()
    {
        string testCode = """
            using System.Text.Json.Serialization;
            using WebDriverBiDi;

            namespace TestApp
            {
                public record MyResult : CommandResult
                {
                    /// <summary>
                    /// Gets the value the remote end sent.
                    /// </summary>
                    [JsonPropertyName("value")]
                    public string {|#0:Value|} { get; internal set; } = string.Empty;
                }
            }
            """;

        string fixedCode = """
            using System.Text.Json.Serialization;
            using WebDriverBiDi;

            namespace TestApp
            {
                public record MyResult : CommandResult
                {
                    /// <summary>
                    /// Gets the value the remote end sent.
                    /// </summary>
                    [JsonPropertyName("value")]
                    [JsonInclude]
                    public string Value { get; internal set; } = string.Empty;
                }
            }
            """;

        await VerifyFixAsync(testCode, fixedCode, "Value", "value", "its setter is not public", "MyResult");
    }

    /// <summary>
    /// Tests FixableDiagnosticIds contains BIDI037.
    /// </summary>
    [Fact]
    public void FixableDiagnosticIds_ContainsBIDI037()
    {
        BiDiDriver037_UnsettableDeserializedPropertyCodeFixProvider provider = new();
        ImmutableArray<string> ids = provider.FixableDiagnosticIds;

        Assert.Single(ids);
        Assert.Equal(BiDiDriver037_UnsettableDeserializedPropertyAnalyzer.DiagnosticId, ids[0]);
    }

    /// <summary>
    /// Tests GetFixAllProvider returns the batch fixer.
    /// </summary>
    [Fact]
    public void GetFixAllProvider_ReturnsBatchFixer()
    {
        BiDiDriver037_UnsettableDeserializedPropertyCodeFixProvider provider = new();
        FixAllProvider fixAllProvider = provider.GetFixAllProvider();

        Assert.NotNull(fixAllProvider);
        Assert.Equal(WellKnownFixAllProviders.BatchFixer, fixAllProvider);
    }

    private static async Task VerifyFixAsync(string testCode, string fixedCode, params object[] arguments)
    {
        DiagnosticResult expected = new DiagnosticResult(BiDiDriver037_UnsettableDeserializedPropertyAnalyzer.DiagnosticId, DiagnosticSeverity.Warning)
            .WithLocation(0)
            .WithArguments(arguments);

        RealAssemblyCodeFixTest<BiDiDriver037_UnsettableDeserializedPropertyAnalyzer, BiDiDriver037_UnsettableDeserializedPropertyCodeFixProvider> testState = new()
        {
            TestCode = testCode,
            FixedCode = fixedCode,
        };
        testState.ExpectedDiagnostics.Add(expected);

        await testState.RunAsync(TestContext.Current.CancellationToken);
    }
}
