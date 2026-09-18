// <copyright file="BiDiDriver033AnalyzerTests.cs" company="WebDriverBiDi.NET Committers">
// Copyright (c) WebDriverBiDi.NET Committers. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.
// </copyright>

namespace WebDriverBiDi.Analyzers.Tests;

using System.Threading.Tasks;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Testing;

/// <summary>
/// Tests for the BiDiDriver033 analyzer.
/// </summary>
public class BiDiDriver033AnalyzerTests
{
    private const string CustomParametersDeclarations = """

        namespace TestApp
        {
            using System.Text.Json.Serialization;
            using WebDriverBiDi;

            public record CustomResult : CommandResult
            {
            }

            public class CustomParameters : CommandParameters<CustomResult>
            {
                [JsonIgnore]
                public override string MethodName => "custom.command";

                public static string StaticName => "static";

                public string PlainName { get; set; } = string.Empty;

                [JsonPropertyName("included")]
                [JsonInclude]
                internal string Included { get; set; } = string.Empty;

                internal string InternalName { get; set; } = string.Empty;

                [JsonPropertyName("ignored")]
                [JsonIgnore]
                public string Ignored { get; set; } = string.Empty;

                [JsonIgnore(Condition = JsonIgnoreCondition.Always)]
                public string AlwaysIgnored { get; set; } = string.Empty;

                [JsonPropertyName("nullable")]
                [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
                public string? Nullable { get; set; }

                public string WriteOnly { set { } }

                public string this[int index] => string.Empty;
            }
        }
        """;

    /// <summary>
    /// Tests that each way of writing an entry is reported when the entry's name is one the owner already serializes.
    /// </summary>
    /// <param name="statements">The statements writing the entry, with the entry's name marked.</param>
    /// <param name="name">The entry's name.</param>
    /// <param name="ownerTypeName">The name of the type that owns the dictionary.</param>
    /// <returns>A <see cref="Task"/> representing the asynchronous unit test.</returns>
    [Theory]
    [InlineData("SetTimeZoneOverrideCommandParameters parameters = new(); parameters.AdditionalData[{|#0:\"timezone\"|}] = \"UTC\";", "timezone", "SetTimeZoneOverrideCommandParameters")]
    [InlineData("SetTimeZoneOverrideCommandParameters parameters = new(); parameters.AdditionalData.Add({|#0:\"timezone\"|}, null);", "timezone", "SetTimeZoneOverrideCommandParameters")]
    [InlineData("SetTimeZoneOverrideCommandParameters parameters = new(); parameters.AdditionalData.TryAdd({|#0:TimeZoneName|}, null);", "timezone", "SetTimeZoneOverrideCommandParameters")]
    [InlineData("SetTimeZoneOverrideCommandParameters parameters = new() { AdditionalData = { [{|#0:\"timezone\"|}] = \"UTC\" } };", "timezone", "SetTimeZoneOverrideCommandParameters")]
    [InlineData("SetTimeZoneOverrideCommandParameters parameters = new() { AdditionalData = { { {|#0:\"timezone\"|}, null } } };", "timezone", "SetTimeZoneOverrideCommandParameters")]
    [InlineData("SetCookieCommandParameters parameters = new(new PartialCookie(\"name\", BytesValue.FromString(\"value\"), \"example.com\")) { Cookie = { AdditionalData = { [{|#0:\"domain\"|}] = \"other.example.com\" } } };", "domain", "PartialCookie")]
    [InlineData("DirectProxyConfiguration proxy = new(); proxy.AdditionalData[{|#0:\"proxyType\"|}] = \"manual\";", "proxyType", "DirectProxyConfiguration")]
    [InlineData("CapabilityRequest capabilities = new(); capabilities.AdditionalCapabilities[{|#0:\"browserName\"|}] = \"chrome\";", "browserName", "CapabilityRequest")]
    [InlineData("Command command = new(1, new CustomParameters()); command.AdditionalCommandProperties[{|#0:\"params\"|}] = null;", "params", "Command")]
    [InlineData("CustomParameters custom = new(); custom.AdditionalData[{|#0:\"PlainName\"|}] = 1;", "PlainName", "CustomParameters")]
    [InlineData("CustomParameters custom = new(); custom.AdditionalData[{|#0:\"included\"|}] = 1;", "included", "CustomParameters")]
    [InlineData("CustomParameters custom = new(); custom.AdditionalData[{|#0:\"nullable\"|}] = 1;", "nullable", "CustomParameters")]
    public async Task EntryNamedForASerializedProperty_ReportsError(string statements, string name, string ownerTypeName)
    {
        string testCode = $$"""
            using WebDriverBiDi;
            using WebDriverBiDi.Emulation;
            using WebDriverBiDi.Network;
            using WebDriverBiDi.Protocol;
            using WebDriverBiDi.Session;
            using WebDriverBiDi.Storage;

            namespace TestApp
            {
                public class TestClass
                {
                    private const string TimeZoneName = "timezone";

                    public void TestMethod()
                    {
                        {{statements}}
                    }
                }
            }
            """ + CustomParametersDeclarations;

        DiagnosticResult expected = new DiagnosticResult(BiDiDriver033_ExtensionDataShadowsPropertyAnalyzer.DiagnosticId, DiagnosticSeverity.Error)
            .WithLocation(0)
            .WithArguments(name, ownerTypeName);

        await AnalyzerTestHelpers.VerifyAnalyzerAsync<BiDiDriver033_ExtensionDataShadowsPropertyAnalyzer>(testCode, expected);
    }

