// <copyright file="BiDiDriver015AnalyzerTests.cs" company="WebDriverBiDi.NET Committers">
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
/// Tests for the BiDiDriver015 analyzer that detects string literals instead of ObservableEvent.EventName.
/// </summary>
[TestFixture]
public class BiDiDriver015AnalyzerTests
{
    /// <summary>
    /// Tests that string literal in SubscribeAsync reports a warning.
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous unit test.</returns>
    [Test]
    public async Task StringLiteral_InSubscribeAsync_ReportsWarning()
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
                        await driver.Session.SubscribeAsync(new SubscribeCommandParameters(new[] { {|#0:"log.entryAdded"|} }));
                    }
                }
            }
            """;

        DiagnosticResult expected = new DiagnosticResult(BiDiDriver015_StringLiteralInsteadOfEventNameAnalyzer.DiagnosticId, Microsoft.CodeAnalysis.DiagnosticSeverity.Warning)
            .WithLocation(0)
            .WithArguments("driver.Log.OnEntryAdded.EventName", "log.entryAdded");

        CSharpAnalyzerTest<BiDiDriver015_StringLiteralInsteadOfEventNameAnalyzer, DefaultVerifier> testState = new()
        {
            TestCode = test,
            ReferenceAssemblies = ReferenceAssemblies.Net.Net80,
        };
        testState.ExpectedDiagnostics.Add(expected);

        await testState.RunAsync();
    }

    /// <summary>
    /// Tests that using EventName property does not report a diagnostic.
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous unit test.</returns>
    [Test]
    public async Task EventNameProperty_InSubscribeAsync_NoDiagnostic()
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

        CSharpAnalyzerTest<BiDiDriver015_StringLiteralInsteadOfEventNameAnalyzer, DefaultVerifier> testState = new()
        {
            TestCode = test,
            ReferenceAssemblies = ReferenceAssemblies.Net.Net80,
        };

        await testState.RunAsync();
    }

    /// <summary>
    /// Tests that multiple string literals are all detected.
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous unit test.</returns>
    [Test]
    public async Task MultipleStringLiterals_InSubscribeAsync_ReportsMultipleWarnings()
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
                        driver.Network.OnBeforeRequestSent.AddObserver(async (e) => { });
                        await driver.Session.SubscribeAsync(new SubscribeCommandParameters(new[] { {|#0:"log.entryAdded"|}, {|#1:"network.beforeRequestSent"|} }));
                    }
                }
            }
            """;

        DiagnosticResult expected1 = new DiagnosticResult(BiDiDriver015_StringLiteralInsteadOfEventNameAnalyzer.DiagnosticId, Microsoft.CodeAnalysis.DiagnosticSeverity.Warning)
            .WithLocation(0)
            .WithArguments("driver.Log.OnEntryAdded.EventName", "log.entryAdded");

        DiagnosticResult expected2 = new DiagnosticResult(BiDiDriver015_StringLiteralInsteadOfEventNameAnalyzer.DiagnosticId, Microsoft.CodeAnalysis.DiagnosticSeverity.Warning)
            .WithLocation(1)
            .WithArguments("driver.Network.OnBeforeRequestSent.EventName", "network.beforeRequestSent");

        CSharpAnalyzerTest<BiDiDriver015_StringLiteralInsteadOfEventNameAnalyzer, DefaultVerifier> testState = new()
        {
            TestCode = test,
            ReferenceAssemblies = ReferenceAssemblies.Net.Net80,
        };
        testState.ExpectedDiagnostics.Add(expected1);
        testState.ExpectedDiagnostics.Add(expected2);

        await testState.RunAsync();
    }

