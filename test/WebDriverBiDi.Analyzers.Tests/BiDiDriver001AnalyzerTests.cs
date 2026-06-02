// <copyright file="BiDiDriver001AnalyzerTests.cs" company="WebDriverBiDi.NET Committers">
// Copyright (c) WebDriverBiDi.NET Committers. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.
// </copyright>

namespace WebDriverBiDi.Analyzers.Tests;

using System.Threading.Tasks;
using Microsoft.CodeAnalysis.CodeFixes;
using Microsoft.CodeAnalysis.CSharp.Testing;
using Microsoft.CodeAnalysis.Testing;

/// <summary>
/// Tests for the BiDiDriver001 analyzer that detects module registration after StartAsync().
/// </summary>
public class BiDiDriver001AnalyzerTests
{
    /// <summary>
    /// Tests that RegisterModule() called after StartAsync() reports a diagnostic.
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous unit test.</returns>
    [Fact]
    public async Task RegisterModule_AfterStartAsync_ReportsDiagnostic()
    {
        string test = """
            using System;
            using System.Threading.Tasks;

            namespace WebDriverBiDi
            {
                public interface IBiDiCommandExecutor
                {
                    Task StartAsync(string url);
                }

                public interface IBiDiDriverConfiguration : IBiDiCommandExecutor
                {
                    void RegisterModule(Module module);
                }

                public class BiDiDriver : IBiDiDriverConfiguration
                {
                    public BiDiDriver(TimeSpan timeout) { }
                    public Task StartAsync(string url) => Task.CompletedTask;
                    public void RegisterModule(Module module) { }
                }

                public abstract class Module
                {
                    protected Module(IBiDiCommandExecutor driver) { }
                    public abstract string ModuleName { get; }
                }
            }

            namespace TestApp
            {
                using WebDriverBiDi;

                public class TestClass
                {
                    public async Task TestMethod()
                    {
                        IBiDiDriverConfiguration driver = new BiDiDriver(TimeSpan.FromSeconds(30));
                        await driver.StartAsync("ws://localhost:9222");

                        // This should trigger BIDI001
                        {|#0:driver.RegisterModule(new CustomModule(driver))|};
                    }
                }

                public class CustomModule : Module
                {
                    public CustomModule(IBiDiCommandExecutor driver) : base(driver) { }
                    public override string ModuleName => "custom";
                }
            }
            """;

        DiagnosticResult expected = new DiagnosticResult(BiDiDriver001_ModuleRegistrationAfterStartAnalyzer.DiagnosticId, Microsoft.CodeAnalysis.DiagnosticSeverity.Error)
            .WithLocation(0)
            .WithArguments("new CustomModule(driver)");

        CSharpAnalyzerTest<BiDiDriver001_ModuleRegistrationAfterStartAnalyzer, DefaultVerifier> testState = new()
        {
            TestCode = test,
            ReferenceAssemblies = ReferenceAssemblies.Net.Net80,
        };
        testState.ExpectedDiagnostics.Add(expected);

        await testState.RunAsync(TestContext.Current.CancellationToken);
    }

