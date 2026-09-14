// <copyright file="BiDiDriver029AnalyzerTests.cs" company="WebDriverBiDi.NET Committers">
// Copyright (c) WebDriverBiDi.NET Committers. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.
// </copyright>

namespace WebDriverBiDi.Analyzers.Tests;

using System.Threading.Tasks;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Testing;

/// <summary>
/// Tests for the BiDiDriver029 analyzer.
/// </summary>
public class BiDiDriver029AnalyzerTests
{
    /// <summary>
    /// A custom driver, module and result used by the tests that exercise the module-command path and
    /// the disposal of a driver's own member. A user cannot add a property to <c>BiDiDriver</c>, so a
    /// derived driver is the only way a module other than the fifteen built-in ones is reached as
    /// <c>driver.Something.Method()</c>.
    /// </summary>
    private const string CustomDriverDeclarations = """

        namespace TestApp
        {
            public record CustomResult : CommandResult
            {
            }

            public class CustomModule : Module
            {
                public CustomModule(IBiDiModuleHost driver) : base(driver) { }

                public override string ModuleName => "custom";

                public void DoWork() { }

                public Task<int> ComputeAsync() => Task.FromResult(0);

                public Task<CustomResult> SendAsync() => Task.FromResult(new CustomResult());
            }

            public class OwnedResource : IAsyncDisposable
            {
                public ValueTask DisposeAsync() => default;
            }

            public class CustomDriver : BiDiDriver
            {
                public CustomDriver()
                {
                    this.Custom = new CustomModule(this);
                }

                public CustomModule Custom { get; }

                public OwnedResource Resource { get; } = new();
            }
        }
        """;

