// <copyright file="BiDiDriver001CodeFixProviderTests.cs" company="WebDriverBiDi.NET Committers">
// Copyright (c) WebDriverBiDi.NET Committers. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.
// </copyright>

namespace WebDriverBiDi.Analyzers.Tests;

using System.Threading.Tasks;
using Microsoft.CodeAnalysis.Testing;

/// <summary>
/// Tests for the BiDiDriver001 code fix provider.
/// </summary>
public class BiDiDriver001CodeFixProviderTests
{
    /// <summary>
    /// Tests that no fix is offered when the diagnostic is reported in a constructor: the fix
    /// rearranges statements of a method declaration, which does not exist in that context
    /// (previously the provider threw while building the fix).
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous unit test.</returns>
    [Fact]
    public async Task RegisterModule_AfterStartAsyncInConstructor_NoFixOffered()
    {
        string testCode = """
            using System;
            using System.Threading.Tasks;
            using WebDriverBiDi;

            namespace TestApp
            {
                public class TestClass
                {
                    public TestClass()
                    {
                        BiDiDriver driver = new BiDiDriver(TimeSpan.FromSeconds(30));
                        driver.StartAsync("ws://localhost:9222").Wait();
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

        DiagnosticResult expected = new DiagnosticResult(
            BiDiDriver001_ModuleRegistrationAfterStartAnalyzer.DiagnosticId,
            Microsoft.CodeAnalysis.DiagnosticSeverity.Error)
            .WithLocation(0)
            .WithArguments("new CustomModule(driver)");

        RealAssemblyCodeFixTest<BiDiDriver001_ModuleRegistrationAfterStartAnalyzer, BiDiDriver001_ModuleRegistrationAfterStartCodeFixProvider> testState = new()
        {
            TestCode = testCode,
            FixedCode = testCode,
        };
        testState.ExpectedDiagnostics.Add(expected);

        await testState.RunAsync(TestContext.Current.CancellationToken);
    }

    /// <summary>
    /// Tests that the code fix moves RegisterModule() before StartAsync().
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous unit test.</returns>
    [Fact]
    public async Task RegisterModule_AfterStartAsync_CodeFixMovesItBefore()
    {
        string testCode = """
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

        string fixedCode = """
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
                        driver.RegisterModule(new CustomModule(driver));
                        await driver.StartAsync("ws://localhost:9222");
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

        RealAssemblyCodeFixTest<BiDiDriver001_ModuleRegistrationAfterStartAnalyzer, BiDiDriver001_ModuleRegistrationAfterStartCodeFixProvider> testState = new()
        {
            TestCode = testCode,
            FixedCode = fixedCode,
        };
        testState.ExpectedDiagnostics.Add(expected);

        await testState.RunAsync(TestContext.Current.CancellationToken);
    }

    [Fact]
    public async Task RegisterModule_AfterBlockingStartWithWait_CodeFixMovesItBefore()
    {
        string testCode = """
            using System;
            using System.Threading.Tasks;
            using WebDriverBiDi;

            namespace TestApp
            {
                public class TestClass
                {
                    public void TestMethod()
                    {
                        BiDiDriver driver = new BiDiDriver(TimeSpan.FromSeconds(30));
                        driver.StartAsync("ws://localhost:9222").Wait();
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

        string fixedCode = """
            using System;
            using System.Threading.Tasks;
            using WebDriverBiDi;

            namespace TestApp
            {
                public class TestClass
                {
                    public void TestMethod()
                    {
                        BiDiDriver driver = new BiDiDriver(TimeSpan.FromSeconds(30));
                        driver.RegisterModule(new CustomModule(driver));
                        driver.StartAsync("ws://localhost:9222").Wait();
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

        RealAssemblyCodeFixTest<BiDiDriver001_ModuleRegistrationAfterStartAnalyzer, BiDiDriver001_ModuleRegistrationAfterStartCodeFixProvider> testState = new()
        {
            TestCode = testCode,
            FixedCode = fixedCode,
        };
        testState.ExpectedDiagnostics.Add(expected);

        await testState.RunAsync(TestContext.Current.CancellationToken);
    }

    /// <summary>
    /// Tests that when another driver's StartAsync appears textually before the flagged driver is
    /// declared, the fix moves RegisterModule before the flagged driver's OWN StartAsync rather than
    /// before the other driver's StartAsync (which would place it ahead of the declaration, CS0841).
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous unit test.</returns>
    [Fact]
    public async Task RegisterModule_WithAnotherDriverStartedFirst_CodeFixMovesBeforeOwnStartAsync()
    {
        string testCode = """
            using System;
            using System.Threading.Tasks;
            using WebDriverBiDi;

            namespace TestApp
            {
                public class TestClass
                {
                    public async Task TestMethod()
                    {
                        BiDiDriver other = new BiDiDriver(TimeSpan.FromSeconds(30));
                        await other.StartAsync("ws://localhost:1111");
                        BiDiDriver driver = new BiDiDriver(TimeSpan.FromSeconds(30));
                        await driver.StartAsync("ws://localhost:2222");
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

        string fixedCode = """
            using System;
            using System.Threading.Tasks;
            using WebDriverBiDi;

            namespace TestApp
            {
                public class TestClass
                {
                    public async Task TestMethod()
                    {
                        BiDiDriver other = new BiDiDriver(TimeSpan.FromSeconds(30));
                        await other.StartAsync("ws://localhost:1111");
                        BiDiDriver driver = new BiDiDriver(TimeSpan.FromSeconds(30));
                        driver.RegisterModule(new CustomModule(driver));
                        await driver.StartAsync("ws://localhost:2222");
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

        RealAssemblyCodeFixTest<BiDiDriver001_ModuleRegistrationAfterStartAnalyzer, BiDiDriver001_ModuleRegistrationAfterStartCodeFixProvider> testState = new()
        {
            TestCode = testCode,
            FixedCode = fixedCode,
        };
        testState.ExpectedDiagnostics.Add(expected);

        await testState.RunAsync(TestContext.Current.CancellationToken);
    }

    /// <summary>
    /// Tests that the driver-variable filter skips StartAsync calls whose receiver is a chained
    /// member access or a this-rooted field (neither matches the flagged simple driver variable),
    /// and still moves the registration before the flagged driver's own StartAsync.
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous unit test.</returns>
    [Fact]
    public async Task RegisterModule_WithNonMatchingStartAsyncReceivers_CodeFixMovesBeforeOwnStartAsync()
    {
        string testCode = """
            using System;
            using System.Threading.Tasks;
            using WebDriverBiDi;

            namespace TestApp
            {
                public class DriverHolder
                {
                    public BiDiDriver Driver { get; set; } = new BiDiDriver(TimeSpan.FromSeconds(30));
                }

                public class TestClass
                {
                    private BiDiDriver field = new BiDiDriver(TimeSpan.FromSeconds(30));

                    public async Task TestMethod()
                    {
                        DriverHolder holder = new DriverHolder();
                        await holder.Driver.StartAsync("ws://localhost:1111");
                        await this.field.StartAsync("ws://localhost:2222");
                        BiDiDriver driver = new BiDiDriver(TimeSpan.FromSeconds(30));
                        await driver.StartAsync("ws://localhost:3333");
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

        string fixedCode = """
            using System;
            using System.Threading.Tasks;
            using WebDriverBiDi;

            namespace TestApp
            {
                public class DriverHolder
                {
                    public BiDiDriver Driver { get; set; } = new BiDiDriver(TimeSpan.FromSeconds(30));
                }

                public class TestClass
                {
                    private BiDiDriver field = new BiDiDriver(TimeSpan.FromSeconds(30));

                    public async Task TestMethod()
                    {
                        DriverHolder holder = new DriverHolder();
                        await holder.Driver.StartAsync("ws://localhost:1111");
                        await this.field.StartAsync("ws://localhost:2222");
                        BiDiDriver driver = new BiDiDriver(TimeSpan.FromSeconds(30));
                        driver.RegisterModule(new CustomModule(driver));
                        await driver.StartAsync("ws://localhost:3333");
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

        RealAssemblyCodeFixTest<BiDiDriver001_ModuleRegistrationAfterStartAnalyzer, BiDiDriver001_ModuleRegistrationAfterStartCodeFixProvider> testState = new()
        {
            TestCode = testCode,
            FixedCode = fixedCode,
        };
        testState.ExpectedDiagnostics.Add(expected);

        await testState.RunAsync(TestContext.Current.CancellationToken);
    }
}