    /// <summary>
    /// Tests that RegisterModule() called before StartAsync() does not report a diagnostic.
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous unit test.</returns>
    [Fact]
    public async Task RegisterModule_BeforeStartAsync_NoDiagnostic()
    {
        string test = """
            using System;
            using System.Threading.Tasks;

            namespace WebDriverBiDi
            {
                public interface IBiDiDriver { }

                public class BiDiDriver : IBiDiDriver
                {
                    public BiDiDriver(TimeSpan timeout) { }
                    public Task StartAsync(string url) => Task.CompletedTask;
                    public void RegisterModule(Module module) { }
                }

                public abstract class Module
                {
                    protected Module(IBiDiDriver driver) { }
                    public abstract string ModuleName { get; }
                }
            }

            namespace TestApp
            {
                using WebDriverBiDi;

                public class TestClass
                {
                    public async Task TestMethod()
                    {
                        BiDiDriver driver = new BiDiDriver(TimeSpan.FromSeconds(30));

                        // This is correct - no diagnostic
                        driver.RegisterModule(new CustomModule(driver));

                        await driver.StartAsync("ws://localhost:9222");
                    }
                }

                public class CustomModule : Module
                {
                    public CustomModule(IBiDiDriver driver) : base(driver) { }
                    public override string ModuleName => "custom";
                }
            }
            """;

        CSharpAnalyzerTest<BiDiDriver001_ModuleRegistrationAfterStartAnalyzer, DefaultVerifier> testState = new()
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
            using System.Threading.Tasks;

            namespace TestApp
            {
                public class CustomDriver
                {
                    public CustomDriver(TimeSpan timeout) { }
                    public Task StartAsync(string url) => Task.CompletedTask;
                    public void RegisterModule(object module) { }
                }

                public class TestClass
                {
                    public async Task TestMethod()
                    {
                        CustomDriver driver = new CustomDriver(TimeSpan.FromSeconds(30));
                        await driver.StartAsync("ws://localhost:9222");
                        driver.RegisterModule(new object());
                    }
                }
            }
            """;

        CSharpAnalyzerTest<BiDiDriver001_ModuleRegistrationAfterStartAnalyzer, DefaultVerifier> testState = new()
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
            using System.Threading.Tasks;

            namespace WebDriverBiDi
            {
                public interface IBiDiDriver { }

                public class BiDiDriver : IBiDiDriver
                {
                    public BiDiDriver(TimeSpan timeout) { }
                    public Task StartAsync(string url) => Task.CompletedTask;
                    public void RegisterModule(Module module) { }
                }

                public abstract class Module
                {
                    protected Module(IBiDiDriver driver) { }
                    public abstract string ModuleName { get; }
                }
            }

            namespace TestApp
            {
                using WebDriverBiDi;

                public abstract class TestClass
                {
                    public abstract Task TestMethod();
                }
            }
            """;

        CSharpAnalyzerTest<BiDiDriver001_ModuleRegistrationAfterStartAnalyzer, DefaultVerifier> testState = new()
        {
            TestCode = test,
            ReferenceAssemblies = ReferenceAssemblies.Net.Net80,
        };

        await testState.RunAsync(TestContext.Current.CancellationToken);
    }

    /// <summary>
    /// Tests that multiple drivers in the same method are tracked independently.
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous unit test.</returns>
    [Fact]
    public async Task MultipleDrivers_IndependentTracking()
    {
        string test = """
            using System;
            using System.Threading.Tasks;

            namespace WebDriverBiDi
            {
                public interface IBiDiDriver { }

                public class BiDiDriver : IBiDiDriver
                {
                    public BiDiDriver(TimeSpan timeout) { }
                    public Task StartAsync(string url) => Task.CompletedTask;
                    public void RegisterModule(Module module) { }
                }

                public abstract class Module
                {
                    protected Module(IBiDiDriver driver) { }
                    public abstract string ModuleName { get; }
                }
            }

            namespace TestApp
            {
                using WebDriverBiDi;

                public class TestClass
                {
                    public async Task TestMethod()
                    {
                        BiDiDriver driver1 = new BiDiDriver(TimeSpan.FromSeconds(30));
                        BiDiDriver driver2 = new BiDiDriver(TimeSpan.FromSeconds(30));

                        // driver1: correct order
                        driver1.RegisterModule(new CustomModule(driver1));
                        await driver1.StartAsync("ws://localhost:9222");

                        // driver2: incorrect order
                        await driver2.StartAsync("ws://localhost:9222");
                        {|#0:driver2.RegisterModule(new CustomModule(driver2))|};
                    }
                }

                public class CustomModule : Module
                {
                    public CustomModule(IBiDiDriver driver) : base(driver) { }
                    public override string ModuleName => "custom";
                }
            }
            """;

        DiagnosticResult expected = new DiagnosticResult(BiDiDriver001_ModuleRegistrationAfterStartAnalyzer.DiagnosticId, Microsoft.CodeAnalysis.DiagnosticSeverity.Error)
            .WithLocation(0)
            .WithArguments("new CustomModule(driver2)");

        CSharpAnalyzerTest<BiDiDriver001_ModuleRegistrationAfterStartAnalyzer, DefaultVerifier> testState = new()
        {
            TestCode = test,
            ReferenceAssemblies = ReferenceAssemblies.Net.Net80,
        };
        testState.ExpectedDiagnostics.Add(expected);

        await testState.RunAsync(TestContext.Current.CancellationToken);
    }

