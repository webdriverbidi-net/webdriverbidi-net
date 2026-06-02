// <copyright file="BiDiDriver003AnalyzerTests.cs" company="WebDriverBiDi.NET Committers">
// Copyright (c) WebDriverBiDi.NET Committers. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.
// </copyright>

namespace WebDriverBiDi.Analyzers.Tests;

using System.Threading.Tasks;
using Microsoft.CodeAnalysis.CodeFixes;
using Microsoft.CodeAnalysis.CSharp.Testing;
using Microsoft.CodeAnalysis.Testing;

/// <summary>
/// Tests for the BiDiDriver003 analyzer that detects RegisterTypeInfoResolver after StartAsync.
/// </summary>
public class BiDiDriver003AnalyzerTests
{
    /// <summary>
    /// Tests that RegisterTypeInfoResolver called after StartAsync reports an error diagnostic.
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous unit test.</returns>
    [Fact]
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
                    Task RegisterTypeInfoResolver(IJsonTypeInfoResolver resolver);
                }

                public class BiDiDriver : IBiDiDriverConfiguration
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

        await testState.RunAsync(TestContext.Current.CancellationToken);
    }

    /// <summary>
    /// Tests that RegisterTypeInfoResolver called before StartAsync does not report a diagnostic.
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous unit test.</returns>
    [Fact]
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

        CSharpAnalyzerTest<BiDiDriver003_TypeInfoResolverRegistrationAfterStartAnalyzer, DefaultVerifier> testState = new()
        {
            TestCode = test,
            ReferenceAssemblies = ReferenceAssemblies.Net.Net80,
        };

        await testState.RunAsync(TestContext.Current.CancellationToken);
    }

    /// <summary>
    /// Tests that methods without body are handled gracefully.
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous unit test.</returns>
    [Fact]
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
                    public Task RegisterTypeInfoResolver(IJsonTypeInfoResolver resolver) => Task.CompletedTask;
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

        await testState.RunAsync(TestContext.Current.CancellationToken);
    }

    /// <summary>
    /// Tests that non-BiDiDriver types are not analyzed.
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous unit test.</returns>
    [Fact]
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
                    public Task RegisterTypeInfoResolver(IJsonTypeInfoResolver resolver) => Task.CompletedTask;
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

        await testState.RunAsync(TestContext.Current.CancellationToken);
    }

    /// <summary>
    /// Tests that multiple RegisterTypeInfoResolver calls after StartAsync report errors.
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous unit test.</returns>
    [Fact]
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
                    public Task RegisterTypeInfoResolver(IJsonTypeInfoResolver resolver) => Task.CompletedTask;
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

        await testState.RunAsync(TestContext.Current.CancellationToken);
    }

    /// <summary>
    /// Tests that assignment expressions with invocations are handled.
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous unit test.</returns>
    [Fact]
    public async Task AssignmentExpression_AfterStartAsync_ReportsError()
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
                    Task RegisterTypeInfoResolver(IJsonTypeInfoResolver resolver);
                }

                public class BiDiDriver : IBiDiDriverConfiguration
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
                        IBiDiDriverConfiguration driver = new BiDiDriver(TimeSpan.FromSeconds(30));
                        await driver.StartAsync("ws://localhost:9222");
                        Task result;
                        result = {|#0:driver.RegisterTypeInfoResolver(resolver)|};
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

        await testState.RunAsync(TestContext.Current.CancellationToken);
    }

    /// <summary>
    /// Tests that invocations without member access are ignored.
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous unit test.</returns>
    [Fact]
    public async Task Invocation_WithoutMemberAccess_NoDiagnostic()
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
                        Func<Task> action = async () => { };
                        await action();
                    }
                }
            }
            """;

        CSharpAnalyzerTest<BiDiDriver003_TypeInfoResolverRegistrationAfterStartAnalyzer, DefaultVerifier> testState = new()
        {
            TestCode = test,
            ReferenceAssemblies = ReferenceAssemblies.Net.Net80,
        };

        await testState.RunAsync(TestContext.Current.CancellationToken);
    }

    /// <summary>
    /// Tests that unresolved method symbols are handled gracefully.
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous unit test.</returns>
    [Fact]
    public async Task UnresolvedMethodSymbol_NoDiagnostic()
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
                        driver.{|CS1061:NonExistentMethod|}();
                    }
                }
            }
            """;

        CSharpAnalyzerTest<BiDiDriver003_TypeInfoResolverRegistrationAfterStartAnalyzer, DefaultVerifier> testState = new()
        {
            TestCode = test,
            ReferenceAssemblies = ReferenceAssemblies.Net.Net80,
        };

        await testState.RunAsync(TestContext.Current.CancellationToken);
    }

    /// <summary>
    /// Tests that variables without initializers are handled.
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous unit test.</returns>
    [Fact]
    public async Task VariableWithoutInitializer_NoDiagnostic()
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
                        BiDiDriver driver;
                        driver = new BiDiDriver(TimeSpan.FromSeconds(30));
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

        await testState.RunAsync(TestContext.Current.CancellationToken);
    }

    /// <summary>
    /// Tests that RegisterTypeInfoResolver on non-tracked drivers doesn't report diagnostic.
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous unit test.</returns>
    [Fact]
    public async Task RegisterTypeInfoResolverOnNonTrackedDriver_NoDiagnostic()
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
                    public Task RegisterTypeInfoResolver(IJsonTypeInfoResolver resolver) => Task.CompletedTask;
                }
            }

            namespace TestApp
            {
                using System.Text.Json.Serialization.Metadata;
                using WebDriverBiDi;

                public class TestClass
                {
                    private BiDiDriver fieldDriver = new BiDiDriver(TimeSpan.FromSeconds(30));

                    public async Task TestMethod(IJsonTypeInfoResolver resolver)
                    {
                        await fieldDriver.StartAsync("ws://localhost:9222");
                        fieldDriver.RegisterTypeInfoResolver(resolver);
                    }
                }
            }
            """;

        CSharpAnalyzerTest<BiDiDriver003_TypeInfoResolverRegistrationAfterStartAnalyzer, DefaultVerifier> testState = new()
        {
            TestCode = test,
            ReferenceAssemblies = ReferenceAssemblies.Net.Net80,
        };

        await testState.RunAsync(TestContext.Current.CancellationToken);
    }

    /// <summary>
    /// Tests that multiple drivers are tracked independently.
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous unit test.</returns>
    [Fact]
    public async Task MultipleDrivers_IndependentTracking()
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
                        BiDiDriver driver1 = new BiDiDriver(TimeSpan.FromSeconds(30));
                        BiDiDriver driver2 = new BiDiDriver(TimeSpan.FromSeconds(30));

                        // driver1: correct order
                        driver1.RegisterTypeInfoResolver(resolver);
                        await driver1.StartAsync("ws://localhost:9222");

                        // driver2: incorrect order
                        await driver2.StartAsync("ws://localhost:9222");
                        {|#0:driver2.RegisterTypeInfoResolver(resolver)|};
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

        await testState.RunAsync(TestContext.Current.CancellationToken);
    }

    /// <summary>
    /// Tests that complex expression statements are handled.
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous unit test.</returns>
    [Fact]
    public async Task ComplexExpressionStatement_NoDiagnostic()
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
                        int x = 5;
                        x++;
                    }
                }
            }
            """;

        CSharpAnalyzerTest<BiDiDriver003_TypeInfoResolverRegistrationAfterStartAnalyzer, DefaultVerifier> testState = new()
        {
            TestCode = test,
            ReferenceAssemblies = ReferenceAssemblies.Net.Net80,
        };

        await testState.RunAsync(TestContext.Current.CancellationToken);
    }

    /// <summary>
    /// Tests that complex member access expressions are handled.
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous unit test.</returns>
    [Fact]
    public async Task ComplexMemberAccessExpression_NoDiagnostic()
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
                        (driver ?? driver).RegisterTypeInfoResolver(resolver);
                    }
                }
            }
            """;

        CSharpAnalyzerTest<BiDiDriver003_TypeInfoResolverRegistrationAfterStartAnalyzer, DefaultVerifier> testState = new()
        {
            TestCode = test,
            ReferenceAssemblies = ReferenceAssemblies.Net.Net80,
        };

        await testState.RunAsync(TestContext.Current.CancellationToken);
    }

    /// <summary>
    /// Tests that chained member access expressions are tracked correctly.
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous unit test.</returns>
    [Fact]
    public async Task ChainedMemberAccess_AfterStartAsync_ReportsError()
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
                    Task RegisterTypeInfoResolver(IJsonTypeInfoResolver resolver);
                    IBiDiDriverConfiguration Self { get; }
                }

                public class BiDiDriver : IBiDiDriverConfiguration
                {
                    public BiDiDriver(TimeSpan timeout) { }
                    public Task StartAsync(string url) => Task.CompletedTask;
                    public Task RegisterTypeInfoResolver(IJsonTypeInfoResolver resolver) => Task.CompletedTask;
                    public IBiDiDriverConfiguration Self => this;
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
                        {|#0:driver.Self.RegisterTypeInfoResolver(resolver)|};
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

        await testState.RunAsync(TestContext.Current.CancellationToken);
    }

    /// <summary>
    /// Tests GetFixAllProvider returns the correct provider.
    /// </summary>
    [Fact]
    public void GetFixAllProvider_ReturnsBatchFixer()
    {
        BiDiDriver003_TypeInfoResolverRegistrationAfterStartCodeFixProvider provider = new BiDiDriver003_TypeInfoResolverRegistrationAfterStartCodeFixProvider();
        FixAllProvider fixAllProvider = provider.GetFixAllProvider();

        Assert.Equal(WellKnownFixAllProviders.BatchFixer, fixAllProvider);
    }

    /// <summary>
    /// Tests FixableDiagnosticIds property.
    /// </summary>
    [Fact]
    public void FixableDiagnosticIds_ContainsBIDI003()
    {
        BiDiDriver003_TypeInfoResolverRegistrationAfterStartCodeFixProvider provider = new BiDiDriver003_TypeInfoResolverRegistrationAfterStartCodeFixProvider();

        Assert.Contains(BiDiDriver003_TypeInfoResolverRegistrationAfterStartAnalyzer.DiagnosticId, provider.FixableDiagnosticIds);
        Assert.Single(provider.FixableDiagnosticIds);
    }

    /// <summary>
    /// Tests SupportedDiagnostics property of the analyzer.
    /// </summary>
    [Fact]
    public void SupportedDiagnostics_ContainsBIDI003()
    {
        BiDiDriver003_TypeInfoResolverRegistrationAfterStartAnalyzer analyzer = new BiDiDriver003_TypeInfoResolverRegistrationAfterStartAnalyzer();

        Assert.Single(analyzer.SupportedDiagnostics);
        Assert.Equal(BiDiDriver003_TypeInfoResolverRegistrationAfterStartAnalyzer.DiagnosticId, analyzer.SupportedDiagnostics[0].Id);
    }

    [Fact]
    public async Task RegisterTypeInfoResolver_OnCustomTypeImplementingInterface_AfterStartAsync_ReportsError()
    {
        // The driver variable is of type MyCustomDriver, whose name is NOT "BiDiDriver" or
        // "IBiDiDriverConfiguration". AnalyzerSymbolHelpers.IsDriverConfigurationType must
        // walk to the AllInterfaces check (line 45-47 in AnalyzerSymbolHelpers.cs) to
        // recognise the type.
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
                    Task RegisterTypeInfoResolver(IJsonTypeInfoResolver resolver);
                }
            }

            namespace TestApp
            {
                using System.Text.Json.Serialization.Metadata;
                using WebDriverBiDi;

                public class MyCustomDriver : IBiDiDriverConfiguration
                {
                    public Task StartAsync(string url) => Task.CompletedTask;
                    public Task RegisterTypeInfoResolver(IJsonTypeInfoResolver resolver) => Task.CompletedTask;
                }

                public class TestClass
                {
                    public async Task TestMethod(IJsonTypeInfoResolver resolver)
                    {
                        MyCustomDriver driver = new MyCustomDriver();
                        await driver.StartAsync("ws://localhost:9222");
                        {|#0:driver.RegisterTypeInfoResolver(resolver)|};
                    }
                }
            }
            """;

        DiagnosticResult expected = new DiagnosticResult(
            BiDiDriver003_TypeInfoResolverRegistrationAfterStartAnalyzer.DiagnosticId,
            Microsoft.CodeAnalysis.DiagnosticSeverity.Error)
            .WithLocation(0);

        CSharpAnalyzerTest<BiDiDriver003_TypeInfoResolverRegistrationAfterStartAnalyzer, DefaultVerifier> testState = new()
        {
            TestCode = test,
            ReferenceAssemblies = ReferenceAssemblies.Net.Net80,
        };
        testState.ExpectedDiagnostics.Add(expected);

        await testState.RunAsync(TestContext.Current.CancellationToken);
    }

    /// <summary>
    /// Tests that an if-statement in a method body does not crash — exercises the
    /// statement-is-neither-local-nor-expression branch (line 77).
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous unit test.</returns>
    [Fact]
    public async Task MethodWithIfStatement_DoesNotCrash()
    {
        string test = """
            using System;
            using System.Threading.Tasks;

            namespace WebDriverBiDi
            {
                public interface IBiDiCommandExecutor
                {
                    Task StartAsync(string url);
                    Task RegisterTypeInfoResolverAsync(object resolver);
                }

                public class BiDiDriver : IBiDiCommandExecutor
                {
                    public BiDiDriver(TimeSpan timeout) { }
                    public Task StartAsync(string url) => Task.CompletedTask;
                    public Task RegisterTypeInfoResolverAsync(object resolver) => Task.CompletedTask;
                }
            }

            namespace TestApp
            {
                using WebDriverBiDi;

                public class TestClass
                {
                    public async Task TestMethod()
                    {
                        IBiDiCommandExecutor driver = new BiDiDriver(TimeSpan.FromSeconds(30));
                        await driver.StartAsync("ws://localhost:9222");
                        // An if-statement exercises the neither-branch path.
                        if (true) { }
                    }
                }
            }
            """;

        CSharpAnalyzerTest<BiDiDriver003_TypeInfoResolverRegistrationAfterStartAnalyzer, DefaultVerifier> testState = new()
        {
            TestCode = test,
            ReferenceAssemblies = ReferenceAssemblies.Net.Net80,
        };

        await testState.RunAsync(TestContext.Current.CancellationToken);
    }

    /// <summary>
    /// Tests that awaiting a task variable (not an invocation) does not crash — exercises
    /// the awaitExpression.Expression is not InvocationExpressionSyntax path (line 122).
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous unit test.</returns>
    [Fact]
    public async Task MethodWithAwaitedTaskVariable_DoesNotCrash()
    {
        string test = """
            using System;
            using System.Threading.Tasks;

            namespace WebDriverBiDi
            {
                public interface IBiDiCommandExecutor
                {
                    Task StartAsync(string url);
                    Task RegisterTypeInfoResolverAsync(object resolver);
                }

                public class BiDiDriver : IBiDiCommandExecutor
                {
                    public BiDiDriver(TimeSpan timeout) { }
                    public Task StartAsync(string url) => Task.CompletedTask;
                    public Task RegisterTypeInfoResolverAsync(object resolver) => Task.CompletedTask;
                }
            }

            namespace TestApp
            {
                using WebDriverBiDi;

                public class TestClass
                {
                    public async Task TestMethod()
                    {
                        IBiDiCommandExecutor driver = new BiDiDriver(TimeSpan.FromSeconds(30));
                        await driver.StartAsync("ws://localhost:9222");
                        // Awaiting a variable exercises the non-invocation await path.
                        Task t = Task.CompletedTask;
                        await t;
                    }
                }
            }
            """;

        CSharpAnalyzerTest<BiDiDriver003_TypeInfoResolverRegistrationAfterStartAnalyzer, DefaultVerifier> testState = new()
        {
            TestCode = test,
            ReferenceAssemblies = ReferenceAssemblies.Net.Net80,
        };

        await testState.RunAsync(TestContext.Current.CancellationToken);
    }
}
