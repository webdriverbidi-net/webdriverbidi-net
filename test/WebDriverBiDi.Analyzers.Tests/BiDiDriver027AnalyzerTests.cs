// <copyright file="BiDiDriver027AnalyzerTests.cs" company="WebDriverBiDi.NET Committers">
// Copyright (c) WebDriverBiDi.NET Committers. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.
// </copyright>

namespace WebDriverBiDi.Analyzers.Tests;

using System.Threading.Tasks;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Testing;
using Microsoft.CodeAnalysis.Testing;

/// <summary>
/// Tests for the BiDiDriver027 analyzer, which flags a RegisterEvent call whose event name is a
/// built-in protocol event.
/// </summary>
public class BiDiDriver027AnalyzerTests
{
    [Fact]
    public async Task RegisterEvent_WithBuiltInName_ReportsError()
    {
        string testCode = """
            using System.Threading.Tasks;
            using WebDriverBiDi;

            namespace TestApp
            {
                public class TestClass
                {
                    public void TestMethod(BiDiDriver driver)
                    {
                        driver.RegisterEvent<int>({|#0:"log.entryAdded"|}, info => Task.CompletedTask);
                    }
                }
            }
            """;

        DiagnosticResult expected = new DiagnosticResult(
            BiDiDriver027_RegisterEventWithBuiltInNameAnalyzer.DiagnosticId,
            DiagnosticSeverity.Error)
            .WithLocation(0)
            .WithArguments("log.entryAdded");

        RealAssemblyAnalyzerTest<BiDiDriver027_RegisterEventWithBuiltInNameAnalyzer> testState = new()
        {
            TestCode = testCode,
        };
        testState.ExpectedDiagnostics.Add(expected);

        await testState.RunAsync(TestContext.Current.CancellationToken);
    }

    [Fact]
    public async Task RegisterEvent_WithCustomName_NoDiagnostic()
    {
        string testCode = """
            using System.Threading.Tasks;
            using WebDriverBiDi;

            namespace TestApp
            {
                public class TestClass
                {
                    public void TestMethod(BiDiDriver driver)
                    {
                        driver.RegisterEvent<int>("my.customEvent", info => Task.CompletedTask);
                    }
                }
            }
            """;

        await AnalyzerTestHelpers.VerifyAnalyzerAsync<BiDiDriver027_RegisterEventWithBuiltInNameAnalyzer>(testCode);
    }

    [Fact]
    public async Task RegisterEvent_WithNonConstantName_NoDiagnostic()
    {
        string testCode = """
            using System.Threading.Tasks;
            using WebDriverBiDi;

            namespace TestApp
            {
                public class TestClass
                {
                    public void TestMethod(BiDiDriver driver, string eventName)
                    {
                        driver.RegisterEvent<int>(eventName, info => Task.CompletedTask);
                    }
                }
            }
            """;

        await AnalyzerTestHelpers.VerifyAnalyzerAsync<BiDiDriver027_RegisterEventWithBuiltInNameAnalyzer>(testCode);
    }

    [Fact]
    public async Task RegisterEvent_OnNonExecutorType_NoDiagnostic()
    {
        // A same-named RegisterEvent on a type that is not a command executor is not the library's
        // method, so a built-in name is not reported.
        string testCode = """
            using System;
            using System.Threading.Tasks;
            using WebDriverBiDi;

            namespace TestApp
            {
                public class FakeExecutor
                {
                    public void RegisterEvent<T>(string eventName, Func<int, Task> eventInvoker)
                    {
                    }
                }

                public class TestClass
                {
                    public void TestMethod()
                    {
                        var fake = new FakeExecutor();
                        fake.RegisterEvent<int>("log.entryAdded", i => Task.CompletedTask);
                    }
                }
            }
            """;

        await AnalyzerTestHelpers.VerifyAnalyzerAsync<BiDiDriver027_RegisterEventWithBuiltInNameAnalyzer>(testCode);
    }

    [Fact]
    public async Task NonRegisterEventInvocations_NoDiagnostic()
    {
        string testCode = """
            using System;
            using WebDriverBiDi;

            namespace TestApp
            {
                public class TestClass
                {
                    public void TestMethod(BiDiDriver driver)
                    {
                        NoOp();                     // invocation that is not a member access
                        Console.WriteLine("x");     // member access whose name is not RegisterEvent
                    }

                    private void NoOp()
                    {
                    }
                }
            }
            """;

        await AnalyzerTestHelpers.VerifyAnalyzerAsync<BiDiDriver027_RegisterEventWithBuiltInNameAnalyzer>(testCode);
    }

    [Fact]
    public async Task UnresolvedRegisterEvent_NoDiagnostic()
    {
        // A method named RegisterEvent that does not exist yields a null symbol; the analyzer must not
        // report or crash. Cannot be reproduced against the real API.
        string testCode = """
            using System.Threading.Tasks;
            using WebDriverBiDi;

            namespace TestApp
            {
                public class Foo
                {
                }

                public class TestClass
                {
                    public void TestMethod()
                    {
                        var foo = new Foo();
                        foo.{|CS1061:RegisterEvent<int>|}("log.entryAdded", info => Task.CompletedTask);
                    }
                }
            }
            """;

        RealAssemblyAnalyzerTest<BiDiDriver027_RegisterEventWithBuiltInNameAnalyzer> testState = new()
        {
            TestCode = testCode,
        };

        await testState.RunAsync(TestContext.Current.CancellationToken);
    }

    [Fact]
    public async Task RegisterEvent_LibraryNotReferenced_NoDiagnostic()
    {
        // Without the WebDriverBiDi assembly, the ObservableEventName attribute type is not found, so
        // there are no built-in names and the analyzer registers no per-invocation action.
        string testCode = """
            using System;
            using System.Threading.Tasks;

            namespace TestApp
            {
                public class FakeDriver
                {
                    public void RegisterEvent<T>(string eventName, Func<int, Task> eventInvoker)
                    {
                    }
                }

                public class TestClass
                {
                    public void TestMethod()
                    {
                        var driver = new FakeDriver();
                        driver.RegisterEvent<int>("log.entryAdded", i => Task.CompletedTask);
                    }
                }
            }
            """;

        CSharpAnalyzerTest<BiDiDriver027_RegisterEventWithBuiltInNameAnalyzer, DefaultVerifier> testState = new()
        {
            TestCode = testCode,
            ReferenceAssemblies = ReferenceAssemblies.Net.Net80,
        };

        await testState.RunAsync(TestContext.Current.CancellationToken);
    }
}
