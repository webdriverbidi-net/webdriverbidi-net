// <copyright file="BiDiDriver032AnalyzerTests.cs" company="WebDriverBiDi.NET Committers">
// Copyright (c) WebDriverBiDi.NET Committers. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.
// </copyright>

namespace WebDriverBiDi.Analyzers.Tests;

using System.Threading.Tasks;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Testing;

/// <summary>
/// Tests for the BiDiDriver032 analyzer.
/// </summary>
public class BiDiDriver032AnalyzerTests
{
    /// <summary>
    /// Tests that each way of adding an observer to OnDataReceived is reported on a connection the same body wraps in a
    /// transport, before or after the transport is constructed, and however the connection is written.
    /// </summary>
    /// <param name="statements">The statements, with the reported call marked.</param>
    /// <param name="methodName">The name of the reported call.</param>
    /// <returns>A <see cref="Task"/> representing the asynchronous unit test.</returns>
    [Theory]
    [InlineData("Transport transport = new(connection); {|#0:connection.OnDataReceived.AddObserver(e => { })|};", "AddObserver")]
    [InlineData("{|#0:connection.OnDataReceived.AddDataCollector()|}; Transport transport = new((connection));", "AddDataCollector")]
    [InlineData("IDisposable subscription = {|#0:connection.OnDataReceived.ToObservable().Subscribe(new Sink())|}; Transport transport = new(connection!);", "Subscribe")]
    [InlineData("Transport transport = new((Connection)connection); connection?{|#0:.OnDataReceived.AddObserver(e => { })|};", "AddObserver")]
    public async Task ObserverAddedToWrappedConnection_ReportsWarning(string statements, string methodName)
    {
        string testCode = $$"""
            using System;
            using WebDriverBiDi;
            using WebDriverBiDi.Protocol;

            namespace TestApp
            {
                public class TestClass
                {
                    public void TestMethod(WebSocketConnection connection)
                    {
                        {{statements}}
                    }
                }

                public class Sink : IObserver<ConnectionDataReceivedEventArgs>
                {
                    public void OnCompleted() { }
                    public void OnError(Exception error) { }
                    public void OnNext(ConnectionDataReceivedEventArgs value) { }
                }
            }
            """;

        DiagnosticResult expected = new DiagnosticResult(BiDiDriver032_ConnectionDataReceivedObserverAnalyzer.DiagnosticId, DiagnosticSeverity.Warning)
            .WithLocation(0)
            .WithArguments(methodName, "connection");

        await AnalyzerTestHelpers.VerifyAnalyzerAsync<BiDiDriver032_ConnectionDataReceivedObserverAnalyzer>(testCode, expected);
    }

    /// <summary>
    /// Tests the code the rule leaves alone: a connection no transport in this body wraps, a transport given nothing or
    /// something other than a local or parameter, reads of the event that add no observer, a connection reached through
    /// more than one member or through <c>this</c>, and a same-named member of another type handed to a transport.
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous unit test.</returns>
    [Fact]
    public async Task CodeThatAddsNoObserverToAWrappedConnection_NoDiagnostic()
    {
        string testCode = """
            using System;
            using WebDriverBiDi;
            using WebDriverBiDi.Protocol;

            namespace TestApp
            {
                public class TestClass
                {
                    private readonly WebSocketConnection field = new();

                    public void Standalone(WebSocketConnection connection)
                    {
                        connection.OnDataReceived.AddObserver(e => { });
                    }

                    public void TestMethod(WebSocketConnection connection, WebSocketConnection other, Holder holder, Helper helper)
                    {
                        Transport unwrapped = new();
                        Transport fromCall = new(Create());
                        Transport fromField = new(this.field);
                        HelperTransport custom = new(connection, helper);
                        Uri unrelated = new("ws://localhost");

                        other.OnDataReceived.AddObserver(e => { });
                        this.field.OnDataReceived.AddObserver(e => { });
                        holder.Connection.OnDataReceived.AddObserver(e => { });
                        helper.OnDataReceived.AddObserver(e => { });
                        helper.OnDataReceivedField.AddObserver(e => { });

                        ObservableEvent<ConnectionDataReceivedEventArgs> read = connection.OnDataReceived;
                        int count = connection.OnDataReceived.CurrentObserverCount;
                        connection.OnDataReceived.RemoveObserver("id");
                        IObservable<ConnectionDataReceivedEventArgs> observable = connection.OnDataReceived.ToObservable();
                        string text = connection.OnDataReceived.ToObservable().ToString()!;
                        Func<IObserver<ConnectionDataReceivedEventArgs>, IDisposable> subscribe = connection.OnDataReceived.ToObservable().Subscribe;
                        string name = nameof(Connection.OnDataReceived);
                    }

                    private static WebSocketConnection Create() => new();
                }

                public class Holder
                {
                    public WebSocketConnection Connection { get; } = new();
                }

                public class Helper
                {
                    public ObservableEvent<ConnectionDataReceivedEventArgs> OnDataReceived => null!;

                    public ObservableEvent<ConnectionDataReceivedEventArgs> OnDataReceivedField = null!;
                }

                public class HelperTransport : Transport
                {
                    public HelperTransport(Connection connection, Helper helper)
                        : base(connection)
                    {
                    }
                }

                public class DerivedConnection : WebSocketConnection
                {
                    public void Observe(WebSocketConnection other)
                    {
                        Transport transport = new(other);
                        OnDataReceived.AddObserver(e => { });
                        ObservableEvent<ConnectionDataReceivedEventArgs> read = OnDataReceived;
                    }
                }
            }
            """;

        await AnalyzerTestHelpers.VerifyAnalyzerAsync<BiDiDriver032_ConnectionDataReceivedObserverAnalyzer>(testCode);
    }
}
