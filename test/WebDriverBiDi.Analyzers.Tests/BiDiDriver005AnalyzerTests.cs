// <copyright file="BiDiDriver005AnalyzerTests.cs" company="WebDriverBiDi.NET Committers">
// Copyright (c) WebDriverBiDi.NET Committers. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.
// </copyright>

namespace WebDriverBiDi.Analyzers.Tests;

using System.Collections.Immutable;
using System.IO;
using System.Threading.Tasks;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Testing;
using Microsoft.CodeAnalysis.Testing;

/// <summary>
/// Tests for the BiDiDriver005 analyzer that detects missing Session.SubscribeAsync calls.
/// </summary>
public class BiDiDriver005AnalyzerTests
{
    /// <summary>
    /// Tests that AddObserver without SubscribeAsync reports a warning.
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous unit test.</returns>
    [Fact]
    public async Task AddObserver_WithoutSubscribeAsync_ReportsWarning()
    {
        string test = """
            using System;
            using WebDriverBiDi;
            using System.Threading.Tasks;

            namespace TestApp
            {
                public class TestClass
                {
                    public async Task TestMethod()
                    {
                        BiDiDriver driver = new BiDiDriver(TimeSpan.FromSeconds(30));
                        {|#0:driver.Log.OnEntryAdded.AddObserver(async (e) => { })|};
                    }
                }
            }
            """;

        DiagnosticResult expected = new DiagnosticResult(BiDiDriver005_MissingEventSubscriptionAnalyzer.DiagnosticId, Microsoft.CodeAnalysis.DiagnosticSeverity.Warning)
            .WithLocation(0)
            .WithArguments("log.entryAdded");

        RealAssemblyAnalyzerTest<BiDiDriver005_MissingEventSubscriptionAnalyzer> testState = new()
        {
            TestCode = test,
        };
        testState.ExpectedDiagnostics.Add(expected);

        await testState.RunAsync(TestContext.Current.CancellationToken);
    }

    /// <summary>
    /// Tests that AddObserver with SubscribeAsync does not report a diagnostic.
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous unit test.</returns>
    [Fact]
    public async Task AddObserver_WithSubscribeAsync_NoDiagnostic()
    {
        string test = """
            using System;
            using WebDriverBiDi;
            using WebDriverBiDi.Session;
            using System.Threading.Tasks;

            namespace TestApp
            {
                public class TestClass
                {
                    public async Task TestMethod()
                    {
                        BiDiDriver driver = new BiDiDriver(TimeSpan.FromSeconds(30));
                        driver.Log.OnEntryAdded.AddObserver(async (e) => { });
                        await driver.Session.SubscribeAsync(new SubscribeCommandParameters(new[] { "log.entryAdded" }));
                    }
                }
            }
            """;

        RealAssemblyAnalyzerTest<BiDiDriver005_MissingEventSubscriptionAnalyzer> testState = new()
        {
            TestCode = test,
        };

        await testState.RunAsync(TestContext.Current.CancellationToken);
    }

    /// <summary>
    /// Tests that AddObserver on driver internal events does not report a diagnostic.
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous unit test.</returns>
    [Fact]
    public async Task AddObserver_OnDriverInternalEvent_NoDiagnostic()
    {
        // The driver-level OnLogMessage event is exposed directly on the driver rather than through a
        // module property, so BIDI005 does not treat it as a protocol event needing a subscription.
        string test = """
            using System;
            using WebDriverBiDi;
            using System.Threading.Tasks;

            namespace TestApp
            {
                public class TestClass
                {
                    public async Task TestMethod()
                    {
                        BiDiDriver driver = new BiDiDriver(TimeSpan.FromSeconds(30));
                        driver.OnLogMessage.AddObserver(async (e) => { });
                    }
                }
            }
            """;

        RealAssemblyAnalyzerTest<BiDiDriver005_MissingEventSubscriptionAnalyzer> testState = new()
        {
            TestCode = test,
        };

        await testState.RunAsync(TestContext.Current.CancellationToken);
    }

    /// <summary>
    /// Tests that multiple AddObserver calls without SubscribeAsync report warnings.
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous unit test.</returns>
    [Fact]
    public async Task MultipleAddObserver_WithoutSubscribeAsync_ReportsMultipleWarnings()
    {
        string test = """
            using System;
            using WebDriverBiDi;
            using System.Threading.Tasks;

            namespace TestApp
            {
                public class TestClass
                {
                    public async Task TestMethod()
                    {
                        BiDiDriver driver = new BiDiDriver(TimeSpan.FromSeconds(30));
                        {|#0:driver.Log.OnEntryAdded.AddObserver(async (e) => { })|};
                        {|#1:driver.Network.OnBeforeRequestSent.AddObserver(async (e) => { })|};
                    }
                }
            }
            """;

        DiagnosticResult expected1 = new DiagnosticResult(BiDiDriver005_MissingEventSubscriptionAnalyzer.DiagnosticId, Microsoft.CodeAnalysis.DiagnosticSeverity.Warning)
            .WithLocation(0)
            .WithArguments("log.entryAdded");

        DiagnosticResult expected2 = new DiagnosticResult(BiDiDriver005_MissingEventSubscriptionAnalyzer.DiagnosticId, Microsoft.CodeAnalysis.DiagnosticSeverity.Warning)
            .WithLocation(1)
            .WithArguments("network.beforeRequestSent");

        RealAssemblyAnalyzerTest<BiDiDriver005_MissingEventSubscriptionAnalyzer> testState = new()
        {
            TestCode = test,
        };
        testState.ExpectedDiagnostics.Add(expected1);
        testState.ExpectedDiagnostics.Add(expected2);

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
            using WebDriverBiDi;
            using System.Threading.Tasks;

            namespace TestApp
            {
                public abstract class TestClass
                {
                    public abstract Task TestMethod();
                }
            }
            """;

        RealAssemblyAnalyzerTest<BiDiDriver005_MissingEventSubscriptionAnalyzer> testState = new()
        {
            TestCode = test,
        };

        await testState.RunAsync(TestContext.Current.CancellationToken);
    }

    /// <summary>
    /// Tests that AddObserver on non-module property does not report a diagnostic.
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous unit test.</returns>
    [Fact]
    public async Task AddObserver_OnNonModuleProperty_NoDiagnostic()
    {
        // OnEventReceived is an ObservableEvent exposed directly on the driver, not reached through a
        // module property, so BIDI005 does not require a matching Session.SubscribeAsync call.
        string test = """
            using System;
            using WebDriverBiDi;
            using System.Threading.Tasks;

            namespace TestApp
            {
                public class TestClass
                {
                    public async Task TestMethod()
                    {
                        BiDiDriver driver = new BiDiDriver(TimeSpan.FromSeconds(30));
                        driver.OnEventReceived.AddObserver(async (e) => { });
                    }
                }
            }
            """;

        RealAssemblyAnalyzerTest<BiDiDriver005_MissingEventSubscriptionAnalyzer> testState = new()
        {
            TestCode = test,
        };

        await testState.RunAsync(TestContext.Current.CancellationToken);
    }

    /// <summary>
    /// Tests that AddObserver on non-ObservableEvent does not report a diagnostic.
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous unit test.</returns>
    [Fact]
    public async Task AddObserver_OnNonObservableEvent_NoDiagnostic()
    {
        // SYNTHETIC: keeps a hand-written stub. This exercises the analyzer's "receiver type name is
        // not ObservableEvent" branch with a custom CustomEvent type that has an AddObserver method but
        // is not an ObservableEvent. Every real module event is a genuine ObservableEvent<T>, so this
        // name-only lookalike cannot be reproduced against the real assembly.
        string test = """
            using System;
            using System.Threading.Tasks;

            namespace WebDriverBiDi
            {
                public interface IBiDiDriver { }

                public class BiDiDriver : IBiDiDriver
                {
                    public BiDiDriver(TimeSpan timeout) { }
                    public CustomModule Custom { get; } = new CustomModule();
                }

                public class CustomModule
                {
                    public CustomEvent OnCustomEvent { get; } = new CustomEvent();
                }

                public class CustomEvent
                {
                    public void AddObserver(Func<object, Task> handler) { }
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
                        driver.Custom.OnCustomEvent.AddObserver(async (e) => { });
                    }
                }
            }
            """;

        CSharpAnalyzerTest<BiDiDriver005_MissingEventSubscriptionAnalyzer, DefaultVerifier> testState = new()
        {
            TestCode = test,
            ReferenceAssemblies = ReferenceAssemblies.Net.Net80,
        };

        await testState.RunAsync(TestContext.Current.CancellationToken);
    }

    /// <summary>
    /// Tests that AddObserver on non-BiDiDriver variable does not report a diagnostic.
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous unit test.</returns>
    [Fact]
    public async Task AddObserver_OnNonDriverVariable_NoDiagnostic()
    {
        // The AddObserver receiver is rooted at a LogModule variable rather than at a driver (command
        // executor), so BIDI005 does not attribute the event to a driver and reports nothing.
        string test = """
            using System;
            using System;
            using WebDriverBiDi;
            using System.Threading.Tasks;

            namespace TestApp
            {
                public class TestClass
                {
                    public async Task TestMethod()
                    {
                        var log = new BiDiDriver(TimeSpan.FromSeconds(30)).Log;
                        log.OnEntryAdded.AddObserver(async (e) => { });
                    }
                }
            }
            """;

        RealAssemblyAnalyzerTest<BiDiDriver005_MissingEventSubscriptionAnalyzer> testState = new()
        {
            TestCode = test,
        };

        await testState.RunAsync(TestContext.Current.CancellationToken);
    }

    /// <summary>
    /// Tests that AddObserver with wrong event name in SubscribeAsync reports a warning.
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous unit test.</returns>
    [Fact]
    public async Task AddObserver_WithWrongEventNameInSubscribe_ReportsWarning()
    {
        string test = """
            using System;
            using WebDriverBiDi;
            using WebDriverBiDi.Session;
            using System.Threading.Tasks;

