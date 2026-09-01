// <copyright file="BiDiDriver005_MissingEventSubscriptionCodeFixProvider.cs" company="WebDriverBiDi.NET Committers">
// Copyright (c) WebDriverBiDi.NET Committers. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.
// </copyright>

namespace WebDriverBiDi.Analyzers;

using System.Collections.Immutable;
using System.Composition;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CodeActions;
using Microsoft.CodeAnalysis.CodeFixes;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

/// <summary>
/// Code fix provider for BIDI005 analyzer.
/// </summary>
[ExportCodeFixProvider(LanguageNames.CSharp, Name = nameof(BiDiDriver005_MissingEventSubscriptionCodeFixProvider))]
[Shared]
public class BiDiDriver005_MissingEventSubscriptionCodeFixProvider : CodeFixProvider
{
    /// <inheritdoc/>
    public sealed override ImmutableArray<string> FixableDiagnosticIds => ImmutableArray.Create(BiDiDriver005_MissingEventSubscriptionAnalyzer.DiagnosticId);

    /// <inheritdoc/>
    public sealed override FixAllProvider GetFixAllProvider()
    {
        return WellKnownFixAllProviders.BatchFixer;
    }

    /// <inheritdoc/>
    public sealed override async Task RegisterCodeFixesAsync(CodeFixContext context)
    {
        SyntaxNode root = (await context.Document.GetSyntaxRootAsync(context.CancellationToken).ConfigureAwait(false))!;

        Diagnostic diagnostic = context.Diagnostics.First();
        Microsoft.CodeAnalysis.Text.TextSpan diagnosticSpan = diagnostic.Location.SourceSpan;

        // Find the AddObserver invocation
        InvocationExpressionSyntax addObserverCall = root.FindToken(diagnosticSpan.Start)
            .Parent!
            .AncestorsAndSelf()
            .OfType<InvocationExpressionSyntax>()
            .First();

        // Only offer the fix when the enclosing member is a block-bodied method that already has a
        // Session.SubscribeAsync call to amend. The analyzer also fires in constructors and
        // top-level programs, where there is no method to rewrite; and creating a brand-new
        // subscription statement is out of scope, so registering an action that leaves the
        // document unchanged would be misleading.
        MethodDeclarationSyntax? method = addObserverCall.FirstAncestorOrSelf<MethodDeclarationSyntax>();
        if (method?.Body is null)
        {
            return;
        }

        SemanticModel semanticModel = (await context.Document.GetSemanticModelAsync(context.CancellationToken).ConfigureAwait(false))!;
        if (FindSubscribeCall(method, semanticModel, context.CancellationToken) is null)
        {
            return;
        }

        // Get the event name from the diagnostic message for the code action title.
        string diagnosticMessage = diagnostic.GetMessage();
        int startIndex = diagnosticMessage.IndexOf('\'') + 1;
        int endIndex = diagnosticMessage.IndexOf('\'', startIndex);
        string eventName = diagnosticMessage.Substring(startIndex, endIndex - startIndex);

        // Register a code action
        context.RegisterCodeFix(
            CodeAction.Create(
                title: $"Add '{eventName}' to Session.SubscribeAsync",
                createChangedDocument: c => AddEventToSubscribeAsync(context.Document, root, addObserverCall, c),
                equivalenceKey: nameof(BiDiDriver005_MissingEventSubscriptionCodeFixProvider)),
            diagnostic);
    }

    private static InvocationExpressionSyntax? FindSubscribeCall(
        MethodDeclarationSyntax method,
        SemanticModel semanticModel,
        CancellationToken cancellationToken)
    {
        foreach (InvocationExpressionSyntax invocation in method.Body!.DescendantNodes().OfType<InvocationExpressionSyntax>())
        {
            if (invocation.Expression is MemberAccessExpressionSyntax
                && semanticModel.GetSymbolInfo(invocation, cancellationToken).Symbol is IMethodSymbol methodSymbol
                && methodSymbol.Name == "SubscribeAsync"
                && methodSymbol.ContainingType.Name == "SessionModule")
            {
                return invocation;
            }
        }

        return null;
    }

    private static async Task<Document> AddEventToSubscribeAsync(
        Document document,
        SyntaxNode root,
        InvocationExpressionSyntax addObserverCall,
        CancellationToken cancellationToken)
    {
        MethodDeclarationSyntax method = addObserverCall.FirstAncestorOrSelf<MethodDeclarationSyntax>()!;
        SemanticModel semanticModel = (await document.GetSemanticModelAsync(cancellationToken).ConfigureAwait(false))!;

        // RegisterCodeFixesAsync only offers the fix when a SubscribeAsync call is present, so this is
        // guaranteed to be found.
        InvocationExpressionSyntax subscribeCall = FindSubscribeCall(method, semanticModel, cancellationToken)!;

        // Reference the event through its ObservableEvent's EventName property rather than inserting a
        // hardcoded string literal, so the added argument does not itself trigger BIDI015. The receiver
        // of the AddObserver call is exactly that ObservableEvent (for example driver.Log.OnEntryAdded).
        ExpressionSyntax observableEvent = ((MemberAccessExpressionSyntax)addObserverCall.Expression).Expression;
        ExpressionSyntax eventNameExpression = SyntaxFactory.MemberAccessExpression(
            SyntaxKind.SimpleMemberAccessExpression,
            observableEvent.WithoutTrivia(),
            SyntaxFactory.IdentifierName("EventName"));

        SyntaxNode newRoot = root.ReplaceNode(subscribeCall, AddEventNameToSubscribeCall(subscribeCall, eventNameExpression, semanticModel));
        return document.WithSyntaxRoot(newRoot);
    }

