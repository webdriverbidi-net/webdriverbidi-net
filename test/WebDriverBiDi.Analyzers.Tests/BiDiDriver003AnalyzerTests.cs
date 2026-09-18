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
/// Tests for the BiDiDriver003 analyzer that detects RegisterTypeInfoResolverAsync after StartAsync.
/// </summary>
public class BiDiDriver003AnalyzerTests
{
    /// <summary>
    /// Tests that RegisterTypeInfoResolverAsync called after StartAsync reports an error diagnostic.
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous unit test.</returns>
    [Fact]
    public async Task RegisterTypeInfoResolverAsync_AfterStartAsync_ReportsError()
    {
        string test = """
            using System;
            using System.Text.Json.Serialization.Metadata;
            using System.Threading.Tasks;
            using WebDriverBiDi;

            namespace TestApp
            {
                public class TestClass
                {
                    public async Task TestMethod(IJsonTypeInfoResolver resolver)
                    {
                        BiDiDriver driver = new BiDiDriver(TimeSpan.FromSeconds(30));
                        await driver.StartAsync("ws://localhost:9222");
                        {|#0:driver.RegisterTypeInfoResolverAsync(resolver)|};
                    }
                }
            }
            """;

        DiagnosticResult expected = new DiagnosticResult(BiDiDriver003_TypeInfoResolverRegistrationAfterStartAnalyzer.DiagnosticId, Microsoft.CodeAnalysis.DiagnosticSeverity.Error)
            .WithLocation(0);

        RealAssemblyAnalyzerTest<BiDiDriver003_TypeInfoResolverRegistrationAfterStartAnalyzer> testState = new()
        {
            TestCode = test,
        };
        testState.ExpectedDiagnostics.Add(expected);

        await testState.RunAsync(TestContext.Current.CancellationToken);
    }

