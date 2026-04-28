// <copyright file="BiDiDriver020CodeFixProviderTests.cs" company="WebDriverBiDi.NET Committers">
// Copyright (c) WebDriverBiDi.NET Committers. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.
// </copyright>

namespace WebDriverBiDi.Analyzers.Tests;

using System.Threading.Tasks;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Testing;
using Microsoft.CodeAnalysis.Testing;

/// <summary>
/// Tests for the BiDiDriver020 code fix provider.
/// </summary>
[TestFixture]
public class BiDiDriver020CodeFixProviderTests
{
    [Test]
    public async Task WaitForAsync_WithoutStartCapturing_InsertsStartCapturing()
    {
        string testCode = """
            using System;
            using System.Threading.Tasks;
            using WebDriverBiDi;
            using WebDriverBiDi.BrowsingContext;

            namespace TestNamespace
            {
                public class TestClass
                {
                    public async Task TestMethod()
                    {
                        BiDiDriver driver = new();
                        EventObserver<NavigationEventArgs> observer = driver.BrowsingContext.OnLoad.AddObserver(args => { });
                        Task[] tasks = await {|#0:observer.WaitForCapturedTasksAsync(1, TimeSpan.FromSeconds(10))|};
                    }
                }
            }
            """;

        string fixedCode = """
            using System;
            using System.Threading.Tasks;
            using WebDriverBiDi;
            using WebDriverBiDi.BrowsingContext;

            namespace TestNamespace
            {
                public class TestClass
                {
                    public async Task TestMethod()
                    {
                        BiDiDriver driver = new();
                        EventObserver<NavigationEventArgs> observer = driver.BrowsingContext.OnLoad.AddObserver(args => { });
                        observer.StartCapturingTasks();
                        Task[] tasks = await observer.WaitForCapturedTasksAsync(1, TimeSpan.FromSeconds(10));
                    }
                }
            }
            """;

        DiagnosticResult expected = new DiagnosticResult(
            BiDiDriver020_CaptureSessionNotStartedAnalyzer.DiagnosticId,
            DiagnosticSeverity.Error)
            .WithLocation(0)
            .WithArguments("WaitForCapturedTasksAsync", "observer");

        CSharpCodeFixTest<BiDiDriver020_CaptureSessionNotStartedAnalyzer, BiDiDriver020_CaptureSessionNotStartedCodeFixProvider, DefaultVerifier> testState = new()
        {
            TestCode = testCode,
            FixedCode = fixedCode,
            ReferenceAssemblies = ReferenceAssemblies.Net.Net80,
        };
        testState.TestState.AdditionalReferences.Add(MetadataReference.CreateFromFile(AnalyzerTestHelpers.GetWebDriverBiDiAssemblyPath()));
        testState.ExpectedDiagnostics.Add(expected);

        await testState.RunAsync();
    }

