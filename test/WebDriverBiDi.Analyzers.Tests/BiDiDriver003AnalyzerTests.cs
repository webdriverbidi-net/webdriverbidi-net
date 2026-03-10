// <copyright file="BiDiDriver003AnalyzerTests.cs" company="WebDriverBiDi.NET Committers">
// Copyright (c) WebDriverBiDi.NET Committers. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.
// </copyright>

namespace WebDriverBiDi.Analyzers.Tests;

using System.Threading.Tasks;
using Microsoft.CodeAnalysis.CSharp.Testing;
using Microsoft.CodeAnalysis.Testing;

/// <summary>
/// Tests for the BiDiDriver003 analyzer that detects RegisterTypeInfoResolver after StartAsync.
/// </summary>
[TestFixture]
public class BiDiDriver003AnalyzerTests
{
    /// <summary>
    /// Tests that RegisterTypeInfoResolver called after StartAsync reports an error diagnostic.
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous unit test.</returns>
    [Test]
    public async Task RegisterTypeInfoResolver_AfterStartAsync_ReportsError()
    {
        string test = """
            using System;
            using System.Text.Json.Serialization.Metadata;
            using System.Threading.Tasks;

            namespace WebDriverBiDi
            {
                public interface IBiDiCommandExecutor
                {
                    Task StartAsync(string url);
                }

                public interface IBiDiDriverConfiguration : IBiDiCommandExecutor
                {
                    void RegisterTypeInfoResolver(IJsonTypeInfoResolver resolver);
                }

                public class BiDiDriver : IBiDiDriverConfiguration
                {
                    public BiDiDriver(TimeSpan timeout) { }
                    public Task StartAsync(string url) => Task.CompletedTask;
                    public void RegisterTypeInfoResolver(IJsonTypeInfoResolver resolver) { }
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
                        IBiDiDriverConfiguration driver = new BiDiDriver(TimeSpan.FromSeconds(30));
                        await driver.StartAsync("ws://localhost:9222");
                        {|#0:driver.RegisterTypeInfoResolver(resolver)|};
                    }
                }
            }
            """;

        DiagnosticResult expected = new DiagnosticResult(BiDiDriver003_TypeInfoResolverRegistrationAfterStartAnalyzer.DiagnosticId, Microsoft.CodeAnalysis.DiagnosticSeverity.Error)
            .WithLocation(0);

        CSharpAnalyzerTest<BiDiDriver003_TypeInfoResolverRegistrationAfterStartAnalyzer, DefaultVerifier> testState = new()
        {
            TestCode = test,
            ReferenceAssemblies = ReferenceAssemblies.Net.Net80,
        };
        testState.ExpectedDiagnostics.Add(expected);

        await testState.RunAsync();
    }