            namespace TestApp
            {
                public class TestClass
                {
                    public async Task TestMethod()
                    {
                        BiDiDriver driver = new BiDiDriver(TimeSpan.FromSeconds(30));
                        {|#0:driver.Log.OnEntryAdded.AddObserver(async (e) => { })|};
                        // Wrong event name in subscription
                        await driver.Session.SubscribeAsync(new SubscribeCommandParameters(new[] { "network.beforeRequestSent" }));
                    }
                }
            }
            """;

        DiagnosticResult expected = new DiagnosticResult(BiDiDriver005_MissingEventSubscriptionAnalyzer.DiagnosticId, Microsoft.CodeAnalysis.DiagnosticSeverity.Warning)
            .WithLocation(0)
            .WithArguments("log.entryAdded");

        RealAssemblyAnalyzerTest<BiDiDriver005_MissingEventSubscriptionAnalyzer> testState = new()
        {
            TestCode = test,
        };
        testState.ExpectedDiagnostics.Add(expected);

        await testState.RunAsync(TestContext.Current.CancellationToken);
    }

    /// <summary>
    /// Tests that multiple events with partial subscription reports warnings for unsubscribed events.
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous unit test.</returns>
    [Fact]
    public async Task MultipleAddObserver_WithPartialSubscribe_ReportsWarningForUnsubscribed()
    {
        string test = """
            using System;
            using WebDriverBiDi;
            using WebDriverBiDi.Session;
            using System.Threading.Tasks;

            namespace TestApp
            {
                public class TestClass
                {
                    public async Task TestMethod()
                    {
                        BiDiDriver driver = new BiDiDriver(TimeSpan.FromSeconds(30));
                        driver.Log.OnEntryAdded.AddObserver(async (e) => { });
                        driver.Network.OnBeforeRequestSent.AddObserver(async (e) => { });
                        {|#0:driver.Network.OnResponseCompleted.AddObserver(async (e) => { })|};
                        // Only subscribed to log.entryAdded and network.beforeRequestSent, missing network.responseCompleted
                        await driver.Session.SubscribeAsync(new SubscribeCommandParameters(new[] { "log.entryAdded", "network.beforeRequestSent" }));
                    }
                }
            }
            """;

        DiagnosticResult expected = new DiagnosticResult(BiDiDriver005_MissingEventSubscriptionAnalyzer.DiagnosticId, Microsoft.CodeAnalysis.DiagnosticSeverity.Warning)
            .WithLocation(0)
            .WithArguments("network.responseCompleted");

        RealAssemblyAnalyzerTest<BiDiDriver005_MissingEventSubscriptionAnalyzer> testState = new()
        {
            TestCode = test,
        };
        testState.ExpectedDiagnostics.Add(expected);

        await testState.RunAsync(TestContext.Current.CancellationToken);
    }

    /// <summary>
    /// Tests that AddObserver with array creation syntax in SubscribeAsync works correctly.
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous unit test.</returns>
    [Fact]
    public async Task AddObserver_WithArrayCreationSyntax_NoDiagnostic()
    {
        string test = """
            using System;
            using WebDriverBiDi;
            using WebDriverBiDi.Session;
            using System.Threading.Tasks;

            namespace TestApp
            {
                public class TestClass
                {
                    public async Task TestMethod()
                    {
                        BiDiDriver driver = new BiDiDriver(TimeSpan.FromSeconds(30));
                        driver.Log.OnEntryAdded.AddObserver(async (e) => { });
                        // Using explicit array creation
                        await driver.Session.SubscribeAsync(new SubscribeCommandParameters(new string[] { "log.entryAdded" }));
                    }
                }
            }
            """;

        RealAssemblyAnalyzerTest<BiDiDriver005_MissingEventSubscriptionAnalyzer> testState = new()
        {
            TestCode = test,
        };

        await testState.RunAsync(TestContext.Current.CancellationToken);
    }

    /// <summary>
    /// Tests that AddObserver with C# 12 collection expression syntax in SubscribeAsync does not report a diagnostic.
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous unit test.</returns>
    [Fact]
    public async Task AddObserver_WithCollectionExpressionSyntax_NoDiagnostic()
    {
        string test = """
            using System;
            using WebDriverBiDi;
            using WebDriverBiDi.Session;
            using System.Threading.Tasks;

            namespace TestApp
            {
                public class TestClass
                {
                    public async Task TestMethod()
                    {
                        BiDiDriver driver = new BiDiDriver(TimeSpan.FromSeconds(30));
                        driver.Log.OnEntryAdded.AddObserver(async (e) => { });
                        // Using C# 12 collection expression syntax
                        await driver.Session.SubscribeAsync(new SubscribeCommandParameters(["log.entryAdded"]));
                    }
                }
            }
            """;

        RealAssemblyAnalyzerTest<BiDiDriver005_MissingEventSubscriptionAnalyzer> testState = new()
        {
            TestCode = test,
        };

        await testState.RunAsync(TestContext.Current.CancellationToken);
    }

    /// <summary>
    /// Tests that AddObserver with C# 12 collection expression syntax and a wrong event name reports a warning.
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous unit test.</returns>
    [Fact]
    public async Task AddObserver_WithCollectionExpressionSyntax_WrongEvent_ReportsWarning()
    {
        string test = """
            using System;
            using WebDriverBiDi;
            using WebDriverBiDi.Session;
            using System.Threading.Tasks;

            namespace TestApp
            {
                public class TestClass
                {
                    public async Task TestMethod()
                    {
                        BiDiDriver driver = new BiDiDriver(TimeSpan.FromSeconds(30));
                        {|#0:driver.Log.OnEntryAdded.AddObserver(async (e) => { })|};
                        // Wrong event name in collection expression
                        await driver.Session.SubscribeAsync(new SubscribeCommandParameters(["network.beforeRequestSent"]));
                    }
                }
            }
            """;

        DiagnosticResult expected = new DiagnosticResult(BiDiDriver005_MissingEventSubscriptionAnalyzer.DiagnosticId, Microsoft.CodeAnalysis.DiagnosticSeverity.Warning)
            .WithLocation(0)
            .WithArguments("log.entryAdded");

        RealAssemblyAnalyzerTest<BiDiDriver005_MissingEventSubscriptionAnalyzer> testState = new()
        {
            TestCode = test,
        };
        testState.ExpectedDiagnostics.Add(expected);

        await testState.RunAsync(TestContext.Current.CancellationToken);
    }

    /// <summary>
    /// Tests that BIDI005 fires correctly when WebDriverBiDi types are metadata-backed
    /// (i.e., referenced as a compiled assembly rather than defined in source).
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous unit test.</returns>
    [Fact]
    public async Task AddObserver_WithoutSubscribeAsync_MetadataBacked_ReportsWarning()
    {
        string test = """
            using System;
            using System.Threading.Tasks;

            namespace TestApp
            {
                public class TestClass
                {
                    public async Task TestMethod(WebDriverBiDi.BiDiDriver driver)
                    {
                        {|#0:driver.Log.OnEntryAdded.AddObserver(async (e) => { })|};
                    }
                }
            }
            """;

        DiagnosticResult expected = new DiagnosticResult(BiDiDriver005_MissingEventSubscriptionAnalyzer.DiagnosticId, DiagnosticSeverity.Warning)
            .WithLocation(0)
            .WithArguments("log.entryAdded");

        CSharpAnalyzerTest<BiDiDriver005_MissingEventSubscriptionAnalyzer, DefaultVerifier> testState = new()
        {
            TestCode = test,
            ReferenceAssemblies = ReferenceAssemblies.Net.Net80,
        };
        testState.TestState.AdditionalReferences.Add(await CreateFakeLibMetadataReference());
        testState.ExpectedDiagnostics.Add(expected);

        await testState.RunAsync(TestContext.Current.CancellationToken);
    }

    /// <summary>
    /// Tests that BIDI005 does not fire when SubscribeAsync uses .EventName property access instead of a string literal.
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous unit test.</returns>
    [Fact]
    public async Task AddObserver_WithSubscribeAsync_EventNamePropertyAccess_NoDiagnostic()
    {
        string test = """
            using System;
            using WebDriverBiDi;
            using WebDriverBiDi.Session;
            using System.Threading.Tasks;

            namespace TestApp
            {
                public class TestClass
                {
                    public async Task TestMethod()
                    {
                        BiDiDriver driver = new BiDiDriver(TimeSpan.FromSeconds(30));
                        driver.Log.OnEntryAdded.AddObserver(async (e) => { });
                        await driver.Session.SubscribeAsync(new SubscribeCommandParameters(new[] { driver.Log.OnEntryAdded.EventName }));
                    }
                }
            }
            """;

        RealAssemblyAnalyzerTest<BiDiDriver005_MissingEventSubscriptionAnalyzer> testState = new()
        {
            TestCode = test,
        };

        await testState.RunAsync(TestContext.Current.CancellationToken);
    }

    /// <summary>
    /// Tests that BIDI005 does not fire when SubscribeAsync uses .EventName property access, with metadata-backed types.
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous unit test.</returns>
    [Fact]
    public async Task AddObserver_WithSubscribeAsync_EventNamePropertyAccess_MetadataBacked_NoDiagnostic()
    {
        string test = """
            using System;
            using System.Threading.Tasks;

            namespace TestApp
            {
                public class TestClass
                {
                    public async Task TestMethod(WebDriverBiDi.BiDiDriver driver)
                    {
                        driver.Log.OnEntryAdded.AddObserver(async (e) => { });
                        await driver.Session.SubscribeAsync(new WebDriverBiDi.SubscribeCommandParameters(new[] { driver.Log.OnEntryAdded.EventName }));
                    }
                }
            }
            """;

        CSharpAnalyzerTest<BiDiDriver005_MissingEventSubscriptionAnalyzer, DefaultVerifier> testState = new()
        {
            TestCode = test,
            ReferenceAssemblies = ReferenceAssemblies.Net.Net80,
        };
        testState.TestState.AdditionalReferences.Add(await CreateFakeLibMetadataReference());

        await testState.RunAsync(TestContext.Current.CancellationToken);
    }

    /// <summary>
    /// Tests that BIDI005 does not fire when SubscribeAsync is present, with metadata-backed types.
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous unit test.</returns>
    [Fact]
    public async Task AddObserver_WithSubscribeAsync_MetadataBacked_NoDiagnostic()
    {
        string test = """
            using System;
            using System.Threading.Tasks;

