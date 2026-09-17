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
                    public CustomModule(IBiDiModuleHost driver) : base(driver) { }
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
                    public CustomModule(IBiDiModuleHost driver) : base(driver) { }
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
                    public CustomModule(IBiDiModuleHost driver) : base(driver) { }
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
                    public CustomModule(IBiDiModuleHost driver) : base(driver) { }
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
                    public CustomModule(IBiDiModuleHost driver) : base(driver) { }
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
                    public CustomModule(IBiDiModuleHost driver) : base(driver) { }
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
                    public CustomModule(IBiDiModuleHost driver) : base(driver) { }
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
                    public CustomModule(IBiDiModuleHost driver) : base(driver) { }
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
                    public CustomModule(IBiDiModuleHost driver) : base(driver) { }
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
    /// Tests that the local declarations the registration depends on, transitively, move with it: moving the registration alone would use the module variable before its declaration (CS0841). Unrelated statements stay.
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous unit test.</returns>
    [Fact]
    public async Task RegisterModule_DependingOnLocalsDeclaredAfterStart_CodeFixMovesDeclarationsToo()
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
                        string name = "custom";
                        Console.WriteLine(name);
                        CustomModule module = new CustomModule(driver, name);
                        {|#0:driver.RegisterModule(module)|};
                    }
                }

                public class CustomModule : Module
                {
                    public CustomModule(IBiDiModuleHost driver, string name) : base(driver) { }
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
                        string name = "custom";
                        CustomModule module = new CustomModule(driver, name);
                        driver.RegisterModule(module);
                        await driver.StartAsync("ws://localhost:9222");
                        Console.WriteLine(name);
                    }
                }

                public class CustomModule : Module
                {
                    public CustomModule(IBiDiModuleHost driver, string name) : base(driver) { }
                    public override string ModuleName => "custom";
                }
            }
            """;

        DiagnosticResult expected0 = new DiagnosticResult(BiDiDriver001_ModuleRegistrationAfterStartAnalyzer.DiagnosticId, Microsoft.CodeAnalysis.DiagnosticSeverity.Error)
            .WithLocation(0)
            .WithArguments("module");

        RealAssemblyCodeFixTest<BiDiDriver001_ModuleRegistrationAfterStartAnalyzer, BiDiDriver001_ModuleRegistrationAfterStartCodeFixProvider> testState = new()
        {
            TestCode = testCode,
            FixedCode = fixedCode,
        };
        testState.ExpectedDiagnostics.Add(expected0);

        await testState.RunAsync(TestContext.Current.CancellationToken);
    }

    /// <summary>
    /// Tests that no fix is offered when a declaration the registration depends on awaits something: it may need the started driver, so it cannot move before the start.
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous unit test.</returns>
    [Fact]
    public async Task RegisterModule_DependingOnAwaitedLocal_NoFixOffered()
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
                        string name = (await driver.Session.StatusAsync()).Message;
                        CustomModule module = new CustomModule(driver, name);
                        {|#0:driver.RegisterModule(module)|};
                    }
                }

                public class CustomModule : Module
                {
                    public CustomModule(IBiDiModuleHost driver, string name) : base(driver) { }
                    public override string ModuleName => "custom";
                }
            }
            """;

        
        DiagnosticResult expected0 = new DiagnosticResult(BiDiDriver001_ModuleRegistrationAfterStartAnalyzer.DiagnosticId, Microsoft.CodeAnalysis.DiagnosticSeverity.Error)
            .WithLocation(0)
            .WithArguments("module");

        RealAssemblyCodeFixTest<BiDiDriver001_ModuleRegistrationAfterStartAnalyzer, BiDiDriver001_ModuleRegistrationAfterStartCodeFixProvider> testState = new()
        {
            TestCode = testCode,
            FixedCode = testCode,
        };
        testState.ExpectedDiagnostics.Add(expected0);

        await testState.RunAsync(TestContext.Current.CancellationToken);
    }

    /// <summary>
    /// Tests that a dependency declared before the start is already in scope and is not moved.
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous unit test.</returns>
    [Fact]
    public async Task RegisterModule_DependingOnLocalDeclaredBeforeStart_CodeFixMovesRegistrationOnly()
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
                        CustomModule module = new CustomModule(driver);
                        await driver.StartAsync("ws://localhost:9222");
                        int unrelated = 1;
                        {|#0:driver.RegisterModule(module)|};
                    }
                }

                public class CustomModule : Module
                {
                    public CustomModule(IBiDiModuleHost driver) : base(driver) { }
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
                        CustomModule module = new CustomModule(driver);
                        driver.RegisterModule(module);
                        await driver.StartAsync("ws://localhost:9222");
                        int unrelated = 1;
                    }
                }

                public class CustomModule : Module
                {
                    public CustomModule(IBiDiModuleHost driver) : base(driver) { }
                    public override string ModuleName => "custom";
                }
            }
            """;

        DiagnosticResult expected0 = new DiagnosticResult(BiDiDriver001_ModuleRegistrationAfterStartAnalyzer.DiagnosticId, Microsoft.CodeAnalysis.DiagnosticSeverity.Error)
            .WithLocation(0)
            .WithArguments("module");

        RealAssemblyCodeFixTest<BiDiDriver001_ModuleRegistrationAfterStartAnalyzer, BiDiDriver001_ModuleRegistrationAfterStartCodeFixProvider> testState = new()
        {
            TestCode = testCode,
            FixedCode = fixedCode,
        };
        testState.ExpectedDiagnostics.Add(expected0);

        await testState.RunAsync(TestContext.Current.CancellationToken);
    }

    /// <summary>
    /// Tests that no fix is offered for a registration that is the embedded statement of an <c>if</c> (not inside a block): hoisting it above the start would make it unconditional.
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous unit test.</returns>
    [Fact]
    public async Task RegisterModule_AsEmbeddedStatement_NoFixOffered()
    {
        string testCode = """
            using System;
            using System.Threading.Tasks;
            using WebDriverBiDi;

            namespace TestApp
            {
                public class TestClass
                {
                    public async Task TestMethod(bool register)
                    {
                        BiDiDriver driver = new BiDiDriver(TimeSpan.FromSeconds(30));
                        await driver.StartAsync("ws://localhost:9222");
                        if (register)
                            {|#0:driver.RegisterModule(new CustomModule(driver))|};
                    }
                }

                public class CustomModule : Module
                {
                    public CustomModule(IBiDiModuleHost driver) : base(driver) { }
                    public override string ModuleName => "custom";
                }
            }
            """;


        DiagnosticResult expected0 = new DiagnosticResult(BiDiDriver001_ModuleRegistrationAfterStartAnalyzer.DiagnosticId, Microsoft.CodeAnalysis.DiagnosticSeverity.Error)
            .WithLocation(0)
            .WithArguments("new CustomModule(driver)");

        RealAssemblyCodeFixTest<BiDiDriver001_ModuleRegistrationAfterStartAnalyzer, BiDiDriver001_ModuleRegistrationAfterStartCodeFixProvider> testState = new()
        {
            TestCode = testCode,
            FixedCode = testCode,
        };
        testState.ExpectedDiagnostics.Add(expected0);

        await testState.RunAsync(TestContext.Current.CancellationToken);
    }

    /// <summary>
    /// Tests that no fix is offered when a local the registration depends on is assigned, rather than
    /// declared, after the start: only declarations can move, so the assignment would be left behind the
    /// registration and the moved code would read an unassigned variable.
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous unit test.</returns>
    [Fact]
    public async Task RegisterModule_DependingOnLocalAssignedAfterStart_NoFixOffered()
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
                        CustomModule module;
                        await driver.StartAsync("ws://localhost:9222");
                        module = new CustomModule(driver);
                        {|#0:driver.RegisterModule(module)|};
                    }
                }

                public class CustomModule : Module
                {
                    public CustomModule(IBiDiModuleHost driver) : base(driver) { }
                    public override string ModuleName => "custom";
                }
            }
            """;

        DiagnosticResult expected0 = new DiagnosticResult(BiDiDriver001_ModuleRegistrationAfterStartAnalyzer.DiagnosticId, Microsoft.CodeAnalysis.DiagnosticSeverity.Error)
            .WithLocation(0)
            .WithArguments("module");

        RealAssemblyCodeFixTest<BiDiDriver001_ModuleRegistrationAfterStartAnalyzer, BiDiDriver001_ModuleRegistrationAfterStartCodeFixProvider> testState = new()
        {
            TestCode = testCode,
            FixedCode = testCode,
        };
        testState.ExpectedDiagnostics.Add(expected0);

        await testState.RunAsync(TestContext.Current.CancellationToken);
    }

    /// <summary>
    /// Tests that no fix is offered when a local the registration depends on is written through an
    /// <c>out</c> argument after the start, for the same reason as an assignment.
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous unit test.</returns>
    [Fact]
    public async Task RegisterModule_DependingOnOutArgumentAfterStart_NoFixOffered()
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
                        CustomModule module;
                        await driver.StartAsync("ws://localhost:9222");
                        Create(out module);
                        {|#0:driver.RegisterModule(module)|};
                    }

                    // The helper does not take the driver: a driver handed to a helper after the start may have
                    // been stopped by it, so the registration would not be reported at all.
                    private static void Create(out CustomModule module)
                    {
                        module = new CustomModule(new BiDiDriver(TimeSpan.FromSeconds(30)));
                    }
                }

                public class CustomModule : Module
                {
                    public CustomModule(IBiDiModuleHost driver) : base(driver) { }
                    public override string ModuleName => "custom";
                }
            }
            """;

        DiagnosticResult expected0 = new DiagnosticResult(BiDiDriver001_ModuleRegistrationAfterStartAnalyzer.DiagnosticId, Microsoft.CodeAnalysis.DiagnosticSeverity.Error)
            .WithLocation(0)
            .WithArguments("module");

        RealAssemblyCodeFixTest<BiDiDriver001_ModuleRegistrationAfterStartAnalyzer, BiDiDriver001_ModuleRegistrationAfterStartCodeFixProvider> testState = new()
        {
            TestCode = testCode,
            FixedCode = testCode,
        };
        testState.ExpectedDiagnostics.Add(expected0);

        await testState.RunAsync(TestContext.Current.CancellationToken);
    }

    /// <summary>
    /// Tests that an assignment between the start and the registration that writes a name the
    /// registration does not read leaves the fix available: only the registration moves.
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous unit test.</returns>
    [Fact]
    public async Task RegisterModule_WithUnrelatedAssignmentAfterStart_CodeFixMovesRegistrationOnly()
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
                        int attempts = 0;
                        await driver.StartAsync("ws://localhost:9222");
                        attempts = 1;
                        Console.WriteLine(attempts);
                        {|#0:driver.RegisterModule(new CustomModule(driver))|};
                    }
                }

                public class CustomModule : Module
                {
                    public CustomModule(IBiDiModuleHost driver) : base(driver) { }
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
                        int attempts = 0;
                        driver.RegisterModule(new CustomModule(driver));
                        await driver.StartAsync("ws://localhost:9222");
                        attempts = 1;
                        Console.WriteLine(attempts);
                    }
                }

                public class CustomModule : Module
                {
                    public CustomModule(IBiDiModuleHost driver) : base(driver) { }
                    public override string ModuleName => "custom";
                }
            }
            """;

        DiagnosticResult expected0 = new DiagnosticResult(BiDiDriver001_ModuleRegistrationAfterStartAnalyzer.DiagnosticId, Microsoft.CodeAnalysis.DiagnosticSeverity.Error)
            .WithLocation(0)
            .WithArguments("new CustomModule(driver)");

        RealAssemblyCodeFixTest<BiDiDriver001_ModuleRegistrationAfterStartAnalyzer, BiDiDriver001_ModuleRegistrationAfterStartCodeFixProvider> testState = new()
        {
            TestCode = testCode,
            FixedCode = fixedCode,
        };
        testState.ExpectedDiagnostics.Add(expected0);

        await testState.RunAsync(TestContext.Current.CancellationToken);
    }

    /// <summary>
    /// Tests that a comment on the same line as the moved registration moves with it instead of being dropped.
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous unit test.</returns>
    [Fact]
    public async Task RegisterModule_WithTrailingComment_CodeFixKeepsCommentOnMovedStatement()
    {
        string testCode = """
            using System;
            using System.Threading.Tasks;
            using WebDriverBiDi;

            namespace TestNamespace
            {
                public class TestClass
                {
                    public async Task TestMethod()
                    {
                        BiDiDriver driver = new BiDiDriver(TimeSpan.FromSeconds(30));
                        await driver.StartAsync("ws://localhost:9222");
                        {|#0:driver.RegisterModule(new CustomModule(driver))|}; // adds the custom commands
                    }
                }

                public class CustomModule : Module
                {
                    public CustomModule(IBiDiModuleHost driver) : base(driver) { }
                    public override string ModuleName => "custom";
                }
            }
            """;

        string fixedCode = """
            using System;
            using System.Threading.Tasks;
            using WebDriverBiDi;

            namespace TestNamespace
            {
                public class TestClass
                {
                    public async Task TestMethod()
                    {
                        BiDiDriver driver = new BiDiDriver(TimeSpan.FromSeconds(30));
                        driver.RegisterModule(new CustomModule(driver)); // adds the custom commands
                        await driver.StartAsync("ws://localhost:9222");
                    }
                }

                public class CustomModule : Module
                {
                    public CustomModule(IBiDiModuleHost driver) : base(driver) { }
                    public override string ModuleName => "custom";
                }
            }
            """;

        DiagnosticResult expected = new DiagnosticResult(BiDiDriver001_ModuleRegistrationAfterStartAnalyzer.DiagnosticId, Microsoft.CodeAnalysis.DiagnosticSeverity.Error).WithLocation(0).WithArguments("new CustomModule(driver)");

        RealAssemblyCodeFixTest<BiDiDriver001_ModuleRegistrationAfterStartAnalyzer, BiDiDriver001_ModuleRegistrationAfterStartCodeFixProvider> testState = new()
        {
            TestCode = testCode,
            FixedCode = fixedCode,
        };
        testState.ExpectedDiagnostics.Add(expected);

        await testState.RunAsync(TestContext.Current.CancellationToken);
    }

    /// <summary>
    /// Tests that a registration made through a wrapped receiver, after a StartAsync made through one, is reported
    /// and moved before that StartAsync.
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous unit test.</returns>
    [Fact]
    public async Task RegisterModuleThroughWrappedReceiver_AfterWrappedStartAsync_CodeFixMovesItBefore()
    {
        string testCode = """
            using System.Threading.Tasks;
            using WebDriverBiDi;

            namespace TestApp
            {
                public class TestClass
                {
                    public async Task TestMethod()
                    {
                        BiDiDriver? driver = new();
                        await (driver?.StartAsync("ws://localhost:9222") ?? Task.CompletedTask);
                        {|#0:driver!.RegisterModule(new CustomModule(driver))|};
                    }
                }

                public class CustomModule : Module
                {
                    public CustomModule(IBiDiModuleHost driver) : base(driver) { }
                    public override string ModuleName => "custom";
                }
            }
            """;

        string fixedCode = """
            using System.Threading.Tasks;
            using WebDriverBiDi;

            namespace TestApp
            {
                public class TestClass
                {
                    public async Task TestMethod()
                    {
                        BiDiDriver? driver = new();
                        driver!.RegisterModule(new CustomModule(driver));
                        await (driver?.StartAsync("ws://localhost:9222") ?? Task.CompletedTask);
                    }
                }

                public class CustomModule : Module
                {
                    public CustomModule(IBiDiModuleHost driver) : base(driver) { }
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
