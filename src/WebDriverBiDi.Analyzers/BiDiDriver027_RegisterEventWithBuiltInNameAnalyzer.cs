// <copyright file="BiDiDriver027_RegisterEventWithBuiltInNameAnalyzer.cs" company="WebDriverBiDi.NET Committers">
// Copyright (c) WebDriverBiDi.NET Committers. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.
// </copyright>

namespace WebDriverBiDi.Analyzers;

using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;

/// <summary>
/// Analyzer that detects a <c>RegisterEvent</c> call whose event name is a built-in protocol event.
/// The modules register those names in their constructors, so <c>RegisterEvent</c> always throws an
/// <c>ArgumentException</c> ("An event named '...' has already been registered.") at runtime.
/// </summary>
[DiagnosticAnalyzer(LanguageNames.CSharp)]
public class BiDiDriver027_RegisterEventWithBuiltInNameAnalyzer : DiagnosticAnalyzer
{
    /// <summary>
    /// The diagnostic ID for this analyzer.
    /// </summary>
    public const string DiagnosticId = "BIDI027";

    private const string Category = "Usage";

    private static readonly LocalizableString Title = "RegisterEvent called with a built-in event name";

    private static readonly LocalizableString MessageFormat = "'{0}' is a built-in protocol event that its module already registers, so RegisterEvent throws an ArgumentException at runtime. Observe the event through its ObservableEvent property (and subscribe with Session.SubscribeAsync) instead of registering it.";

    private static readonly LocalizableString Description = "The WebDriver BiDi modules register their protocol events (for example 'log.entryAdded') in their constructors. Calling RegisterEvent with one of those names throws an ArgumentException because the name is already registered. RegisterEvent is only for custom, non-built-in events; to receive a built-in event, subscribe to it with Session.SubscribeAsync and add an observer to the corresponding ObservableEvent property.";

    private static readonly DiagnosticDescriptor Rule = new(
        DiagnosticId,
        Title,
        MessageFormat,
        Category,
        DiagnosticSeverity.Error,
        isEnabledByDefault: true,
        description: Description,
        helpLinkUri: "https://webdriverbidi-net.github.io/webdriverbidi-net/articles/advanced/analyzers.html#bidi027");

    /// <inheritdoc/>
    public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics => ImmutableArray.Create(Rule);

    /// <inheritdoc/>
    public override void Initialize(AnalysisContext context)
    {
        context.ConfigureGeneratedCodeAnalysis(GeneratedCodeAnalysisFlags.None);
        context.EnableConcurrentExecution();

        context.RegisterCompilationStartAction(compilationStart =>
        {
            // The set of built-in event names is derived from the [ObservableEventName] attributes the
            // library applies to its ObservableEvent properties, so it never needs hand-maintaining. If
            // the library is not referenced, there are no built-in names and nothing to report.
            INamedTypeSymbol? attributeSymbol = compilationStart.Compilation.GetTypeByMetadataName("WebDriverBiDi.ObservableEventNameAttribute");
            if (attributeSymbol is null)
            {
                return;
            }

            // Collect lazily: the (one-time) walk of the library assembly happens only if the
            // compilation actually contains a RegisterEvent call worth checking.
            Lazy<ImmutableHashSet<string>> builtInEventNames = new(() => CollectBuiltInEventNames(attributeSymbol));

            compilationStart.RegisterSyntaxNodeAction(
                nodeContext => AnalyzeInvocation(nodeContext, builtInEventNames),
                SyntaxKind.InvocationExpression);
        });
    }

    private static void AnalyzeInvocation(SyntaxNodeAnalysisContext context, Lazy<ImmutableHashSet<string>> builtInEventNames)
    {
        InvocationExpressionSyntax invocation = (InvocationExpressionSyntax)context.Node;

        if (invocation.Expression is not MemberAccessExpressionSyntax memberAccess)
        {
            return;
        }

        if (memberAccess.Name.Identifier.Text != "RegisterEvent")
        {
            return;
        }

        IMethodSymbol? methodSymbol = context.SemanticModel.GetSymbolInfo(invocation).Symbol as IMethodSymbol;
        if (methodSymbol == null)
        {
            return;
        }

        // Only the library's RegisterEvent (on BiDiDriver / IBiDiCommandExecutor) is of interest.
        if (!AnalyzerSymbolHelpers.IsCommandExecutorType(methodSymbol.ContainingType))
        {
            return;
        }

        ImmutableHashSet<string> names = builtInEventNames.Value;

        // RegisterEvent(string eventName, Func<...> eventInvoker) has exactly one string parameter, so
        // the only constant-string argument is the event name; matching by shape also tolerates a named
        // or reordered argument list.
        foreach (ArgumentSyntax argument in invocation.ArgumentList.Arguments)
        {
            if (context.SemanticModel.GetConstantValue(argument.Expression) is { HasValue: true, Value: string eventName }
                && names.Contains(eventName))
            {
                Diagnostic diagnostic = Diagnostic.Create(Rule, argument.Expression.GetLocation(), eventName);
                context.ReportDiagnostic(diagnostic);
                return;
            }
        }
    }

    private static ImmutableHashSet<string> CollectBuiltInEventNames(INamedTypeSymbol attributeSymbol)
    {
        ImmutableHashSet<string>.Builder builder = ImmutableHashSet.CreateBuilder<string>(StringComparer.Ordinal);

        foreach (INamedTypeSymbol type in GetAllTypes(attributeSymbol.ContainingAssembly.GlobalNamespace))
        {
            foreach (ISymbol member in type.GetMembers())
            {
                foreach (AttributeData attribute in member.GetAttributes())
                {
                    if (SymbolEqualityComparer.Default.Equals(attribute.AttributeClass, attributeSymbol))
                    {
                        // The library always applies [ObservableEventName] with a single non-null string
                        // literal on every ObservableEvent property, so the constructor argument is a
                        // string; this walk only ever inspects the library's own assembly.
                        builder.Add((string)attribute.ConstructorArguments[0].Value!);
                    }
                }
            }
        }

        return builder.ToImmutable();
    }

    private static IEnumerable<INamedTypeSymbol> GetAllTypes(INamespaceOrTypeSymbol symbol)
    {
        foreach (INamedTypeSymbol type in symbol.GetTypeMembers())
        {
            yield return type;

            foreach (INamedTypeSymbol nested in GetAllTypes(type))
            {
                yield return nested;
            }
        }

        if (symbol is INamespaceSymbol namespaceSymbol)
        {
            foreach (INamespaceSymbol childNamespace in namespaceSymbol.GetNamespaceMembers())
            {
                foreach (INamedTypeSymbol type in GetAllTypes(childNamespace))
                {
                    yield return type;
                }
            }
        }
    }
}