            namespace TestApp
            {
                public class TestClass
                {
                    public async Task TestMethod(WebDriverBiDi.BiDiDriver driver)
                    {
                        driver.Log.OnEntryAdded.AddObserver(async (e) => { });
                        await driver.Session.SubscribeAsync(new WebDriverBiDi.SubscribeCommandParameters(new[] { "log.entryAdded" }));
                    }
                }
            }
            """;

        CSharpAnalyzerTest<BiDiDriver005_MissingEventSubscriptionAnalyzer, DefaultVerifier> testState = new()
        {
            TestCode = test,
            ReferenceAssemblies = ReferenceAssemblies.Net.Net80,
        };
        testState.TestState.AdditionalReferences.Add(await CreateFakeLibMetadataReference());

        await testState.RunAsync(TestContext.Current.CancellationToken);
    }

    /// <summary>
    /// Tests SupportedDiagnostics property.
    /// </summary>
    [Fact]
    public void SupportedDiagnostics_ContainsBIDI005()
    {
        BiDiDriver005_MissingEventSubscriptionAnalyzer analyzer = new();
        System.Collections.Immutable.ImmutableArray<DiagnosticDescriptor> diagnostics = analyzer.SupportedDiagnostics;

        Assert.Single(diagnostics);
        Assert.Equal(BiDiDriver005_MissingEventSubscriptionAnalyzer.DiagnosticId, diagnostics[0].Id);
    }

    /// <summary>
    /// Tests that AddObserver called via non-member-access invocation is handled gracefully.
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous unit test.</returns>
    [Fact]
    public async Task AddObserver_ViaDelegate_NoDiagnostic()
    {
        // SYNTHETIC: keeps a hand-written stub. It captures AddObserver as a method group into a
        // single-parameter delegate. The real ObservableEvent<T>.AddObserver has additional optional
        // parameters, so a method-group conversion to a one-argument delegate does not bind, and the
        // real ObservableEvent<T> constructor is protected (no standalone instance can be created), so
        // this non-member-access invocation cannot be reproduced against the real assembly.
        string test = """
            using System;
            using System.Threading.Tasks;

            namespace WebDriverBiDi
            {
                public class WebDriverBiDiEventArgs { }
                public class EntryAddedEventArgs : WebDriverBiDiEventArgs { }

                public class ObservableEvent<T> where T : WebDriverBiDiEventArgs
                {
                    public ObservableEvent(string eventName) { EventName = eventName; }
                    public string EventName { get; }
                    public EventObserver<T> AddObserver(Func<T, Task> handler) => null!;
                }

                public class EventObserver<T> where T : WebDriverBiDiEventArgs
                {
                    public void Dispose() { }
                }
            }

            namespace TestApp
            {
                using WebDriverBiDi;

                public class TestClass
                {
                    public async Task TestMethod()
                    {
                        ObservableEvent<EntryAddedEventArgs> evt = new ObservableEvent<EntryAddedEventArgs>("test.event");
                        Func<Func<EntryAddedEventArgs, Task>, EventObserver<EntryAddedEventArgs>> addObserverFunc = evt.AddObserver;
                        addObserverFunc(async (e) => { });
                    }
                }
            }
            """;

        CSharpAnalyzerTest<BiDiDriver005_MissingEventSubscriptionAnalyzer, DefaultVerifier> testState = new()
        {
            TestCode = test,
            ReferenceAssemblies = ReferenceAssemblies.Net.Net80,
        };

        await testState.RunAsync(TestContext.Current.CancellationToken);
    }

    /// <summary>
    /// Tests that unresolved AddObserver method is handled gracefully.
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous unit test.</returns>
    [Fact]
    public async Task AddObserver_UnresolvedMethod_NoDiagnostic()
    {
        // SYNTHETIC: keeps a hand-written stub. It defines an ObservableEvent<T> with no AddObserver
        // method so the call binds to nothing (CS1061). The real ObservableEvent<T> always defines
        // AddObserver, so an unresolved AddObserver cannot be produced against the real assembly.
        string test = """
            using System;
            using System.Threading.Tasks;

            namespace WebDriverBiDi
            {
                public interface IBiDiDriver { }

                public class BiDiDriver : IBiDiDriver
                {
                    public BiDiDriver(TimeSpan timeout) { }
                    public LogModule Log { get; } = new LogModule();
                }

                public class LogModule
                {
                    [ObservableEventName("log.entryAdded")]
                    public ObservableEvent<EntryAddedEventArgs> OnEntryAdded { get; } = new ObservableEvent<EntryAddedEventArgs>("log.entryAdded");
                }

                public class WebDriverBiDiEventArgs { }
                public class EntryAddedEventArgs : WebDriverBiDiEventArgs { }

                public class ObservableEvent<T> where T : WebDriverBiDiEventArgs
                {
                    public ObservableEvent(string eventName) { EventName = eventName; }
                    public string EventName { get; }
                }

                [System.AttributeUsage(System.AttributeTargets.Property)]
                public sealed class ObservableEventNameAttribute : System.Attribute
                {
                    public ObservableEventNameAttribute(string eventName) { EventName = eventName; }
                    public string EventName { get; }
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
                        driver.Log.OnEntryAdded.{|CS1061:AddObserver|}(async (e) => { });
                    }
                }
            }
            """;

        CSharpAnalyzerTest<BiDiDriver005_MissingEventSubscriptionAnalyzer, DefaultVerifier> testState = new()
        {
            TestCode = test,
            ReferenceAssemblies = ReferenceAssemblies.Net.Net80,
        };

        await testState.RunAsync(TestContext.Current.CancellationToken);
    }

    /// <summary>
    /// Tests that AddObserver on expression with null type is handled gracefully.
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous unit test.</returns>
    [Fact]
    public async Task AddObserver_OnNullTypeExpression_NoDiagnostic()
    {
        // SYNTHETIC: keeps a hand-written stub. LogModule.OnEntryAdded is typed as object, so
        // AddObserver does not resolve (CS1061). The real Log module event is a genuine
        // ObservableEvent<T> that defines AddObserver, so this cannot be reproduced against the
        // real assembly.
        string test = """
            using System;
            using System.Threading.Tasks;

            namespace WebDriverBiDi
            {
                public interface IBiDiDriver { }

                public class BiDiDriver : IBiDiDriver
                {
                    public BiDiDriver(TimeSpan timeout) { }
                    public LogModule Log { get; } = new LogModule();
                }

                public class LogModule
                {
                    public object OnEntryAdded { get; }
                }

                public class EventObserver<T>
                {
                    public void Dispose() { }
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
                        driver.Log.OnEntryAdded.{|CS1061:AddObserver|}(async (e) => { });
                    }
                }
            }
            """;

        CSharpAnalyzerTest<BiDiDriver005_MissingEventSubscriptionAnalyzer, DefaultVerifier> testState = new()
        {
            TestCode = test,
            ReferenceAssemblies = ReferenceAssemblies.Net.Net80,
        };

        await testState.RunAsync(TestContext.Current.CancellationToken);
    }

    /// <summary>
    /// Tests that AddObserver on ObservableEvent without ObservableEventNameAttribute is handled gracefully.
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous unit test.</returns>
    [Fact]
    public async Task AddObserver_WithoutObservableEventNameAttribute_NoDiagnostic()
    {
        // SYNTHETIC: keeps a hand-written stub. The module event property lacks the
        // [ObservableEventName] attribute, so no event name can be resolved. Every real module event
        // carries the attribute, so a module ObservableEvent without it cannot be reproduced against
        // the real assembly.
        string test = """
            using System;
            using System.Threading.Tasks;

            namespace WebDriverBiDi
            {
                public interface IBiDiDriver { }

                public class BiDiDriver : IBiDiDriver
                {
                    public BiDiDriver(TimeSpan timeout) { }
                    public LogModule Log { get; } = new LogModule();
                }

                public class LogModule
                {
                    public ObservableEvent<EntryAddedEventArgs> OnEntryAdded { get; } = new ObservableEvent<EntryAddedEventArgs>("log.entryAdded");
                }

                public class WebDriverBiDiEventArgs { }
                public class EntryAddedEventArgs : WebDriverBiDiEventArgs { }

                public class ObservableEvent<T> where T : WebDriverBiDiEventArgs
                {
                    public ObservableEvent(string eventName) { EventName = eventName; }
                    public string EventName { get; }
                    public EventObserver<T> AddObserver(Func<T, Task> handler) => null!;
                }

                public class EventObserver<T> where T : WebDriverBiDiEventArgs
                {
                    public void Dispose() { }
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
                        driver.Log.OnEntryAdded.AddObserver(async (e) => { });
                    }
                }
            }
            """;

        CSharpAnalyzerTest<BiDiDriver005_MissingEventSubscriptionAnalyzer, DefaultVerifier> testState = new()
        {
            TestCode = test,
            ReferenceAssemblies = ReferenceAssemblies.Net.Net80,
        };

        await testState.RunAsync(TestContext.Current.CancellationToken);
    }

    /// <summary>
    /// Tests that SubscribeAsync called via non-member-access invocation is handled gracefully.
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous unit test.</returns>
    [Fact]
    public async Task SubscribeAsync_ViaDelegate_NoDiagnostic()
    {
        // SYNTHETIC: keeps a hand-written stub. It captures SubscribeAsync as a method group into a
        // single-parameter delegate so the subscription is not seen as a member-access call. The real
        // SessionModule.SubscribeAsync has additional optional parameters, so a method-group conversion
        // to a one-argument delegate does not bind, and this cannot be reproduced against the real assembly.
        string test = """
            using System;
            using System.Threading.Tasks;

            namespace WebDriverBiDi
            {
                public interface IBiDiDriver { }

                public class BiDiDriver : IBiDiDriver
                {
                    public BiDiDriver(TimeSpan timeout) { }
                    public LogModule Log { get; } = new LogModule();
                    public SessionModule Session { get; } = new SessionModule();
                }

                public class LogModule
                {
                    [ObservableEventName("log.entryAdded")]
                    public ObservableEvent<EntryAddedEventArgs> OnEntryAdded { get; } = new ObservableEvent<EntryAddedEventArgs>("log.entryAdded");
                }

                public class SessionModule
                {
                    public Task<SubscribeCommandResult> SubscribeAsync(SubscribeCommandParameters parameters) => Task.FromResult(new SubscribeCommandResult());
                }

                public class SubscribeCommandParameters
                {
                    public SubscribeCommandParameters(string[] events) { }
                }

                public class SubscribeCommandResult { }

                public class WebDriverBiDiEventArgs { }
                public class EntryAddedEventArgs : WebDriverBiDiEventArgs { }

                public class ObservableEvent<T> where T : WebDriverBiDiEventArgs
                {
                    public ObservableEvent(string eventName) { EventName = eventName; }
                    public string EventName { get; }
                    public EventObserver<T> AddObserver(Func<T, Task> handler) => null!;
                }

                public class EventObserver<T> where T : WebDriverBiDiEventArgs
                {
                    public void Dispose() { }
                }

