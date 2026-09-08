// <copyright file="BiDiDriver005_MissingEventSubscriptionCodeFixProvider.cs" company="WebDriverBiDi.NET Committers">
// Copyright (c) WebDriverBiDi.NET Committers. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.
// </copyright>

namespace WebDriverBiDi.Analyzers;

using System.Collections.Immutable;
using System.Composition;
using System.Globalization;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CodeActions;
using Microsoft.CodeAnalysis.CodeFixes;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Text;

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
    public sealed override Task RegisterCodeFixesAsync(CodeFixContext context)
    {
        Diagnostic diagnostic = context.Diagnostics.First();

        // The analyzer records the events argument it located and validated. Its absence means there is
        // no subscription this fix can amend — no Session.SubscribeAsync in the analyzed body, or one
        // whose events are not written as an argument it can extend — and creating a new subscription
        // statement is out of scope. Reading the analyzer's conclusion rather than repeating its search
        // keeps the two from disagreeing about which calls count or which shapes can be amended.
        if (!diagnostic.Properties.TryGetValue(BiDiDriver005_MissingEventSubscriptionAnalyzer.EventsArgumentSpanKey, out string? eventsArgumentSpan))
        {
            return Task.CompletedTask;
        }

        // Get the event name from the diagnostic message for the code action title.
        string diagnosticMessage = diagnostic.GetMessage();
        int startIndex = diagnosticMessage.IndexOf('\'') + 1;
        int endIndex = diagnosticMessage.IndexOf('\'', startIndex);
        string eventName = diagnosticMessage.Substring(startIndex, endIndex - startIndex);

        context.RegisterCodeFix(
            CodeAction.Create(
                title: $"Add '{eventName}' to Session.SubscribeAsync",
                createChangedDocument: c => AddEventToSubscribeAsync(context.Document, diagnostic, ParseSpan(eventsArgumentSpan!), c),
                equivalenceKey: nameof(BiDiDriver005_MissingEventSubscriptionCodeFixProvider)),
            diagnostic);

        return Task.CompletedTask;
    }

    /// <summary>
    /// Parses the "start,length" span the analyzer recorded.
    /// </summary>
    /// <param name="spanText">The recorded span.</param>
    /// <returns>The span it denotes.</returns>
    private static TextSpan ParseSpan(string spanText)
    {
        int separatorIndex = spanText.IndexOf(',');
        int start = int.Parse(spanText.Substring(0, separatorIndex), CultureInfo.InvariantCulture);
        int length = int.Parse(spanText.Substring(separatorIndex + 1), CultureInfo.InvariantCulture);
        return new TextSpan(start, length);
    }

    private static async Task<Document> AddEventToSubscribeAsync(
        Document document,
        Diagnostic diagnostic,
        TextSpan eventsArgumentSpan,
        CancellationToken cancellationToken)
    {
        SyntaxNode root = (await document.GetSyntaxRootAsync(cancellationToken).ConfigureAwait(false))!;

        // The AddObserver invocation the diagnostic was reported on, and the events argument the
        // analyzer validated. Both spans come from the analyzer, so both nodes are present.
        InvocationExpressionSyntax addObserverCall = (InvocationExpressionSyntax)root.FindNode(diagnostic.Location.SourceSpan, getInnermostNodeForTie: true);
        ExpressionSyntax eventsArgument = (ExpressionSyntax)root.FindNode(eventsArgumentSpan, getInnermostNodeForTie: true);

        ExpressionSyntax updatedEventsArgument = AddEventNameToArrayExpression(eventsArgument, BuildEventNameExpression(addObserverCall), CodeFixHelpers.GetLanguageVersion(document));
        return document.WithSyntaxRoot(root.ReplaceNode(eventsArgument, updatedEventsArgument));
    }

    /// <summary>
    /// Builds the expression that names the event: the AddObserver receiver's EventName property.
    /// </summary>
    /// <param name="addObserverCall">The AddObserver invocation the diagnostic was reported on.</param>
    /// <returns>The event-name expression to add to the subscription.</returns>
    /// <remarks>
    /// The event is referenced through its ObservableEvent's EventName property rather than by inserting
    /// a hardcoded string literal, so the added argument does not itself trigger BIDI015. The receiver of
    /// the AddObserver call is exactly that ObservableEvent (for example driver.Log.OnEntryAdded).
    /// </remarks>
    private static ExpressionSyntax BuildEventNameExpression(InvocationExpressionSyntax addObserverCall)
    {
        ExpressionSyntax observableEvent = ((MemberAccessExpressionSyntax)addObserverCall.Expression).Expression;
        return SyntaxFactory.MemberAccessExpression(
            SyntaxKind.SimpleMemberAccessExpression,
            observableEvent.WithoutTrivia(),
            SyntaxFactory.IdentifierName("EventName"));
    }

    private static ExpressionSyntax AddEventNameToArrayExpression(ExpressionSyntax arrayExpression, ExpressionSyntax newElement, LanguageVersion languageVersion)
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

        // Otherwise this is the single-event constructor: new SubscribeCommandParameters("event1") or
        // new SubscribeCommandParameters(driver.Log.OnEntryAdded.EventName). The existing argument and
        // the new event become a collection expression.
        //
        // No type test guards this last case, because it cannot be reached with anything but a
        // string-typed expression: BIDI005 reports only when it could read every subscribed event name
        // (see TryGetSubscribedEventNames), and the shapes it can read are exactly an array creation, a
        // collection expression, and a string constant or ObservableEvent.EventName access — the first
        // two of which are handled above. A shape it cannot read makes the subscription set unknowable
        // and produces no diagnostic, so the fix is never asked about it.
        //
        // A collection expression is the natural spelling, but it is C# 12. Emitting one into a project
        // on an older language version hands the developer a fix that does not compile, and C# 7.3 is
        // the default for the netstandard2.0 and net472 consumers this library supports. The implicit
        // array form means the same thing, compiles on every version, and is one of the shapes the
        // analyzer already reads back, so it is used wherever a collection expression is not available.
        if (languageVersion < LanguageVersion.CSharp12)
        {
            return SyntaxFactory.ImplicitArrayCreationExpression(
                SyntaxFactory.InitializerExpression(
                    SyntaxKind.ArrayInitializerExpression,
                    SyntaxFactory.SeparatedList<ExpressionSyntax>(new SyntaxNodeOrToken[]
                    {
                        arrayExpression.WithoutTrivia(),
                        SyntaxFactory.Token(SyntaxKind.CommaToken).WithTrailingTrivia(SyntaxFactory.Space),
                        newElement,
                    })));
        }

        return SyntaxFactory.CollectionExpression(
            SyntaxFactory.SeparatedList<CollectionElementSyntax>(new SyntaxNodeOrToken[]
            {
                SyntaxFactory.ExpressionElement(arrayExpression.WithoutTrivia()),
                SyntaxFactory.Token(SyntaxKind.CommaToken).WithTrailingTrivia(SyntaxFactory.Space),
                SyntaxFactory.ExpressionElement(newElement),
            }));
    }
}