    /// <summary>
    /// Tests the writes the rule leaves alone: names no property is written under, names that are not constants, members
    /// that are not serialized, dictionaries that are not the library's extension data, and calls that add nothing.
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous unit test.</returns>
    [Fact]
    public async Task EntriesThatDoNotShadowASerializedProperty_NoDiagnostic()
    {
        string testCode = """
            using System.Collections.Generic;
            using System.Text.Json.Serialization;
            using WebDriverBiDi;
            using WebDriverBiDi.Emulation;
            using WebDriverBiDi.Network;

            namespace TestApp
            {
                public class TestClass
                {
                    public void TestMethod(string key, SetExtraHeadersCommandParameters headers, Header header)
                    {
                        SetTimeZoneOverrideCommandParameters parameters = new() { AdditionalData = { ["goog:extra"] = 1 } };
                        SetTimeZoneOverrideCommandParameters paired = new() { AdditionalData = { { key, 2 } } };
                        parameters.AdditionalData["goog:other"] = 3;
                        parameters.AdditionalData[key] = 4;
                        parameters.AdditionalData.Add("goog:added", 5);
                        parameters.AdditionalData.Remove("timezone");
                        parameters.AdditionalData["TimeZone"] = 6;
                        Add("timezone");

                        CustomParameters custom = new();
                        custom.AdditionalData["MethodName"] = 7;
                        custom.AdditionalData["static"] = 8;
                        custom.AdditionalData["StaticName"] = 8;
                        custom.AdditionalData["Item"] = 9;
                        custom.AdditionalData["WriteOnly"] = 10;
                        custom.AdditionalData["InternalName"] = 11;
                        custom.AdditionalData["ignored"] = 12;
                        custom.AdditionalData["AlwaysIgnored"] = 12;
                        parameters.AdditionalData[null!] = 16;
                        key = "timezone";
                        custom.AdditionalData["AdditionalData"] = 13;

                        headers.Headers[0] = header;
                        UserOwner owner = new();
                        owner.AdditionalData["name"] = 14;
                        owner.AdditionalData.Add("name", 15);
                    }

                    private static void Add(string name)
                    {
                    }
                }

                public class UserOwner
                {
                    public string Name { get; set; } = string.Empty;

                    [JsonPropertyName("name")]
                    public string Label { get; set; } = string.Empty;

                    [JsonExtensionData]
                    public Dictionary<string, object?> AdditionalData { get; } = [];
                }
            }
            """ + CustomParametersDeclarations;

        await AnalyzerTestHelpers.VerifyAnalyzerAsync<BiDiDriver033_ExtensionDataShadowsPropertyAnalyzer>(testCode);
    }

    /// <summary>
    /// Tests that code being typed, which does not compile, does not make the rule throw: an index or an entry without a
    /// name, a call without arguments, and a dictionary initializer in a position no object initializer can take.
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous unit test.</returns>
    [Fact]
    public async Task IncompleteCode_NoDiagnosticAndNoFailure()
    {
        string testCode = """
            using WebDriverBiDi;
            using WebDriverBiDi.Emulation;

            namespace TestApp
            {
                public class TestClass : SetTimeZoneOverrideCommandParameters
                {
                    public void TestMethod(SetTimeZoneOverrideCommandParameters parameters)
                    {
                        parameters.AdditionalData[] = 1;
                        parameters.AdditionalData.Add();
                        SetTimeZoneOverrideCommandParameters created = new() { AdditionalData = { [] = 1, { } } };
                        object[] values = { AdditionalData = { ["timezone"] = 1 } };
                        AdditionalData = { ["timezone"] = 1 };
                    }
                }
            }
            """;

        RealAssemblyAnalyzerTest<BiDiDriver033_ExtensionDataShadowsPropertyAnalyzer> testState = new()
        {
            TestCode = testCode,
            CompilerDiagnostics = CompilerDiagnostics.None,
        };

        await testState.RunAsync(TestContext.Current.CancellationToken);
    }
}