                [System.AttributeUsage(System.AttributeTargets.Property)]
                public sealed class ObservableEventNameAttribute : System.Attribute
                {
                    public ObservableEventNameAttribute(string eventName) { EventName = eventName; }
                    public string EventName { get; }
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
                        {|#0:driver.Log.OnEntryAdded.AddObserver(async (e) => { })|};

                        Func<SubscribeCommandParameters, Task<SubscribeCommandResult>> subscribeFunc = driver.Session.SubscribeAsync;
                        await subscribeFunc(new SubscribeCommandParameters(new[] { "log.entryAdded" }));
                    }
                }
            }
            """;

        DiagnosticResult expected = new DiagnosticResult(BiDiDriver005_MissingEventSubscriptionAnalyzer.DiagnosticId, Microsoft.CodeAnalysis.DiagnosticSeverity.Warning)
            .WithLocation(0)
            .WithArguments("log.entryAdded");

        CSharpAnalyzerTest<BiDiDriver005_MissingEventSubscriptionAnalyzer, DefaultVerifier> testState = new()
        {
            TestCode = test,
            ReferenceAssemblies = ReferenceAssemblies.Net.Net80,
        };
        testState.ExpectedDiagnostics.Add(expected);

        await testState.RunAsync(TestContext.Current.CancellationToken);
    }

    /// <summary>
    /// Tests that unresolved SubscribeAsync method is handled gracefully.
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous unit test.</returns>
    [Fact]
    public async Task SubscribeAsync_UnresolvedMethod_StillReportsWarning()
    {
        // SYNTHETIC: keeps a hand-written stub. The SessionModule has no SubscribeAsync method, so the
        // call binds to nothing (CS1061). The real SessionModule always defines SubscribeAsync, so an
        // unresolved SubscribeAsync cannot be produced against the real assembly.
        string test = """
            using System;
            using System.Threading.Tasks;

            namespace WebDriverBiDi
            {
                public interface IBiDiDriver { }

                public class BiDiDriver : IBiDiDriver
                {
                    public BiDiDriver(TimeSpan timeout) { }
                    public LogModule Log { get; } = new LogModule();
                    public SessionModule Session { get; } = new SessionModule();
                }

                public class LogModule
                {
                    [ObservableEventName("log.entryAdded")]
                    public ObservableEvent<EntryAddedEventArgs> OnEntryAdded { get; } = new ObservableEvent<EntryAddedEventArgs>("log.entryAdded");
                }

                public class SessionModule
                {
                }

                public class WebDriverBiDiEventArgs { }
                public class EntryAddedEventArgs : WebDriverBiDiEventArgs { }

                public class ObservableEvent<T> where T : WebDriverBiDiEventArgs
                {
                    public ObservableEvent(string eventName) { EventName = eventName; }
                    public string EventName { get; }
                    public EventObserver<T> AddObserver(Func<T, Task> handler) => null!;
                }

                public class EventObserver<T> where T : WebDriverBiDiEventArgs
                {
                    public void Dispose() { }
                }

                [System.AttributeUsage(System.AttributeTargets.Property)]
                public sealed class ObservableEventNameAttribute : System.Attribute
                {
                    public ObservableEventNameAttribute(string eventName) { EventName = eventName; }
                    public string EventName { get; }
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
                        {|#0:driver.Log.OnEntryAdded.AddObserver(async (e) => { })|};
                        await driver.Session.{|CS1061:SubscribeAsync|}(null);
                    }
                }
            }
            """;

        DiagnosticResult expected = new DiagnosticResult(BiDiDriver005_MissingEventSubscriptionAnalyzer.DiagnosticId, Microsoft.CodeAnalysis.DiagnosticSeverity.Warning)
            .WithLocation(0)
            .WithArguments("log.entryAdded");

        CSharpAnalyzerTest<BiDiDriver005_MissingEventSubscriptionAnalyzer, DefaultVerifier> testState = new()
        {
            TestCode = test,
            ReferenceAssemblies = ReferenceAssemblies.Net.Net80,
        };
        testState.ExpectedDiagnostics.Add(expected);

        await testState.RunAsync(TestContext.Current.CancellationToken);
    }

    /// <summary>
    /// Tests that AddObserver on a field (not property) is handled gracefully.
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous unit test.</returns>
    [Fact]
    public async Task AddObserver_OnField_NoDiagnostic()
    {
        // SYNTHETIC: keeps a hand-written stub. The driver exposes Log (and the module exposes
        // OnEntryAdded) as fields rather than properties, exercising the analyzer's IPropertySymbol
        // requirement. The real driver and modules expose these as properties, so field-based access
        // cannot be reproduced against the real assembly.
        string test = """
            using System;
            using System.Threading.Tasks;

            namespace WebDriverBiDi
            {
                public interface IBiDiDriver { }

                public class BiDiDriver : IBiDiDriver
                {
                    public BiDiDriver(TimeSpan timeout) { }
                    public LogModule Log = new LogModule();
                }

                public class LogModule
                {
                    [ObservableEventName("log.entryAdded")]
                    public ObservableEvent<EntryAddedEventArgs> OnEntryAdded = new ObservableEvent<EntryAddedEventArgs>("log.entryAdded");
                }

                public class WebDriverBiDiEventArgs { }
                public class EntryAddedEventArgs : WebDriverBiDiEventArgs { }

                public class ObservableEvent<T> where T : WebDriverBiDiEventArgs
                {
                    public ObservableEvent(string eventName) { EventName = eventName; }
                    public string EventName { get; }
                    public EventObserver<T> AddObserver(Func<T, Task> handler) => null!;
                }

                public class EventObserver<T> where T : WebDriverBiDiEventArgs
                {
                    public void Dispose() { }
                }

                [System.AttributeUsage(System.AttributeTargets.Property | System.AttributeTargets.Field)]
                public sealed class ObservableEventNameAttribute : System.Attribute
                {
                    public ObservableEventNameAttribute(string eventName) { EventName = eventName; }
                    public string EventName { get; }
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
                        driver.Log.OnEntryAdded.AddObserver(async (e) => { });
                    }
                }
            }
            """;

        CSharpAnalyzerTest<BiDiDriver005_MissingEventSubscriptionAnalyzer, DefaultVerifier> testState = new()
        {
            TestCode = test,
            ReferenceAssemblies = ReferenceAssemblies.Net.Net80,
        };

        await testState.RunAsync(TestContext.Current.CancellationToken);
    }

    /// <summary>
    /// Tests that SubscribeAsync with EventName property on local variable (non-property expression) cannot extract the event name.
    /// The analyzer reports a warning because it cannot resolve the event name from a local variable's EventName property.
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous unit test.</returns>
    [Fact]
    public async Task SubscribeAsync_WithLocalVariableEventName_ReportsWarning()
    {
        string test = """
            using System;
            using WebDriverBiDi;
            using WebDriverBiDi.Log;
            using WebDriverBiDi.Session;
            using System.Threading.Tasks;

            namespace TestApp
            {
                public class TestClass
                {
                    public async Task TestMethod()
                    {
                        BiDiDriver driver = new BiDiDriver(TimeSpan.FromSeconds(30));
                        {|#0:driver.Log.OnEntryAdded.AddObserver(async (e) => { })|};

                        ObservableEvent<EntryAddedEventArgs> localEvent = driver.Log.OnEntryAdded;
                        await driver.Session.SubscribeAsync(new SubscribeCommandParameters(new[] { localEvent.EventName }));
                    }
                }
            }
            """;

        DiagnosticResult expected = new DiagnosticResult(BiDiDriver005_MissingEventSubscriptionAnalyzer.DiagnosticId, Microsoft.CodeAnalysis.DiagnosticSeverity.Warning)
            .WithLocation(0)
            .WithArguments("log.entryAdded");

        RealAssemblyAnalyzerTest<BiDiDriver005_MissingEventSubscriptionAnalyzer> testState = new()
        {
            TestCode = test,
        };
        testState.ExpectedDiagnostics.Add(expected);

        await testState.RunAsync(TestContext.Current.CancellationToken);
    }

    /// <summary>
    /// Tests that a <c>SubscribeAsync</c> on a user type coincidentally named <c>SessionModule</c> in
    /// a namespace other than <c>WebDriverBiDi</c> is not treated as the library's session, so it does
    /// not count as a subscription; a proper library subscription still covers the observer, so no
    /// diagnostic is reported.
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous unit test.</returns>
    [Fact]
    public async Task SubscribeAsync_OnSameNamedSessionModuleInOtherNamespace_NoDiagnostic()
    {
        string test = """
            using System;
            using System.Threading.Tasks;
            using WebDriverBiDi;
            using WebDriverBiDi.Session;

            namespace TestApp
            {
                public class SessionModule
                {
                    public Task SubscribeAsync(string eventName) => Task.CompletedTask;
                }

                public class NotASession
                {
                    public Task SubscribeAsync(string eventName) => Task.CompletedTask;
                }

                public class TestClass
                {
                    public async Task TestMethod()
                    {
                        BiDiDriver driver = new BiDiDriver(TimeSpan.FromSeconds(30));
                        driver.Log.OnEntryAdded.AddObserver(async (e) => { });
                        await driver.Session.SubscribeAsync(new SubscribeCommandParameters(new[] { "log.entryAdded" }));

                        SessionModule fakeSession = new SessionModule();
                        await fakeSession.SubscribeAsync("some.other.event");

                        NotASession other = new NotASession();
                        await other.SubscribeAsync("some.other.event");
                    }
                }
            }
            """;

        RealAssemblyAnalyzerTest<BiDiDriver005_MissingEventSubscriptionAnalyzer> testState = new()
        {
            TestCode = test,
        };

        await testState.RunAsync(TestContext.Current.CancellationToken);
    }

    /// <summary>
    /// Compiles a fake WebDriverBiDi-shaped library in memory so that its types appear as
    /// metadata-backed symbols in analyzer tests, matching the real-world package-consumer scenario.
    /// </summary>
    private static async Task<MetadataReference> CreateFakeLibMetadataReference()
    {
        const string librarySource = """
            using System;
            using System.Threading.Tasks;

            namespace WebDriverBiDi
            {
                [AttributeUsage(AttributeTargets.Property)]
                public sealed class ObservableEventNameAttribute : Attribute
                {
                    public ObservableEventNameAttribute(string eventName) { EventName = eventName; }
                    public string EventName { get; }
                }

                public class WebDriverBiDiEventArgs { }
                public class EntryAddedEventArgs : WebDriverBiDiEventArgs { }

                public class ObservableEvent<T> where T : WebDriverBiDiEventArgs
                {
                    public ObservableEvent(string eventName) { EventName = eventName; }
                    public string EventName { get; }
                    public EventObserver<T> AddObserver(Func<T, Task> handler) => null!;
                }

                public class EventObserver<T> where T : WebDriverBiDiEventArgs { }

                public abstract class Module { }

                public class LogModule : Module
                {
                    [ObservableEventName("log.entryAdded")]
                    public ObservableEvent<EntryAddedEventArgs> OnEntryAdded { get; } = new("log.entryAdded");
                }

                public class SessionModule
                {
                    public Task SubscribeAsync(SubscribeCommandParameters p) => Task.CompletedTask;
                }