    private static InvocationExpressionSyntax AddEventNameToSubscribeCall(
        InvocationExpressionSyntax subscribeCall,
        ExpressionSyntax eventNameExpression,
        SemanticModel semanticModel)
    {
        if (subscribeCall.ArgumentList.Arguments.Count == 0)
        {
            return subscribeCall;
        }

        // Get the first argument (SubscribeCommandParameters)
        ArgumentSyntax firstArg = subscribeCall.ArgumentList.Arguments[0];
        ExpressionSyntax paramExpression = firstArg.Expression;

        // Handle: new SubscribeCommandParameters(new[] { ... })
        if (paramExpression is ObjectCreationExpressionSyntax objectCreation &&
            objectCreation.ArgumentList?.Arguments.Count > 0)
        {
            ArgumentSyntax eventsArg = objectCreation.ArgumentList.Arguments[0];
            ExpressionSyntax eventsExpression = eventsArg.Expression;

            ExpressionSyntax newEventsExpression = AddEventNameToArrayExpression(eventsExpression, eventNameExpression, semanticModel);

            if (newEventsExpression != eventsExpression)
            {
                ArgumentSyntax newEventsArg = eventsArg.WithExpression(newEventsExpression);
                SeparatedSyntaxList<ArgumentSyntax> newArgs = objectCreation.ArgumentList.Arguments.Replace(eventsArg, newEventsArg);
                ArgumentListSyntax newArgList = objectCreation.ArgumentList.WithArguments(newArgs);
                ObjectCreationExpressionSyntax newObjectCreation = objectCreation.WithArgumentList(newArgList);
                ArgumentSyntax newFirstArg = firstArg.WithExpression(newObjectCreation);
                SeparatedSyntaxList<ArgumentSyntax> newSubscribeArgs = subscribeCall.ArgumentList.Arguments.Replace(firstArg, newFirstArg);
                return subscribeCall.WithArgumentList(subscribeCall.ArgumentList.WithArguments(newSubscribeArgs));
            }
        }

        return subscribeCall;
    }

    private static ExpressionSyntax AddEventNameToArrayExpression(ExpressionSyntax arrayExpression, ExpressionSyntax newElement, SemanticModel semanticModel)
    {
        // Handle: new[] { "event1", "event2" }
        if (arrayExpression is ImplicitArrayCreationExpressionSyntax implicitArray)
        {
            SeparatedSyntaxList<ExpressionSyntax> newExpressions = implicitArray.Initializer.Expressions.Add(newElement);
            InitializerExpressionSyntax newInitializer = implicitArray.Initializer.WithExpressions(newExpressions);
            return implicitArray.WithInitializer(newInitializer);
        }

        // Handle: new string[] { "event1", "event2" }
        if (arrayExpression is ArrayCreationExpressionSyntax arrayCreation && arrayCreation.Initializer != null)
        {
            SeparatedSyntaxList<ExpressionSyntax> newExpressions = arrayCreation.Initializer.Expressions.Add(newElement);
            InitializerExpressionSyntax newInitializer = arrayCreation.Initializer.WithExpressions(newExpressions);
            return arrayCreation.WithInitializer(newInitializer);
        }

        // Handle: ["event1", "event2"] (C# 12 collection expressions)
        if (arrayExpression is CollectionExpressionSyntax collectionExpression)
        {
            ExpressionElementSyntax newElementSyntax = SyntaxFactory.ExpressionElement(newElement);
            SeparatedSyntaxList<CollectionElementSyntax> newElements = collectionExpression.Elements.Add(newElementSyntax);
            return collectionExpression.WithElements(newElements);
        }

        // Handle the single-event constructor: new SubscribeCommandParameters("event1") or
        // new SubscribeCommandParameters(driver.Log.OnEntryAdded.EventName). The string-typed
        // argument becomes a collection expression holding both the existing and the new event.
        // The comparison goes through SymbolEqualityComparer, which handles an unresolvable
        // (error-typed) argument without a separate null check.
        INamedTypeSymbol stringType = semanticModel.Compilation.GetSpecialType(SpecialType.System_String);
        if (SymbolEqualityComparer.Default.Equals(semanticModel.GetTypeInfo(arrayExpression).Type, stringType))
        {
            return SyntaxFactory.CollectionExpression(
                SyntaxFactory.SeparatedList<CollectionElementSyntax>(new SyntaxNodeOrToken[]
                {
                    SyntaxFactory.ExpressionElement(arrayExpression.WithoutTrivia()),
                    SyntaxFactory.Token(SyntaxKind.CommaToken).WithTrailingTrivia(SyntaxFactory.Space),
                    SyntaxFactory.ExpressionElement(newElement),
                }));
        }

        // Any other shape — for example a variable holding the event-name array — cannot be
        // rewritten in place. Return it unchanged so the caller leaves the call site alone.
        return arrayExpression;
    }
}