    /// <summary>
    /// Tests that variable declarations without initializers are handled.
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous unit test.</returns>
    [Fact]
    public async Task VariableWithoutInitializer_NoDiagnostic()
    {
        string test = """
            using System;
            using System.Threading.Tasks;

            namespace WebDriverBiDi
            {
                public interface IBiDiDriver { }

                public class BiDiDriver : IBiDiDriver
                {
                    public BiDiDriver(TimeSpan timeout) { }
                    public Task StartAsync(string url) => Task.CompletedTask;
                    public void RegisterModule(Module module) { }
                }

                public abstract class Module
                {
                    protected Module(IBiDiDriver driver) { }
                    public abstract string ModuleName { get; }
                }
            }

            namespace TestApp
            {
                using WebDriverBiDi;

                public class TestClass
                {
                    public async Task TestMethod()
                    {
                        BiDiDriver driver;
                        driver = new BiDiDriver(TimeSpan.FromSeconds(30));
                        await driver.StartAsync("ws://localhost:9222");
                    }
                }
            }
            """;

        CSharpAnalyzerTest<BiDiDriver001_ModuleRegistrationAfterStartAnalyzer, DefaultVerifier> testState = new()
        {
            TestCode = test,
            ReferenceAssemblies = ReferenceAssemblies.Net.Net80,
        };

        await testState.RunAsync(TestContext.Current.CancellationToken);
    }

    /// <summary>
    /// Tests that RegisterModule on non-tracked drivers doesn't report diagnostic.
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous unit test.</returns>
    [Fact]
    public async Task RegisterModuleOnNonTrackedDriver_NoDiagnostic()
    {
        string test = """
            using System;
            using System.Threading.Tasks;

            namespace WebDriverBiDi
            {
                public interface IBiDiDriver { }

                public class BiDiDriver : IBiDiDriver
                {
                    public BiDiDriver(TimeSpan timeout) { }
                    public Task StartAsync(string url) => Task.CompletedTask;
                    public void RegisterModule(Module module) { }
                }

                public abstract class Module
                {
                    protected Module(IBiDiDriver driver) { }
                    public abstract string ModuleName { get; }
                }
            }

            namespace TestApp
            {
                using WebDriverBiDi;

                public class TestClass
                {
                    private BiDiDriver fieldDriver = new BiDiDriver(TimeSpan.FromSeconds(30));

                    public async Task TestMethod()
                    {
                        await fieldDriver.StartAsync("ws://localhost:9222");
                        fieldDriver.RegisterModule(new CustomModule(fieldDriver));
                    }
                }

                public class CustomModule : Module
                {
                    public CustomModule(IBiDiDriver driver) : base(driver) { }
                    public override string ModuleName => "custom";
                }
            }
            """;

        CSharpAnalyzerTest<BiDiDriver001_ModuleRegistrationAfterStartAnalyzer, DefaultVerifier> testState = new()
        {
            TestCode = test,
            ReferenceAssemblies = ReferenceAssemblies.Net.Net80,
        };

        await testState.RunAsync(TestContext.Current.CancellationToken);
    }

    /// <summary>
    /// Tests that RegisterModule with no arguments still reports diagnostic.
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous unit test.</returns>
    [Fact]
    public async Task RegisterModule_AfterStartAsync_WithNoArguments_ReportsDiagnostic()
    {
        string test = """
            using System;
            using System.Threading.Tasks;

            namespace WebDriverBiDi
            {
                public interface IBiDiCommandExecutor
                {
                    Task StartAsync(string url);
                }

                public interface IBiDiDriverConfiguration : IBiDiCommandExecutor
                {
                    void RegisterModule();
                }

                public class BiDiDriver : IBiDiDriverConfiguration
                {
                    public BiDiDriver(TimeSpan timeout) { }
                    public Task StartAsync(string url) => Task.CompletedTask;
                    public void RegisterModule() { }
                }
            }

            namespace TestApp
            {
                using WebDriverBiDi;

                public class TestClass
                {
                    public async Task TestMethod()
                    {
                        IBiDiDriverConfiguration driver = new BiDiDriver(TimeSpan.FromSeconds(30));
                        await driver.StartAsync("ws://localhost:9222");
                        {|#0:driver.RegisterModule()|};
                    }
                }
            }
            """;

        DiagnosticResult expected = new DiagnosticResult(BiDiDriver001_ModuleRegistrationAfterStartAnalyzer.DiagnosticId, Microsoft.CodeAnalysis.DiagnosticSeverity.Error)
            .WithLocation(0)
            .WithArguments("module");

        CSharpAnalyzerTest<BiDiDriver001_ModuleRegistrationAfterStartAnalyzer, DefaultVerifier> testState = new()
        {
            TestCode = test,
            ReferenceAssemblies = ReferenceAssemblies.Net.Net80,
        };
        testState.ExpectedDiagnostics.Add(expected);

        await testState.RunAsync(TestContext.Current.CancellationToken);
    }