                public class SubscribeCommandParameters
                {
                    public SubscribeCommandParameters(string[] events) { }
                }

                public class BiDiDriver
                {
                    public LogModule Log { get; } = new();
                    public SessionModule Session { get; } = new();
                }
            }
            """;

        SyntaxTree tree = CSharpSyntaxTree.ParseText(librarySource);
        ImmutableArray<MetadataReference> netRefs = await ReferenceAssemblies.Net.Net80.ResolveAsync(LanguageNames.CSharp, default);

        CSharpCompilation compilation = CSharpCompilation.Create(
            "FakeLib",
            [tree],
            netRefs,
            new CSharpCompilationOptions(OutputKind.DynamicallyLinkedLibrary));

        using MemoryStream stream = new();
        compilation.Emit(stream);
        stream.Position = 0;
        return MetadataReference.CreateFromStream(stream);
    }

    /// <summary>
    /// Tests that AddObserver called on an expression whose type cannot be resolved does not
    /// report a diagnostic (exercises TryGetEventName typeSymbol null-guard, line 121).
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous unit test.</returns>
    [Fact]
    public async Task AddObserver_OnUnresolvableType_DoesNotReportDiagnostic()
    {
        // The receiver of AddObserver uses a property that doesn't exist on the type,
        // so the semantic model cannot resolve the type of the expression.
        string test = """
            using System;
            using WebDriverBiDi;
            using System.Threading.Tasks;

            namespace TestApp
            {
                public class TestClass
                {
                    public void TestMethod(BiDiDriver driver)
                    {
                        driver.{|CS1061:NonExistentProperty|}.AddObserver(async (e) => { });
                    }
                }
            }
            """;

        RealAssemblyAnalyzerTest<BiDiDriver005_MissingEventSubscriptionAnalyzer> testState = new()
        {
            TestCode = test,
        };

        await testState.RunAsync(TestContext.Current.CancellationToken);
    }

    /// <summary>
    /// Tests that an expression-bodied method (no block body) with an observer does not
    /// cause an exception — exercises GetSubscribedEventNames null-body guard (line 189).
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous unit test.</returns>
    [Fact]
    public async Task AddObserver_InExpressionBodiedMethod_DoesNotReportDiagnostic()
    {
        string test = """
            using System;
            using WebDriverBiDi;
            using WebDriverBiDi.Log;
            using System.Threading.Tasks;

            namespace TestApp
            {
                public class TestClass
                {
                    // Expression-bodied method: method.Body will be null.
                    public EventObserver<EntryAddedEventArgs> GetObserver(BiDiDriver driver) =>
                        driver.Log.OnEntryAdded.AddObserver(async (e) => { });
                }
            }
            """;

        RealAssemblyAnalyzerTest<BiDiDriver005_MissingEventSubscriptionAnalyzer> testState = new()
        {
            TestCode = test,
        };

        await testState.RunAsync(TestContext.Current.CancellationToken);
    }

    /// <summary>
    /// Tests that AddObserver called on a named non-ObservableEvent type does not fire —
    /// exercises the namedType.Name != "ObservableEvent" path (line 121).
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous unit test.</returns>
    [Fact]
    public async Task GetSubscribedEventNames_WithSubscribeCallHavingZeroArgs_DoesNotCrash()
    {
        // SYNTHETIC: keeps a hand-written stub. SubscribeAsync is called with
        // new SubscribeCommandParameters() (no args) to exercise the objectCreation zero-argument
        // branch. The real SubscribeCommandParameters has no parameterless constructor (every overload
        // requires at least one event), so this cannot be reproduced against the real assembly.
        // Use a stub SessionModule that has a SubscribeCommandParameters with no required args,
        // so we can call new SubscribeCommandParameters() — exercises line 224 false branch.
        string testCode = """
            using System;
            using System.Threading.Tasks;

            namespace WebDriverBiDi
            {
                public class IBiDiCommandExecutor { }

                public class BiDiDriver : IBiDiCommandExecutor
                {
                    public BiDiDriver(TimeSpan timeout) { }
                }

                public class SubscribeCommandResult { }

                // Parameterless constructor — allows new SubscribeCommandParameters() with 0 args.
                public class SubscribeCommandParameters { }

                public class SessionModule
                {
                    public Task<SubscribeCommandResult> SubscribeAsync(SubscribeCommandParameters parameters)
                        => Task.FromResult(new SubscribeCommandResult());
                }
            }

            namespace TestApp
            {
                using WebDriverBiDi;

                public class TestClass
                {
                    public async Task TestMethod(SessionModule session)
                    {
                        BiDiDriver driver = new BiDiDriver(TimeSpan.FromSeconds(30));
                        // Zero-argument SubscribeCommandParameters — exercises line 224 false branch.
                        await session.SubscribeAsync(new SubscribeCommandParameters());
                    }
                }
            }
            """;

        CSharpAnalyzerTest<BiDiDriver005_MissingEventSubscriptionAnalyzer, DefaultVerifier> testState0 = new()
        {
            TestCode = testCode,
            ReferenceAssemblies = ReferenceAssemblies.Net.Net80,
        };

        await testState0.RunAsync(TestContext.Current.CancellationToken);
    }

    /// <summary>
    /// Tests that a collection expression with a spread element inside SubscribeAsync
    /// does not crash — exercises element is not ExpressionElementSyntax (line 257 false).
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous unit test.</returns>
    [Fact]
    public async Task GetSubscribedEventNames_WithSpreadInCollectionExpression_DoesNotCrash()
    {
        string testCode = """
            using WebDriverBiDi;
            using WebDriverBiDi.Session;
            using System.Threading.Tasks;

            namespace TestNamespace
            {
                public class TestClass
                {
                    public async Task TestMethod()
                    {
                        BiDiDriver driver = new BiDiDriver();
                        string[] extra = ["network.beforeRequestSent"];
                        // Spread element in collection expression — not ExpressionElementSyntax.
                        await driver.Session.SubscribeAsync(new SubscribeCommandParameters([..extra]));
                    }
                }
            }
            """;

        RealAssemblyAnalyzerTest<BiDiDriver005_MissingEventSubscriptionAnalyzer> testState1 = new()
        {
            TestCode = testCode,
        };

        await testState1.RunAsync(TestContext.Current.CancellationToken);
    }

    /// <summary>
    /// Tests that a property whose ObservableEventName attribute has a different class
    /// name is not matched — exercises attr.AttributeClass?.Name != expected (line 168).
    /// Also exercises the real library path where the attribute IS ObservableEventNameAttribute.
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous unit test.</returns>
    [Fact]
    public async Task GetEventNameFromAttribute_WithDifferentAttributeClass_IsNotMatched()
    {
        string testCode = """
            using WebDriverBiDi;
            using WebDriverBiDi.Session;
            using System.Threading.Tasks;

            namespace TestNamespace
            {
                public class TestClass
                {
                    public async Task TestMethod()
                    {
                        BiDiDriver driver = new BiDiDriver();
                        driver.Log.OnEntryAdded.AddObserver(async (e) => { });
                        await driver.Session.SubscribeAsync(new SubscribeCommandParameters(new[] { "log.entryAdded" }));
                    }
                }
            }
            """;

        RealAssemblyAnalyzerTest<BiDiDriver005_MissingEventSubscriptionAnalyzer> testState2 = new()
        {
            TestCode = testCode,
        };

        await testState2.RunAsync(TestContext.Current.CancellationToken);
    }

    /// <summary>
    /// Tests that SubscribeAsync with a member access whose name is not "EventName" does
    /// not match the EventName property path — exercises line 280 MemberAccess name != "EventName".
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous unit test.</returns>
    [Fact]
    public async Task GetSubscribedEventNames_WithNonEventNamePropertyAccess_IsIgnored()
    {
        string testCode = """
            using WebDriverBiDi;
            using WebDriverBiDi.Session;
            using System.Threading.Tasks;

            namespace TestNamespace
            {
                public class TestClass
                {
                    public async Task TestMethod()
                    {
                        BiDiDriver driver = new BiDiDriver();
                        // GetType().FullName is a MemberAccess but NOT "EventName".
                        await driver.Session.SubscribeAsync(new SubscribeCommandParameters(new[] { driver.GetType().FullName! }));
                    }
                }
            }
            """;

        RealAssemblyAnalyzerTest<BiDiDriver005_MissingEventSubscriptionAnalyzer> testState3 = new()
        {
            TestCode = testCode,
        };

        await testState3.RunAsync(TestContext.Current.CancellationToken);
    }

    /// <summary>
    /// Tests that AddObserver called on a named non-ObservableEvent type does not fire —
    /// exercises the namedType.Name != "ObservableEvent" path (line 121).
    /// </summary>
    [Fact]
    public async Task TryGetEventName_WithNonObservableEventNamedType_ExercisesNameCheckBranch()
    {
        // SYNTHETIC: keeps a hand-written stub. It calls AddObserver on a custom named type that is not
        // ObservableEvent<T>, a name-only lookalike used to hit the analyzer's type-name check. The real
        // module events are all genuine ObservableEvent<T>, so this cannot be reproduced against the
        // real assembly.
        // AddObserver on a custom named type (NOT ObservableEvent) where the type IS
        // INamedTypeSymbol — exercises namedType.Name != "ObservableEvent" true (line 121 branch 1).
        string test = """
            using System;
            using System.Threading.Tasks;

            namespace WebDriverBiDi
            {
                // Named type that is NOT ObservableEvent<T>.
                public class CustomEventSource
                {
                    public void AddObserver(Func<object, Task> h) { }
                }

                public class LogModule
                {
                    public CustomEventSource OnEntryAdded { get; } = new();
                }

                public class BiDiDriver
                {
                    public BiDiDriver(TimeSpan timeout) { }
                    public LogModule Log { get; } = new();
                }
            }

            namespace TestApp
            {
                using WebDriverBiDi;

                public class TestClass
                {
                    public void TestMethod(BiDiDriver driver)
                    {
                        driver.Log.OnEntryAdded.AddObserver(async (e) => { });
                    }
                }
            }
            """;

        CSharpAnalyzerTest<BiDiDriver005_MissingEventSubscriptionAnalyzer, DefaultVerifier> testState = new()
        {
            TestCode = test,
            ReferenceAssemblies = ReferenceAssemblies.Net.Net80,
        };

        await testState.RunAsync(TestContext.Current.CancellationToken);
    }

    /// <summary>
    /// Tests that when AddObserver is called and the property's attribute class name is NOT
    /// ObservableEventNameAttribute the event name is not extracted —
    /// exercises attr.AttributeClass.Name != expected path (BIDI005 line 168 branch 0).
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous unit test.</returns>
    [Fact]
    public async Task GetEventNameFromProperty_WithDifferentAttributeClass_ExercisesFalseBranch()
    {
        // SYNTHETIC: keeps a hand-written stub. The module event property is annotated with a
        // differently named attribute (NotObservableEventNameAttribute) to exercise the analyzer's
        // attribute-class-name check. Every real module event uses the real ObservableEventNameAttribute,
        // so a mismatched attribute class cannot be reproduced against the real assembly.
        string test = """
            using System;
            using System.Threading.Tasks;