    [Fact]
    public async Task ModuleCommandAfterDispose_ReportsError()
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
                        BiDiDriver driver = new();
                        await driver.DisposeAsync();
                        await {|#0:driver.Session.StatusAsync()|};
                    }
                }
            }
            """;

        DiagnosticResult expected = new DiagnosticResult(
            BiDiDriver029_DriverUseAfterDisposalAnalyzer.DiagnosticId,
            DiagnosticSeverity.Error)
            .WithLocation(0)
            .WithArguments("StatusAsync", "driver");

        await AnalyzerTestHelpers.VerifyAnalyzerAsync<BiDiDriver029_DriverUseAfterDisposalAnalyzer>(testCode, expected);
    }

    [Fact]
    public async Task DisposalGuardedDriverMembersAfterDispose_ReportError()
    {
        string testCode = """
            using System;
            using System.Text.Json.Serialization.Metadata;
            using System.Threading.Tasks;
            using WebDriverBiDi;
            using WebDriverBiDi.Session;

            namespace TestNamespace
            {
                public class TestClass
                {
                    public async Task TestMethod(string url)
                    {
                        BiDiDriver driver = new();
                        await driver.DisposeAsync();
                        await {|#0:driver.StartAsync(url)|};
                        await {|#1:driver.ExecuteCommandAsync<StatusCommandResult>(new StatusCommandParameters())|};
                        {|#2:driver.RegisterEvent<string>("custom.event", info => Task.CompletedTask)|};
                        SessionModule module = {|#3:driver.GetModule<SessionModule>("session")|};
                        await {|#4:driver.RegisterTypeInfoResolverAsync(new DefaultJsonTypeInfoResolver())|};
                    }
                }
            }
            """;

        // The .NET 10 reference set is required here: the sample names a System.Text.Json type, and the
        // library's own reference to that assembly is newer than the .NET 8 set the default helper uses.
        RealAssemblyAnalyzerTest<BiDiDriver029_DriverUseAfterDisposalAnalyzer> testState = new()
        {
            TestCode = testCode,
        };
        testState.ExpectedDiagnostics.AddRange(
        [
            CreateExpected(0, "StartAsync"),
            CreateExpected(1, "ExecuteCommandAsync"),
            CreateExpected(2, "RegisterEvent"),
            CreateExpected(3, "GetModule"),
            CreateExpected(4, "RegisterTypeInfoResolverAsync"),
        ]);

        await testState.RunAsync(TestContext.Current.CancellationToken);
    }

    [Fact]
    public async Task StopAsyncAndSecondDisposeAfterDispose_ReportNothing()
    {
        // StopAsync finds the transport already disconnected and returns, and DisposeAsync is idempotent;
        // neither throws on a disposed driver.
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
                        BiDiDriver driver = new();
                        await driver.DisposeAsync();
                        await driver.StopAsync();
                        await driver.DisposeAsync();
                    }
                }
            }
            """;

        await AnalyzerTestHelpers.VerifyAnalyzerAsync<BiDiDriver029_DriverUseAfterDisposalAnalyzer>(testCode);
    }

    [Fact]
    public async Task ObservingEventAfterDispose_ReportsNothing()
    {
        // Reaching an observable event through a module property touches no disposal guard.
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
                        await driver.DisposeAsync();
                        EventObserver<NavigationEventArgs> observer = driver.BrowsingContext.OnLoad.AddObserver(args => { });
                        observer.Unobserve();
                    }
                }
            }
            """;

        await AnalyzerTestHelpers.VerifyAnalyzerAsync<BiDiDriver029_DriverUseAfterDisposalAnalyzer>(testCode);
    }

    [Fact]
    public async Task CommandBeforeDispose_ReportsNothing()
    {
        string testCode = """
            using System;
            using System.Threading.Tasks;
            using WebDriverBiDi;

            namespace TestNamespace
            {
                public class TestClass
                {
                    public async Task TestMethod(string url)
                    {
                        BiDiDriver driver = new();
                        await driver.StartAsync(url);
                        await driver.Session.StatusAsync();
                        await driver.DisposeAsync();
                    }
                }
            }
            """;

        await AnalyzerTestHelpers.VerifyAnalyzerAsync<BiDiDriver029_DriverUseAfterDisposalAnalyzer>(testCode);
    }

    [Fact]
    public async Task ReassignedAfterDispose_ReportsNothing()
    {
        // Rebinding the variable makes it name a different driver, so the disposal no longer applies.
        string testCode = """
            using System;
            using System.Threading.Tasks;
            using WebDriverBiDi;

            namespace TestNamespace
            {
                public class TestClass
                {
                    public async Task TestMethod(string url)
                    {
                        BiDiDriver driver = new();
                        await driver.DisposeAsync();
                        driver = new BiDiDriver();
                        await driver.StartAsync(url);
                    }
                }
            }
            """;

        await AnalyzerTestHelpers.VerifyAnalyzerAsync<BiDiDriver029_DriverUseAfterDisposalAnalyzer>(testCode);
    }

    [Fact]
    public async Task UnrelatedAssignments_ReportNothing()
    {
        string testCode = """
            using System;
            using System.Threading.Tasks;
            using WebDriverBiDi;

            namespace TestNamespace
            {
                public class TestClass
                {
                    private int count;

                    public async Task TestMethod(string url)
                    {
                        BiDiDriver driver = new();
                        int local = 0;
                        this.count = 1;
                        local = 2;
                        Console.WriteLine(local);
                        await driver.StartAsync(url);
                    }
                }
            }
            """;

        await AnalyzerTestHelpers.VerifyAnalyzerAsync<BiDiDriver029_DriverUseAfterDisposalAnalyzer>(testCode);
    }

    [Fact]
    public async Task DisposeInOneBranchOnly_ReportsNothing()
    {
        string testCode = """
            using System;
            using System.Threading.Tasks;
            using WebDriverBiDi;

            namespace TestNamespace
            {
                public class TestClass
                {
                    public async Task TestMethod(string url, bool shouldDispose)
                    {
                        BiDiDriver driver = new();
                        if (shouldDispose)
                        {
                            await driver.DisposeAsync();
                        }

                        await driver.StartAsync(url);
                    }
                }
            }
            """;

        await AnalyzerTestHelpers.VerifyAnalyzerAsync<BiDiDriver029_DriverUseAfterDisposalAnalyzer>(testCode);
    }

    [Fact]
    public async Task DisposeInBothBranches_ReportsError()
    {
        string testCode = """
            using System;
            using System.Threading.Tasks;
            using WebDriverBiDi;

            namespace TestNamespace
            {
                public class TestClass
                {
                    public async Task TestMethod(string url, bool flag)
                    {
                        BiDiDriver driver = new();
                        if (flag)
                        {
                            await driver.DisposeAsync();
                        }
                        else
                        {
                            await driver.DisposeAsync();
                        }

                        await {|#0:driver.StartAsync(url)|};
                    }
                }
            }
            """;

        await AnalyzerTestHelpers.VerifyAnalyzerAsync<BiDiDriver029_DriverUseAfterDisposalAnalyzer>(testCode, CreateExpected(0, "StartAsync"));
    }

    [Fact]
    public async Task UseInsideBranchAfterDisposeInCondition_ReportsError()
    {
        // A call in the condition executes unconditionally, before either branch.
        string testCode = """
            using System;
            using System.Threading.Tasks;
            using WebDriverBiDi;

            namespace TestNamespace
            {
                public class TestClass
                {
                    public bool Release(BiDiDriver driver)
                    {
                        return true;
                    }

                    public async Task TestMethod(string url, BiDiDriver other)
                    {
                        BiDiDriver driver = new();
                        await driver.DisposeAsync();
                        if (this.Release(other))
                        {
                            await {|#0:driver.StartAsync(url)|};
                        }
                    }
                }
            }
            """;

        await AnalyzerTestHelpers.VerifyAnalyzerAsync<BiDiDriver029_DriverUseAfterDisposalAnalyzer>(testCode, CreateExpected(0, "StartAsync"));
    }

    [Fact]
    public async Task DisposeInEverySwitchSection_ReportsError()
    {
        string testCode = """
            using System;
            using System.Threading.Tasks;
            using WebDriverBiDi;

            namespace TestNamespace
            {
                public class TestClass
                {
                    public async Task TestMethod(string url, int mode)
                    {
                        BiDiDriver driver = new();
                        await driver.DisposeAsync();
                        switch (mode)
                        {
                            case 1:
                                Console.WriteLine("one");
                                break;
                            default:
                                Console.WriteLine("other");
                                break;
                        }

                        await {|#0:driver.StartAsync(url)|};
                    }
                }
            }
            """;

        await AnalyzerTestHelpers.VerifyAnalyzerAsync<BiDiDriver029_DriverUseAfterDisposalAnalyzer>(testCode, CreateExpected(0, "StartAsync"));
    }

    [Fact]
    public async Task DisposeInOneSwitchSectionOnly_ReportsNothing()
    {
        string testCode = """
            using System;
            using System.Threading.Tasks;
            using WebDriverBiDi;

            namespace TestNamespace
            {
                public class TestClass
                {
                    public async Task TestMethod(string url, int mode)
                    {
                        BiDiDriver driver = new();
                        switch (mode)
                        {
                            case 1:
                                await driver.DisposeAsync();
                                break;
                        }

                        await driver.StartAsync(url);
                    }
                }
            }
            """;

        await AnalyzerTestHelpers.VerifyAnalyzerAsync<BiDiDriver029_DriverUseAfterDisposalAnalyzer>(testCode);
    }

    [Fact]
    public async Task DisposeInTryUseInCatch_ReportsNothing()
    {
        // A catch clause may begin after any prefix of the try block, so the dispose may not have run.
        string testCode = """
            using System;
            using System.Threading.Tasks;
            using WebDriverBiDi;

            namespace TestNamespace
            {
                public class TestClass
                {
                    public async Task TestMethod(string url)
                    {
                        BiDiDriver driver = new();
                        try
                        {
                            await driver.StartAsync(url);
                            await driver.DisposeAsync();
                        }
                        catch (InvalidOperationException ex) when (ex.Message.Length > 0)
                        {
                            await driver.Session.StatusAsync();
                        }
                        catch (Exception)
                        {
                            await driver.Session.StatusAsync();
                        }
                        finally
                        {
                            await driver.StopAsync();
                        }
                    }
                }
            }
            """;

        await AnalyzerTestHelpers.VerifyAnalyzerAsync<BiDiDriver029_DriverUseAfterDisposalAnalyzer>(testCode);
    }

    [Fact]
    public async Task DisposeBeforeTryUseInCatchAndFinally_ReportsError()
    {
        // Disposed on entry and never rebound in the try, so every partial execution leaves it disposed.
        string testCode = """
            using System;
            using System.Threading.Tasks;
            using WebDriverBiDi;

            namespace TestNamespace
            {
                public class TestClass
                {
                    public async Task TestMethod(string url)
                    {
                        BiDiDriver driver = new();
                        await driver.DisposeAsync();
                        try
                        {
                            Console.WriteLine("work");
                        }
                        catch (Exception)
                        {
                            await {|#0:driver.StartAsync(url)|};
                        }
                        finally
                        {
                            await {|#1:driver.Session.StatusAsync()|};
                        }
                    }
                }
            }
            """;

        DiagnosticResult[] expected = [CreateExpected(0, "StartAsync"), CreateExpected(1, "StatusAsync")];
        await AnalyzerTestHelpers.VerifyAnalyzerAsync<BiDiDriver029_DriverUseAfterDisposalAnalyzer>(testCode, expected);
    }

    [Fact]
    public async Task UseAfterTryThatDisposesOnEveryPath_ReportsError()
    {
        string testCode = """
            using System;
            using System.Threading.Tasks;
            using WebDriverBiDi;

            namespace TestNamespace
            {
                public class TestClass
                {
                    public async Task TestMethod(string url)
                    {
                        BiDiDriver driver = new();
                        try
                        {
                            await driver.DisposeAsync();
                        }
                        catch (Exception)
                        {
                            await driver.DisposeAsync();
                        }

                        await {|#0:driver.StartAsync(url)|};
                    }
                }
            }
            """;

        await AnalyzerTestHelpers.VerifyAnalyzerAsync<BiDiDriver029_DriverUseAfterDisposalAnalyzer>(testCode, CreateExpected(0, "StartAsync"));
    }

    [Fact]
    public async Task UseAfterTryThatDisposesOnlyInTryBlock_ReportsNothing()
    {
        string testCode = """
            using System;
            using System.Threading.Tasks;
            using WebDriverBiDi;

            namespace TestNamespace
            {
                public class TestClass
                {
                    public async Task TestMethod(string url)
                    {
                        BiDiDriver driver = new();
                        try
                        {
                            await driver.DisposeAsync();
                        }
                        catch (Exception)
                        {
                            Console.WriteLine("failed");
                        }

                        await driver.StartAsync(url);
                    }
                }
            }
            """;

        await AnalyzerTestHelpers.VerifyAnalyzerAsync<BiDiDriver029_DriverUseAfterDisposalAnalyzer>(testCode);
    }

    [Fact]
    public async Task UseAfterUsingStatementOverExistingDriver_ReportsError()
    {
        // The using statement disposes the driver when its body finishes.
        string testCode = """
            using System;
            using System.Threading.Tasks;
            using WebDriverBiDi;

            namespace TestNamespace
            {
                public class TestClass
                {
                    public async Task TestMethod(string url)
                    {
                        BiDiDriver driver = new();
                        await using (driver)
                        {
                            await driver.StartAsync(url);
                        }

                        await {|#0:driver.Session.StatusAsync()|};
                    }
                }
            }
            """;

        await AnalyzerTestHelpers.VerifyAnalyzerAsync<BiDiDriver029_DriverUseAfterDisposalAnalyzer>(testCode, CreateExpected(0, "StatusAsync"));
    }

    [Fact]
    public async Task UsingStatementDeclarationAndUnrelatedResource_ReportNothing()
    {
        // A using statement that declares its own driver disposes it at a point no later statement can
        // reach, and a using statement over an unrelated resource leaves the driver alone.
        string testCode = """
            using System;
            using System.IO;
            using System.Threading.Tasks;
            using WebDriverBiDi;

            namespace TestNamespace
            {
                public class TestClass
                {
                    public async Task TestMethod(string url, MemoryStream stream)
                    {
                        await using (BiDiDriver scoped = new())
                        {
                            await scoped.StartAsync(url);
                        }

                        BiDiDriver driver = new();
                        using (stream)
                        {
                            Console.WriteLine(stream.Length);
                        }

                        await driver.StartAsync(url);
                    }
                }
            }
            """;

        await AnalyzerTestHelpers.VerifyAnalyzerAsync<BiDiDriver029_DriverUseAfterDisposalAnalyzer>(testCode);
    }

    [Fact]
    public async Task UsingDeclarationForDriver_ReportsNothing()
    {
        string testCode = """
            using System;
            using System.Threading.Tasks;
            using WebDriverBiDi;

            namespace TestNamespace
            {
                public class TestClass
                {
                    public async Task TestMethod(string url)
                    {
                        await using BiDiDriver driver = new();
                        await driver.StartAsync(url);
                        await driver.Session.StatusAsync();
                    }
                }
            }
            """;

        await AnalyzerTestHelpers.VerifyAnalyzerAsync<BiDiDriver029_DriverUseAfterDisposalAnalyzer>(testCode);
    }

    [Fact]
    public async Task DisposedOrReboundInsideNestedFunction_ReportsNothing()
    {
        // A nested function runs when its delegate is invoked, so neither the dispose nor the rebinding
        // can be placed in this method's execution order.
        string testCode = """
            using System;
            using System.Threading.Tasks;
            using WebDriverBiDi;

            namespace TestNamespace
            {
                public class TestClass
                {
                    public async Task TestMethod(string url)
                    {
                        BiDiDriver disposedInLambda = new();
                        Func<ValueTask> release = async () => await disposedInLambda.DisposeAsync();
                        await release();
                        await disposedInLambda.StartAsync(url);

                        BiDiDriver reboundInLambda = new();
                        await reboundInLambda.DisposeAsync();
                        Action rebind = () => reboundInLambda = new BiDiDriver();
                        rebind();
                        await reboundInLambda.StartAsync(url);
                    }
                }
            }
            """;

        await AnalyzerTestHelpers.VerifyAnalyzerAsync<BiDiDriver029_DriverUseAfterDisposalAnalyzer>(testCode);
    }

    [Fact]
    public async Task UsedInsideNestedFunctionAfterDispose_ReportsNothing()
    {
        // The lambda body is not judged against the disposal state at the point the lambda is written.
        string testCode = """
            using System;
            using System.Threading.Tasks;
            using WebDriverBiDi;

            namespace TestNamespace
            {
                public class TestClass
                {
                    public async Task TestMethod(string url)
                    {
                        BiDiDriver driver = new();
                        await driver.DisposeAsync();
                        Func<Task> restart = async () => await driver.StartAsync(url);
                        await restart();
                    }
                }
            }
            """;

        await AnalyzerTestHelpers.VerifyAnalyzerAsync<BiDiDriver029_DriverUseAfterDisposalAnalyzer>(testCode);
    }

    [Fact]
    public async Task PassedByReference_ReportsNothing()
    {
        // A by-reference parameter lets the callee rebind the caller's variable, so the state is unknown.
        string testCode = """
            using System;
            using System.Threading.Tasks;
            using WebDriverBiDi;

            namespace TestNamespace
            {
                public class TestClass
                {
                    public void Replace(ref BiDiDriver driver)
                    {
                        driver = new BiDiDriver();
                    }

                    public void Increment(ref int value)
                    {
                        value++;
                    }

                    public async Task TestMethod(string url)
                    {
                        BiDiDriver driver = new();
                        int counter = 0;
                        this.Increment(ref counter);
                        await driver.DisposeAsync();
                        this.Replace(ref driver);
                        await driver.StartAsync(url);
                        Console.WriteLine(counter);
                    }
                }
            }
            """;

        await AnalyzerTestHelpers.VerifyAnalyzerAsync<BiDiDriver029_DriverUseAfterDisposalAnalyzer>(testCode);
    }

    [Fact]
    public async Task PassedByValueAfterDispose_ReportsError()
    {
        // Passing the driver by value cannot change which object the variable names, and nothing the
        // callee does can undo disposal, so an ordinary argument does not stop the tracking.
        string testCode = """
            using System;
            using System.Threading.Tasks;
            using WebDriverBiDi;

            namespace TestNamespace
            {
                public class TestClass
                {
                    public void Inspect(BiDiDriver driver)
                    {
                    }

                    public async Task TestMethod(string url)
                    {
                        BiDiDriver driver = new();
                        await driver.DisposeAsync();
                        this.Inspect(driver);
                        await {|#0:driver.StartAsync(url)|};
                    }
                }
            }
            """;

        await AnalyzerTestHelpers.VerifyAnalyzerAsync<BiDiDriver029_DriverUseAfterDisposalAnalyzer>(testCode, CreateExpected(0, "StartAsync"));
    }

    [Fact]
    public async Task NonDriverReceiversAndFieldDriver_ReportNothing()
    {
        // Only a local driver variable is tracked: a field reached through 'this' and a receiver of
        // another type are both left alone.
        string testCode = """
            using System;
            using System.Threading.Tasks;
            using WebDriverBiDi;

            namespace TestNamespace
            {
                public class TestClass
                {
                    private BiDiDriver driver = new();

                    public async Task TestMethod(string url)
                    {
                        BiDiDriver local = new();
                        string text = "value";
                        await local.DisposeAsync();
                        await this.driver.DisposeAsync();
                        await this.driver.StartAsync(url);
                        Console.WriteLine(text.ToUpperInvariant());
                        Console.WriteLine(nameof(local));
                    }
                }
            }
            """;

        await AnalyzerTestHelpers.VerifyAnalyzerAsync<BiDiDriver029_DriverUseAfterDisposalAnalyzer>(testCode);
    }

    [Fact]
    public async Task InvocationsBeforeAnyDriverDeclaration_ReportNothing()
    {
        string testCode = """
            using System;
            using System.Threading.Tasks;
            using WebDriverBiDi;

            namespace TestNamespace
            {
                public class TestClass
                {
                    public async Task TestMethod(string url)
                    {
                        Console.WriteLine("before");
                        BiDiDriver driver = new();
                        await driver.StartAsync(url);
                    }
                }
            }
            """;

        await AnalyzerTestHelpers.VerifyAnalyzerAsync<BiDiDriver029_DriverUseAfterDisposalAnalyzer>(testCode);
    }

    [Fact]
    public async Task CustomModuleCommandAfterDispose_ReportsError()
    {
        string testCode = """
            using System;
            using System.Threading.Tasks;
            using WebDriverBiDi;
            using TestApp;

            namespace TestNamespace
            {
                public class TestClass
                {
                    public async Task TestMethod()
                    {
                        CustomDriver driver = new();
                        await driver.DisposeAsync();
                        await {|#0:driver.Custom.SendAsync()|};
                    }
                }
            }
            """ + CustomDriverDeclarations;

        await AnalyzerTestHelpers.VerifyAnalyzerAsync<BiDiDriver029_DriverUseAfterDisposalAnalyzer>(testCode, CreateExpected(0, "SendAsync"));
    }

    [Fact]
    public async Task CustomModuleNonCommandMembersAfterDispose_ReportNothing()
    {
        // A module member that is not a command reaches no disposal guard: it neither ends in Async nor
        // returns a Task of a CommandResult.
        string testCode = """
            using System;
            using System.Threading.Tasks;
            using WebDriverBiDi;
            using TestApp;

            namespace TestNamespace
            {
                public class TestClass
                {
                    public async Task TestMethod()
                    {
                        CustomDriver driver = new();
                        await driver.DisposeAsync();
                        driver.Custom.DoWork();
                        int value = await driver.Custom.ComputeAsync();
                        Console.WriteLine(value);
                    }
                }
            }
            """ + CustomDriverDeclarations;

        await AnalyzerTestHelpers.VerifyAnalyzerAsync<BiDiDriver029_DriverUseAfterDisposalAnalyzer>(testCode);
    }

    [Fact]
    public async Task DisposingDriverMemberDoesNotDisposeDriver_ReportsNothing()
    {
        // The disposal that matters is the driver's own; disposing something the driver exposes is
        // neither a disposal of the driver nor, once the driver really is disposed, a use of it.
        string testCode = """
            using System;
            using System.Threading.Tasks;
            using WebDriverBiDi;
            using TestApp;

            namespace TestNamespace
            {
                public class TestClass
                {
                    public async Task TestMethod(string url)
                    {
                        CustomDriver driver = new();
                        await driver.Resource.DisposeAsync();
                        await driver.StartAsync(url);
                        await driver.DisposeAsync();
                        await driver.Resource.DisposeAsync();
                    }
                }
            }
            """ + CustomDriverDeclarations;

        await AnalyzerTestHelpers.VerifyAnalyzerAsync<BiDiDriver029_DriverUseAfterDisposalAnalyzer>(testCode);
    }

    [Fact]
    public async Task DisposeInElseBranchOnly_ReportsNothing()
    {
        string testCode = """
            using System;
            using System.Threading.Tasks;
            using WebDriverBiDi;

            namespace TestNamespace
            {
                public class TestClass
                {
                    public async Task TestMethod(string url, bool keepOpen)
                    {
                        BiDiDriver driver = new();
                        if (keepOpen)
                        {
                            Console.WriteLine("keeping");
                        }
                        else
                        {
                            await driver.DisposeAsync();
                        }

                        await driver.StartAsync(url);
                    }
                }
            }
            """;

        await AnalyzerTestHelpers.VerifyAnalyzerAsync<BiDiDriver029_DriverUseAfterDisposalAnalyzer>(testCode);
    }

    [Fact]
    public async Task LateBoundInvocation_ReportsNothing()
    {
        // A dynamic invocation resolves to no symbol, so there is nothing for the rule to classify.
        string testCode = """
            using System;
            using System.Threading.Tasks;
            using WebDriverBiDi;

            namespace TestNamespace
            {
                public class TestClass
                {
                    public async Task TestMethod(string url, dynamic helper)
                    {
                        BiDiDriver driver = new();
                        helper.Prepare();
                        await driver.StartAsync(url);
                    }
                }
            }
            """;

        await AnalyzerTestHelpers.VerifyAnalyzerAsync<BiDiDriver029_DriverUseAfterDisposalAnalyzer>(testCode);
    }

    [Fact]
    public async Task UseAfterDisposeInTopLevelProgram_ReportsError()
    {
        string testCode = """
            using System;
            using System.Threading.Tasks;
            using WebDriverBiDi;

            BiDiDriver driver = new();
            await driver.DisposeAsync();
            await {|#0:driver.StartAsync("ws://localhost:9222")|};
            """;

        RealAssemblyAnalyzerTest<BiDiDriver029_DriverUseAfterDisposalAnalyzer> testState = new()
        {
            TestCode = testCode,
            TestState = { OutputKind = OutputKind.ConsoleApplication },
        };
        testState.ExpectedDiagnostics.Add(CreateExpected(0, "StartAsync"));

        await testState.RunAsync(TestContext.Current.CancellationToken);
    }

    private static DiagnosticResult CreateExpected(int location, string methodName)
    {
        return new DiagnosticResult(
            BiDiDriver029_DriverUseAfterDisposalAnalyzer.DiagnosticId,
            DiagnosticSeverity.Error)
            .WithLocation(location)
            .WithArguments(methodName, "driver");
    }

    [Fact]
    public async Task ModuleCommandAfterDisposeInsideForLoop_ReportsNothing()
    {
        // A for loop's body may run zero times, so a disposal inside it does not make the driver
        // certainly disposed for the code after the loop.
        string testCode = """
            using System;
            using System.Threading.Tasks;
            using WebDriverBiDi;

            namespace TestNamespace
            {
                public class TestClass
                {
                    public async Task TestMethod(int count)
                    {
                        BiDiDriver driver = new();
                        for (int i = 0; i < count; i++)
                        {
                            await driver.DisposeAsync();
                            break;
                        }

                        await driver.Session.StatusAsync();
                    }
                }
            }
            """;

        await AnalyzerTestHelpers.VerifyAnalyzerAsync<BiDiDriver029_DriverUseAfterDisposalAnalyzer>(testCode);
    }

    [Fact]
    public async Task ModuleCommandAfterDisposeInsideForeachLoop_ReportsNothing()
    {
        string testCode = """
            using System;
            using System.Threading.Tasks;
            using WebDriverBiDi;

            namespace TestNamespace
            {
                public class TestClass
                {
                    public async Task TestMethod(string[] urls)
                    {
                        BiDiDriver driver = new();
                        foreach (string url in urls)
                        {
                            await driver.DisposeAsync();
                            break;
                        }

                        await driver.Session.StatusAsync();
                    }
                }
            }
            """;

        await AnalyzerTestHelpers.VerifyAnalyzerAsync<BiDiDriver029_DriverUseAfterDisposalAnalyzer>(testCode);
    }

    [Fact]
    public async Task ModuleCommandAfterDisposeInsideLoopBody_ReportsError()
    {
        // Within one execution of the body the disposal is certain, so a use later in the same body is
        // reported exactly as it would be in straight-line code.
        string testCode = """
            using System;
            using System.Threading.Tasks;
            using WebDriverBiDi;

            namespace TestNamespace
            {
                public class TestClass
                {
                    public async Task TestMethod(bool flag)
                    {
                        BiDiDriver driver = new();
                        while (flag)
                        {
                            await driver.DisposeAsync();
                            await {|#0:driver.Session.StatusAsync()|};
                        }
                    }
                }
            }
            """;

        DiagnosticResult expected = new DiagnosticResult(
            BiDiDriver029_DriverUseAfterDisposalAnalyzer.DiagnosticId,
            DiagnosticSeverity.Error)
            .WithLocation(0)
            .WithArguments("StatusAsync", "driver");

        await AnalyzerTestHelpers.VerifyAnalyzerAsync<BiDiDriver029_DriverUseAfterDisposalAnalyzer>(testCode, expected);
    }

    [Fact]
    public async Task ModuleCommandAfterDisposeBeforeLoop_ReportsError()
    {
        // A driver already disposed before the loop stays disposed whether or not the body runs.
        string testCode = """
            using System;
            using System.Threading.Tasks;
            using WebDriverBiDi;

            namespace TestNamespace
            {
                public class TestClass
                {
                    public async Task TestMethod(string[] urls)
                    {
                        BiDiDriver driver = new();
                        await driver.DisposeAsync();
                        foreach (string url in urls)
                        {
                            Console.WriteLine(url);
                        }

                        await {|#0:driver.Session.StatusAsync()|};
                    }
                }
            }
            """;

        DiagnosticResult expected = new DiagnosticResult(
            BiDiDriver029_DriverUseAfterDisposalAnalyzer.DiagnosticId,
            DiagnosticSeverity.Error)
            .WithLocation(0)
            .WithArguments("StatusAsync", "driver");

        await AnalyzerTestHelpers.VerifyAnalyzerAsync<BiDiDriver029_DriverUseAfterDisposalAnalyzer>(testCode, expected);
    }

    /// <summary>
    /// Tests that a command called through a local holding one of the driver's modules is reported
    /// after the driver has been disposed.
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous unit test.</returns>
    [Fact]
    public async Task CommandThroughModuleAlias_AfterDisposal_ReportsError()
    {
        string testCode = """
            using System.Threading.Tasks;
            using WebDriverBiDi;
            using WebDriverBiDi.BrowsingContext;

            namespace TestApp
            {
                public class TestClass
                {
                    public async Task TestMethod()
                    {
                        BiDiDriver driver = new();
                        BrowsingContextModule context = driver.BrowsingContext;
                        await driver.StartAsync("ws://localhost:9222");
                        await context.GetTreeAsync();
                        await driver.DisposeAsync();
                        await {|#0:context.GetTreeAsync()|};
                    }
                }
            }
            """;

        RealAssemblyAnalyzerTest<BiDiDriver029_DriverUseAfterDisposalAnalyzer> testState = new()
        {
            TestCode = testCode,
        };

        testState.ExpectedDiagnostics.Add(new DiagnosticResult(
            BiDiDriver029_DriverUseAfterDisposalAnalyzer.DiagnosticId,
            DiagnosticSeverity.Error)
            .WithLocation(0)
            .WithArguments("GetTreeAsync", "driver"));

        await testState.RunAsync(TestContext.Current.CancellationToken);
    }

    [Fact]
    public async Task UseAfterTryWhoseFinallyDisposes_ReportsError()
    {
        // A finally runs however the try ends, so its DisposeAsync certainly leaves the driver disposed for the use that follows.
        string testCode = """
            using System;
            using System.Threading.Tasks;
            using WebDriverBiDi;

            namespace TestNamespace
            {
                public class TestClass
                {
                    public async Task TestMethod(string url)
                    {
                        BiDiDriver driver = new();
                        try
                        {
                            await driver.StartAsync(url);
                        }
                        finally
                        {
                            await driver.DisposeAsync();
                        }

                        await {|#0:driver.Session.StatusAsync()|};
                    }
                }
            }
            """;

        await AnalyzerTestHelpers.VerifyAnalyzerAsync<BiDiDriver029_DriverUseAfterDisposalAnalyzer>(testCode, CreateExpected(0, "StatusAsync"));
    }

    [Fact]
    public async Task UseInFinallyAfterDisposeInTry_ReportsNothing()
    {
        // The finally may run after an exception thrown before the DisposeAsync in the try, so the use in it is not certain to follow disposal.
        string testCode = """
            using System;
            using System.Threading.Tasks;
            using WebDriverBiDi;

            namespace TestNamespace
            {
                public class TestClass
                {
                    public async Task TestMethod(string url)
                    {
                        BiDiDriver driver = new();
                        try
                        {
                            await driver.StartAsync(url);
                            await driver.DisposeAsync();
                        }
                        finally
                        {
                            await driver.Session.StatusAsync();
                        }
                    }
                }
            }
            """;

        await AnalyzerTestHelpers.VerifyAnalyzerAsync<BiDiDriver029_DriverUseAfterDisposalAnalyzer>(testCode);
    }

    [Fact]
    public async Task UseAfterConditionalExpressionsThatMayDispose_ReportsNothing()
    {
        // Only one arm of a conditional expression runs, so a dispose in either arm may not have run.
        string testCode = """
            using System;
            using System.Threading.Tasks;
            using WebDriverBiDi;

            namespace TestNamespace
            {
                public class TestClass
                {
                    public async Task TestMethod(string url, bool retry, int mode)
                    {
                        BiDiDriver driver = new();
                        await (retry ? driver.DisposeAsync().AsTask() : Task.CompletedTask);
                        await (retry ? Task.CompletedTask : driver.DisposeAsync().AsTask());
                        await driver.Session.StatusAsync();
                    }
                }
            }
            """;

        await AnalyzerTestHelpers.VerifyAnalyzerAsync<BiDiDriver029_DriverUseAfterDisposalAnalyzer>(testCode);
    }

    [Fact]
    public async Task UseAfterSwitchExpressionThatDisposesInEveryArm_ReportsError()
    {
        // Every arm of the switch expression disposes the driver, including the one guarded by a when clause, so it is disposed on every path.
        string testCode = """
            using System;
            using System.Threading.Tasks;
            using WebDriverBiDi;

            namespace TestNamespace
            {
                public class TestClass
                {
                    public async Task TestMethod(string url, bool retry, int mode)
                    {
                        BiDiDriver driver = new();
                        await (mode switch
                        {
                            1 when retry => driver.DisposeAsync().AsTask(),
                            _ => driver.DisposeAsync().AsTask(),
                        });
                        await {|#0:driver.Session.StatusAsync()|};
                    }
                }
            }
            """;

        await AnalyzerTestHelpers.VerifyAnalyzerAsync<BiDiDriver029_DriverUseAfterDisposalAnalyzer>(testCode, CreateExpected(0, "StatusAsync"));
    }

    [Fact]
    public async Task UseAfterEmptySwitchExpression_ReportsError()
    {
        // A switch expression with no arms always throws, so it adds no path and leaves the driver disposed.
        string testCode = """
            using System;
            using System.Threading.Tasks;
            using WebDriverBiDi;

            namespace TestNamespace
            {
                public class TestClass
                {
                    public async Task TestMethod(string url, bool retry, int mode)
                    {
                        BiDiDriver driver = new();
                        await driver.DisposeAsync();
                        #pragma warning disable CS8509
                        Task pending = mode switch { };
                        #pragma warning restore CS8509
                        await pending;
                        await {|#0:driver.Session.StatusAsync()|};
                    }
                }
            }
            """;

        await AnalyzerTestHelpers.VerifyAnalyzerAsync<BiDiDriver029_DriverUseAfterDisposalAnalyzer>(testCode, CreateExpected(0, "StatusAsync"));
    }

    [Fact]
    public async Task UseAfterCoalesceThatMayDispose_ReportsNothing()
    {
        // The right operand of ?? runs only when the left is null, so the dispose in it may not have run.
        string testCode = """
            using System;
            using System.Threading.Tasks;
            using WebDriverBiDi;

            namespace TestNamespace
            {
                public class TestClass
                {
                    public async Task TestMethod(string url, bool retry, int mode)
                    {
                        BiDiDriver driver = new();
                        Task pending = null;
                        await (pending ?? driver.DisposeAsync().AsTask());
                        await driver.Session.StatusAsync();
                    }
                }
            }
            """;

        await AnalyzerTestHelpers.VerifyAnalyzerAsync<BiDiDriver029_DriverUseAfterDisposalAnalyzer>(testCode);
    }

    [Fact]
    public async Task UseAfterCoalesceAssignmentThatMayDispose_ReportsNothing()
    {
        // The right operand of ??= runs only when the variable is null, so the dispose in it may not have run.
        string testCode = """
            using System;
            using System.Threading.Tasks;
            using WebDriverBiDi;

            namespace TestNamespace
            {
                public class TestClass
                {
                    public async Task TestMethod(string url, bool retry, int mode)
                    {
                        BiDiDriver driver = new();
                        Task pending = null;
                        pending ??= driver.DisposeAsync().AsTask();
                        await pending;
                        await driver.Session.StatusAsync();
                    }
                }
            }
            """;

        await AnalyzerTestHelpers.VerifyAnalyzerAsync<BiDiDriver029_DriverUseAfterDisposalAnalyzer>(testCode);
    }

    [Fact]
    public async Task UseAfterCoalesceFollowingDisposal_ReportsError()
    {
        // A ?? that does not rebind the driver leaves a driver disposed before it disposed afterwards.
        string testCode = """
            using System;
            using System.Threading.Tasks;
            using WebDriverBiDi;

            namespace TestNamespace
            {
                public class TestClass
                {
                    public async Task TestMethod(string url, bool retry, int mode)
                    {
                        BiDiDriver driver = new();
                        await driver.DisposeAsync();
                        Task pending = null;
                        await (pending ?? Task.CompletedTask);
                        await {|#0:driver.Session.StatusAsync()|};
                    }
                }
            }
            """;

        await AnalyzerTestHelpers.VerifyAnalyzerAsync<BiDiDriver029_DriverUseAfterDisposalAnalyzer>(testCode, CreateExpected(0, "StatusAsync"));
    }
}