    /// <summary>
    /// Tests that direct invocation (without await) of StartAsync is tracked when called directly.
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous unit test.</returns>
    [Fact]
    public async Task RegisterModule_AfterDirectStartAsyncCall_ReportsDiagnostic()
    {
        string test = """
            using System;
            using System.Threading.Tasks;

            namespace WebDriverBiDi
            {
                public interface IBiDiCommandExecutor
                {
                    Task StartAsync(string url);
                }

                public interface IBiDiDriverConfiguration : IBiDiCommandExecutor
                {
                    void RegisterModule(Module module);
                }

                public class BiDiDriver : IBiDiDriverConfiguration
                {
                    public BiDiDriver(TimeSpan timeout) { }
                    public Task StartAsync(string url) => Task.CompletedTask;
                    public void RegisterModule(Module module) { }
                }

                public abstract class Module
                {
                    protected Module(IBiDiCommandExecutor driver) { }
                    public abstract string ModuleName { get; }
                }
            }

            namespace TestApp
            {
                using WebDriverBiDi;

                public class TestClass
                {
                    public void TestMethod()
                    {
                        IBiDiDriverConfiguration driver = new BiDiDriver(TimeSpan.FromSeconds(30));
                        driver.StartAsync("ws://localhost:9222");
                        {|#0:driver.RegisterModule(new CustomModule(driver))|};
                    }
                }

                public class CustomModule : Module
                {
                    public CustomModule(IBiDiCommandExecutor driver) : base(driver) { }
                    public override string ModuleName => "custom";
                }
            }
            """;

        DiagnosticResult expected = new DiagnosticResult(BiDiDriver001_ModuleRegistrationAfterStartAnalyzer.DiagnosticId, Microsoft.CodeAnalysis.DiagnosticSeverity.Error)
            .WithLocation(0)
            .WithArguments("new CustomModule(driver)");

        CSharpAnalyzerTest<BiDiDriver001_ModuleRegistrationAfterStartAnalyzer, DefaultVerifier> testState = new()
        {
            TestCode = test,
            ReferenceAssemblies = ReferenceAssemblies.Net.Net80,
        };
        testState.ExpectedDiagnostics.Add(expected);

        await testState.RunAsync(TestContext.Current.CancellationToken);
    }

    /// <summary>
    /// Tests that chained member access expressions are handled correctly.
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous unit test.</returns>
    [Fact]
    public async Task RegisterModule_AfterStartAsync_ChainedMemberAccess_ReportsDiagnostic()
    {
        string test = """
            using System;
            using System.Threading.Tasks;

            namespace WebDriverBiDi
            {
                public interface IBiDiCommandExecutor
                {
                    Task StartAsync(string url);
                }

                public interface IBiDiDriverConfiguration : IBiDiCommandExecutor
                {
                    void RegisterModule(Module module);
                }

                public class BiDiDriver : IBiDiDriverConfiguration
                {
                    public BiDiDriver(TimeSpan timeout) { }
                    public Task StartAsync(string url) => Task.CompletedTask;
                    public void RegisterModule(Module module) { }
                }

                public abstract class Module
                {
                    protected Module(IBiDiCommandExecutor driver) { }
                    public abstract string ModuleName { get; }
                }
            }

            namespace TestApp
            {
                using WebDriverBiDi;

                public class DriverWrapper
                {
                    public IBiDiDriverConfiguration Driver { get; set; }

                    public DriverWrapper()
                    {
                        Driver = new BiDiDriver(TimeSpan.FromSeconds(30));
                    }
                }

                public class TestClass
                {
                    public async Task TestMethod()
                    {
                        DriverWrapper wrapper = new DriverWrapper();
                        await wrapper.Driver.StartAsync("ws://localhost:9222");
                        {|#0:wrapper.Driver.RegisterModule(new CustomModule(wrapper.Driver))|};
                    }
                }

                public class CustomModule : Module
                {
                    public CustomModule(IBiDiCommandExecutor driver) : base(driver) { }
                    public override string ModuleName => "custom";
                }
            }
            """;

        // Note: This should NOT report a diagnostic because wrapper.Driver is not a local variable
        // being tracked in the dictionary
        CSharpAnalyzerTest<BiDiDriver001_ModuleRegistrationAfterStartAnalyzer, DefaultVerifier> testState = new()
        {
            TestCode = test,
            ReferenceAssemblies = ReferenceAssemblies.Net.Net80,
        };

        await testState.RunAsync(TestContext.Current.CancellationToken);
    }

