// <copyright file="BiDiDriver034AnalyzerTests.cs" company="WebDriverBiDi.NET Committers">
// Copyright (c) WebDriverBiDi.NET Committers. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.
// </copyright>

namespace WebDriverBiDi.Analyzers.Tests;

using System.Threading.Tasks;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Testing;

/// <summary>
/// Tests for the BiDiDriver034 analyzer.
/// </summary>
/// <remarks>
/// The contexts in these tests are abstract so that they compile without the System.Text.Json source generator, which
/// would otherwise supply the members a concrete context must implement. The rule reads only the attributes.
/// </remarks>
public class BiDiDriver034AnalyzerTests
{
    /// <summary>
    /// Tests that each kind of library envelope named by a [JsonSerializable] attribute is reported.
    /// </summary>
    /// <param name="envelope">The envelope type, as written in the typeof expression.</param>
    /// <param name="displayName">The type's name as the diagnostic reports it.</param>
    /// <returns>A <see cref="Task"/> representing the asynchronous unit test.</returns>
    [Theory]
    [InlineData("CommandResponseMessage<ShoppingResult>", "CommandResponseMessage<ShoppingResult>")]
    [InlineData("EventMessage<ShoppingEventArgs>", "EventMessage<ShoppingEventArgs>")]
    [InlineData("ErrorResponseMessage", "ErrorResponseMessage")]
    [InlineData("Message", "Message")]
    [InlineData("CommandResponseMessage<>", "CommandResponseMessage<>")]
    public async Task EnvelopeTypeRegistered_ReportsWarning(string envelope, string displayName)
    {
        string testCode = $$"""
            using System.Text.Json;
            using System.Text.Json.Serialization;
            using WebDriverBiDi;
            using WebDriverBiDi.Protocol;

            namespace TestApp
            {
                [JsonSerializable(typeof(ShoppingResult))]
                [JsonSerializable({|#0:typeof({{envelope}})|})]
                public abstract class ShoppingJsonContext : JsonSerializerContext
                {
                    protected ShoppingJsonContext(JsonSerializerOptions options)
                        : base(options)
                    {
                    }
                }

                public record ShoppingResult : CommandResult
                {
                }

                public record ShoppingEventArgs : WebDriverBiDiEventArgs
                {
                }
            }
            """;

        DiagnosticResult expected = new DiagnosticResult(BiDiDriver034_EnvelopeTypeInSerializerContextAnalyzer.DiagnosticId, DiagnosticSeverity.Warning)
            .WithLocation(0)
            .WithArguments(displayName);

        await AnalyzerTestHelpers.VerifyAnalyzerAsync<BiDiDriver034_EnvelopeTypeInSerializerContextAnalyzer>(testCode, expected);
    }

    /// <summary>
    /// Tests the attributes the rule leaves alone: the user's own result and event args types, a library type that is not
    /// an envelope, a user type deriving from Message, a same-named attribute from another namespace, an attribute whose
    /// argument is not a typeof, and other attributes.
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous unit test.</returns>
    [Fact]
    public async Task NonEnvelopeRegistrations_NoDiagnostic()
    {
        string testCode = """
            using System;
            using System.Text.Json;
            using System.Text.Json.Serialization;
            using WebDriverBiDi;
            using WebDriverBiDi.Protocol;
            using WebDriverBiDi.Session;

            namespace TestApp
            {
                [JsonSerializable(typeof(ShoppingResult))]
                [JsonSerializable(typeof(ShoppingEventArgs))]
                [JsonSerializable(typeof(StatusCommandParameters))]
                [JsonSerializable(typeof(ShoppingMessage))]
                [Other.JsonSerializable(typeof(Message))]
                [JsonSerializable(typeof(Message[]))]
                [Obsolete()]
                [Serializable]
                public abstract class ShoppingJsonContext : JsonSerializerContext
                {
                    protected ShoppingJsonContext(JsonSerializerOptions options)
                        : base(options)
                    {
                    }
                }

                public record ShoppingResult : CommandResult
                {
                }

                public record ShoppingEventArgs : WebDriverBiDiEventArgs
                {
                }

                public class ShoppingMessage : Message
                {
                }
            }

            namespace Other
            {
                [AttributeUsage(AttributeTargets.Class, AllowMultiple = true)]
                public sealed class JsonSerializableAttribute : Attribute
                {
                    public JsonSerializableAttribute(Type type)
                    {
                    }
                }
            }
            """;

        await AnalyzerTestHelpers.VerifyAnalyzerAsync<BiDiDriver034_EnvelopeTypeInSerializerContextAnalyzer>(testCode);
    }

    /// <summary>
    /// Tests that an attribute that does not resolve, as while it is being typed, is left alone rather than making the
    /// rule throw.
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous unit test.</returns>
    [Fact]
    public async Task UnresolvedAttribute_NoDiagnosticAndNoFailure()
    {
        string testCode = """
            using WebDriverBiDi.Protocol;

            namespace TestApp
            {
                [JsonSerializabl(typeof(Message))]
                public class ShoppingJsonContext
                {
                }
            }
            """;

        RealAssemblyAnalyzerTest<BiDiDriver034_EnvelopeTypeInSerializerContextAnalyzer> testState = new()
        {
            TestCode = testCode,
            CompilerDiagnostics = CompilerDiagnostics.None,
        };

        await testState.RunAsync(TestContext.Current.CancellationToken);
    }
}