    [Test]
    public async Task WaitForCapturedTasksAsync_WithoutStartCapturing_InsertsStartCapturing()
    {
        string testCode = """
            using System;
            using System.Threading.Tasks;
            using WebDriverBiDi;
            using WebDriverBiDi.BrowsingContext;

            namespace TestNamespace
            {
                public class TestClass
                {
                    public async Task TestMethod()
                    {
                        BiDiDriver driver = new();
                        EventObserver<NavigationEventArgs> observer = driver.BrowsingContext.OnLoad.AddObserver(args => { });
                        bool occurred = await {|#0:observer.WaitForCapturedTasksCompleteAsync(1, TimeSpan.FromSeconds(10))|};
                    }
                }
            }
            """;

        string fixedCode = """
            using System;
            using System.Threading.Tasks;
            using WebDriverBiDi;
            using WebDriverBiDi.BrowsingContext;

            namespace TestNamespace
            {
                public class TestClass
                {
                    public async Task TestMethod()
                    {
                        BiDiDriver driver = new();
                        EventObserver<NavigationEventArgs> observer = driver.BrowsingContext.OnLoad.AddObserver(args => { });
                        observer.StartCapturingTasks();
                        bool occurred = await observer.WaitForCapturedTasksCompleteAsync(1, TimeSpan.FromSeconds(10));
                    }
                }
            }
            """;

        DiagnosticResult expected = new DiagnosticResult(
            BiDiDriver020_CaptureSessionNotStartedAnalyzer.DiagnosticId,
            DiagnosticSeverity.Error)
            .WithLocation(0)
            .WithArguments("WaitForCapturedTasksCompleteAsync", "observer");

        CSharpCodeFixTest<BiDiDriver020_CaptureSessionNotStartedAnalyzer, BiDiDriver020_CaptureSessionNotStartedCodeFixProvider, DefaultVerifier> testState = new()
        {
            TestCode = testCode,
            FixedCode = fixedCode,
            ReferenceAssemblies = ReferenceAssemblies.Net.Net80,
        };
        testState.TestState.AdditionalReferences.Add(MetadataReference.CreateFromFile(AnalyzerTestHelpers.GetWebDriverBiDiAssemblyPath()));
        testState.ExpectedDiagnostics.Add(expected);

        await testState.RunAsync();
    }

    [Test]
    public async Task WaitForAsync_AfterStopCapturing_InsertsStartCapturingBeforeWait()
    {
        string testCode = """
            using System;
            using System.Threading.Tasks;
            using WebDriverBiDi;
            using WebDriverBiDi.BrowsingContext;

            namespace TestNamespace
            {
                public class TestClass
                {
                    public async Task TestMethod()
                    {
                        BiDiDriver driver = new();
                        EventObserver<NavigationEventArgs> observer = driver.BrowsingContext.OnLoad.AddObserver(args => { });
                        observer.StartCapturingTasks();
                        Task[] first = await observer.WaitForCapturedTasksAsync(1, TimeSpan.FromSeconds(10));
                        observer.StopCapturingTasks();
                        Task[] second = await {|#0:observer.WaitForCapturedTasksAsync(1, TimeSpan.FromSeconds(10))|};
                    }
                }
            }
            """;

        string fixedCode = """
            using System;
            using System.Threading.Tasks;
            using WebDriverBiDi;
            using WebDriverBiDi.BrowsingContext;

            namespace TestNamespace
            {
                public class TestClass
                {
                    public async Task TestMethod()
                    {
                        BiDiDriver driver = new();
                        EventObserver<NavigationEventArgs> observer = driver.BrowsingContext.OnLoad.AddObserver(args => { });
                        observer.StartCapturingTasks();
                        Task[] first = await observer.WaitForCapturedTasksAsync(1, TimeSpan.FromSeconds(10));
                        observer.StopCapturingTasks();
                        observer.StartCapturingTasks();
                        Task[] second = await observer.WaitForCapturedTasksAsync(1, TimeSpan.FromSeconds(10));
                    }
                }
            }
            """;

        DiagnosticResult expected = new DiagnosticResult(
            BiDiDriver020_CaptureSessionNotStartedAnalyzer.DiagnosticId,
            DiagnosticSeverity.Error)
            .WithLocation(0)
            .WithArguments("WaitForCapturedTasksAsync", "observer");

        CSharpCodeFixTest<BiDiDriver020_CaptureSessionNotStartedAnalyzer, BiDiDriver020_CaptureSessionNotStartedCodeFixProvider, DefaultVerifier> testState = new()
        {
            TestCode = testCode,
            FixedCode = fixedCode,
            ReferenceAssemblies = ReferenceAssemblies.Net.Net80,
        };
        testState.TestState.AdditionalReferences.Add(MetadataReference.CreateFromFile(AnalyzerTestHelpers.GetWebDriverBiDiAssemblyPath()));
        testState.ExpectedDiagnostics.Add(expected);

        await testState.RunAsync();
    }
}
