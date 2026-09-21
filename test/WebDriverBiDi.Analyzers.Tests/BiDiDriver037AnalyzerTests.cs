// <copyright file="BiDiDriver037AnalyzerTests.cs" company="WebDriverBiDi.NET Committers">
// Copyright (c) WebDriverBiDi.NET Committers. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.
// </copyright>

namespace WebDriverBiDi.Analyzers.Tests;

using System.Threading.Tasks;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Testing;
using Microsoft.CodeAnalysis.Testing;

/// <summary>
/// Tests for the BiDiDriver037 analyzer.
/// </summary>
/// <remarks>
/// The behaviour these tests pin was measured against System.Text.Json 10: a public property with a
/// non-public setter, and a non-public property with any setter, are both left at their default value
/// by <c>JsonSerializer.Deserialize</c> unless the property is marked <c>[JsonInclude]</c>, while a
/// constructor parameter of the same name assigns it whatever the accessors say.
/// </remarks>
public class BiDiDriver037AnalyzerTests
{
    [Fact]
    public async Task PublicPropertyWithInternalSetter_ReportsWarning()
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

        await VerifyAsync(testCode, Expect("Value", "value", "its setter is not public", "MyResult"));
    }

    [Fact]
    public async Task PublicPropertyWithPrivateInit_ReportsWarning()
    {
        // An init accessor is a setter the deserializer may use, and a non-public one is as invisible
        // to it as a non-public set.
        string testCode = """
            using System.Text.Json.Serialization;
            using WebDriverBiDi;

            namespace TestApp
            {
                public record MyResult : CommandResult
                {
                    [JsonPropertyName("value")]
                    public string {|#0:Value|} { get; private init; } = string.Empty;
                }
            }
            """;

        await VerifyAsync(testCode, Expect("Value", "value", "its setter is not public", "MyResult"));
    }

    [Fact]
    public async Task NonPublicPropertyWithPublicSetter_ReportsWarning()
    {
        string testCode = """
            using System.Text.Json.Serialization;
            using WebDriverBiDi;

            namespace TestApp
            {
                public record MyResult : CommandResult
                {
                    [JsonPropertyName("value")]
                    internal string {|#0:Value|} { get; set; } = string.Empty;
                }
            }
            """;

        await VerifyAsync(testCode, Expect("Value", "value", "the property is not public", "MyResult"));
    }

    [Fact]
    public async Task EventArgsProperty_ReportsWarning()
    {
        string testCode = """
            using System.Text.Json.Serialization;
            using WebDriverBiDi;

            namespace TestApp
            {
                public record MyEventArgs : WebDriverBiDiEventArgs
                {
                    [JsonPropertyName("context")]
                    public string {|#0:Context|} { get; internal set; } = string.Empty;
                }
            }
            """;

        await VerifyAsync(testCode, Expect("Context", "context", "its setter is not public", "MyEventArgs"));
    }

    [Fact]
    public async Task IndirectlyDerivedResult_ReportsWarning()
    {
        string testCode = """
            using System.Text.Json.Serialization;
            using WebDriverBiDi;

            namespace TestApp
            {
                public record MyResult : CommandResult
                {
                }

                public record MyDerivedResult : MyResult
                {
                    [JsonPropertyName("value")]
                    public string {|#0:Value|} { get; internal set; } = string.Empty;
                }
            }
            """;

        await VerifyAsync(testCode, Expect("Value", "value", "its setter is not public", "MyDerivedResult"));
    }

    [Fact]
    public async Task ConditionallyIgnoredProperty_ReportsWarning()
    {
        // A condition such as WhenWritingNull leaves the property mapped for reading.
        string testCode = """
            using System.Text.Json.Serialization;
            using WebDriverBiDi;

            namespace TestApp
            {
                public record MyResult : CommandResult
                {
                    [JsonPropertyName("value")]
                    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
                    public string? {|#0:Value|} { get; internal set; }
                }
            }
            """;

        await VerifyAsync(testCode, Expect("Value", "value", "its setter is not public", "MyResult"));
    }

    [Fact]
    public async Task ParameterlessConstructorAlongsideParameterized_ReportsWarning()
    {
        // The serializer takes the public parameterless constructor and then assigns properties, so
        // the parameter of the other constructor never carries the value.
        string testCode = """
            using System.Text.Json.Serialization;
            using WebDriverBiDi;

            namespace TestApp
            {
                public record MyResult : CommandResult
                {
                    public MyResult()
                    {
                    }

                    public MyResult(string value)
                    {
                        this.Value = value;
                    }

                    [JsonPropertyName("value")]
                    public string {|#0:Value|} { get; private set; } = string.Empty;
                }
            }
            """;

        await VerifyAsync(testCode, Expect("Value", "value", "its setter is not public", "MyResult"));
    }

    [Fact]
    public async Task TwoPublicConstructorsNeitherParameterless_ReportsWarning()
    {
        // With no parameterless constructor and no [JsonConstructor], no constructor is the
        // serializer's, so none of them can be said to assign the property.
        string testCode = """
            using System.Text.Json.Serialization;
            using WebDriverBiDi;

            namespace TestApp
            {
                public record MyResult : CommandResult
                {
                    public MyResult(string value)
                    {
                        this.Value = value;
                    }

                    public MyResult(int count)
                    {
                        this.Value = count.ToString();
                    }

                    [JsonPropertyName("value")]
                    public string {|#0:Value|} { get; private set; } = string.Empty;
                }
            }
            """;

        await VerifyAsync(testCode, Expect("Value", "value", "its setter is not public", "MyResult"));
    }

    [Fact]
    public async Task PropertyWithJsonInclude_ReportsNothing()
    {
        string testCode = """
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

        await VerifyAsync(testCode);
    }

    [Fact]
    public async Task PropertyWithoutJsonPropertyName_ReportsNothing()
    {
        // Without the name attribute the property is not declared to be part of the payload; it may be
        // state the type keeps for itself.
        string testCode = """
            using System.Text.Json.Serialization;
            using WebDriverBiDi;

            namespace TestApp
            {
                public record MyResult : CommandResult
                {
                    public string Value { get; internal set; } = string.Empty;
                }
            }
            """;

        await VerifyAsync(testCode);
    }

    [Fact]
    public async Task PublicSetter_ReportsNothing()
    {
        string testCode = """
            using System.Text.Json.Serialization;
            using WebDriverBiDi;

            namespace TestApp
            {
                public record MyResult : CommandResult
                {
                    [JsonPropertyName("value")]
                    public string Value { get; set; } = string.Empty;
                }
            }
            """;

        await VerifyAsync(testCode);
    }

    [Fact]
    public async Task GetOnlyProperty_ReportsNothing()
    {
        // [JsonInclude] gives the deserializer an existing accessor to use; it cannot supply one.
        string testCode = """
            using System.Text.Json.Serialization;
            using WebDriverBiDi;

            namespace TestApp
            {
                public record MyResult : CommandResult
                {
                    [JsonPropertyName("value")]
                    public string Value => string.Empty;
                }
            }
            """;

        await VerifyAsync(testCode);
    }

    [Fact]
    public async Task StaticProperty_ReportsNothing()
    {
        string testCode = """
            using System.Text.Json.Serialization;
            using WebDriverBiDi;

            namespace TestApp
            {
                public record MyResult : CommandResult
                {
                    [JsonPropertyName("value")]
                    public static string Value { get; private set; } = string.Empty;
                }
            }
            """;

        await VerifyAsync(testCode);
    }

    [Fact]
    public async Task Indexer_ReportsNothing()
    {
        string testCode = """
            using System.Text.Json.Serialization;
            using WebDriverBiDi;

            namespace TestApp
            {
                public record MyResult : CommandResult
                {
                    [JsonPropertyName("value")]
                    public string this[int index]
                    {
                        get => string.Empty;
                        internal set { }
                    }
                }
            }
            """;

        await VerifyAsync(testCode);
    }

    [Fact]
    public async Task UnrelatedType_ReportsNothing()
    {
        string testCode = """
            using System.Text.Json.Serialization;

            namespace TestApp
            {
                public record MyPayload
                {
                    [JsonPropertyName("value")]
                    public string Value { get; internal set; } = string.Empty;
                }
            }
            """;

        await VerifyAsync(testCode);
    }

    [Fact]
    public async Task TypeWithJsonConverter_ReportsNothing()
    {
        // A converter builds the object itself, so the property metadata the rule reasons about is
        // never consulted.
        string testCode = """
            using System;
            using System.Text.Json;
            using System.Text.Json.Serialization;
            using WebDriverBiDi;

            namespace TestApp
            {
                [JsonConverter(typeof(MyResultConverter))]
                public record MyResult : CommandResult
                {
                    [JsonPropertyName("value")]
                    public string Value { get; internal set; } = string.Empty;
                }

                public class MyResultConverter : JsonConverter<MyResult>
                {
                    public override MyResult Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options) => new MyResult();

                    public override void Write(Utf8JsonWriter writer, MyResult value, JsonSerializerOptions options)
                    {
                    }
                }
            }
            """;

        await VerifyAsync(testCode);
    }

    [Fact]
    public async Task AlwaysIgnoredProperty_ReportsNothing()
    {
        string testCode = """
            using System.Text.Json.Serialization;
            using WebDriverBiDi;

            namespace TestApp
            {
                public record MyResult : CommandResult
                {
                    [JsonPropertyName("value")]
                    [JsonIgnore]
                    public string Value { get; internal set; } = string.Empty;
                }
            }
            """;

        await VerifyAsync(testCode);
    }

    [Fact]
    public async Task JsonConstructorParameter_ReportsNothing()
    {
        // The constructor carries the value, whatever the setter's accessibility says, and the
        // serializer matches its parameter to the property without regard to case.
        string testCode = """
            using System.Text.Json.Serialization;
            using WebDriverBiDi;

            namespace TestApp
            {
                public record MyResult : CommandResult
                {
                    [JsonConstructor]
                    public MyResult(string value)
                    {
                        this.Value = value;
                    }

                    [JsonPropertyName("value")]
                    public string Value { get; private set; }
                }
            }
            """;

        await VerifyAsync(testCode);
    }

    [Fact]
    public async Task SinglePublicConstructorParameter_ReportsNothing()
    {
        string testCode = """
            using System.Text.Json.Serialization;
            using WebDriverBiDi;

            namespace TestApp
            {
                public record MyResult : CommandResult
                {
                    public MyResult(string value)
                    {
                        this.Value = value;
                    }

                    [JsonPropertyName("value")]
                    public string Value { get; private set; }
                }
            }
            """;

        await VerifyAsync(testCode);
    }

    [Fact]
    public async Task CompilationWithoutTheLibrary_ReportsNothing()
    {
        // Neither base type is in the compilation, so the rule registers nothing at all.
        string testCode = """
            using System.Text.Json.Serialization;

            namespace TestApp
            {
                public record MyResult
                {
                    [JsonPropertyName("value")]
                    public string Value { get; internal set; } = string.Empty;
                }
            }
            """;

        CSharpAnalyzerTest<BiDiDriver037_UnsettableDeserializedPropertyAnalyzer, DefaultVerifier> testState = new()
        {
            TestCode = testCode,
            ReferenceAssemblies = ReferenceAssemblies.Net.Net80,
        };

        await testState.RunAsync(TestContext.Current.CancellationToken);
    }

    private static DiagnosticResult Expect(string propertyName, string jsonName, string reason, string typeName)
    {
        return new DiagnosticResult(BiDiDriver037_UnsettableDeserializedPropertyAnalyzer.DiagnosticId, DiagnosticSeverity.Warning)
            .WithLocation(0)
            .WithArguments(propertyName, jsonName, reason, typeName);
    }

    private static async Task VerifyAsync(string testCode, params DiagnosticResult[] expected)
    {
        RealAssemblyAnalyzerTest<BiDiDriver037_UnsettableDeserializedPropertyAnalyzer> testState = new()
        {
            TestCode = testCode,
        };
        testState.ExpectedDiagnostics.AddRange(expected);

        await testState.RunAsync(TestContext.Current.CancellationToken);
    }
}