    /// <summary>
    /// Tests that RegisterTypeInfoResolverAsync called before StartAsync does not report a diagnostic.
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous unit test.</returns>
    [Fact]
    public async Task RegisterTypeInfoResolverAsync_BeforeStartAsync_NoDiagnostic()
    {
        string test = """
            using System;
            using System.Text.Json.Serialization.Metadata;
            using System.Threading.Tasks;
            using WebDriverBiDi;

            namespace TestApp
            {
                public class TestClass
                {
                    public async Task TestMethod(IJsonTypeInfoResolver resolver)
                    {
                        BiDiDriver driver = new BiDiDriver(TimeSpan.FromSeconds(30));
                        driver.RegisterTypeInfoResolverAsync(resolver);
                        await driver.StartAsync("ws://localhost:9222");
                    }
                }
            }
            """;

        RealAssemblyAnalyzerTest<BiDiDriver003_TypeInfoResolverRegistrationAfterStartAnalyzer> testState = new()
        {
            TestCode = test,
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
            using WebDriverBiDi;

            namespace TestApp
            {
                public abstract class TestClass
                {
                    public abstract Task TestMethod(IJsonTypeInfoResolver resolver);
                }
            }
            """;

        RealAssemblyAnalyzerTest<BiDiDriver003_TypeInfoResolverRegistrationAfterStartAnalyzer> testState = new()
        {
            TestCode = test,
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
                    public Task RegisterTypeInfoResolverAsync(IJsonTypeInfoResolver resolver) => Task.CompletedTask;
                }

                public class TestClass
                {
                    public async Task TestMethod(IJsonTypeInfoResolver resolver)
                    {
                        CustomDriver driver = new CustomDriver(TimeSpan.FromSeconds(30));
                        await driver.StartAsync("ws://localhost:9222");
                        driver.RegisterTypeInfoResolverAsync(resolver);
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
    /// Tests that multiple RegisterTypeInfoResolverAsync calls after StartAsync report errors.
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous unit test.</returns>
    [Fact]
    public async Task MultipleRegisterTypeInfoResolverAsync_AfterStartAsync_ReportsMultipleErrors()
    {
        string test = """
            using System;
            using System.Text.Json.Serialization.Metadata;
            using System.Threading.Tasks;
            using WebDriverBiDi;

            namespace TestApp
            {
                public class TestClass
                {
                    public async Task TestMethod(IJsonTypeInfoResolver resolver1, IJsonTypeInfoResolver resolver2)
                    {
                        BiDiDriver driver = new BiDiDriver(TimeSpan.FromSeconds(30));
                        await driver.StartAsync("ws://localhost:9222");
                        {|#0:driver.RegisterTypeInfoResolverAsync(resolver1)|};
                        {|#1:driver.RegisterTypeInfoResolverAsync(resolver2)|};
                    }
                }
            }
            """;

        DiagnosticResult expected1 = new DiagnosticResult(BiDiDriver003_TypeInfoResolverRegistrationAfterStartAnalyzer.DiagnosticId, Microsoft.CodeAnalysis.DiagnosticSeverity.Error)
            .WithLocation(0);

        DiagnosticResult expected2 = new DiagnosticResult(BiDiDriver003_TypeInfoResolverRegistrationAfterStartAnalyzer.DiagnosticId, Microsoft.CodeAnalysis.DiagnosticSeverity.Error)
            .WithLocation(1);

        RealAssemblyAnalyzerTest<BiDiDriver003_TypeInfoResolverRegistrationAfterStartAnalyzer> testState = new()
        {
            TestCode = test,
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
            using WebDriverBiDi;

            namespace TestApp
            {
                public class TestClass
                {
                    public async Task TestMethod(IJsonTypeInfoResolver resolver)
                    {
                        BiDiDriver driver = new BiDiDriver(TimeSpan.FromSeconds(30));
                        await driver.StartAsync("ws://localhost:9222");
                        Task result;
                        result = {|#0:driver.RegisterTypeInfoResolverAsync(resolver)|};
                    }
                }
            }
            """;

        DiagnosticResult expected = new DiagnosticResult(BiDiDriver003_TypeInfoResolverRegistrationAfterStartAnalyzer.DiagnosticId, Microsoft.CodeAnalysis.DiagnosticSeverity.Error)
            .WithLocation(0);

        RealAssemblyAnalyzerTest<BiDiDriver003_TypeInfoResolverRegistrationAfterStartAnalyzer> testState = new()
        {
            TestCode = test,
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
            using WebDriverBiDi;

            namespace TestApp
            {
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

        RealAssemblyAnalyzerTest<BiDiDriver003_TypeInfoResolverRegistrationAfterStartAnalyzer> testState = new()
        {
            TestCode = test,
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
            using WebDriverBiDi;

            namespace TestApp
            {
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

        RealAssemblyAnalyzerTest<BiDiDriver003_TypeInfoResolverRegistrationAfterStartAnalyzer> testState = new()
        {
            TestCode = test,
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
            using WebDriverBiDi;

            namespace TestApp
            {
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

        RealAssemblyAnalyzerTest<BiDiDriver003_TypeInfoResolverRegistrationAfterStartAnalyzer> testState = new()
        {
            TestCode = test,
        };

        await testState.RunAsync(TestContext.Current.CancellationToken);
    }

    /// <summary>
    /// Tests that RegisterTypeInfoResolverAsync on non-tracked drivers doesn't report diagnostic.
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous unit test.</returns>
    [Fact]
    public async Task RegisterTypeInfoResolverAsyncOnNonTrackedDriver_NoDiagnostic()
    {
        string test = """
            using System;
            using System.Text.Json.Serialization.Metadata;
            using System.Threading.Tasks;
            using WebDriverBiDi;

            namespace TestApp
            {
                public class TestClass
                {
                    private BiDiDriver fieldDriver = new BiDiDriver(TimeSpan.FromSeconds(30));

                    public async Task TestMethod(IJsonTypeInfoResolver resolver)
                    {
                        await fieldDriver.StartAsync("ws://localhost:9222");
                        fieldDriver.RegisterTypeInfoResolverAsync(resolver);
                    }
                }
            }
            """;

        RealAssemblyAnalyzerTest<BiDiDriver003_TypeInfoResolverRegistrationAfterStartAnalyzer> testState = new()
        {
            TestCode = test,
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
            using WebDriverBiDi;

            namespace TestApp
            {
                public class TestClass
                {
                    public async Task TestMethod(IJsonTypeInfoResolver resolver)
                    {
                        BiDiDriver driver1 = new BiDiDriver(TimeSpan.FromSeconds(30));
                        BiDiDriver driver2 = new BiDiDriver(TimeSpan.FromSeconds(30));

                        // driver1: correct order
                        driver1.RegisterTypeInfoResolverAsync(resolver);
                        await driver1.StartAsync("ws://localhost:9222");

                        // driver2: incorrect order
                        await driver2.StartAsync("ws://localhost:9222");
                        {|#0:driver2.RegisterTypeInfoResolverAsync(resolver)|};
                    }
                }
            }
            """;

        DiagnosticResult expected = new DiagnosticResult(BiDiDriver003_TypeInfoResolverRegistrationAfterStartAnalyzer.DiagnosticId, Microsoft.CodeAnalysis.DiagnosticSeverity.Error)
            .WithLocation(0);

        RealAssemblyAnalyzerTest<BiDiDriver003_TypeInfoResolverRegistrationAfterStartAnalyzer> testState = new()
        {
            TestCode = test,
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
            using WebDriverBiDi;

            namespace TestApp
            {
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

        RealAssemblyAnalyzerTest<BiDiDriver003_TypeInfoResolverRegistrationAfterStartAnalyzer> testState = new()
        {
            TestCode = test,
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
            using WebDriverBiDi;

            namespace TestApp
            {
                public class TestClass
                {
                    public async Task TestMethod(IJsonTypeInfoResolver resolver)
                    {
                        BiDiDriver driver = new BiDiDriver(TimeSpan.FromSeconds(30));
                        await driver.StartAsync("ws://localhost:9222");
                        (driver ?? driver).RegisterTypeInfoResolverAsync(resolver);
                    }
                }
            }
            """;

        RealAssemblyAnalyzerTest<BiDiDriver003_TypeInfoResolverRegistrationAfterStartAnalyzer> testState = new()
        {
            TestCode = test,
        };

        await testState.RunAsync(TestContext.Current.CancellationToken);
    }

    /// <summary>
    /// Tests that chained member access expressions are tracked correctly.
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous unit test.</returns>
    /// <remarks>
    /// This test keeps a hand-written stub rather than the real assembly because the real
    /// <c>BiDiDriver</c> exposes <c>RegisterTypeInfoResolverAsync</c> directly and has no property
    /// whose type in turn exposes that method. A chained <c>driver.Self.RegisterTypeInfoResolverAsync(...)</c>
    /// call whose base identifier is still the tracked driver variable cannot be expressed against the
    /// real API, so a stub <c>Self</c> property is required to express the shape. A registration on
    /// something reached through the driver is not a registration on the driver — a custom module may
    /// declare a method of the same name — so it is not reported.
    /// </remarks>
    [Fact]
    public async Task ChainedMemberAccess_AfterStartAsync_NoDiagnostic()
    {
        string test = """
            using System;
            using System.Text.Json.Serialization.Metadata;
            using System.Threading.Tasks;

            namespace WebDriverBiDi
            {
                public interface IBiDiDriverLifecycleManager
                {
                    Task StartAsync(string url);
                }

                public interface IBiDiDriverConfiguration : IBiDiDriverLifecycleManager
                {
                    Task RegisterTypeInfoResolverAsync(IJsonTypeInfoResolver resolver);
                    IBiDiDriverConfiguration Self { get; }
                }

                public class BiDiDriver : IBiDiDriverConfiguration
                {
                    public BiDiDriver(TimeSpan timeout) { }
                    public Task StartAsync(string url) => Task.CompletedTask;
                    public Task RegisterTypeInfoResolverAsync(IJsonTypeInfoResolver resolver) => Task.CompletedTask;
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
                        driver.Self.RegisterTypeInfoResolverAsync(resolver);
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
    public async Task RegisterTypeInfoResolverAsync_OnCustomTypeImplementingInterface_AfterStartAsync_ReportsError()
    {
        // The driver variable is of type MyCustomDriver, whose name is NOT "BiDiDriver" or
        // "IBiDiDriverConfiguration". AnalyzerSymbolHelpers.IsDriverConfigurationType must
        // walk to the AllInterfaces check (line 45-47 in AnalyzerSymbolHelpers.cs) to
        // recognise the type. MyCustomDriver implements the real IBiDiDriverConfiguration interface.
        string test = """
            using System;
            using System.Text.Json.Serialization.Metadata;
            using System.Threading;
            using System.Threading.Tasks;
            using WebDriverBiDi;

            namespace TestApp
            {
                public class MyCustomDriver : IBiDiDriverConfiguration
                {
                    public Task StartAsync(string url) => Task.CompletedTask;
                    public void RegisterModule(Module module) { }
                    public Task RegisterTypeInfoResolverAsync(IJsonTypeInfoResolver resolver, CancellationToken cancellationToken = default) => Task.CompletedTask;
                }

                public class TestClass
                {
                    public async Task TestMethod(IJsonTypeInfoResolver resolver)
                    {
                        MyCustomDriver driver = new MyCustomDriver();
                        await driver.StartAsync("ws://localhost:9222");
                        {|#0:driver.RegisterTypeInfoResolverAsync(resolver)|};
                    }
                }
            }
            """;

        DiagnosticResult expected = new DiagnosticResult(
            BiDiDriver003_TypeInfoResolverRegistrationAfterStartAnalyzer.DiagnosticId,
            Microsoft.CodeAnalysis.DiagnosticSeverity.Error)
            .WithLocation(0);

        RealAssemblyAnalyzerTest<BiDiDriver003_TypeInfoResolverRegistrationAfterStartAnalyzer> testState = new()
        {
            TestCode = test,
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
            using WebDriverBiDi;

            namespace TestApp
            {
                public class TestClass
                {
                    public async Task TestMethod()
                    {
                        BiDiDriver driver = new BiDiDriver(TimeSpan.FromSeconds(30));
                        await driver.StartAsync("ws://localhost:9222");
                        // An if-statement exercises the neither-branch path.
                        if (true) { }
                    }
                }
            }
            """;

        RealAssemblyAnalyzerTest<BiDiDriver003_TypeInfoResolverRegistrationAfterStartAnalyzer> testState = new()
        {
            TestCode = test,
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
            using WebDriverBiDi;

            namespace TestApp
            {
                public class TestClass
                {
                    public async Task TestMethod()
                    {
                        BiDiDriver driver = new BiDiDriver(TimeSpan.FromSeconds(30));
                        await driver.StartAsync("ws://localhost:9222");
                        // Awaiting a variable exercises the non-invocation await path.
                        Task t = Task.CompletedTask;
                        await t;
                    }
                }
            }
            """;

        RealAssemblyAnalyzerTest<BiDiDriver003_TypeInfoResolverRegistrationAfterStartAnalyzer> testState = new()
        {
            TestCode = test,
        };

        await testState.RunAsync(TestContext.Current.CancellationToken);
    }

    /// <summary>
    /// Tests that RegisterTypeInfoResolverAsync called after StopAsync does not report a diagnostic,
    /// because the driver is no longer started.
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous unit test.</returns>
    [Fact]
    public async Task RegisterTypeInfoResolverAsync_AfterStopAsync_NoDiagnostic()
    {
        string test = """
            using System;
            using System.Text.Json.Serialization.Metadata;
            using System.Threading.Tasks;
            using WebDriverBiDi;

            namespace TestApp
            {
                public class TestClass
                {
                    public async Task TestMethod(IJsonTypeInfoResolver resolver)
                    {
                        BiDiDriver driver = new BiDiDriver(TimeSpan.FromSeconds(30));
                        await driver.StartAsync("ws://localhost:9222");
                        await driver.StopAsync();
                        await driver.RegisterTypeInfoResolverAsync(resolver);
                    }
                }
            }
            """;

        RealAssemblyAnalyzerTest<BiDiDriver003_TypeInfoResolverRegistrationAfterStartAnalyzer> testState = new()
        {
            TestCode = test,
        };

        await testState.RunAsync(TestContext.Current.CancellationToken);
    }

    /// <summary>
    /// Tests that a RegisterTypeInfoResolverAsync nested inside an if block, after a top-level
    /// StartAsync, is now flagged. The previous top-level-only walk never saw calls inside nested
    /// blocks.
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous unit test.</returns>
    [Fact]
    public async Task RegisterTypeInfoResolverAsync_InsideIfAfterStartAsync_ReportsError()
    {
        string test = """
            using System;
            using System.Text.Json.Serialization.Metadata;
            using System.Threading.Tasks;
            using WebDriverBiDi;

            namespace TestApp
            {
                public class TestClass
                {
                    public async Task TestMethod(IJsonTypeInfoResolver resolver, bool condition)
                    {
                        BiDiDriver driver = new BiDiDriver(TimeSpan.FromSeconds(30));
                        await driver.StartAsync("ws://localhost:9222");
                        if (condition)
                        {
                            {|#0:driver.RegisterTypeInfoResolverAsync(resolver)|};
                        }
                    }
                }
            }
            """;

        DiagnosticResult expected = new DiagnosticResult(BiDiDriver003_TypeInfoResolverRegistrationAfterStartAnalyzer.DiagnosticId, Microsoft.CodeAnalysis.DiagnosticSeverity.Error)
            .WithLocation(0);

        RealAssemblyAnalyzerTest<BiDiDriver003_TypeInfoResolverRegistrationAfterStartAnalyzer> testState = new()
        {
            TestCode = test,
        };
        testState.ExpectedDiagnostics.Add(expected);

        await testState.RunAsync(TestContext.Current.CancellationToken);
    }

    /// <summary>
    /// Tests that starting the driver in one branch of an if/else does not mark it started for the
    /// other branch, so a RegisterTypeInfoResolverAsync in the branch that did not start is not
    /// flagged.
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous unit test.</returns>
    [Fact]
    public async Task RegisterTypeInfoResolverAsync_InBranchThatDidNotStart_NoDiagnostic()
    {
        string test = """
            using System;
            using System.Text.Json.Serialization.Metadata;
            using System.Threading.Tasks;
            using WebDriverBiDi;

            namespace TestApp
            {
                public class TestClass
                {
                    public async Task TestMethod(IJsonTypeInfoResolver resolver, bool condition)
                    {
                        BiDiDriver driver = new BiDiDriver(TimeSpan.FromSeconds(30));
                        if (condition)
                        {
                            await driver.RegisterTypeInfoResolverAsync(resolver);
                        }
                        else
                        {
                            await driver.StartAsync("ws://localhost:9222");
                        }
                    }
                }
            }
            """;

        RealAssemblyAnalyzerTest<BiDiDriver003_TypeInfoResolverRegistrationAfterStartAnalyzer> testState = new()
        {
            TestCode = test,
        };

        await testState.RunAsync(TestContext.Current.CancellationToken);
    }

    /// <summary>
    /// Tests that RegisterTypeInfoResolverAsync after a StartAsync call written with a <c>.ConfigureAwait(false)</c> continuation is still reported.
    /// The wrapper is the outermost invocation, so without unwrapping the chain the analyzer sees a
    /// call it does not recognize and never marks the driver as started.
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous unit test.</returns>
    [Fact]
    public async Task RegisterTypeInfoResolverAsync_AfterStartAsyncWithConfigureAwait_ReportsError()
    {
        string test = """
            using System;
            using System.Text.Json.Serialization.Metadata;
            using System.Threading.Tasks;
            using WebDriverBiDi;

            namespace TestApp
            {
                public class TestClass
                {
                    public async Task TestMethod(IJsonTypeInfoResolver resolver)
                    {
                        BiDiDriver driver = new BiDiDriver(TimeSpan.FromSeconds(30));
                        await driver.StartAsync("ws://localhost:9222").ConfigureAwait(false);
                        {|#0:driver.RegisterTypeInfoResolverAsync(resolver)|};
                    }
                }
            }
            """;

        DiagnosticResult expected = new DiagnosticResult(BiDiDriver003_TypeInfoResolverRegistrationAfterStartAnalyzer.DiagnosticId, Microsoft.CodeAnalysis.DiagnosticSeverity.Error)
            .WithLocation(0);

        RealAssemblyAnalyzerTest<BiDiDriver003_TypeInfoResolverRegistrationAfterStartAnalyzer> testState = new()
        {
            TestCode = test,
        };
        testState.ExpectedDiagnostics.Add(expected);

        await testState.RunAsync(TestContext.Current.CancellationToken);
    }

    /// <summary>
    /// Tests that RegisterTypeInfoResolverAsync after a StartAsync call written with a blocking <c>.Wait()</c> is still reported.
    /// The wrapper is the outermost invocation, so without unwrapping the chain the analyzer sees a
    /// call it does not recognize and never marks the driver as started.
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous unit test.</returns>
    [Fact]
    public async Task RegisterTypeInfoResolverAsync_AfterStartAsyncWithBlockingWait_ReportsError()
    {
        string test = """
            using System;
            using System.Text.Json.Serialization.Metadata;
            using System.Threading.Tasks;
            using WebDriverBiDi;

            namespace TestApp
            {
                public class TestClass
                {
                    public async Task TestMethod(IJsonTypeInfoResolver resolver)
                    {
                        BiDiDriver driver = new BiDiDriver(TimeSpan.FromSeconds(30));
                        driver.StartAsync("ws://localhost:9222").Wait();
                        {|#0:driver.RegisterTypeInfoResolverAsync(resolver)|};
                    }
                }
            }
            """;

        DiagnosticResult expected = new DiagnosticResult(BiDiDriver003_TypeInfoResolverRegistrationAfterStartAnalyzer.DiagnosticId, Microsoft.CodeAnalysis.DiagnosticSeverity.Error)
            .WithLocation(0);

        RealAssemblyAnalyzerTest<BiDiDriver003_TypeInfoResolverRegistrationAfterStartAnalyzer> testState = new()
        {
            TestCode = test,
        };
        testState.ExpectedDiagnostics.Add(expected);

        await testState.RunAsync(TestContext.Current.CancellationToken);
    }

    /// <summary>
    /// Tests that RegisterTypeInfoResolverAsync after a StartAsync call written with a blocking <c>.ConfigureAwait(false).GetAwaiter().GetResult()</c> chain is still reported.
    /// The wrapper is the outermost invocation, so without unwrapping the chain the analyzer sees a
    /// call it does not recognize and never marks the driver as started.
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous unit test.</returns>
    [Fact]
    public async Task RegisterTypeInfoResolverAsync_AfterStartAsyncWithGetAwaiterGetResult_ReportsError()
    {
        string test = """
            using System;
            using System.Text.Json.Serialization.Metadata;
            using System.Threading.Tasks;
            using WebDriverBiDi;

            namespace TestApp
            {
                public class TestClass
                {
                    public async Task TestMethod(IJsonTypeInfoResolver resolver)
                    {
                        BiDiDriver driver = new BiDiDriver(TimeSpan.FromSeconds(30));
                        driver.StartAsync("ws://localhost:9222").ConfigureAwait(false).GetAwaiter().GetResult();
                        {|#0:driver.RegisterTypeInfoResolverAsync(resolver)|};
                    }
                }
            }
            """;

        DiagnosticResult expected = new DiagnosticResult(BiDiDriver003_TypeInfoResolverRegistrationAfterStartAnalyzer.DiagnosticId, Microsoft.CodeAnalysis.DiagnosticSeverity.Error)
            .WithLocation(0);

        RealAssemblyAnalyzerTest<BiDiDriver003_TypeInfoResolverRegistrationAfterStartAnalyzer> testState = new()
        {
            TestCode = test,
        };
        testState.ExpectedDiagnostics.Add(expected);

        await testState.RunAsync(TestContext.Current.CancellationToken);
    }

    /// <summary>
    /// Tests that starting a driver instance returned by another call does not mark a differently
    /// obtained local as started. The StartAsync receiver is itself an invocation, so this also pins
    /// that only the known task-chaining wrappers are unwrapped: unwrapping every
    /// invocation-receivered member access would discard the StartAsync call entirely.
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous unit test.</returns>
    [Fact]
    public async Task RegisterTypeInfoResolverAsync_AfterStartAsyncOnDifferentReturnedDriver_NoDiagnostic()
    {
        string test = """
            using System;
            using System.Text.Json.Serialization.Metadata;
            using System.Threading.Tasks;
            using WebDriverBiDi;

            namespace TestApp
            {
                public class TestClass
                {
                    public async Task TestMethod(IJsonTypeInfoResolver resolver)
                    {
                        BiDiDriver driver = GetDriver();
                        await GetDriver().StartAsync("ws://localhost:9222");
                        driver.RegisterTypeInfoResolverAsync(resolver);
                    }

                    private static BiDiDriver GetDriver() => new BiDiDriver(TimeSpan.FromSeconds(30));
                }
            }
            """;

        RealAssemblyAnalyzerTest<BiDiDriver003_TypeInfoResolverRegistrationAfterStartAnalyzer> testState = new()
        {
            TestCode = test,
        };

        await testState.RunAsync(TestContext.Current.CancellationToken);
    }

    /// <summary>
    /// Tests that declaring the start task, rather than assigning it, is still recognized as starting the
    /// driver. The connect attempt begins at the call in both spellings.
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous unit test.</returns>
    [Fact]
    public async Task RegisterTypeInfoResolverAsync_AfterStartAsyncDeclaredAsTask_ReportsError()
    {
        string test = """
            using System;
            using System.Text.Json.Serialization.Metadata;
            using System.Threading.Tasks;
            using WebDriverBiDi;

            namespace TestApp
            {
                public class TestClass
                {
                    public async Task TestMethod(IJsonTypeInfoResolver resolver)
                    {
                        BiDiDriver driver = new BiDiDriver(TimeSpan.FromSeconds(30));
                        Task startTask = driver.StartAsync("ws://localhost:9222");
                        {|#0:driver.RegisterTypeInfoResolverAsync(resolver)|};
                        await startTask;
                    }
                }
            }
            """;

        DiagnosticResult expected = new DiagnosticResult(BiDiDriver003_TypeInfoResolverRegistrationAfterStartAnalyzer.DiagnosticId, Microsoft.CodeAnalysis.DiagnosticSeverity.Error)
            .WithLocation(0);

        RealAssemblyAnalyzerTest<BiDiDriver003_TypeInfoResolverRegistrationAfterStartAnalyzer> testState = new()
        {
            TestCode = test,
        };
        testState.ExpectedDiagnostics.Add(expected);

        await testState.RunAsync(TestContext.Current.CancellationToken);
    }

    /// <summary>
    /// Tests that the same driver name declared in two sibling loop bodies does not crash the analyzer (the second declaration used to be added to an immutable dictionary that already held the key) and is tracked afresh in each body.
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous unit test.</returns>
    [Fact]
    public async Task RegisterTypeInfoResolverAsync_DriverRedeclaredInSiblingLoopBodies_DoesNotCrashAndReportsOnlyStartedOne()
    {
        string testCode = """
            using System;
            using System.Text.Json.Serialization.Metadata;
            using System.Threading.Tasks;
            using WebDriverBiDi;

            namespace TestApp
            {
                public class TestClass
                {
                    public async Task TestMethod(IJsonTypeInfoResolver resolver, string[] urls)
                    {
                        foreach (string url in urls)
                        {
                            BiDiDriver driver = new BiDiDriver(TimeSpan.FromSeconds(30));
                            await driver.StartAsync(url);
                            await {|#0:driver.RegisterTypeInfoResolverAsync(resolver)|};
                        }

                        foreach (string url in urls)
                        {
                            BiDiDriver driver = new BiDiDriver(TimeSpan.FromSeconds(30));
                            await driver.RegisterTypeInfoResolverAsync(resolver);
                            await driver.StartAsync(url);
                        }
                    }
                }
            }
            """;

        DiagnosticResult expected0 = new DiagnosticResult(BiDiDriver003_TypeInfoResolverRegistrationAfterStartAnalyzer.DiagnosticId, Microsoft.CodeAnalysis.DiagnosticSeverity.Error)
            .WithLocation(0);

        RealAssemblyAnalyzerTest<BiDiDriver003_TypeInfoResolverRegistrationAfterStartAnalyzer> testState = new()
        {
            TestCode = testCode,
        };
        testState.ExpectedDiagnostics.Add(expected0);

        await testState.RunAsync(TestContext.Current.CancellationToken);
    }

    /// <summary>
    /// Tests that a registration in one switch section is not judged against a StartAsync in a different, mutually exclusive section.
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous unit test.</returns>
    [Fact]
    public async Task RegisterTypeInfoResolverAsync_InSwitchSectionAfterStartInAnotherSection_NoDiagnostic()
    {
        string testCode = """
            using System;
            using System.Text.Json.Serialization.Metadata;
            using System.Threading.Tasks;
            using WebDriverBiDi;

            namespace TestApp
            {
                public class TestClass
                {
                    public async Task TestMethod(IJsonTypeInfoResolver resolver, string browser)
                    {
                        BiDiDriver driver = new BiDiDriver(TimeSpan.FromSeconds(30));
                        switch (browser)
                        {
                            case "chrome":
                                await driver.StartAsync("ws://localhost:9222");
                                break;
                            case "firefox":
                                await driver.RegisterTypeInfoResolverAsync(resolver);
                                await driver.StartAsync("ws://localhost:9223");
                                break;
                        }
                    }
                }
            }
            """;

        RealAssemblyAnalyzerTest<BiDiDriver003_TypeInfoResolverRegistrationAfterStartAnalyzer> testState = new()
        {
            TestCode = testCode,
        };

        await testState.RunAsync(TestContext.Current.CancellationToken);
    }

    /// <summary>
    /// Tests that a registration in a catch clause after a StartAsync in the try block is not reported: the start may have failed, leaving the driver not started.
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous unit test.</returns>
    [Fact]
    public async Task RegisterTypeInfoResolverAsync_InCatchAfterFailedStartInTry_NoDiagnostic()
    {
        string testCode = """
            using System;
            using System.Text.Json.Serialization.Metadata;
            using System.Threading.Tasks;
            using WebDriverBiDi;

            namespace TestApp
            {
                public class TestClass
                {
                    public async Task TestMethod(IJsonTypeInfoResolver resolver)
                    {
                        BiDiDriver driver = new BiDiDriver(TimeSpan.FromSeconds(30));
                        try
                        {
                            await driver.StartAsync("ws://localhost:9222");
                        }
                        catch (WebDriverBiDiException)
                        {
                            await driver.RegisterTypeInfoResolverAsync(resolver);
                            await driver.StartAsync("ws://localhost:9223");
                        }
                    }
                }
            }
            """;

        RealAssemblyAnalyzerTest<BiDiDriver003_TypeInfoResolverRegistrationAfterStartAnalyzer> testState = new()
        {
            TestCode = testCode,
        };

        await testState.RunAsync(TestContext.Current.CancellationToken);
    }

    /// <summary>
    /// Tests that a driver declared by a classic <c>await using (T x = ...)</c> statement is tracked like one declared by a local declaration statement.
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous unit test.</returns>
    [Fact]
    public async Task RegisterTypeInfoResolverAsync_InClassicUsingStatementDeclaration_ReportsError()
    {
        string testCode = """
            using System;
            using System.Text.Json.Serialization.Metadata;
            using System.Threading.Tasks;
            using WebDriverBiDi;

            namespace TestApp
            {
                public class TestClass
                {
                    public async Task TestMethod(IJsonTypeInfoResolver resolver)
                    {
                        await using (BiDiDriver driver = new BiDiDriver(TimeSpan.FromSeconds(30)))
                        {
                            await driver.StartAsync("ws://localhost:9222");
                            await {|#0:driver.RegisterTypeInfoResolverAsync(resolver)|};
                        }
                    }
                }
            }
            """;

        DiagnosticResult expected0 = new DiagnosticResult(BiDiDriver003_TypeInfoResolverRegistrationAfterStartAnalyzer.DiagnosticId, Microsoft.CodeAnalysis.DiagnosticSeverity.Error)
            .WithLocation(0);

        RealAssemblyAnalyzerTest<BiDiDriver003_TypeInfoResolverRegistrationAfterStartAnalyzer> testState = new()
        {
            TestCode = testCode,
        };
        testState.ExpectedDiagnostics.Add(expected0);

        await testState.RunAsync(TestContext.Current.CancellationToken);
    }

    /// <summary>
    /// Tests that a type info resolver registration after the driver has been handed to a helper is not
    /// reported: the helper may have stopped the driver, which this rule cannot see.
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous unit test.</returns>
    [Fact]
    public async Task RegisterTypeInfoResolverAsync_AfterDriverPassedToHelperThatStopsIt_NoDiagnostic()
    {
        string test = """
            using System;
            using System.Text.Json.Serialization.Metadata;
            using System.Threading.Tasks;
            using WebDriverBiDi;

            namespace TestApp
            {
                public class TestClass
                {
                    public async Task TestMethod(IJsonTypeInfoResolver resolver)
                    {
                        BiDiDriver driver = new BiDiDriver(TimeSpan.FromSeconds(30));
                        await driver.StartAsync("ws://localhost:9222");
                        await StopHelperAsync(driver);
                        await driver.RegisterTypeInfoResolverAsync(resolver);
                    }

                    private static async Task StopHelperAsync(BiDiDriver driver)
                    {
                        await driver.StopAsync();
                    }
                }
            }
            """;

        RealAssemblyAnalyzerTest<BiDiDriver003_TypeInfoResolverRegistrationAfterStartAnalyzer> testState = new()
        {
            TestCode = test,
        };

        await testState.RunAsync(TestContext.Current.CancellationToken);
    }

    /// <summary>
    /// Tests the walker paths the reference describes but this rule's own tests never reached: a start in
    /// the right operand of <c>??</c> or <c>??=</c>, which may not run, and a hand-off through a cast,
    /// which stops tracking altogether.
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous unit test.</returns>
    [Fact]
    public async Task HandOffAndShortCircuitForms_NoDiagnostic()
    {
        string test = """
            using System.Text.Json.Serialization.Metadata;
            using System.Threading.Tasks;
            using WebDriverBiDi;

            namespace TestApp
            {
                public class TestClass
                {
                    public async Task TestMethod(string url, IJsonTypeInfoResolver resolver)
                    {
            // The right operand of ?? may not run, so the start it holds is not certain on every path.
            BiDiDriver coalesced = new();
            Task? pendingCoalesce = null;
            await (pendingCoalesce ?? coalesced.StartAsync(url));
            await coalesced.RegisterTypeInfoResolverAsync(resolver);

            // ??= carries the same uncertainty in its compound form.
            BiDiDriver assigned = new();
            Task? pendingAssign = null;
            pendingAssign ??= assigned.StartAsync(url);
            await pendingAssign;
            await assigned.RegisterTypeInfoResolverAsync(resolver);

            // A cast hand-off stops tracking entirely, so even a certain start leaves nothing reported.
            BiDiDriver handedOff = new();
            await handedOff.StartAsync(url);
            await StartHelperAsync((IBiDiDriverLifecycleManager)handedOff);
            await handedOff.RegisterTypeInfoResolverAsync(resolver);
                    }

                    private static Task StartHelperAsync(IBiDiDriverLifecycleManager manager) => Task.CompletedTask;
                }
            }
            """;

        RealAssemblyAnalyzerTest<BiDiDriver003_TypeInfoResolverRegistrationAfterStartAnalyzer> testState = new()
        {
            TestCode = test,
        };

        await testState.RunAsync(TestContext.Current.CancellationToken);
    }
}