            namespace WebDriverBiDi
            {
                [System.AttributeUsage(System.AttributeTargets.Property)]
                public class NotObservableEventNameAttribute : System.Attribute
                {
                    public NotObservableEventNameAttribute(string name) { }
                }

                public class WebDriverBiDiEventArgs { }
                public class LogEntryAddedEventArgs : WebDriverBiDiEventArgs { }

                public class EventObserver<T> : IDisposable where T : WebDriverBiDiEventArgs
                {
                    public void Dispose() { }
                }

                public class ObservableEvent<T> where T : WebDriverBiDiEventArgs
                {
                    public EventObserver<T> AddObserver(Func<T, Task> handler) => new EventObserver<T>();
                }

                public abstract class Module { }

                public class LogModule
                {
                    // Different attribute class — attr.AttributeClass.Name != "ObservableEventNameAttribute".
                    [NotObservableEventNameAttribute("log.entryAdded")]
                    public ObservableEvent<LogEntryAddedEventArgs> OnEntryAdded { get; } = new();
                }

                public class IBiDiCommandExecutor { }

                public class BiDiDriver : IBiDiCommandExecutor
                {
                    public BiDiDriver(TimeSpan timeout) { }
                    public LogModule Log { get; } = new();
                }
            }

            namespace TestApp
            {
                using WebDriverBiDi;

                public class TestClass
                {
                    public void TestMethod(BiDiDriver driver)
                    {
                        // AddObserver on ObservableEvent with non-matching attribute class.
                        // BIDI005 cannot extract the event name (no ObservableEventNameAttribute)
                        // so no diagnostic fires — exercises attr.AttributeClass.Name != expected
                        // (line 168 branch 0 = false path taken).
                        using EventObserver<LogEntryAddedEventArgs> obs =
                            driver.Log.OnEntryAdded.AddObserver(async (e) => { });
                    }
                }
            }
            """;

        CSharpAnalyzerTest<BiDiDriver005_MissingEventSubscriptionAnalyzer, DefaultVerifier> testState = new()
        {
            TestCode = test,
            ReferenceAssemblies = ReferenceAssemblies.Net.Net80,
        };

        await testState.RunAsync(TestContext.Current.CancellationToken);
    }

    /// <summary>
    /// Tests with real library to exercise GetSubscribedEventNames object-creation count branch.
    /// </summary>
    [Fact]
    public async Task GetSubscribedEventNames_ExercisesObjectCreationCountGreaterThanZero()
    {
        // Uses real library — SubscribeAsync(new SubscribeCommandParameters("event")) has
        // objectCreation.ArgumentList.Arguments.Count == 1 > 0 (line 224 branch 1 = true).
        // BIDI005 fires because "other.event" is subscribed but "log.entryAdded" is not.
        string testCode = """
            using WebDriverBiDi;
            using WebDriverBiDi.Session;
            using System.Threading.Tasks;

            namespace TestNamespace
            {
                public class TestClass
                {
                    public async Task TestMethod()
                    {
                        BiDiDriver driver = new BiDiDriver();
                        {|#0:driver.Log.OnEntryAdded.AddObserver(async (e) => { })|};
                        await driver.Session.SubscribeAsync(new SubscribeCommandParameters("network.beforeRequestSent"));
                    }
                }
            }
            """;

        DiagnosticResult expected = new DiagnosticResult(
            BiDiDriver005_MissingEventSubscriptionAnalyzer.DiagnosticId,
            DiagnosticSeverity.Warning)
            .WithLocation(0)
            .WithArguments("log.entryAdded");

        RealAssemblyAnalyzerTest<BiDiDriver005_MissingEventSubscriptionAnalyzer> testState = new()
        {
            TestCode = testCode,
        };
        testState.ExpectedDiagnostics.Add(expected);

        await testState.RunAsync(TestContext.Current.CancellationToken);
    }

    /// <summary>
    /// Tests that SubscribeAsync with a collection expression containing both expression
    /// elements and a spread exercises ExpressionElementSyntax true branch (line 257 = true).
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous unit test.</returns>
    [Fact]
    public async Task GetSubscribedEventNames_CollectionExpressionWithExpressionAndSpread()
    {
        // Collection expression [..extra, "network.beforeRequestSent"] has both spread and
        // expression elements — exercises element is ExpressionElementSyntax true (line 257).
        string testCode = """
            using WebDriverBiDi;
            using WebDriverBiDi.Session;
            using System.Threading.Tasks;

            namespace TestNamespace
            {
                public class TestClass
                {
                    public async Task TestMethod()
                    {
                        BiDiDriver driver = new BiDiDriver();
                        {|#0:driver.Log.OnEntryAdded.AddObserver(async (e) => { })|};
                        string[] extra = [];
                        await driver.Session.SubscribeAsync(new SubscribeCommandParameters([..extra, "network.beforeRequestSent"]));
                    }
                }
            }
            """;

        DiagnosticResult expected = new DiagnosticResult(
            BiDiDriver005_MissingEventSubscriptionAnalyzer.DiagnosticId,
            DiagnosticSeverity.Warning)
            .WithLocation(0)
            .WithArguments("log.entryAdded");

        RealAssemblyAnalyzerTest<BiDiDriver005_MissingEventSubscriptionAnalyzer> testState = new()
        {
            TestCode = testCode,
        };
        testState.ExpectedDiagnostics.Add(expected);

        await testState.RunAsync(TestContext.Current.CancellationToken);
    }

    /// <summary>
    /// Tests that using .EventName in SubscribeAsync suppresses BIDI005 — exercises the
    /// expression is MemberAccess with name "EventName" true path (line 280 branches true).
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous unit test.</returns>
    [Fact]
    public async Task SubscribeAsync_WithEventNameProperty_DoesNotReportDiagnostic()
    {
        // Uses the single-event SubscribeCommandParameters(string) constructor with
        // driver.Log.OnEntryAdded.EventName as the argument — the form recommended by BIDI015
        // and used throughout the documentation. The event name is resolved from the
        // [ObservableEventName] attribute on the property, so the observer is subscribed
        // and no diagnostic is reported.
        string testCode = """
            using WebDriverBiDi;
            using WebDriverBiDi.Session;
            using System.Threading.Tasks;

            namespace TestNamespace
            {
                public class TestClass
                {
                    public async Task TestMethod()
                    {
                        BiDiDriver driver = new BiDiDriver();
                        driver.Log.OnEntryAdded.AddObserver(async (e) => { });
                        await driver.Session.SubscribeAsync(new SubscribeCommandParameters(driver.Log.OnEntryAdded.EventName));
                    }
                }
            }
            """;

        RealAssemblyAnalyzerTest<BiDiDriver005_MissingEventSubscriptionAnalyzer> testState = new()
        {
            TestCode = testCode,
        };

        await testState.RunAsync(TestContext.Current.CancellationToken);
    }

    /// <summary>
    /// Tests that AddObserver called on a named non-ObservableEvent type does not fire —
    /// exercises the namedType.Name != "ObservableEvent" path (line 121).
    /// </summary>
    [Fact]
    public async Task AddObserver_OnNamedNonObservableEventType_DoesNotReportDiagnostic()
    {
        // SYNTHETIC: keeps a hand-written stub. The module event property is typed as a custom
        // SomeEventSource (with an AddObserver method) rather than ObservableEvent<T>, a name-only
        // lookalike. Real module events are genuine ObservableEvent<T>, so this cannot be reproduced
        // against the real assembly.
        string test = """
            using System;
            using System.Threading.Tasks;

            namespace WebDriverBiDi
            {
                public class SomeEventSource
                {
                    public void AddObserver(Func<object, Task> handler) { }
                }

                public class LogModule
                {
                    public SomeEventSource OnEntryAdded { get; } = new();
                }

                public class BiDiDriver
                {
                    public BiDiDriver(TimeSpan timeout) { }
                    public LogModule Log { get; } = new();
                }
            }

            namespace TestApp
            {
                using WebDriverBiDi;

                public class TestClass
                {
                    public void TestMethod(BiDiDriver driver)
                    {
                        driver.Log.OnEntryAdded.AddObserver(async (e) => { });
                    }
                }
            }
            """;

        CSharpAnalyzerTest<BiDiDriver005_MissingEventSubscriptionAnalyzer, DefaultVerifier> testState = new()
        {
            TestCode = test,
            ReferenceAssemblies = ReferenceAssemblies.Net.Net80,
        };

        await testState.RunAsync(TestContext.Current.CancellationToken);
    }

    /// <summary>
    /// Tests that an <c>AddObserver</c> extension method whose receiver type is not an
    /// <see cref="INamedTypeSymbol"/> (an array type) is ignored rather than crashing.
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous unit test.</returns>
    [Fact]
    public async Task AddObserverExtensionOnArrayReceiver_DoesNotReportDiagnostic()
    {
        string test = """
            using System;

            namespace TestApp
            {
                public static class Extensions
                {
                    // An AddObserver whose receiver is an array type, so the receiver's type
                    // symbol is an IArrayTypeSymbol rather than an INamedTypeSymbol.
                    public static object AddObserver(this int[] source, object handler) => source;
                }

                public class TestClass
                {
                    public void Setup()
                    {
                        int[] values = new int[1];
                        values.AddObserver(new object());
                    }
                }
            }
            """;

        await AnalyzerTestHelpers.VerifyAnalyzerAsync<BiDiDriver005_MissingEventSubscriptionAnalyzer>(test);
    }

    /// <summary>
    /// Tests that an <c>ObservableEventName</c> attribute declared without constructor arguments
    /// yields no resolvable event name, so no subscription diagnostic is produced.
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous unit test.</returns>
    [Fact]
    public async Task ObservableEventNameAttributeWithoutArguments_DoesNotReportDiagnostic()
    {
        // SYNTHETIC: keeps a hand-written stub. It declares an ObservableEventNameAttribute with a
        // parameterless constructor so no event name can be read from it. The real attribute always
        // requires a string event-name argument, so this cannot be reproduced against the real assembly.
        string test = """
            using System;
            using System.Threading.Tasks;

            namespace WebDriverBiDi
            {
                public class ObservableEventNameAttribute : Attribute
                {
                    public ObservableEventNameAttribute() { }
                }

