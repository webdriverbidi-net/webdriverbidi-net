// <copyright file="BiDiDriver036AnalyzerTests.cs" company="WebDriverBiDi.NET Committers">
// Copyright (c) WebDriverBiDi.NET Committers. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.
// </copyright>

namespace WebDriverBiDi.Analyzers.Tests;

using System.Threading.Tasks;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Testing;

/// <summary>
/// Tests for the BiDiDriver036 analyzer.
/// </summary>
public class BiDiDriver036AnalyzerTests
{
    /// <summary>
    /// Tests that a constant connection string that is not an absolute WebSocket URL is reported for a driver constructed
    /// with the default transport, however that driver is spelled.
    /// </summary>
    /// <param name="setup">The statements that create the driver and start it, with the connection string marked.</param>
    /// <param name="connectionString">The connection string, as the diagnostic reports it.</param>
    /// <returns>A <see cref="Task"/> representing the asynchronous unit test.</returns>
    [Theory]
    [InlineData("BiDiDriver driver = new(); await driver.StartAsync({|#0:\"http://localhost:9222\"|});", "http://localhost:9222")]
    [InlineData("BiDiDriver driver = new(TimeSpan.FromSeconds(5)); await driver.StartAsync({|#0:\"localhost:9222/session\"|});", "localhost:9222/session")]
    [InlineData("await using BiDiDriver driver = new BiDiDriver(); await driver!.StartAsync(connectionString: {|#0:\"not a url\"|});", "not a url")]
    [InlineData("await using (BiDiDriver driver = new()) { await (driver).StartAsync({|#0:\"ftp://localhost\"|}); }", "ftp://localhost")]
    [InlineData("await new BiDiDriver().StartAsync({|#0:Endpoint|});", "https://localhost:9222")]
    public async Task NonWebSocketConnectionStringOnDefaultTransport_ReportsWarning(string setup, string connectionString)
    {
        string testCode = $$"""
            using System;
            using System.Threading.Tasks;
            using WebDriverBiDi;

            namespace TestApp
            {
                public class TestClass
                {
                    private const string Endpoint = "https://localhost:9222";

                    public async Task TestMethod()
                    {
                        {{setup}}
                    }
                }
            }
            """;

        DiagnosticResult expected = new DiagnosticResult(BiDiDriver036_InvalidWebSocketConnectionStringAnalyzer.DiagnosticId, DiagnosticSeverity.Warning)
            .WithLocation(0)
            .WithArguments(connectionString);

        await AnalyzerTestHelpers.VerifyAnalyzerAsync<BiDiDriver036_InvalidWebSocketConnectionStringAnalyzer>(testCode, expected);
    }

    /// <summary>
    /// Tests that a driver declared by a top-level program is judged too; its statements have no enclosing block.
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous unit test.</returns>
    [Fact]
    public async Task NonWebSocketConnectionStringInTopLevelProgram_ReportsWarning()
    {
        string testCode = """
            using WebDriverBiDi;

            BiDiDriver driver = new();
            await driver.StartAsync({|#0:"http://localhost:9222"|});
            """;

        DiagnosticResult expected = new DiagnosticResult(BiDiDriver036_InvalidWebSocketConnectionStringAnalyzer.DiagnosticId, DiagnosticSeverity.Warning)
            .WithLocation(0)
            .WithArguments("http://localhost:9222");

        RealAssemblyAnalyzerTest<BiDiDriver036_InvalidWebSocketConnectionStringAnalyzer> testState = new()
        {
            TestCode = testCode,
        };
        testState.TestState.OutputKind = OutputKind.ConsoleApplication;
        testState.ExpectedDiagnostics.Add(expected);

        await testState.RunAsync(TestContext.Current.CancellationToken);
    }

    /// <summary>
    /// Tests the calls the rule leaves alone: a valid WebSocket URL, a connection string that is not a constant, a driver
    /// given a transport or of a derived type, one whose construction is not visible here or that is rebound, and a
    /// StartAsync that is not the driver's.
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous unit test.</returns>
    [Fact]
    public async Task CallsThatCannotBeJudgedOrAreValid_NoDiagnostic()
    {
        string testCode = """
            using System;
            using System.Threading.Tasks;
            using WebDriverBiDi;
            using WebDriverBiDi.Protocol;

            namespace TestApp
            {
                public class TestClass
                {
                    private readonly BiDiDriver field = new();

                    public async Task TestMethod(BiDiDriver parameter, string url, PipeConnection pipe)
                    {
                        BiDiDriver valid = new();
                        await valid.StartAsync("ws://localhost:9222/session");
                        await valid.StartAsync("wss://localhost:9222/session");
                        await valid.StartAsync("WSS://localhost:9222/session");
                        await valid.StartAsync(url);
                        await valid.StartAsync(cancellationToken: default, connectionString: url);

                        BiDiDriver withTransport = new(new Transport(pipe));
                        await withTransport.StartAsync("/tmp/browser.pipe");

                        CustomDriver derived = new();
                        await derived.StartAsync("pipe:browser");

                        await parameter.StartAsync("http://localhost:9222");
                        await this.field.StartAsync("http://localhost:9222");
                        await CreateDriver().StartAsync("http://localhost:9222");

                        BiDiDriver rebound = new();
                        rebound = parameter;
                        await rebound.StartAsync("http://localhost:9222");

                        BiDiDriver passedByReference = new();
                        Replace(ref passedByReference);
                        await passedByReference.StartAsync("http://localhost:9222");

                        BiDiDriver fromFactory = CreateDriver();
                        await fromFactory.StartAsync("http://localhost:9222");

                        await new Browser().StartAsync("http://localhost:9222");
                        string trimmed = url.Trim();
                        dynamic late = valid;
                        await late.StartAsync("http://localhost:9222");

                        foreach (BiDiDriver looped in new[] { parameter })
                        {
                            await looped.StartAsync("http://localhost:9222");
                        }

                        BiDiDriver declaredLater;
                        declaredLater = new();
                        await declaredLater.StartAsync("http://localhost:9222");
                    }

                    private static BiDiDriver CreateDriver() => new();

                    private static void Replace(ref BiDiDriver driver) => driver = new();
                }

                public class CustomDriver : BiDiDriver
                {
                }

                public class Browser
                {
                    public Task StartAsync(string url) => Task.CompletedTask;
                }
            }
            """;

        await AnalyzerTestHelpers.VerifyAnalyzerAsync<BiDiDriver036_InvalidWebSocketConnectionStringAnalyzer>(testCode);
    }
}
