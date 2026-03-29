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
[TestFixture]
public class BiDiDriver005AnalyzerTests
{
    /// <summary>
    /// Tests that AddObserver without SubscribeAsync reports a warning.
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous unit test.</returns>
    [Test]
    public async Task AddObserver_WithoutSubscribeAsync_ReportsWarning()
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

        await testState.RunAsync();
    }

    /// <summary>
    /// Tests that AddObserver with SubscribeAsync does not report a diagnostic.
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous unit test.</returns>
    [Test]
    public async Task AddObserver_WithSubscribeAsync_NoDiagnostic()
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
                        driver.Log.OnEntryAdded.AddObserver(async (e) => { });
                        await driver.Session.SubscribeAsync(new SubscribeCommandParameters(new[] { "log.entryAdded" }));
                    }
                }
            }
            """;

        CSharpAnalyzerTest<BiDiDriver005_MissingEventSubscriptionAnalyzer, DefaultVerifier> testState = new()
        {
            TestCode = test,
            ReferenceAssemblies = ReferenceAssemblies.Net.Net80,
        };

        await testState.RunAsync();
    }

    /// <summary>
    /// Tests that AddObserver on driver internal events does not report a diagnostic.
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous unit test.</returns>
    [Test]
    public async Task AddObserver_OnDriverInternalEvent_NoDiagnostic()
    {
        string test = """
            using System;
            using System.Threading.Tasks;

            namespace WebDriverBiDi
            {
                public interface IBiDiDriver
                {
                    ObservableEvent<LogMessageEventArgs> OnLogMessage { get; }
                }

                public class BiDiDriver : IBiDiDriver
                {
                    public BiDiDriver(TimeSpan timeout) { }
                    public ObservableEvent<LogMessageEventArgs> OnLogMessage { get; } = new ObservableEvent<LogMessageEventArgs>("driver.logMessage");
                }

                public class WebDriverBiDiEventArgs { }
                public class LogMessageEventArgs : WebDriverBiDiEventArgs { }

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
                        driver.OnLogMessage.AddObserver(async (e) => { });
                    }
                }
            }
            """;

        CSharpAnalyzerTest<BiDiDriver005_MissingEventSubscriptionAnalyzer, DefaultVerifier> testState = new()
        {
            TestCode = test,
            ReferenceAssemblies = ReferenceAssemblies.Net.Net80,
        };

        await testState.RunAsync();
    }

    /// <summary>
    /// Tests that multiple AddObserver calls without SubscribeAsync report warnings.
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous unit test.</returns>
    [Test]
    public async Task MultipleAddObserver_WithoutSubscribeAsync_ReportsMultipleWarnings()
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
                    public LogModule Log { get; } = new LogModule();
                    public NetworkModule Network { get; } = new NetworkModule();
                }

                public class LogModule
                {
                    [ObservableEventName("log.entryAdded")]
                    public ObservableEvent<EntryAddedEventArgs> OnEntryAdded { get; } = new ObservableEvent<EntryAddedEventArgs>("log.entryAdded");
                }

                public class NetworkModule
                {
                    [ObservableEventName("network.beforeRequestSent")]
                    public ObservableEvent<BeforeRequestSentEventArgs> OnBeforeRequestSent { get; } = new ObservableEvent<BeforeRequestSentEventArgs>("network.beforeRequestSent");
                }

                public class WebDriverBiDiEventArgs { }
                public class EntryAddedEventArgs : WebDriverBiDiEventArgs { }
                public class BeforeRequestSentEventArgs : WebDriverBiDiEventArgs { }

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

        CSharpAnalyzerTest<BiDiDriver005_MissingEventSubscriptionAnalyzer, DefaultVerifier> testState = new()
        {
            TestCode = test,
            ReferenceAssemblies = ReferenceAssemblies.Net.Net80,
        };
        testState.ExpectedDiagnostics.Add(expected1);
        testState.ExpectedDiagnostics.Add(expected2);

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

                public abstract class TestClass
                {
                    public abstract Task TestMethod();
                }
            }
            """;

        CSharpAnalyzerTest<BiDiDriver005_MissingEventSubscriptionAnalyzer, DefaultVerifier> testState = new()
        {
            TestCode = test,
            ReferenceAssemblies = ReferenceAssemblies.Net.Net80,
        };

        await testState.RunAsync();
    }

    /// <summary>
    /// Tests that AddObserver on non-module property does not report a diagnostic.
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous unit test.</returns>
    [Test]
    public async Task AddObserver_OnNonModuleProperty_NoDiagnostic()
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
                    public ObservableEvent<CustomEventArgs> CustomEvent { get; } = new ObservableEvent<CustomEventArgs>("custom.event");
                }

                public class WebDriverBiDiEventArgs { }
                public class CustomEventArgs : WebDriverBiDiEventArgs { }

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
                        driver.CustomEvent.AddObserver(async (e) => { });
                    }
                }
            }
            """;

        CSharpAnalyzerTest<BiDiDriver005_MissingEventSubscriptionAnalyzer, DefaultVerifier> testState = new()
        {
            TestCode = test,
            ReferenceAssemblies = ReferenceAssemblies.Net.Net80,
        };

        await testState.RunAsync();
    }

    /// <summary>
    /// Tests that AddObserver on non-ObservableEvent does not report a diagnostic.
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous unit test.</returns>
    [Test]
    public async Task AddObserver_OnNonObservableEvent_NoDiagnostic()
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

        await testState.RunAsync();
    }

    /// <summary>
    /// Tests that AddObserver on non-BiDiDriver variable does not report a diagnostic.
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous unit test.</returns>
    [Test]
    public async Task AddObserver_OnNonDriverVariable_NoDiagnostic()
    {
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

                public class LogModule
                {
                    [ObservableEventName("log.entryAdded")]
                    public ObservableEvent<EntryAddedEventArgs> OnEntryAdded { get; } = new ObservableEvent<EntryAddedEventArgs>("log.entryAdded");
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
                        LogModule log = new LogModule();
                        log.OnEntryAdded.AddObserver(async (e) => { });
                    }
                }
            }
            """;

        CSharpAnalyzerTest<BiDiDriver005_MissingEventSubscriptionAnalyzer, DefaultVerifier> testState = new()
        {
            TestCode = test,
            ReferenceAssemblies = ReferenceAssemblies.Net.Net80,
        };

        await testState.RunAsync();
    }

    /// <summary>
    /// Tests that AddObserver with wrong event name in SubscribeAsync reports a warning.
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous unit test.</returns>
    [Test]
    public async Task AddObserver_WithWrongEventNameInSubscribe_ReportsWarning()
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
                    public ObservableEvent(string eventName) { }
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
                        // Wrong event name in subscription
                        await driver.Session.SubscribeAsync(new SubscribeCommandParameters(new[] { "network.beforeRequestSent" }));
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

        await testState.RunAsync();
    }

    /// <summary>
    /// Tests that multiple events with partial subscription reports warnings for unsubscribed events.
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous unit test.</returns>
    [Test]
    public async Task MultipleAddObserver_WithPartialSubscribe_ReportsWarningForUnsubscribed()
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
                    public LogModule Log { get; } = new LogModule();
                    public NetworkModule Network { get; } = new NetworkModule();
                    public SessionModule Session { get; } = new SessionModule();
                }

                public class LogModule
                {
                    [ObservableEventName("log.entryAdded")]
                    public ObservableEvent<EntryAddedEventArgs> OnEntryAdded { get; } = new ObservableEvent<EntryAddedEventArgs>("log.entryAdded");
                }

                public class NetworkModule
                {
                    [ObservableEventName("network.beforeRequestSent")]
                    public ObservableEvent<BeforeRequestSentEventArgs> OnBeforeRequestSent { get; } = new ObservableEvent<BeforeRequestSentEventArgs>("network.beforeRequestSent");
                    [ObservableEventName("network.responseCompleted")]
                    public ObservableEvent<ResponseCompletedEventArgs> OnResponseCompleted { get; } = new ObservableEvent<ResponseCompletedEventArgs>("network.responseCompleted");
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
                public class BeforeRequestSentEventArgs : WebDriverBiDiEventArgs { }
                public class ResponseCompletedEventArgs : WebDriverBiDiEventArgs { }

                public class ObservableEvent<T> where T : WebDriverBiDiEventArgs
                {
                    public ObservableEvent(string eventName) { }
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

        CSharpAnalyzerTest<BiDiDriver005_MissingEventSubscriptionAnalyzer, DefaultVerifier> testState = new()
        {
            TestCode = test,
            ReferenceAssemblies = ReferenceAssemblies.Net.Net80,
        };
        testState.ExpectedDiagnostics.Add(expected);

        await testState.RunAsync();
    }

    /// <summary>
    /// Tests that AddObserver with array creation syntax in SubscribeAsync works correctly.
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous unit test.</returns>
    [Test]
    public async Task AddObserver_WithArrayCreationSyntax_NoDiagnostic()
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
                    public ObservableEvent(string eventName) { }
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
                        driver.Log.OnEntryAdded.AddObserver(async (e) => { });
                        // Using explicit array creation
                        await driver.Session.SubscribeAsync(new SubscribeCommandParameters(new string[] { "log.entryAdded" }));
                    }
                }
            }
            """;

        CSharpAnalyzerTest<BiDiDriver005_MissingEventSubscriptionAnalyzer, DefaultVerifier> testState = new()
        {
            TestCode = test,
            ReferenceAssemblies = ReferenceAssemblies.Net.Net80,
        };

        await testState.RunAsync();
    }

    /// <summary>
    /// Tests that AddObserver with C# 12 collection expression syntax in SubscribeAsync does not report a diagnostic.
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous unit test.</returns>
    [Test]
    public async Task AddObserver_WithCollectionExpressionSyntax_NoDiagnostic()
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
                        driver.Log.OnEntryAdded.AddObserver(async (e) => { });
                        // Using C# 12 collection expression syntax
                        await driver.Session.SubscribeAsync(new SubscribeCommandParameters(["log.entryAdded"]));
                    }
                }
            }
            """;

        CSharpAnalyzerTest<BiDiDriver005_MissingEventSubscriptionAnalyzer, DefaultVerifier> testState = new()
        {
            TestCode = test,
            ReferenceAssemblies = ReferenceAssemblies.Net.Net80,
        };

        await testState.RunAsync();
    }

    /// <summary>
    /// Tests that AddObserver with C# 12 collection expression syntax and a wrong event name reports a warning.
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous unit test.</returns>
    [Test]
    public async Task AddObserver_WithCollectionExpressionSyntax_WrongEvent_ReportsWarning()
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
                        // Wrong event name in collection expression
                        await driver.Session.SubscribeAsync(new SubscribeCommandParameters(["network.beforeRequestSent"]));
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

        await testState.RunAsync();
    }

    /// <summary>
    /// Tests that BIDI005 fires correctly when WebDriverBiDi types are metadata-backed
    /// (i.e., referenced as a compiled assembly rather than defined in source).
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous unit test.</returns>
    [Test]
    public async Task AddObserver_WithoutSubscribeAsync_MetadataBacked_ReportsWarning()
    {
        string test = """
            using System;
            using System.Threading.Tasks;

            namespace TestApp
            {
                public class TestClass
                {
                    public async Task TestMethod(FakeLib.BiDiDriver driver)
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

        await testState.RunAsync();
    }

    /// <summary>
    /// Tests that BIDI005 does not fire when SubscribeAsync uses .EventName property access instead of a string literal.
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous unit test.</returns>
    [Test]
    public async Task AddObserver_WithSubscribeAsync_EventNamePropertyAccess_NoDiagnostic()
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
                        driver.Log.OnEntryAdded.AddObserver(async (e) => { });
                        await driver.Session.SubscribeAsync(new SubscribeCommandParameters(new[] { driver.Log.OnEntryAdded.EventName }));
                    }
                }
            }
            """;

        CSharpAnalyzerTest<BiDiDriver005_MissingEventSubscriptionAnalyzer, DefaultVerifier> testState = new()
        {
            TestCode = test,
            ReferenceAssemblies = ReferenceAssemblies.Net.Net80,
        };

        await testState.RunAsync();
    }

    /// <summary>
    /// Tests that BIDI005 does not fire when SubscribeAsync uses .EventName property access, with metadata-backed types.
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous unit test.</returns>
    [Test]
    public async Task AddObserver_WithSubscribeAsync_EventNamePropertyAccess_MetadataBacked_NoDiagnostic()
    {
        string test = """
            using System;
            using System.Threading.Tasks;

            namespace TestApp
            {
                public class TestClass
                {
                    public async Task TestMethod(FakeLib.BiDiDriver driver)
                    {
                        driver.Log.OnEntryAdded.AddObserver(async (e) => { });
                        await driver.Session.SubscribeAsync(new FakeLib.SubscribeCommandParameters(new[] { driver.Log.OnEntryAdded.EventName }));
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

        await testState.RunAsync();
    }

    /// <summary>
    /// Tests that BIDI005 does not fire when SubscribeAsync is present, with metadata-backed types.
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous unit test.</returns>
    [Test]
    public async Task AddObserver_WithSubscribeAsync_MetadataBacked_NoDiagnostic()
    {
        string test = """
            using System;
            using System.Threading.Tasks;

            namespace TestApp
            {
                public class TestClass
                {
                    public async Task TestMethod(FakeLib.BiDiDriver driver)
                    {
                        driver.Log.OnEntryAdded.AddObserver(async (e) => { });
                        await driver.Session.SubscribeAsync(new FakeLib.SubscribeCommandParameters(new[] { "log.entryAdded" }));
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

        await testState.RunAsync();
    }

    /// <summary>
    /// Tests SupportedDiagnostics property.
    /// </summary>
    [Test]
    public void SupportedDiagnostics_ContainsBIDI005()
    {
        BiDiDriver005_MissingEventSubscriptionAnalyzer analyzer = new();
        System.Collections.Immutable.ImmutableArray<DiagnosticDescriptor> diagnostics = analyzer.SupportedDiagnostics;

        Assert.That(diagnostics, Has.Length.EqualTo(1));
        Assert.That(diagnostics[0].Id, Is.EqualTo(BiDiDriver005_MissingEventSubscriptionAnalyzer.DiagnosticId));
    }

    /// <summary>
    /// Tests that AddObserver called via non-member-access invocation is handled gracefully.
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous unit test.</returns>
    [Test]
    public async Task AddObserver_ViaDelegate_NoDiagnostic()
    {
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

        await testState.RunAsync();
    }

    /// <summary>
    /// Tests that unresolved AddObserver method is handled gracefully.
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous unit test.</returns>
    [Test]
    public async Task AddObserver_UnresolvedMethod_NoDiagnostic()
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

        await testState.RunAsync();
    }

    /// <summary>
    /// Tests that AddObserver on expression with null type is handled gracefully.
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous unit test.</returns>
    [Test]
    public async Task AddObserver_OnNullTypeExpression_NoDiagnostic()
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

        await testState.RunAsync();
    }

    /// <summary>
    /// Tests that AddObserver on ObservableEvent without ObservableEventNameAttribute is handled gracefully.
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous unit test.</returns>
    [Test]
    public async Task AddObserver_WithoutObservableEventNameAttribute_NoDiagnostic()
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

        await testState.RunAsync();
    }

    /// <summary>
    /// Tests that SubscribeAsync called via non-member-access invocation is handled gracefully.
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous unit test.</returns>
    [Test]
    public async Task SubscribeAsync_ViaDelegate_NoDiagnostic()
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

        await testState.RunAsync();
    }

    /// <summary>
    /// Tests that unresolved SubscribeAsync method is handled gracefully.
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous unit test.</returns>
    [Test]
    public async Task SubscribeAsync_UnresolvedMethod_StillReportsWarning()
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

        await testState.RunAsync();
    }

    /// <summary>
    /// Tests that AddObserver on a field (not property) is handled gracefully.
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous unit test.</returns>
    [Test]
    public async Task AddObserver_OnField_NoDiagnostic()
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

        await testState.RunAsync();
    }

    /// <summary>
    /// Tests that SubscribeAsync with EventName property on local variable (non-property expression) cannot extract the event name.
    /// The analyzer reports a warning because it cannot resolve the event name from a local variable's EventName property.
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous unit test.</returns>
    [Test]
    public async Task SubscribeAsync_WithLocalVariableEventName_ReportsWarning()
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

                        ObservableEvent<EntryAddedEventArgs> localEvent = driver.Log.OnEntryAdded;
                        await driver.Session.SubscribeAsync(new SubscribeCommandParameters(new[] { localEvent.EventName }));
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

        await testState.RunAsync();
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

            namespace FakeLib
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

                public class LogModule
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
}