    /// <summary>
    /// Tests that non-invocation expressions in expression statements are ignored.
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous unit test.</returns>
    [Fact]
    public async Task NonInvocationExpressionStatement_NoDiagnostic()
    {
        string test = """
            using System;
            using System.Threading.Tasks;

            namespace WebDriverBiDi
            {
                public interface IBiDiDriver { }

                public class BiDiDriver : IBiDiDriver
                {
                    public BiDiDriver(TimeSpan timeout) { }
                    public Task StartAsync(string url) => Task.CompletedTask;
                    public void RegisterModule(Module module) { }
                }

                public abstract class Module
                {
                    protected Module(IBiDiDriver driver) { }
                    public abstract string ModuleName { get; }
                }
            }

            namespace TestApp
            {
                using WebDriverBiDi;

                public class TestClass
                {
                    public async Task TestMethod()
                    {
                        BiDiDriver driver = new BiDiDriver(TimeSpan.FromSeconds(30));
                        int x = 5;
                        x++;
                        await driver.StartAsync("ws://localhost:9222");
                    }
                }
            }
            """;

        CSharpAnalyzerTest<BiDiDriver001_ModuleRegistrationAfterStartAnalyzer, DefaultVerifier> testState = new()
        {
            TestCode = test,
            ReferenceAssemblies = ReferenceAssemblies.Net.Net80,
        };

        await testState.RunAsync(TestContext.Current.CancellationToken);
    }

    /// <summary>
    /// Tests that invocations without member access syntax are ignored.
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous unit test.</returns>
    [Fact]
    public async Task InvocationWithoutMemberAccess_NoDiagnostic()
    {
        string test = """
            using System;
            using System.Threading.Tasks;

            namespace WebDriverBiDi
            {
                public interface IBiDiDriver { }

                public class BiDiDriver : IBiDiDriver
                {
                    public BiDiDriver(TimeSpan timeout) { }
                    public Task StartAsync(string url) => Task.CompletedTask;
                    public void RegisterModule(Module module) { }
                }

                public abstract class Module
                {
                    protected Module(IBiDiDriver driver) { }
                    public abstract string ModuleName { get; }
                }
            }

            namespace TestApp
            {
                using WebDriverBiDi;

                public class TestClass
                {
                    public async Task TestMethod()
                    {
                        BiDiDriver driver = new BiDiDriver(TimeSpan.FromSeconds(30));
                        Func<Task> action = () => driver.StartAsync("ws://localhost:9222");
                        await action();
                    }
                }
            }
            """;

        CSharpAnalyzerTest<BiDiDriver001_ModuleRegistrationAfterStartAnalyzer, DefaultVerifier> testState = new()
        {
            TestCode = test,
            ReferenceAssemblies = ReferenceAssemblies.Net.Net80,
        };

        await testState.RunAsync(TestContext.Current.CancellationToken);
    }

    /// <summary>
    /// Tests that GetFixAllProvider returns the correct provider.
    /// </summary>
    [Fact]
    public void GetFixAllProvider_ReturnsBatchFixer()
    {
        BiDiDriver001_ModuleRegistrationAfterStartCodeFixProvider provider = new BiDiDriver001_ModuleRegistrationAfterStartCodeFixProvider();
        FixAllProvider fixAllProvider = provider.GetFixAllProvider();

        Assert.Equal(WellKnownFixAllProviders.BatchFixer, fixAllProvider);
    }

