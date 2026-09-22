// <copyright file="BiDiDriver039AnalyzerTests.cs" company="WebDriverBiDi.NET Committers">
// Copyright (c) WebDriverBiDi.NET Committers. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.
// </copyright>

namespace WebDriverBiDi.Analyzers.Tests;

using System.Threading.Tasks;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Testing;

/// <summary>
/// Tests for the BiDiDriver039 analyzer.
/// </summary>
/// <remarks>
/// The control-flow walk these tests exercise is the one BIDI029 and BIDI038 use, so the cases here
/// cover what this rule adds to it: that reading the collected data is the one member that throws
/// after disposal, that a synchronous <c>Dispose</c> counts as one, and that only a call on the
/// collector itself is judged.
/// </remarks>
public class BiDiDriver039AnalyzerTests
{
    [Fact]
    public async Task GetCollectedEventDataAfterDispose_ReportsError()
    {
        string testCode = """
            using System;
            using System.Collections.Generic;
            using WebDriverBiDi;
            using WebDriverBiDi.Log;

            namespace TestApp
            {
                public class TestClass
                {
                    public void TestMethod(BiDiDriver driver)
                    {
                        EventDataCollector<EntryAddedEventArgs> collector = driver.Log.OnEntryAdded.AddDataCollector();
                        collector.Dispose();
                        IReadOnlyList<EntryAddedEventArgs> entries = {|#0:collector.GetCollectedEventData()|};
                    }
                }
            }
            """;

        await VerifyAsync(testCode, Expect("GetCollectedEventData", "collector"));
    }

    [Fact]
    public async Task GetCollectedEventDataAfterDisposeAsync_ReportsError()
    {
        string testCode = """
            using System;
            using System.Collections.Generic;
            using System.Threading.Tasks;
            using WebDriverBiDi;
            using WebDriverBiDi.Log;

            namespace TestApp
            {
                public class TestClass
                {
                    public async Task TestMethod(BiDiDriver driver)
                    {
                        EventDataCollector<EntryAddedEventArgs> collector = driver.Log.OnEntryAdded.AddDataCollector();
                        await collector.DisposeAsync();
                        IReadOnlyList<EntryAddedEventArgs> entries = {|#0:collector.GetCollectedEventData()|};
                    }
                }
            }
            """;

        await VerifyAsync(testCode, Expect("GetCollectedEventData", "collector"));
    }

    [Fact]
    public async Task WrappedReceiverAfterDispose_ReportsError()
    {
        string testCode = """
            using System;
            using System.Collections.Generic;
            using WebDriverBiDi;
            using WebDriverBiDi.Log;

            namespace TestApp
            {
                public class TestClass
                {
                    public void TestMethod(BiDiDriver driver)
                    {
                        EventDataCollector<EntryAddedEventArgs> collector = driver.Log.OnEntryAdded.AddDataCollector();
                        collector.Dispose();
                        IReadOnlyList<EntryAddedEventArgs> entries = {|#0:collector!.GetCollectedEventData()|};
                    }
                }
            }
            """;

        await VerifyAsync(testCode, Expect("GetCollectedEventData", "collector"));
    }

    [Fact]
    public async Task UnguardedMembersAfterDispose_ReportNothing()
    {
        // Disposal is idempotent, ToString reads the observer, which outlives disposal, and Events is a
        // property whose sequence simply ends rather than throwing. The last two calls are not the
        // collector's members at all.
        string testCode = """
            using System;
            using System.Collections.Generic;
            using WebDriverBiDi;
            using WebDriverBiDi.Log;

            namespace TestApp
            {
                public class TestClass
                {
                    public void TestMethod(BiDiDriver driver)
                    {
                        EventDataCollector<EntryAddedEventArgs> collector = driver.Log.OnEntryAdded.AddDataCollector();
                        collector.Dispose();
                        collector.Dispose();
                        IAsyncEnumerable<EntryAddedEventArgs> events = collector.Events;
                        string text = collector.ToString();
                        Type type = collector.GetType();
                        string upper = "literal".ToUpperInvariant();
                    }
                }
            }
            """;

        await VerifyAsync(testCode);
    }

