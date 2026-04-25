// <copyright file="BiDiDriver003CodeFixProviderTests.cs" company="WebDriverBiDi.NET Committers">
// Copyright (c) WebDriverBiDi.NET Committers. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.
// </copyright>

namespace WebDriverBiDi.Analyzers.Tests;

using System.Threading.Tasks;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Testing;
using Microsoft.CodeAnalysis.Testing;

/// <summary>
/// Tests for the BiDiDriver003 code fix provider.
/// </summary>
[TestFixture]
public class BiDiDriver003CodeFixProviderTests
{
    /// <summary>
    /// Tests that the code fix moves RegisterTypeInfoResolver before StartAsync.
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous unit test.</returns>
    [Test]
    public async Task RegisterTypeInfoResolver_CodeFixMovesBeforeStartAsync()
    {
        string testCode = """
                using System;
                using System.Text.Json.Serialization.Metadata;
                using System.Threading.Tasks;

                namespace WebDriverBiDi
                {
                    public interface IBiDiDriver { }

                    public class BiDiDriver : IBiDiDriver
                    {
                        public BiDiDriver(TimeSpan timeout) { }
                        public Task StartAsync(string url) => Task.CompletedTask;
                        public Task RegisterTypeInfoResolver(IJsonTypeInfoResolver resolver) => Task.CompletedTask;
                    }
                }

                namespace TestApp
                {
                    using System.Text.Json.Serialization.Metadata;
                    using WebDriverBiDi;

                    public class TestClass
                    {
                        public async Task TestMethod(IJsonTypeInfoResolver resolver)
                        {
                            BiDiDriver driver = new BiDiDriver(TimeSpan.FromSeconds(30));
                            await driver.StartAsync("ws://localhost:9222");
                            {|#0:driver.RegisterTypeInfoResolver(resolver)|};
                        }
                    }
                }
                """;

        string fixedCode = """
            using System;
            using System.Text.Json.Serialization.Metadata;
            using System.Threading.Tasks;

            namespace WebDriverBiDi
            {
                public interface IBiDiDriver { }

                public class BiDiDriver : IBiDiDriver
                {
                    public BiDiDriver(TimeSpan timeout) { }
                    public Task StartAsync(string url) => Task.CompletedTask;
                    public Task RegisterTypeInfoResolver(IJsonTypeInfoResolver resolver) => Task.CompletedTask;
                }
            }

            namespace TestApp
            {
                using System.Text.Json.Serialization.Metadata;
                using WebDriverBiDi;

                public class TestClass
                {
                    public async Task TestMethod(IJsonTypeInfoResolver resolver)
                    {
                        BiDiDriver driver = new BiDiDriver(TimeSpan.FromSeconds(30));
                        driver.RegisterTypeInfoResolver(resolver);
                        await driver.StartAsync("ws://localhost:9222");
                    }
                }
            }
            """;

        DiagnosticResult expected = new DiagnosticResult(BiDiDriver003_TypeInfoResolverRegistrationAfterStartAnalyzer.DiagnosticId, Microsoft.CodeAnalysis.DiagnosticSeverity.Error)
            .WithLocation(0);

        CSharpCodeFixTest<BiDiDriver003_TypeInfoResolverRegistrationAfterStartAnalyzer, BiDiDriver003_TypeInfoResolverRegistrationAfterStartCodeFixProvider, DefaultVerifier> testState = new()
        {
            TestCode = testCode,
            FixedCode = fixedCode,
            ReferenceAssemblies = ReferenceAssemblies.Net.Net80,
        };
        testState.ExpectedDiagnostics.Add(expected);

        await testState.RunAsync();
    }
}