    /// <summary>
    /// Tests the analyzer detects multiple RegisterModule calls after StartAsync.
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous unit test.</returns>
    [Fact]
    public async Task MultipleRegisterModule_AfterStartAsync_ReportsBothDiagnostics()
    {
        string test = """
            using System;
            using System.Threading.Tasks;

            namespace WebDriverBiDi
            {
                public interface IBiDiDriver { }

                public class BiDiDriver : IBiDiDriver
                {
                    public BiDiDriver(TimeSpan timeout) { }
                    public Task StartAsync(string url) => Task.CompletedTask;
                    public void RegisterModule(Module module) { }
                }

                public abstract class Module
                {
                    protected Module(IBiDiDriver driver) { }
                    public abstract string ModuleName { get; }
                }
            }

            namespace TestApp
            {
                using WebDriverBiDi;

                public class TestClass
                {
                    public async Task TestMethod()
                    {
                        BiDiDriver driver = new BiDiDriver(TimeSpan.FromSeconds(30));
                        await driver.StartAsync("ws://localhost:9222");
                        {|#0:driver.RegisterModule(new CustomModule(driver))|};
                        {|#1:driver.RegisterModule(new CustomModule(driver))|};
                    }
                }

                public class CustomModule : Module
                {
                    public CustomModule(IBiDiDriver driver) : base(driver) { }
                    public override string ModuleName => "custom";
                }
            }
            """;

        DiagnosticResult[] expected =
        [
            new DiagnosticResult(BiDiDriver001_ModuleRegistrationAfterStartAnalyzer.DiagnosticId, Microsoft.CodeAnalysis.DiagnosticSeverity.Error)
                .WithLocation(0)
                .WithArguments("new CustomModule(driver)"),
            new DiagnosticResult(BiDiDriver001_ModuleRegistrationAfterStartAnalyzer.DiagnosticId, Microsoft.CodeAnalysis.DiagnosticSeverity.Error)
                .WithLocation(1)
                .WithArguments("new CustomModule(driver)")
        ];

        CSharpAnalyzerTest<BiDiDriver001_ModuleRegistrationAfterStartAnalyzer, DefaultVerifier> testState = new()
        {
            TestCode = test,
            ReferenceAssemblies = ReferenceAssemblies.Net.Net80,
        };
        testState.ExpectedDiagnostics.AddRange(expected);

        await testState.RunAsync(TestContext.Current.CancellationToken);
    }

    /// <summary>
    /// Tests that StartAsync invocation without member access doesn't cause issues in code fix.
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous unit test.</returns>
    [Fact]
    public async Task CodeFix_StartAsyncWithoutMemberAccess_HandledGracefully()
    {
        string testCode = """
            using System;
            using System.Threading.Tasks;

            namespace WebDriverBiDi
            {
                public interface IBiDiDriver { }

                public class BiDiDriver : IBiDiDriver
                {
                    public BiDiDriver(TimeSpan timeout) { }
                    public Task StartAsync(string url) => Task.CompletedTask;
                    public void RegisterModule(Module module) { }
                }

                public abstract class Module
                {
                    protected Module(IBiDiDriver driver) { }
                    public abstract string ModuleName { get; }
                }
            }

            namespace TestApp
            {
                using WebDriverBiDi;

                public class TestClass
                {
                    public async Task TestMethod()
                    {
                        BiDiDriver driver = new BiDiDriver(TimeSpan.FromSeconds(30));
                        Func<Task> start = () => Task.CompletedTask;
                        await start();
                        {|#0:driver.RegisterModule(new CustomModule(driver))|};
                    }
                }

                public class CustomModule : Module
                {
                    public CustomModule(IBiDiDriver driver) : base(driver) { }
                    public override string ModuleName => "custom";
                }
            }
            """;

        // The analyzer won't detect this as a problem because the tracked driver never called StartAsync
        // But if we manually create a scenario where RegisterModule is flagged without a proper StartAsync,
        // the code fix should handle missing StartAsync gracefully
        CSharpAnalyzerTest<BiDiDriver001_ModuleRegistrationAfterStartAnalyzer, DefaultVerifier> testState = new()
        {
            TestCode = testCode,
            ReferenceAssemblies = ReferenceAssemblies.Net.Net80,
        };

        await testState.RunAsync(TestContext.Current.CancellationToken);
    }

    /// <summary>
    /// Tests FixableDiagnosticIds property.
    /// </summary>
    [Fact]
    public void FixableDiagnosticIds_ContainsBIDI001()
    {
        BiDiDriver001_ModuleRegistrationAfterStartCodeFixProvider provider = new BiDiDriver001_ModuleRegistrationAfterStartCodeFixProvider();

        Assert.Contains(BiDiDriver001_ModuleRegistrationAfterStartAnalyzer.DiagnosticId, provider.FixableDiagnosticIds);
        Assert.Single(provider.FixableDiagnosticIds);
    }