    /// <summary>
    /// Tests that RegisterTypeInfoResolver called before StartAsync does not report a diagnostic.
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous unit test.</returns>
    [Test]
    public async Task RegisterTypeInfoResolver_BeforeStartAsync_NoDiagnostic()
    {
        string test = """
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
                    public void RegisterTypeInfoResolver(IJsonTypeInfoResolver resolver) { }
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

        CSharpAnalyzerTest<BiDiDriver003_TypeInfoResolverRegistrationAfterStartAnalyzer, DefaultVerifier> testState = new()
        {
            TestCode = test,
            ReferenceAssemblies = ReferenceAssemblies.Net.Net80,
        };

        await testState.RunAsync();
    }

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
                        public void RegisterTypeInfoResolver(IJsonTypeInfoResolver resolver) { }
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
                    public void RegisterTypeInfoResolver(IJsonTypeInfoResolver resolver) { }
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

    /// <summary>
    /// Tests that methods without body are handled gracefully.
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous unit test.</returns>
    [Test]
    public async Task MethodWithoutBody_NoDiagnostic()
    {
        string test = """
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
                    public void RegisterTypeInfoResolver(IJsonTypeInfoResolver resolver) { }
                }
            }

            namespace TestApp
            {
                using System.Text.Json.Serialization.Metadata;
                using WebDriverBiDi;

                public abstract class TestClass
                {
                    public abstract Task TestMethod(IJsonTypeInfoResolver resolver);
                }
            }
            """;

        CSharpAnalyzerTest<BiDiDriver003_TypeInfoResolverRegistrationAfterStartAnalyzer, DefaultVerifier> testState = new()
        {
            TestCode = test,
            ReferenceAssemblies = ReferenceAssemblies.Net.Net80,
        };

        await testState.RunAsync();
    }

    /// <summary>
    /// Tests that non-BiDiDriver types are not analyzed.
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous unit test.</returns>
    [Test]
    public async Task NonBiDiDriverType_NoDiagnostic()
    {
        string test = """
            using System;
            using System.Text.Json.Serialization.Metadata;
            using System.Threading.Tasks;

            namespace TestApp
            {
                public class CustomDriver
                {
                    public CustomDriver(TimeSpan timeout) { }
                    public Task StartAsync(string url) => Task.CompletedTask;
                    public void RegisterTypeInfoResolver(IJsonTypeInfoResolver resolver) { }
                }

                public class TestClass
                {
                    public async Task TestMethod(IJsonTypeInfoResolver resolver)
                    {
                        CustomDriver driver = new CustomDriver(TimeSpan.FromSeconds(30));
                        await driver.StartAsync("ws://localhost:9222");
                        driver.RegisterTypeInfoResolver(resolver);
                    }
                }
            }
            """;

        CSharpAnalyzerTest<BiDiDriver003_TypeInfoResolverRegistrationAfterStartAnalyzer, DefaultVerifier> testState = new()
        {
            TestCode = test,
            ReferenceAssemblies = ReferenceAssemblies.Net.Net80,
        };

        await testState.RunAsync();
    }

    /// <summary>
    /// Tests that multiple RegisterTypeInfoResolver calls after StartAsync report errors.
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous unit test.</returns>
    [Test]
    public async Task MultipleRegisterTypeInfoResolver_AfterStartAsync_ReportsMultipleErrors()
    {
        string test = """
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
                    public void RegisterTypeInfoResolver(IJsonTypeInfoResolver resolver) { }
                }
            }

            namespace TestApp
            {
                using System.Text.Json.Serialization.Metadata;
                using WebDriverBiDi;

                public class TestClass
                {
                    public async Task TestMethod(IJsonTypeInfoResolver resolver1, IJsonTypeInfoResolver resolver2)
                    {
                        BiDiDriver driver = new BiDiDriver(TimeSpan.FromSeconds(30));
                        await driver.StartAsync("ws://localhost:9222");
                        {|#0:driver.RegisterTypeInfoResolver(resolver1)|};
                        {|#1:driver.RegisterTypeInfoResolver(resolver2)|};
                    }
                }
            }
            """;

        DiagnosticResult expected1 = new DiagnosticResult(BiDiDriver003_TypeInfoResolverRegistrationAfterStartAnalyzer.DiagnosticId, Microsoft.CodeAnalysis.DiagnosticSeverity.Error)
            .WithLocation(0);

        DiagnosticResult expected2 = new DiagnosticResult(BiDiDriver003_TypeInfoResolverRegistrationAfterStartAnalyzer.DiagnosticId, Microsoft.CodeAnalysis.DiagnosticSeverity.Error)
            .WithLocation(1);

        CSharpAnalyzerTest<BiDiDriver003_TypeInfoResolverRegistrationAfterStartAnalyzer, DefaultVerifier> testState = new()
        {
            TestCode = test,
            ReferenceAssemblies = ReferenceAssemblies.Net.Net80,
        };
        testState.ExpectedDiagnostics.Add(expected1);
        testState.ExpectedDiagnostics.Add(expected2);

        await testState.RunAsync();
    }
}
