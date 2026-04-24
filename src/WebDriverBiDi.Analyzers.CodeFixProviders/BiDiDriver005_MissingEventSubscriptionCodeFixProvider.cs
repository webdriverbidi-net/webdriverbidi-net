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
        SyntaxNode? root = await context.Document.GetSyntaxRootAsync(context.CancellationToken).ConfigureAwait(false);
        if (root == null)
        {
            return;
        }

        Diagnostic diagnostic = context.Diagnostics.First();
        Microsoft.CodeAnalysis.Text.TextSpan diagnosticSpan = diagnostic.Location.SourceSpan;

        // Find the AddObserver invocation
        InvocationExpressionSyntax? addObserverCall = root.FindToken(diagnosticSpan.Start)
            .Parent?
            .AncestorsAndSelf()
            .OfType<InvocationExpressionSyntax>()
            .FirstOrDefault();

        if (addObserverCall == null)
        {
            return;
        }

        // Get the event name from the diagnostic message
        string diagnosticMessage = diagnostic.GetMessage();
        int startIndex = diagnosticMessage.IndexOf('\'') + 1;
        int endIndex = diagnosticMessage.IndexOf('\'', startIndex);
        if (startIndex < 1 || endIndex < 0)
        {
            return;
        }

        string eventName = diagnosticMessage.Substring(startIndex, endIndex - startIndex);

        // Register a code action
        context.RegisterCodeFix(
            CodeAction.Create(
                title: $"Add '{eventName}' to Session.SubscribeAsync",
                createChangedDocument: c => AddEventToSubscribeAsync(context.Document, root, addObserverCall, eventName, c),
                equivalenceKey: nameof(BiDiDriver005_MissingEventSubscriptionCodeFixProvider)),
            diagnostic);
    }

    private static async Task<Document> AddEventToSubscribeAsync(
        Document document,
        SyntaxNode root,
        InvocationExpressionSyntax addObserverCall,
        string eventName,
        CancellationToken cancellationToken)
    {
        // Find the containing method
        MethodDeclarationSyntax? method = addObserverCall.FirstAncestorOrSelf<MethodDeclarationSyntax>();
        if (method?.Body == null)
        {
            return document;
        }

        // Get semantic model once
        SemanticModel? semanticModel = await document.GetSemanticModelAsync(cancellationToken).ConfigureAwait(false);
        if (semanticModel == null)
        {
            return document;
        }

        // Find existing Session.SubscribeAsync call
        InvocationExpressionSyntax? subscribeCall = null;
        foreach (InvocationExpressionSyntax invocation in method.Body.DescendantNodes().OfType<InvocationExpressionSyntax>())
        {
            if (invocation.Expression is MemberAccessExpressionSyntax memberAccess)
            {
                IMethodSymbol? methodSymbol = semanticModel.GetSymbolInfo(invocation, cancellationToken).Symbol as IMethodSymbol;
                if (methodSymbol?.Name == "SubscribeAsync" && methodSymbol.ContainingType?.Name == "SessionModule")
                {
                    subscribeCall = invocation;
                    break;
                }
            }
        }

        if (subscribeCall != null)
        {
            // Add event name to existing SubscribeAsync call
            SyntaxNode newRoot = root.ReplaceNode(subscribeCall, AddEventNameToSubscribeCall(subscribeCall, eventName));
            return document.WithSyntaxRoot(newRoot);
        }
        else
        {
            // Create a new Session.SubscribeAsync call
            // This is more complex - for now, just return the original document
            // TODO: Implement creating new SubscribeAsync call
            return document;
        }
    }

    private static InvocationExpressionSyntax AddEventNameToSubscribeCall(
        InvocationExpressionSyntax subscribeCall,
        string eventName)
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

            ExpressionSyntax newEventsExpression = AddEventNameToArrayExpression(eventsExpression, eventName);

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

    private static ExpressionSyntax AddEventNameToArrayExpression(ExpressionSyntax arrayExpression, string eventName)
    {
        LiteralExpressionSyntax newElement = SyntaxFactory.LiteralExpression(
            SyntaxKind.StringLiteralExpression,
            SyntaxFactory.Literal(eventName));

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

        return arrayExpression;
    }
}