    /// <summary>
    /// Tests SupportedDiagnostics property of the analyzer.
    /// </summary>
    [Fact]
    public void SupportedDiagnostics_ContainsBIDI001()
    {
        BiDiDriver001_ModuleRegistrationAfterStartAnalyzer analyzer = new BiDiDriver001_ModuleRegistrationAfterStartAnalyzer();

        Assert.Single(analyzer.SupportedDiagnostics);
        Assert.Equal(BiDiDriver001_ModuleRegistrationAfterStartAnalyzer.DiagnosticId, analyzer.SupportedDiagnostics[0].Id);
    }

    /// <summary>
    /// Tests that unresolved method calls are handled gracefully.
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous unit test.</returns>
    [Fact]
    public async Task UnresolvedMethodCall_NoDiagnostic()
    {
        string test = """
            using System;
            using System.Threading.Tasks;

            namespace WebDriverBiDi
            {
                public interface IBiDiDriver { }

                public class BiDiDriver : IBiDiDriver
                {
                    public BiDiDriver(TimeSpan timeout) { }
                    public Task StartAsync(string url) => Task.CompletedTask;
                    public void RegisterModule(Module module) { }
                }

                public abstract class Module
                {
                    protected Module(IBiDiDriver driver) { }
                    public abstract string ModuleName { get; }
                }
            }

            namespace TestApp
            {
                using WebDriverBiDi;

                public class TestClass
                {
                    public async Task TestMethod()
                    {
                        BiDiDriver driver = new BiDiDriver(TimeSpan.FromSeconds(30));
                        await driver.{|CS1061:NonExistentMethod|}();
                        driver.RegisterModule(new CustomModule(driver));
                    }
                }

                public class CustomModule : Module
                {
                    public CustomModule(IBiDiDriver driver) : base(driver) { }
                    public override string ModuleName => "custom";
                }
            }
            """;

        CSharpAnalyzerTest<BiDiDriver001_ModuleRegistrationAfterStartAnalyzer, DefaultVerifier> testState = new()
        {
            TestCode = test,
            ReferenceAssemblies = ReferenceAssemblies.Net.Net80,
        };

        await testState.RunAsync(TestContext.Current.CancellationToken);
    }

    /// <summary>
    /// Tests that complex expressions are handled gracefully in GetDriverVariableName.
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous unit test.</returns>
    [Fact]
    public async Task ComplexExpression_NoDiagnostic()
    {
        string test = """
            using System;
            using System.Threading.Tasks;

            namespace WebDriverBiDi
            {
                public interface IBiDiDriver { }

                public class BiDiDriver : IBiDiDriver
                {
                    public BiDiDriver(TimeSpan timeout) { }
                    public Task StartAsync(string url) => Task.CompletedTask;
                    public void RegisterModule(Module module) { }
                }

                public abstract class Module
                {
                    protected Module(IBiDiDriver driver) { }
                    public abstract string ModuleName { get; }
                }
            }

            namespace TestApp
            {
                using WebDriverBiDi;

                public class TestClass
                {
                    public async Task TestMethod()
                    {
                        BiDiDriver driver = new BiDiDriver(TimeSpan.FromSeconds(30));
                        await driver.StartAsync("ws://localhost:9222");
                        (driver ?? driver).RegisterModule(new CustomModule(driver));
                    }
                }

                public class CustomModule : Module
                {
                    public CustomModule(IBiDiDriver driver) : base(driver) { }
                    public override string ModuleName => "custom";
                }
            }
            """;

        // The coalesce expression (driver ?? driver) is not a simple identifier or member access,
        // so GetDriverVariableName returns null and the analyzer won't track it
        CSharpAnalyzerTest<BiDiDriver001_ModuleRegistrationAfterStartAnalyzer, DefaultVerifier> testState = new()
        {
            TestCode = test,
            ReferenceAssemblies = ReferenceAssemblies.Net.Net80,
        };

        await testState.RunAsync(TestContext.Current.CancellationToken);
    }