    [Fact]
    public async Task UseBeforeDispose_ReportsNothing()
    {
        string testCode = """
            using System;
            using System.Collections.Generic;
            using WebDriverBiDi;
            using WebDriverBiDi.Log;

            namespace TestApp
            {
                public class TestClass
                {
                    public void TestMethod(BiDiDriver driver)
                    {
                        EventDataCollector<EntryAddedEventArgs> collector = driver.Log.OnEntryAdded.AddDataCollector();
                        IReadOnlyList<EntryAddedEventArgs> entries = collector.GetCollectedEventData();
                        collector.Dispose();
                    }
                }
            }
            """;

        await VerifyAsync(testCode);
    }

    [Fact]
    public async Task AwaitUsingDeclaration_ReportsNothing()
    {
        // The documented pattern: disposal happens at the end of the scope, so nothing inside it can
        // read the collector after disposal.
        string testCode = """
            using System;
            using System.Collections.Generic;
            using System.Threading.Tasks;
            using WebDriverBiDi;
            using WebDriverBiDi.Log;

            namespace TestApp
            {
                public class TestClass
                {
                    public async Task TestMethod(BiDiDriver driver)
                    {
                        await using EventDataCollector<EntryAddedEventArgs> collector = driver.Log.OnEntryAdded.AddDataCollector();
                        IReadOnlyList<EntryAddedEventArgs> entries = collector.GetCollectedEventData();
                        await Task.Yield();
                    }
                }
            }
            """;

        await VerifyAsync(testCode);
    }

    [Fact]
    public async Task DisposeInOneBranchOnly_ReportsNothing()
    {
        // The walk reports only where disposal has happened on every path reaching the call.
        string testCode = """
            using System;
            using System.Collections.Generic;
            using WebDriverBiDi;
            using WebDriverBiDi.Log;

            namespace TestApp
            {
                public class TestClass
                {
                    public void TestMethod(BiDiDriver driver, bool condition)
                    {
                        EventDataCollector<EntryAddedEventArgs> collector = driver.Log.OnEntryAdded.AddDataCollector();
                        if (condition)
                        {
                            collector.Dispose();
                        }

                        IReadOnlyList<EntryAddedEventArgs> entries = collector.GetCollectedEventData();
                    }
                }
            }
            """;

        await VerifyAsync(testCode);
    }

    [Fact]
    public async Task SimilarlyNamedMemberOnAnotherType_ReportsNothing()
    {
        // The tracked type is the library's, not anything that happens to declare the same members.
        string testCode = """
            using System;
            using System.Collections.Generic;

            namespace TestApp
            {
                public class EventDataCollector<T> : IDisposable
                {
                    public IReadOnlyList<T> GetCollectedEventData() => Array.Empty<T>();

                    public void Dispose()
                    {
                    }
                }

                public class TestClass
                {
                    public void TestMethod()
                    {
                        EventDataCollector<string> collector = new EventDataCollector<string>();
                        collector.Dispose();
                        IReadOnlyList<string> entries = collector.GetCollectedEventData();
                    }
                }
            }
            """;

        await VerifyAsync(testCode);
    }

    private static DiagnosticResult Expect(string methodName, string variableName, int location = 0)
    {
        return new DiagnosticResult(BiDiDriver039_DataCollectorUseAfterDisposalAnalyzer.DiagnosticId, DiagnosticSeverity.Error)
            .WithLocation(location)
            .WithArguments(methodName, variableName);
    }

    private static async Task VerifyAsync(string testCode, params DiagnosticResult[] expected)
    {
        RealAssemblyAnalyzerTest<BiDiDriver039_DataCollectorUseAfterDisposalAnalyzer> testState = new()
        {
            TestCode = testCode,
        };
        testState.ExpectedDiagnostics.AddRange(expected);

        await testState.RunAsync(TestContext.Current.CancellationToken);
    }
}