                public class WebDriverBiDiEventArgs { }

                public class EntryAddedEventArgs : WebDriverBiDiEventArgs { }

                public class EventObserver<T> : IDisposable where T : WebDriverBiDiEventArgs
                {
                    public void Dispose() { }
                }

                public class ObservableEvent<T> where T : WebDriverBiDiEventArgs
                {
                    public string EventName { get; } = string.Empty;

                    public EventObserver<T> AddObserver(Func<T, Task> handler) => new EventObserver<T>();
                }

                public class LogModule
                {
                    [ObservableEventName]
                    public ObservableEvent<EntryAddedEventArgs> OnEntryAdded { get; } = new();
                }

                public class BiDiDriver
                {
                    public LogModule Log { get; } = new();
                }
            }

            namespace TestApp
            {
                using WebDriverBiDi;

                public class TestClass
                {
                    public void Setup()
                    {
                        BiDiDriver driver = new BiDiDriver();
                        using EventObserver<EntryAddedEventArgs> observer =
                            driver.Log.OnEntryAdded.AddObserver(e => Task.CompletedTask);
                    }
                }
            }
            """;

        CSharpAnalyzerTest<BiDiDriver005_MissingEventSubscriptionAnalyzer, DefaultVerifier> testState = new()
        {
            TestCode = test,
            ReferenceAssemblies = ReferenceAssemblies.Net.Net80,
        };

        await testState.RunAsync(TestContext.Current.CancellationToken);
    }

    /// <summary>
    /// Tests that an <c>ObservableEventName</c> attribute whose single constructor argument is not
    /// a string yields no resolvable event name, so no subscription diagnostic is produced.
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous unit test.</returns>
    [Fact]
    public async Task ObservableEventNameAttributeWithNonStringArgument_DoesNotReportDiagnostic()
    {
        // SYNTHETIC: keeps a hand-written stub. It declares an ObservableEventNameAttribute whose
        // constructor argument is an int rather than a string, so no event name can be read. The real
        // attribute takes a string, so this cannot be reproduced against the real assembly.
        string test = """
            using System;
            using System.Threading.Tasks;

            namespace WebDriverBiDi
            {
                public class ObservableEventNameAttribute : Attribute
                {
                    public ObservableEventNameAttribute(int eventId) { }
                }

                public class WebDriverBiDiEventArgs { }

                public class EntryAddedEventArgs : WebDriverBiDiEventArgs { }

                public class EventObserver<T> : IDisposable where T : WebDriverBiDiEventArgs
                {
                    public void Dispose() { }
                }

                public class ObservableEvent<T> where T : WebDriverBiDiEventArgs
                {
                    public string EventName { get; } = string.Empty;

                    public EventObserver<T> AddObserver(Func<T, Task> handler) => new EventObserver<T>();
                }

                public class LogModule
                {
                    [ObservableEventName(42)]
                    public ObservableEvent<EntryAddedEventArgs> OnEntryAdded { get; } = new();
                }

                public class BiDiDriver
                {
                    public LogModule Log { get; } = new();
                }
            }

            namespace TestApp
            {
                using WebDriverBiDi;

                public class TestClass
                {
                    public void Setup()
                    {
                        BiDiDriver driver = new BiDiDriver();
                        using EventObserver<EntryAddedEventArgs> observer =
                            driver.Log.OnEntryAdded.AddObserver(e => Task.CompletedTask);
                    }
                }
            }
            """;

        CSharpAnalyzerTest<BiDiDriver005_MissingEventSubscriptionAnalyzer, DefaultVerifier> testState = new()
        {
            TestCode = test,
            ReferenceAssemblies = ReferenceAssemblies.Net.Net80,
        };

        await testState.RunAsync(TestContext.Current.CancellationToken);
    }

    /// <summary>
    /// Tests that a <c>SubscribeCommandParameters</c> created with no arguments subscribes to
    /// nothing, so an <c>AddObserver</c> call is still reported as missing its subscription.
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous unit test.</returns>
    [Fact]
    public async Task SubscribeParametersWithNoArguments_ReportsWarning()
    {
        // SYNTHETIC: keeps a hand-written stub (SubscribeFakeSource). It constructs
        // new SubscribeCommandParameters() with no arguments so nothing is subscribed. The real
        // SubscribeCommandParameters has no parameterless constructor, so this cannot be reproduced
        // against the real assembly.
        string test = SubscribeFakeSource + """

            namespace TestApp
            {
                using WebDriverBiDi;

                public class TestClass
                {
                    public async Task SetupAsync()
                    {
                        BiDiDriver driver = new BiDiDriver();
                        await driver.Session.SubscribeAsync(new SubscribeCommandParameters());
                        using EventObserver<EntryAddedEventArgs> observer =
                            {|#0:driver.Log.OnEntryAdded.AddObserver(e => Task.CompletedTask)|};
                    }
                }
            }
            """;

        DiagnosticResult expected = new DiagnosticResult(
            BiDiDriver005_MissingEventSubscriptionAnalyzer.DiagnosticId,
            DiagnosticSeverity.Warning)
            .WithLocation(0)
            .WithArguments("log.entryAdded");

        CSharpAnalyzerTest<BiDiDriver005_MissingEventSubscriptionAnalyzer, DefaultVerifier> testState = new()
        {
            TestCode = test,
            ReferenceAssemblies = ReferenceAssemblies.Net.Net80,
        };
        testState.ExpectedDiagnostics.Add(expected);

        await testState.RunAsync(TestContext.Current.CancellationToken);
    }

    /// <summary>
    /// Tests that subscribe-array elements which are neither string constants nor
    /// <c>.EventName</c> property accesses contribute no subscribed event names, so an
    /// <c>AddObserver</c> call is still reported as missing its subscription.
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous unit test.</returns>
    [Fact]
    public async Task SubscribeArrayWithNonConstantElements_ReportsWarning()
    {
        string test = """
            using System;
            using WebDriverBiDi;
            using WebDriverBiDi.Log;
            using WebDriverBiDi.Session;
            using System.Threading.Tasks;

            namespace TestApp
            {
                public class NameHolder
                {
                    public string Other { get; } = "other";
                }

                public class TestClass
                {
                    private static string GetName() => "log.entryAdded";

                    public async Task SetupAsync()
                    {
                        BiDiDriver driver = new BiDiDriver();

                        // Neither element is a compile-time constant: one is a plain identifier and
                        // the other is a member access whose name is not "EventName".
                        string dynamicName = GetName();
                        NameHolder holder = new NameHolder();
                        await driver.Session.SubscribeAsync(
                            new SubscribeCommandParameters(new[] { dynamicName, holder.Other }));
                        using EventObserver<EntryAddedEventArgs> observer =
                            {|#0:driver.Log.OnEntryAdded.AddObserver(e => Task.CompletedTask)|};
                    }
                }
            }
            """;

        DiagnosticResult expected = new DiagnosticResult(
            BiDiDriver005_MissingEventSubscriptionAnalyzer.DiagnosticId,
            DiagnosticSeverity.Warning)
            .WithLocation(0)
            .WithArguments("log.entryAdded");

        RealAssemblyAnalyzerTest<BiDiDriver005_MissingEventSubscriptionAnalyzer> testState = new()
        {
            TestCode = test,
        };
        testState.ExpectedDiagnostics.Add(expected);

        await testState.RunAsync(TestContext.Current.CancellationToken);
    }

    /// <summary>
    /// In-source stand-ins for the driver, log module, and session-subscription types, used by the
    /// subscribe-argument tests above.
    /// </summary>
    private const string SubscribeFakeSource = """
        using System;
        using System.Threading.Tasks;

        namespace WebDriverBiDi
        {
            public class ObservableEventNameAttribute : Attribute
            {
                public ObservableEventNameAttribute(string eventName) { }
            }

            public class WebDriverBiDiEventArgs { }

            public class EntryAddedEventArgs : WebDriverBiDiEventArgs { }

            public class EventObserver<T> : IDisposable where T : WebDriverBiDiEventArgs
            {
                public void Dispose() { }
            }

            public class ObservableEvent<T> where T : WebDriverBiDiEventArgs
            {
                public string EventName { get; } = string.Empty;

                public EventObserver<T> AddObserver(Func<T, Task> handler) => new EventObserver<T>();
            }

            public class LogModule
            {
                [ObservableEventName("log.entryAdded")]
                public ObservableEvent<EntryAddedEventArgs> OnEntryAdded { get; } = new();
            }

            public class SubscribeCommandResult { }

            public class SubscribeCommandParameters
            {
                public SubscribeCommandParameters() { }

                public SubscribeCommandParameters(string[] events) { }
            }

            public class SessionModule
            {
                public Task<SubscribeCommandResult> SubscribeAsync(SubscribeCommandParameters parameters) =>
                    Task.FromResult(new SubscribeCommandResult());
            }

            public class BiDiDriver
            {
                public LogModule Log { get; } = new();

                public SessionModule Session { get; } = new();
            }
        }
        """;

    /// <summary>
    /// Tests that an event access whose member-access chain roots at <c>this</c> rather than at an
    /// identifier is not attributed to a driver variable.
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous unit test.</returns>
    [Fact]
    public async Task AddObserver_OnThisRootedEventAccess_DoesNotReportDiagnostic()
    {
        string test = """
            using WebDriverBiDi;
            using WebDriverBiDi.Log;
            using System.Threading.Tasks;

            namespace TestApp
            {
                public class TestClass
                {
                    private readonly BiDiDriver driver = new BiDiDriver();

                    public void Setup()
                    {
                        // The chain roots at `this`, not at an identifier.
                        using EventObserver<EntryAddedEventArgs> observer =
                            this.driver.Log.OnEntryAdded.AddObserver(e => Task.CompletedTask);
                    }
                }
            }
            """;

        RealAssemblyAnalyzerTest<BiDiDriver005_MissingEventSubscriptionAnalyzer> testState = new()
        {
            TestCode = test,
        };

        await testState.RunAsync(TestContext.Current.CancellationToken);
    }

    /// <summary>
    /// Tests that an event access whose member-access chain roots at a namespace is not attributed
    /// to a driver variable. A namespace qualifier has no type at all.
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous unit test.</returns>
    [Fact]
    public async Task AddObserver_OnNamespaceRootedEventAccess_DoesNotReportDiagnostic()
    {
        string test = """
            using WebDriverBiDi;
            using WebDriverBiDi.Log;
            using System.Threading.Tasks;

            namespace TestApp
            {
                public static class EventHolder
                {
                    public static ObservableEvent<EntryAddedEventArgs> Entry => new BiDiDriver().Log.OnEntryAdded;
                }