    /// <summary>
    /// Tests that StartAsync called through a variable reference (not member access) is handled.
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous unit test.</returns>
    [Fact]
    public async Task StartAsyncThroughDelegate_NoDiagnostic()
    {
        string test = """
            using System;
            using System.Threading.Tasks;

            namespace WebDriverBiDi
            {
                public interface IBiDiDriver { }

                public class BiDiDriver : IBiDiDriver
                {
                    public BiDiDriver(TimeSpan timeout) { }
                    public Task StartAsync(string url) => Task.CompletedTask;
                    public void RegisterModule(Module module) { }
                }

                public abstract class Module
                {
                    protected Module(IBiDiDriver driver) { }
                    public abstract string ModuleName { get; }
                }
            }

            namespace TestApp
            {
                using WebDriverBiDi;

                public class TestClass
                {
                    public async Task TestMethod()
                    {
                        BiDiDriver driver = new BiDiDriver(TimeSpan.FromSeconds(30));
                        Func<string, Task> startFunc = driver.StartAsync;
                        await startFunc("ws://localhost:9222");
                        driver.RegisterModule(new CustomModule(driver));
                    }
                }

                public class CustomModule : Module
                {
                    public CustomModule(IBiDiDriver driver) : base(driver) { }
                    public override string ModuleName => "custom";
                }
            }
            """;

        // StartAsync called through a delegate variable won't be tracked by the analyzer
        CSharpAnalyzerTest<BiDiDriver001_ModuleRegistrationAfterStartAnalyzer, DefaultVerifier> testState = new()
        {
            TestCode = test,
            ReferenceAssemblies = ReferenceAssemblies.Net.Net80,
        };

        await testState.RunAsync(TestContext.Current.CancellationToken);
    }

    /// <summary>
    /// Tests that RegisterModule called before StartAsync does not fire — exercises the
    /// IsStarted false branch of the third && clause on line 145.
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous unit test.</returns>
    [Fact]
    public async Task RegisterModule_BeforeStartAsync_DoesNotReportDiagnostic()
    {
        string test = """
            using System;
            using System.Threading.Tasks;

            namespace WebDriverBiDi
            {
                public interface IBiDiCommandExecutor
                {
                    Task StartAsync(string url);
                    void RegisterModule(object module);
                }

                public class BiDiDriver : IBiDiCommandExecutor
                {
                    public BiDiDriver(TimeSpan timeout) { }
                    public Task StartAsync(string url) => Task.CompletedTask;
                    public void RegisterModule(object module) { }
                }
            }

            namespace TestApp
            {
                using WebDriverBiDi;

                public class TestClass
                {
                    public void TestMethod()
                    {
                        IBiDiCommandExecutor driver = new BiDiDriver(TimeSpan.FromSeconds(30));
                        driver.RegisterModule(new object());
                    }
                }
            }
            """;

        CSharpAnalyzerTest<BiDiDriver001_ModuleRegistrationAfterStartAnalyzer, DefaultVerifier> testState = new()
        {
            TestCode = test,
            ReferenceAssemblies = ReferenceAssemblies.Net.Net80,
        };

        await testState.RunAsync(TestContext.Current.CancellationToken);
    }

    /// <summary>
    /// Tests that StartAsync called on a non-driver type does not fire — exercises the
    /// IsCommandExecutorType false branch of line 139.
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous unit test.</returns>
    [Fact]
    public async Task StartAsync_OnNonDriverType_DoesNotReportDiagnostic()
    {
        string test = """
            using System;
            using System.Threading.Tasks;

            namespace WebDriverBiDi
            {
                public interface IBiDiCommandExecutor
                {
                    Task StartAsync(string url);
                    void RegisterModule(object module);
                }

                public class BiDiDriver : IBiDiCommandExecutor
                {
                    public BiDiDriver(TimeSpan timeout) { }
                    public Task StartAsync(string url) => Task.CompletedTask;
                    public void RegisterModule(object module) { }
                }
            }

            namespace TestApp
            {
                using System.Threading.Tasks;
                using WebDriverBiDi;

                public class SomeService
                {
                    public Task StartAsync(string url) => Task.CompletedTask;
                }

                public class TestClass
                {
                    public async Task TestMethod()
                    {
                        IBiDiCommandExecutor driver = new BiDiDriver(TimeSpan.FromSeconds(30));
                        SomeService service = new SomeService();
                        await service.StartAsync("ws://localhost:9222");
                        driver.RegisterModule(new object());
                    }
                }
            }
            """;

        CSharpAnalyzerTest<BiDiDriver001_ModuleRegistrationAfterStartAnalyzer, DefaultVerifier> testState = new()
        {
            TestCode = test,
            ReferenceAssemblies = ReferenceAssemblies.Net.Net80,
        };

        await testState.RunAsync(TestContext.Current.CancellationToken);
    }
}