    /// <summary>
    /// Tests that string literal for unknown event does not report a diagnostic.
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous unit test.</returns>
    [Test]
    public async Task StringLiteral_ForUnknownEvent_NoDiagnostic()
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
                        // Subscribing to an event that doesn't have a corresponding ObservableEvent
                        await driver.Session.SubscribeAsync(new SubscribeCommandParameters(new[] { "unknown.event" }));
                    }
                }
            }
            """;

        CSharpAnalyzerTest<BiDiDriver015_StringLiteralInsteadOfEventNameAnalyzer, DefaultVerifier> testState = new()
        {
            TestCode = test,
            ReferenceAssemblies = ReferenceAssemblies.Net.Net80,
        };

        await testState.RunAsync();
    }

    /// <summary>
    /// Tests that mixed usage (EventName and string literals) works correctly.
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous unit test.</returns>
    [Test]
    public async Task MixedUsage_EventNameAndStringLiteral_ReportsOnlyStringLiteral()
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
                        driver.Network.OnBeforeRequestSent.AddObserver(async (e) => { });
                        await driver.Session.SubscribeAsync(new SubscribeCommandParameters(new[] { driver.Log.OnEntryAdded.EventName, {|#0:"network.beforeRequestSent"|} }));
                    }
                }
            }
            """;

        DiagnosticResult expected = new DiagnosticResult(BiDiDriver015_StringLiteralInsteadOfEventNameAnalyzer.DiagnosticId, Microsoft.CodeAnalysis.DiagnosticSeverity.Warning)
            .WithLocation(0)
            .WithArguments("driver.Network.OnBeforeRequestSent.EventName", "network.beforeRequestSent");

        CSharpAnalyzerTest<BiDiDriver015_StringLiteralInsteadOfEventNameAnalyzer, DefaultVerifier> testState = new()
        {
            TestCode = test,
            ReferenceAssemblies = ReferenceAssemblies.Net.Net80,
        };
        testState.ExpectedDiagnostics.Add(expected);

        await testState.RunAsync();
    }

    /// <summary>
    /// Tests that code fix replaces string literal with EventName property.
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous unit test.</returns>
    [Test]
    public async Task CodeFix_ReplacesStringLiteralWithEventName()
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
                        await driver.Session.SubscribeAsync(new SubscribeCommandParameters(new[] { {|#0:"log.entryAdded"|} }));
                    }
                }
            }
            """;

        string fixedCode = """
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

        DiagnosticResult expected = new DiagnosticResult(BiDiDriver015_StringLiteralInsteadOfEventNameAnalyzer.DiagnosticId, Microsoft.CodeAnalysis.DiagnosticSeverity.Warning)
            .WithLocation(0)
            .WithArguments("driver.Log.OnEntryAdded.EventName", "log.entryAdded");

        CSharpCodeFixTest<BiDiDriver015_StringLiteralInsteadOfEventNameAnalyzer, BiDiDriver015_StringLiteralInsteadOfEventNameCodeFixProvider, DefaultVerifier> testState = new()
        {
            TestCode = testCode,
            FixedCode = fixedCode,
            ReferenceAssemblies = ReferenceAssemblies.Net.Net80,
        };
        testState.ExpectedDiagnostics.Add(expected);

        await testState.RunAsync();
    }

    /// <summary>
    /// Tests that methods without driver variable are handled gracefully.
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous unit test.</returns>
    [Test]
    public async Task NoDriverVariable_NoDiagnostic()
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
                    public SessionModule Session { get; } = new SessionModule();
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
            }

            namespace TestApp
            {
                using WebDriverBiDi;

                public class TestClass
                {
                    public async Task TestMethod(BiDiDriver driver)
                    {
                        // Driver is a parameter, not a local variable
                        await driver.Session.SubscribeAsync(new SubscribeCommandParameters(new[] { "log.entryAdded" }));
                    }
                }
            }
            """;

        CSharpAnalyzerTest<BiDiDriver015_StringLiteralInsteadOfEventNameAnalyzer, DefaultVerifier> testState = new()
        {
            TestCode = test,
            ReferenceAssemblies = ReferenceAssemblies.Net.Net80,
        };

        await testState.RunAsync();
    }

    /// <summary>
    /// Tests that a string literal in a C# 12 collection expression in SubscribeAsync reports a warning.
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous unit test.</returns>
    [Test]
    public async Task StringLiteral_InCollectionExpression_ReportsWarning()
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
                        await driver.Session.SubscribeAsync(new SubscribeCommandParameters([{|#0:"log.entryAdded"|}]));
                    }
                }
            }
            """;

        DiagnosticResult expected = new DiagnosticResult(BiDiDriver015_StringLiteralInsteadOfEventNameAnalyzer.DiagnosticId, Microsoft.CodeAnalysis.DiagnosticSeverity.Warning)
            .WithLocation(0)
            .WithArguments("driver.Log.OnEntryAdded.EventName", "log.entryAdded");

        CSharpAnalyzerTest<BiDiDriver015_StringLiteralInsteadOfEventNameAnalyzer, DefaultVerifier> testState = new()
        {
            TestCode = test,
            ReferenceAssemblies = ReferenceAssemblies.Net.Net80,
        };
        testState.ExpectedDiagnostics.Add(expected);

        await testState.RunAsync();
    }

    /// <summary>
    /// Tests that using EventName property in a C# 12 collection expression does not report a diagnostic.
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous unit test.</returns>
    [Test]
    public async Task EventNameProperty_InCollectionExpression_NoDiagnostic()
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
                        await driver.Session.SubscribeAsync(new SubscribeCommandParameters([driver.Log.OnEntryAdded.EventName]));
                    }
                }
            }
            """;

        CSharpAnalyzerTest<BiDiDriver015_StringLiteralInsteadOfEventNameAnalyzer, DefaultVerifier> testState = new()
        {
            TestCode = test,
            ReferenceAssemblies = ReferenceAssemblies.Net.Net80,
        };

        await testState.RunAsync();
    }

    /// <summary>
    /// Tests that BIDI015 fires correctly when WebDriverBiDi types are metadata-backed
    /// (i.e., referenced as a compiled assembly rather than defined in source).
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous unit test.</returns>
    [Test]
    public async Task StringLiteral_InSubscribeAsync_MetadataBacked_ReportsWarning()
    {
        string test = """
            using System;
            using System.Threading.Tasks;

            namespace TestApp
            {
                public class TestClass
                {
                    public async Task TestMethod()
                    {
                        FakeLib.BiDiDriver driver = new FakeLib.BiDiDriver();
                        driver.Log.OnEntryAdded.AddObserver(async (e) => { });
                        await driver.Session.SubscribeAsync(new FakeLib.SubscribeCommandParameters(new[] { {|#0:"log.entryAdded"|} }));
                    }
                }
            }
            """;

        DiagnosticResult expected = new DiagnosticResult(BiDiDriver015_StringLiteralInsteadOfEventNameAnalyzer.DiagnosticId, DiagnosticSeverity.Warning)
            .WithLocation(0)
            .WithArguments("driver.Log.OnEntryAdded.EventName", "log.entryAdded");

        CSharpAnalyzerTest<BiDiDriver015_StringLiteralInsteadOfEventNameAnalyzer, DefaultVerifier> testState = new()
        {
            TestCode = test,
            ReferenceAssemblies = ReferenceAssemblies.Net.Net80,
        };
        testState.TestState.AdditionalReferences.Add(await CreateFakeLibMetadataReference());
        testState.ExpectedDiagnostics.Add(expected);

        await testState.RunAsync();
    }

    /// <summary>
    /// Tests that BIDI015 does not fire when EventName property is used, with metadata-backed types.
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous unit test.</returns>
    [Test]
    public async Task EventNameProperty_InSubscribeAsync_MetadataBacked_NoDiagnostic()
    {
        string test = """
            using System;
            using System.Threading.Tasks;

            namespace TestApp
            {
                public class TestClass
                {
                    public async Task TestMethod()
                    {
                        FakeLib.BiDiDriver driver = new FakeLib.BiDiDriver();
                        driver.Log.OnEntryAdded.AddObserver(async (e) => { });
                        await driver.Session.SubscribeAsync(new FakeLib.SubscribeCommandParameters(new[] { driver.Log.OnEntryAdded.EventName }));
                    }
                }
            }
            """;

        CSharpAnalyzerTest<BiDiDriver015_StringLiteralInsteadOfEventNameAnalyzer, DefaultVerifier> testState = new()
        {
            TestCode = test,
            ReferenceAssemblies = ReferenceAssemblies.Net.Net80,
        };
        testState.TestState.AdditionalReferences.Add(await CreateFakeLibMetadataReference());

        await testState.RunAsync();
    }

    /// <summary>
    /// Tests that explicit array creation syntax (new string[] { }) reports a warning.
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous unit test.</returns>
    [Test]
    public async Task StringLiteral_InExplicitArrayCreation_ReportsWarning()
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
                        await driver.Session.SubscribeAsync(new SubscribeCommandParameters(new string[] { {|#0:"log.entryAdded"|} }));
                    }
                }
            }
            """;

        DiagnosticResult expected = new DiagnosticResult(BiDiDriver015_StringLiteralInsteadOfEventNameAnalyzer.DiagnosticId, Microsoft.CodeAnalysis.DiagnosticSeverity.Warning)
            .WithLocation(0)
            .WithArguments("driver.Log.OnEntryAdded.EventName", "log.entryAdded");

        CSharpAnalyzerTest<BiDiDriver015_StringLiteralInsteadOfEventNameAnalyzer, DefaultVerifier> testState = new()
        {
            TestCode = test,
            ReferenceAssemblies = ReferenceAssemblies.Net.Net80,
        };
        testState.ExpectedDiagnostics.Add(expected);

        await testState.RunAsync();
    }

    /// <summary>
    /// Tests that SubscribeAsync with no arguments does not crash the analyzer.
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous unit test.</returns>
    [Test]
    public async Task SubscribeAsync_WithNoArguments_NoDiagnostic()
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
                    public SessionModule Session { get; } = new SessionModule();
                }

                public class SessionModule
                {
                    public Task<SubscribeCommandResult> SubscribeAsync() => Task.FromResult(new SubscribeCommandResult());
                }

                public class SubscribeCommandResult { }
            }

            namespace TestApp
            {
                using WebDriverBiDi;

                public class TestClass
                {
                    public async Task TestMethod()
                    {
                        BiDiDriver driver = new BiDiDriver(TimeSpan.FromSeconds(30));
                        await driver.Session.SubscribeAsync();
                    }
                }
            }
            """;

        CSharpAnalyzerTest<BiDiDriver015_StringLiteralInsteadOfEventNameAnalyzer, DefaultVerifier> testState = new()
        {
            TestCode = test,
            ReferenceAssemblies = ReferenceAssemblies.Net.Net80,
        };

        await testState.RunAsync();
    }

    /// <summary>
    /// Tests that SubscribeAsync with a variable instead of object creation does not crash the analyzer.
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous unit test.</returns>
    [Test]
    public async Task SubscribeAsync_WithVariableArgument_NoDiagnostic()
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
                    public SessionModule Session { get; } = new SessionModule();
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
            }

            namespace TestApp
            {
                using WebDriverBiDi;

                public class TestClass
                {
                    public async Task TestMethod()
                    {
                        BiDiDriver driver = new BiDiDriver(TimeSpan.FromSeconds(30));
                        var parameters = new SubscribeCommandParameters(new[] { "log.entryAdded" });
                        await driver.Session.SubscribeAsync(parameters);
                    }
                }
            }
            """;

        CSharpAnalyzerTest<BiDiDriver015_StringLiteralInsteadOfEventNameAnalyzer, DefaultVerifier> testState = new()
        {
            TestCode = test,
            ReferenceAssemblies = ReferenceAssemblies.Net.Net80,
        };

        await testState.RunAsync();
    }

    /// <summary>
    /// Tests that SubscribeCommandParameters with no arguments does not crash the analyzer.
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous unit test.</returns>
    [Test]
    public async Task SubscribeCommandParameters_WithNoArguments_NoDiagnostic()
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
                    public SessionModule Session { get; } = new SessionModule();
                }

                public class SessionModule
                {
                    public Task<SubscribeCommandResult> SubscribeAsync(SubscribeCommandParameters parameters) => Task.FromResult(new SubscribeCommandResult());
                }

                public class SubscribeCommandParameters
                {
                    public SubscribeCommandParameters() { }
                }

                public class SubscribeCommandResult { }
            }

            namespace TestApp
            {
                using WebDriverBiDi;

                public class TestClass
                {
                    public async Task TestMethod()
                    {
                        BiDiDriver driver = new BiDiDriver(TimeSpan.FromSeconds(30));
                        await driver.Session.SubscribeAsync(new SubscribeCommandParameters());
                    }
                }
            }
            """;

        CSharpAnalyzerTest<BiDiDriver015_StringLiteralInsteadOfEventNameAnalyzer, DefaultVerifier> testState = new()
        {
            TestCode = test,
            ReferenceAssemblies = ReferenceAssemblies.Net.Net80,
        };

        await testState.RunAsync();
    }

    /// <summary>
    /// Tests that non-method invocations are handled gracefully.
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous unit test.</returns>
    [Test]
    public async Task NonMethodInvocation_NoDiagnostic()
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
                    public SessionModule Session { get; } = new SessionModule();
                }

                public class SessionModule
                {
                    public Func<Task<SubscribeCommandResult>> SubscribeAsync { get; set; } = null!;
                }

                public class SubscribeCommandResult { }
            }

            namespace TestApp
            {
                using WebDriverBiDi;

                public class TestClass
                {
                    public async Task TestMethod()
                    {
                        BiDiDriver driver = new BiDiDriver(TimeSpan.FromSeconds(30));
                        await driver.Session.SubscribeAsync();
                    }
                }
            }
            """;

        CSharpAnalyzerTest<BiDiDriver015_StringLiteralInsteadOfEventNameAnalyzer, DefaultVerifier> testState = new()
        {
            TestCode = test,
            ReferenceAssemblies = ReferenceAssemblies.Net.Net80,
        };

        await testState.RunAsync();
    }

    /// <summary>
    /// Tests that expression-bodied methods are handled gracefully.
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous unit test.</returns>
    [Test]
    public async Task ExpressionBodiedMethod_NoDiagnostic()
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
                    public SessionModule Session { get; } = new SessionModule();
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
            }

            namespace TestApp
            {
                using WebDriverBiDi;

                public class TestClass
                {
                    public Task TestMethod(BiDiDriver driver) => driver.Session.SubscribeAsync(new SubscribeCommandParameters(new[] { "log.entryAdded" }));
                }
            }
            """;

        CSharpAnalyzerTest<BiDiDriver015_StringLiteralInsteadOfEventNameAnalyzer, DefaultVerifier> testState = new()
        {
            TestCode = test,
            ReferenceAssemblies = ReferenceAssemblies.Net.Net80,
        };

        await testState.RunAsync();
    }

    /// <summary>
    /// Tests that non-literal expressions in event arrays are handled gracefully.
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous unit test.</returns>
    [Test]
    public async Task NonLiteralExpressionInArray_NoDiagnostic()
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
                    public SessionModule Session { get; } = new SessionModule();
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
            }

            namespace TestApp
            {
                using WebDriverBiDi;

                public class TestClass
                {
                    public async Task TestMethod()
                    {
                        BiDiDriver driver = new BiDiDriver(TimeSpan.FromSeconds(30));
                        string eventName = "log.entryAdded";
                        await driver.Session.SubscribeAsync(new SubscribeCommandParameters(new[] { eventName }));
                    }
                }
            }
            """;

        CSharpAnalyzerTest<BiDiDriver015_StringLiteralInsteadOfEventNameAnalyzer, DefaultVerifier> testState = new()
        {
            TestCode = test,
            ReferenceAssemblies = ReferenceAssemblies.Net.Net80,
        };

        await testState.RunAsync();
    }

    /// <summary>
    /// Tests that numeric literals in event arrays are handled gracefully.
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous unit test.</returns>
    [Test]
    public async Task NumericLiteralInArray_NoDiagnostic()
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
                    public SessionModule Session { get; } = new SessionModule();
                }

                public class SessionModule
                {
                    public Task<SubscribeCommandResult> SubscribeAsync(SubscribeCommandParameters parameters) => Task.FromResult(new SubscribeCommandResult());
                }

                public class SubscribeCommandParameters
                {
                    public SubscribeCommandParameters(object[] events) { }
                }

                public class SubscribeCommandResult { }
            }

            namespace TestApp
            {
                using WebDriverBiDi;

                public class TestClass
                {
                    public async Task TestMethod()
                    {
                        BiDiDriver driver = new BiDiDriver(TimeSpan.FromSeconds(30));
                        await driver.Session.SubscribeAsync(new SubscribeCommandParameters(new object[] { 42 }));
                    }
                }
            }
            """;

        CSharpAnalyzerTest<BiDiDriver015_StringLiteralInsteadOfEventNameAnalyzer, DefaultVerifier> testState = new()
        {
            TestCode = test,
            ReferenceAssemblies = ReferenceAssemblies.Net.Net80,
        };

        await testState.RunAsync();
    }

    /// <summary>
    /// Tests that driver variables without initializers are handled gracefully.
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous unit test.</returns>
    [Test]
    public async Task DriverVariableWithoutInitializer_NoDiagnostic()
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
                        BiDiDriver driver;
                        driver = new BiDiDriver(TimeSpan.FromSeconds(30));
                        driver.Log.OnEntryAdded.AddObserver(async (e) => { });
                        await driver.Session.SubscribeAsync(new SubscribeCommandParameters(new[] { "log.entryAdded" }));
                    }
                }
            }
            """;

        CSharpAnalyzerTest<BiDiDriver015_StringLiteralInsteadOfEventNameAnalyzer, DefaultVerifier> testState = new()
        {
            TestCode = test,
            ReferenceAssemblies = ReferenceAssemblies.Net.Net80,
        };

        await testState.RunAsync();
    }

    /// <summary>
    /// Tests that non-driver types are handled gracefully.
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous unit test.</returns>
    [Test]
    public async Task NonDriverType_NoDiagnostic()
    {
        string test = """
            using System;
            using System.Threading.Tasks;

            namespace WebDriverBiDi
            {
                public class SessionModule
                {
                    public Task<SubscribeCommandResult> SubscribeAsync(SubscribeCommandParameters parameters) => Task.FromResult(new SubscribeCommandResult());
                }

                public class SubscribeCommandParameters
                {
                    public SubscribeCommandParameters(string[] events) { }
                }

                public class SubscribeCommandResult { }
            }

            namespace TestApp
            {
                using WebDriverBiDi;

                public class TestClass
                {
                    public async Task TestMethod()
                    {
                        var driver = "not a driver";
                        SessionModule session = new SessionModule();
                        await session.SubscribeAsync(new SubscribeCommandParameters(new[] { "log.entryAdded" }));
                    }
                }
            }
            """;

        CSharpAnalyzerTest<BiDiDriver015_StringLiteralInsteadOfEventNameAnalyzer, DefaultVerifier> testState = new()
        {
            TestCode = test,
            ReferenceAssemblies = ReferenceAssemblies.Net.Net80,
        };

        await testState.RunAsync();
    }

    /// <summary>
    /// Tests that invocations without member access (e.g., local function calls) are handled gracefully.
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous unit test.</returns>
    [Test]
    public async Task LocalFunctionInvocation_NoDiagnostic()
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
                    public SessionModule Session { get; } = new SessionModule();
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
            }

            namespace TestApp
            {
                using WebDriverBiDi;

                public class TestClass
                {
                    public async Task TestMethod()
                    {
                        BiDiDriver driver = new BiDiDriver(TimeSpan.FromSeconds(30));
                        Task DoSomething() => Task.CompletedTask;
                        await DoSomething();
                        await driver.Session.SubscribeAsync(new SubscribeCommandParameters(new[] { "log.entryAdded" }));
                    }
                }
            }
            """;

        CSharpAnalyzerTest<BiDiDriver015_StringLiteralInsteadOfEventNameAnalyzer, DefaultVerifier> testState = new()
        {
            TestCode = test,
            ReferenceAssemblies = ReferenceAssemblies.Net.Net80,
        };

        await testState.RunAsync();
    }

    /// <summary>
    /// Tests that invocations where method symbol cannot be determined are handled gracefully.
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous unit test.</returns>
    [Test]
    public async Task InvocationWithUnresolvedMethod_NoDiagnostic()
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
                    public SessionModule Session { get; } = new SessionModule();
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
            }

            namespace TestApp
            {
                using WebDriverBiDi;

                public class TestClass
                {
                    public async Task TestMethod()
                    {
                        BiDiDriver driver = new BiDiDriver(TimeSpan.FromSeconds(30));
                        dynamic dynamicObj = driver.Session;
                        await dynamicObj.SubscribeAsync(new SubscribeCommandParameters(new[] { "log.entryAdded" }));
                    }
                }
            }
            """;

        CSharpAnalyzerTest<BiDiDriver015_StringLiteralInsteadOfEventNameAnalyzer, DefaultVerifier> testState = new()
        {
            TestCode = test,
            ReferenceAssemblies = ReferenceAssemblies.Net.Net80,
        };

        await testState.RunAsync();
    }

    /// <summary>
    /// Tests that const string expressions (non-literal) in arrays are handled gracefully.
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous unit test.</returns>
    [Test]
    public async Task ConstStringExpression_NoDiagnostic()
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
                    public SessionModule Session { get; } = new SessionModule();
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
            }

            namespace TestApp
            {
                using WebDriverBiDi;

                public class TestClass
                {
                    private const string EventName = "log.entryAdded";

                    public async Task TestMethod()
                    {
                        BiDiDriver driver = new BiDiDriver(TimeSpan.FromSeconds(30));
                        await driver.Session.SubscribeAsync(new SubscribeCommandParameters(new[] { EventName }));
                    }
                }
            }
            """;

        CSharpAnalyzerTest<BiDiDriver015_StringLiteralInsteadOfEventNameAnalyzer, DefaultVerifier> testState = new()
        {
            TestCode = test,
            ReferenceAssemblies = ReferenceAssemblies.Net.Net80,
        };

        await testState.RunAsync();
    }

    /// <summary>
    /// Tests that interpolated strings in arrays are handled gracefully.
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous unit test.</returns>
    [Test]
    public async Task InterpolatedStringInArray_NoDiagnostic()
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
                        string module = "log";
                        await driver.Session.SubscribeAsync(new SubscribeCommandParameters(new[] { $"{module}.entryAdded" }));
                    }
                }
            }
            """;

        CSharpAnalyzerTest<BiDiDriver015_StringLiteralInsteadOfEventNameAnalyzer, DefaultVerifier> testState = new()
        {
            TestCode = test,
            ReferenceAssemblies = ReferenceAssemblies.Net.Net80,
        };

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