                public class TestClass
                {
                    public void Setup()
                    {
                        // The chain roots at the `TestApp` namespace, which has no type.
                        using EventObserver<EntryAddedEventArgs> observer =
                            TestApp.EventHolder.Entry.AddObserver(e => Task.CompletedTask);
                    }
                }
            }
            """;

        RealAssemblyAnalyzerTest<BiDiDriver005_MissingEventSubscriptionAnalyzer> testState = new()
        {
            TestCode = test,
        };

        await testState.RunAsync(TestContext.Current.CancellationToken);
    }

    /// <summary>
    /// Tests an <c>ObservableEvent</c> type that also advertises the command-executor interface, so
    /// a bare variable of that type is simultaneously the observable event and its own chain root.
    /// The rule requires the event to be reached through a module property, so nothing is reported.
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous unit test.</returns>
    [Fact]
    public async Task AddObserver_OnObservableEventThatIsItselfACommandExecutor_DoesNotReportDiagnostic()
    {
        // SYNTHETIC: keeps a hand-written stub. It defines an ObservableEvent<T> that also implements
        // IBiDiCommandExecutor so a bare variable of that type is both the event and its own chain root.
        // The real ObservableEvent<T> does not implement the command-executor interface (and its
        // constructor is protected), so this cannot be reproduced against the real assembly.
        string test = """
            using System;
            using System.Threading.Tasks;

            namespace WebDriverBiDi
            {
                public interface IBiDiCommandExecutor { }

                public class WebDriverBiDiEventArgs { }

                public class EntryAddedEventArgs : WebDriverBiDiEventArgs { }

                public class EventObserver<T> : IDisposable where T : WebDriverBiDiEventArgs
                {
                    public void Dispose() { }
                }

                public class ObservableEvent<T> : IBiDiCommandExecutor where T : WebDriverBiDiEventArgs
                {
                    public EventObserver<T> AddObserver(Func<T, Task> handler) => new EventObserver<T>();
                }
            }

            namespace TestApp
            {
                using WebDriverBiDi;

                public class TestClass
                {
                    public void Setup()
                    {
                        ObservableEvent<EntryAddedEventArgs> standalone = new ObservableEvent<EntryAddedEventArgs>();
                        using EventObserver<EntryAddedEventArgs> observer =
                            standalone.AddObserver(e => Task.CompletedTask);
                    }
                }
            }
            """;

        CSharpAnalyzerTest<BiDiDriver005_MissingEventSubscriptionAnalyzer, DefaultVerifier> testState = new()
        {
            TestCode = test,
            ReferenceAssemblies = ReferenceAssemblies.Net.Net80,
        };

        await testState.RunAsync(TestContext.Current.CancellationToken);
    }

    /// <summary>
    /// Tests that the single-event SubscribeCommandParameters(string) constructor with a string
    /// literal naming the observed event is recognized as a subscription.
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous unit test.</returns>
    [Fact]
    public async Task SubscribeAsync_WithSingleEventStringLiteral_DoesNotReportDiagnostic()
    {
        string testCode = """
            using WebDriverBiDi;
            using WebDriverBiDi.Session;
            using System.Threading.Tasks;

            namespace TestNamespace
            {
                public class TestClass
                {
                    public async Task TestMethod()
                    {
                        BiDiDriver driver = new BiDiDriver();
                        driver.Log.OnEntryAdded.AddObserver(async (e) => { });
                        await driver.Session.SubscribeAsync(new SubscribeCommandParameters("log.entryAdded"));
                    }
                }
            }
            """;

        await AnalyzerTestHelpers.VerifyAnalyzerAsync<BiDiDriver005_MissingEventSubscriptionAnalyzer>(testCode);
    }

    /// <summary>
    /// Tests that the single-event constructor with a different event still reports the
    /// unsubscribed observer.
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous unit test.</returns>
    [Fact]
    public async Task SubscribeAsync_WithSingleEventForOtherEvent_ReportsDiagnostic()
    {
        string testCode = """
            using WebDriverBiDi;
            using WebDriverBiDi.Session;
            using System.Threading.Tasks;

            namespace TestNamespace
            {
                public class TestClass
                {
                    public async Task TestMethod()
                    {
                        BiDiDriver driver = new BiDiDriver();
                        {|#0:driver.Log.OnEntryAdded.AddObserver(async (e) => { })|};
                        await driver.Session.SubscribeAsync(new SubscribeCommandParameters(driver.Network.OnBeforeRequestSent.EventName));
                    }
                }
            }
            """;

        DiagnosticResult expected = new DiagnosticResult(
            BiDiDriver005_MissingEventSubscriptionAnalyzer.DiagnosticId,
            DiagnosticSeverity.Warning)
            .WithLocation(0)
            .WithArguments("log.entryAdded");

        await AnalyzerTestHelpers.VerifyAnalyzerAsync<BiDiDriver005_MissingEventSubscriptionAnalyzer>(testCode, expected);
    }

    [Fact]
    public async Task AddObserver_WithModuleNameSubscription_NoDiagnostic()
    {
        // Subscribing to a bare module name ("log") subscribes to every event in that module, so an
        // observer for "log.entryAdded" is covered and must not be flagged.
        string test = """
            using System;
            using WebDriverBiDi;
            using WebDriverBiDi.Session;
            using System.Threading.Tasks;

            namespace TestApp
            {
                public class TestClass
                {
                    public async Task TestMethod()
                    {
                        BiDiDriver driver = new BiDiDriver(TimeSpan.FromSeconds(30));
                        driver.Log.OnEntryAdded.AddObserver(async (e) => { });
                        await driver.Session.SubscribeAsync(new SubscribeCommandParameters(new[] { "log" }));
                    }
                }
            }
            """;

        RealAssemblyAnalyzerTest<BiDiDriver005_MissingEventSubscriptionAnalyzer> testState = new()
        {
            TestCode = test,
        };

        await testState.RunAsync(TestContext.Current.CancellationToken);
    }

    [Fact]
    public async Task AddObserver_ForEventNameWithoutModulePrefix_ReportsWarning()
    {
        // SYNTHETIC: keeps a hand-written stub. It defines a module event named "heartbeat" with no
        // module prefix (no '.') to exercise the no-'.' branch of the module-name coverage check. Every
        // real event name is module-qualified, so an unprefixed event name cannot be reproduced against
        // the real assembly.
        // An event name with no module prefix (no '.') cannot be covered by a module-name
        // subscription; without an exact subscription it is still reported. This also exercises the
        // no-'.' branch of the module-name coverage check.
        string test = """
            using System;
            using System.Threading.Tasks;

            namespace WebDriverBiDi
            {
                public interface IBiDiDriver { }

                public class BiDiDriver : IBiDiDriver
                {
                    public BiDiDriver(TimeSpan timeout) { }
                    public LogModule Log { get; } = new LogModule();
                }

                public class LogModule
                {
                    [ObservableEventName("heartbeat")]
                    public ObservableEvent<EntryAddedEventArgs> OnHeartbeat { get; } = new ObservableEvent<EntryAddedEventArgs>("heartbeat");
                }

                public class WebDriverBiDiEventArgs { }
                public class EntryAddedEventArgs : WebDriverBiDiEventArgs { }

                public class ObservableEvent<T> where T : WebDriverBiDiEventArgs
                {
                    public ObservableEvent(string eventName) { EventName = eventName; }
                    public string EventName { get; }
                    public EventObserver<T> AddObserver(Func<T, Task> handler) => null!;
                }

                public class EventObserver<T> where T : WebDriverBiDiEventArgs
                {
                    public void Dispose() { }
                }

                [System.AttributeUsage(System.AttributeTargets.Property)]
                public sealed class ObservableEventNameAttribute : System.Attribute
                {
                    public ObservableEventNameAttribute(string eventName) { EventName = eventName; }
                    public string EventName { get; }
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
                        {|#0:driver.Log.OnHeartbeat.AddObserver(async (e) => { })|};
                    }
                }
            }
            """;

        DiagnosticResult expected = new DiagnosticResult(BiDiDriver005_MissingEventSubscriptionAnalyzer.DiagnosticId, Microsoft.CodeAnalysis.DiagnosticSeverity.Warning)
            .WithLocation(0)
            .WithArguments("heartbeat");

        CSharpAnalyzerTest<BiDiDriver005_MissingEventSubscriptionAnalyzer, DefaultVerifier> testState = new()
        {
            TestCode = test,
            ReferenceAssemblies = ReferenceAssemblies.Net.Net80,
        };
        testState.ExpectedDiagnostics.Add(expected);

        await testState.RunAsync(TestContext.Current.CancellationToken);
    }

    [Fact]
    public async Task AddObserver_WithSubscribeParametersHeldInVariable_NoDiagnostic()
    {
        // The subscription parameters are held in a variable, so the set of subscribed
        // event names cannot be determined from the call site; a warning about missing
        // code must prefer a false negative over a false positive, so no diagnostic may
        // be reported.
        string testCode = """
            using System;
            using System.Threading.Tasks;
            using WebDriverBiDi;
            using WebDriverBiDi.Session;

            namespace TestNamespace
            {
                public class TestClass
                {
                    public async Task TestMethod()
                    {
                        BiDiDriver driver = new BiDiDriver(TimeSpan.FromSeconds(30));
                        driver.Log.OnEntryAdded.AddObserver(e => { });
                        SubscribeCommandParameters subscribeParameters = new SubscribeCommandParameters(driver.Log.OnEntryAdded.EventName);
                        await driver.Session.SubscribeAsync(subscribeParameters);
                    }
                }
            }
            """;

        await AnalyzerTestHelpers.VerifyAnalyzerAsync<BiDiDriver005_MissingEventSubscriptionAnalyzer>(testCode);
    }

    [Fact]
    public async Task AddObserver_WithTargetTypedNewSubscribeParameters_NoDiagnostic()
    {
        // A target-typed new(...) argument is inspectable inline, so the subscribed
        // event name resolves and the matching observer produces no diagnostic.
        string testCode = """
            using System;
            using System.Threading.Tasks;
            using WebDriverBiDi;
            using WebDriverBiDi.Session;

            namespace TestNamespace
            {
                public class TestClass
                {
                    public async Task TestMethod()
                    {
                        BiDiDriver driver = new BiDiDriver(TimeSpan.FromSeconds(30));
                        driver.Log.OnEntryAdded.AddObserver(e => { });
                        await driver.Session.SubscribeAsync(new("log.entryAdded"));
                    }
                }
            }
            """;

        await AnalyzerTestHelpers.VerifyAnalyzerAsync<BiDiDriver005_MissingEventSubscriptionAnalyzer>(testCode);
    }
}
