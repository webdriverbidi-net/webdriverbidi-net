// <copyright file="BiDiDriver035AnalyzerTests.cs" company="WebDriverBiDi.NET Committers">
// Copyright (c) WebDriverBiDi.NET Committers. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.
// </copyright>

namespace WebDriverBiDi.Analyzers.Tests;

using System.Threading.Tasks;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Testing;

/// <summary>
/// Tests for the BiDiDriver035 analyzer.
/// </summary>
public class BiDiDriver035AnalyzerTests
{
    private const string EventArgsDeclarations = """

        namespace TestApp
        {
            public record ShoppingEventArgs : WebDriverBiDiEventArgs
            {
            }

            public record CheckoutEventArgs : ShoppingEventArgs
            {
            }
        }
        """;

    /// <summary>
    /// Tests that asking the parameterless overload for a type other than the event's own is reported, whether the
    /// type requested is unrelated, derived, or a base type.
    /// </summary>
    /// <param name="registeredType">The event data type the event is registered with.</param>
    /// <param name="requestedType">The event args type requested from ToEventArgs.</param>
    /// <returns>A <see cref="Task"/> representing the asynchronous unit test.</returns>
    [Theory]
    [InlineData("ShoppingEventArgs", "CheckoutEventArgs")]
    [InlineData("CheckoutEventArgs", "ShoppingEventArgs")]
    [InlineData("ShoppingEventArgs", "WebDriverBiDiEventArgs")]
    public async Task ParameterlessToEventArgsForAnotherType_ReportsError(string registeredType, string requestedType)
    {
        string testCode = $$"""
            using System.Threading.Tasks;
            using WebDriverBiDi;

            namespace TestApp
            {
                public class TestClass
                {
                    public void TestMethod(BiDiDriver driver)
                    {
                        driver.RegisterEvent<{{registeredType}}>("shop.event", info =>
                        {
                            {{requestedType}} args = {|#0:info.ToEventArgs<{{requestedType}}>()|};
                            return Task.CompletedTask;
                        });
                    }
                }
            }
            """ + EventArgsDeclarations;

        DiagnosticResult expected = new DiagnosticResult(BiDiDriver035_MismatchedToEventArgsTypeAnalyzer.DiagnosticId, DiagnosticSeverity.Error)
            .WithLocation(0)
            .WithArguments(requestedType, registeredType);

        await AnalyzerTestHelpers.VerifyAnalyzerAsync<BiDiDriver035_MismatchedToEventArgsTypeAnalyzer>(testCode, expected);
    }

    /// <summary>
    /// Tests the calls that cannot throw, or whose types this call site cannot see: the event's own type, the factory
    /// overload, a type parameter on either side, and same-named methods that are not the library's.
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous unit test.</returns>
    [Fact]
    public async Task CallsThatCannotBeJudgedOrDoNotThrow_NoDiagnostic()
    {
        string testCode = """
            using System;
            using System.Threading.Tasks;
            using WebDriverBiDi;

            namespace TestApp
            {
                public class TestClass
                {
                    public void TestMethod(BiDiDriver driver, Func<ShoppingEventArgs> produce, EventInfo<ShoppingEventArgs> shopping)
                    {
                        ShoppingEventArgs own = shopping.ToEventArgs<ShoppingEventArgs>();
                        CheckoutEventArgs built = shopping.ToEventArgs(data => new CheckoutEventArgs());
                        string text = shopping.ToString();
                        ShoppingEventArgs produced = produce();
                        dynamic late = shopping;
                        object lateBound = late.ToEventArgs();
                        ShoppingEventArgs local = new LocalInfo<ShoppingEventArgs>().ToEventArgs<ShoppingEventArgs>();
                        CheckoutEventArgs unrelated = new OtherLibrary.EventInfo<ShoppingEventArgs>().ToEventArgs<CheckoutEventArgs>();
                    }

                    public static TArgs Convert<TArgs>(EventInfo<ShoppingEventArgs> info)
                        where TArgs : WebDriverBiDiEventArgs
                    {
                        return info.ToEventArgs<TArgs>();
                    }

                    public static ShoppingEventArgs ConvertFrom<TData>(EventInfo<TData> info)
                        where TData : WebDriverBiDiEventArgs
                    {
                        return info.ToEventArgs<ShoppingEventArgs>();
                    }
                }

                public class LocalInfo<T>
                {
                    public TArgs ToEventArgs<TArgs>() => default!;
                }

            }

            namespace OtherLibrary
            {
                public class EventInfo<T>
                {
                    public TArgs ToEventArgs<TArgs>() => default!;
                }
            }
            """ + EventArgsDeclarations;

        await AnalyzerTestHelpers.VerifyAnalyzerAsync<BiDiDriver035_MismatchedToEventArgsTypeAnalyzer>(testCode);
    }
}
