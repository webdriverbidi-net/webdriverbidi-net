// <copyright file="AnalyzerSymbolHelpers.cs" company="WebDriverBiDi.NET Committers">
// Copyright (c) WebDriverBiDi.NET Committers. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.
// </copyright>

namespace WebDriverBiDi.Analyzers;

using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;
using Microsoft.CodeAnalysis.Text;

/// <summary>
/// Helper methods for identifying WebDriver BiDi driver-related symbols.
/// </summary>
internal static class AnalyzerSymbolHelpers
{
    /// <summary>
    /// The name of the diagnostic property that BIDI007 and BIDI023 set on a diagnostic for an operation that
    /// runs before an <c>async</c> handler's first <c>await</c>, telling their code fix that awaiting first is
    /// what moves the operation off the dispatching thread.
    /// </summary>
    internal const string RunsBeforeFirstAwaitPropertyName = "RunsBeforeFirstAwait";

    /// <summary>
    /// The diagnostic properties that mark an operation running before an <c>async</c> handler's first
    /// <c>await</c>.
    /// </summary>
    internal static readonly ImmutableDictionary<string, string?> RunsBeforeFirstAwaitProperties =
        ImmutableDictionary<string, string?>.Empty.Add(RunsBeforeFirstAwaitPropertyName, "true");

    /// <summary>
    /// Determines whether the symbol represents the driver: <c>BiDiDriver</c> itself, or a type implementing
    /// <c>IBiDiModuleHost</c> or <c>IBiDiDriverLifecycleManager</c>.
    /// </summary>
    /// <param name="type">The symbol to inspect.</param>
    /// <returns><see langword="true"/> if the symbol represents the driver; otherwise <see langword="false"/>.</returns>
    /// <remarks>
    /// The driver's capabilities are split across the two interfaces: commands and event registration are
    /// declared on <c>IBiDiModuleHost</c>, and starting, stopping and disposal on
    /// <c>IBiDiDriverLifecycleManager</c>. Their member names do not overlap, so one predicate serves callers
    /// that look for either kind of member.
    /// </remarks>
    internal static bool IsCommandExecutorType(ITypeSymbol? type)
    {
        return HasTypeOrBaseOrInterface(type, "BiDiDriver", "IBiDiModuleHost", "IBiDiDriverLifecycleManager");
    }

    /// <summary>
    /// Determines whether the symbol represents a driver configuration capability.
    /// </summary>
    /// <param name="type">The symbol to inspect.</param>
    /// <returns><see langword="true"/> if the symbol represents a driver configuration capability; otherwise <see langword="false"/>.</returns>
    internal static bool IsDriverConfigurationType(ITypeSymbol? type)
    {
        return HasTypeOrBaseOrInterface(type, "BiDiDriver", "IBiDiDriverConfiguration");
    }

    /// <summary>
    /// Determines whether the given AddObserver invocation has the RunHandlerAsynchronously option.
    /// </summary>
    /// <param name="context">The analysis context.</param>
    /// <param name="invocation">The AddObserver invocation to inspect.</param>
    /// <returns><see langword="true"/> if the RunHandlerAsynchronously option is present; otherwise <see langword="false"/>.</returns>
    internal static bool HasRunHandlerAsynchronouslyOption(SyntaxNodeAnalysisContext context, InvocationExpressionSyntax invocation)
    {
        foreach (ArgumentSyntax argument in invocation.ArgumentList.Arguments)
        {
            ITypeSymbol? argType = context.SemanticModel.GetTypeInfo(argument.Expression).Type;
            if (argType?.Name != "ObservableEventHandlerOptions")
            {
                continue;
            }

            // Resolve the option value semantically rather than by source text, which fails when the
            // option is passed through a variable. RunHandlerAsynchronously has the underlying value 1.
            // A non-constant argument (for example a variable) cannot be resolved at compile time, so
            // treat it as present to avoid a false positive on code that does opt in.
            Optional<object?> constantValue = context.SemanticModel.GetConstantValue(argument.Expression);
            if (!constantValue.HasValue || constantValue.Value is int and 1)
            {
                return true;
            }
        }

        return false;
    }

    /// <summary>
    /// Determines whether an <c>AddObserver</c> invocation is bound to the overload whose handler is an
    /// <c>Action&lt;T&gt;</c>.
    /// </summary>
    /// <param name="addObserverMethod">The resolved AddObserver overload.</param>
    /// <returns><see langword="true"/> if the handler parameter is an <c>Action&lt;T&gt;</c>; otherwise <see langword="false"/>.</returns>
    /// <remarks>
    /// With <c>RunHandlerAsynchronously</c> the library queues the whole of such a handler to the thread pool,
    /// so none of it runs on the thread dispatching the event. A <c>Task</c>-returning handler is different:
    /// the option detaches only the task it returns, so the code that runs before the handler returns that
    /// task still runs on the dispatching thread.
    /// </remarks>
    internal static bool IsBoundToActionOverload(IMethodSymbol addObserverMethod)
    {
        return addObserverMethod.Parameters[0].Type.Name == "Action";
    }

    /// <summary>
    /// Determines whether a handler passed to <c>AddObserver</c> is <c>async</c>: an <c>async</c> lambda or
    /// anonymous method, or a method group that resolves to an <c>async</c> method.
    /// </summary>
    /// <param name="context">The analysis context.</param>
    /// <param name="handler">The handler expression.</param>
    /// <returns><see langword="true"/> if the handler is <c>async</c>; otherwise <see langword="false"/>.</returns>
    internal static bool IsAsyncHandler(SyntaxNodeAnalysisContext context, ExpressionSyntax handler)
    {
        return handler switch
        {
            AnonymousFunctionExpressionSyntax anonymousFunction => anonymousFunction.AsyncKeyword.IsKind(SyntaxKind.AsyncKeyword),
            IdentifierNameSyntax or MemberAccessExpressionSyntax => context.SemanticModel.GetSymbolInfo(handler).Symbol switch
            {
                IMethodSymbol method => (method.PartialImplementationPart ?? method).IsAsync,
                ILocalSymbol local => GetLambdaHeldInLocal(local)?.AsyncKeyword.IsKind(SyntaxKind.AsyncKeyword) == true,
                _ => false,
            },
            _ => false,
        };
    }

    /// <summary>
    /// Gets the lambda or anonymous method a handler expression names through a local variable, when the local is
    /// declared with one and never given another value.
    /// </summary>
    /// <param name="semanticModel">The semantic model for the handler expression's tree.</param>
    /// <param name="handler">The handler expression.</param>
    /// <returns>The anonymous function, or <see langword="null"/> when the expression does not name such a local.</returns>
    /// <remarks>
    /// <c>Action&lt;EntryAddedEventArgs&gt; handler = e =&gt; Thread.Sleep(1000); observable.AddObserver(handler);</c> passes
    /// exactly the handler an inline lambda would, so the rules that inspect a handler's body read it through the local.
    /// A local that is assigned again, or passed by reference, may hold a different delegate by the time it is passed,
    /// and is left unresolved rather than guessed at.
    /// </remarks>
    internal static AnonymousFunctionExpressionSyntax? GetLambdaHeldInLocal(SemanticModel semanticModel, ExpressionSyntax handler)
    {
        return handler is IdentifierNameSyntax && semanticModel.GetSymbolInfo(handler).Symbol is ILocalSymbol local
            ? GetLambdaHeldInLocal(local)
            : null;
    }

    /// <summary>
    /// Gets the descendant nodes of the scope a local variable is declared in, which is everywhere the local can be
    /// mentioned.
    /// </summary>
    /// <param name="declarator">The local's declarator.</param>
    /// <returns>The descendant nodes of the local's scope.</returns>
    /// <remarks>
    /// A local's scope is the block that declares it; for a local declared in a switch section it is the whole switch
    /// block, and for a top-level statement it is the program's global statements. A local declared by a
    /// <c>for</c>, <c>using</c> or <c>fixed</c> statement is searched through the block enclosing that statement,
    /// which contains its real scope. Searching the scope rather than the whole member keeps a same-named local in a
    /// sibling scope from being mistaken for this one.
    /// </remarks>
    internal static IEnumerable<SyntaxNode> GetLocalScopeDescendantNodes(VariableDeclaratorSyntax declarator)
    {
        // declarator → variable declaration → declaring statement → the statement's container. Every local is
        // declared inside a statement, and every statement has a container, so the chain is always complete.
        SyntaxNode scope = declarator.Parent!.Parent!.Parent!;
        if (scope is SwitchSectionSyntax or GlobalStatementSyntax)
        {
            scope = scope.Parent!;
        }

        return scope is CompilationUnitSyntax ? GetBodyDescendantNodes(scope) : scope.DescendantNodes();
    }

    private static AnonymousFunctionExpressionSyntax? GetLambdaHeldInLocal(ILocalSymbol local)
    {
        if (local.DeclaringSyntaxReferences[0].GetSyntax() is not VariableDeclaratorSyntax { Initializer.Value: AnonymousFunctionExpressionSyntax anonymousFunction } declarator)
        {
            return null;
        }

        string name = declarator.Identifier.ValueText;
        bool isRebound = GetLocalScopeDescendantNodes(declarator)
            .OfType<IdentifierNameSyntax>()
            .Any(identifier => identifier.Identifier.ValueText == name && identifier.Parent switch
            {
                AssignmentExpressionSyntax assignment => assignment.Left == identifier,
                ArgumentSyntax argument => !argument.RefKindKeyword.IsKind(SyntaxKind.None),
                _ => false,
            });

        return isRebound ? null : anonymousFunction;
    }

    /// <summary>
    /// Determines whether a call to a synchronization method is given an explicit timeout of zero, which makes it
    /// return at once rather than wait: <c>semaphore.Wait(0)</c>, <c>handle.WaitOne(TimeSpan.Zero)</c>.
    /// </summary>
    /// <param name="semanticModel">The semantic model for the invocation's tree.</param>
    /// <param name="invocation">The invocation.</param>
    /// <param name="method">The method the invocation binds to.</param>
    /// <returns><see langword="true"/> if the timeout argument is the constant zero or <see cref="TimeSpan.Zero"/>; otherwise <see langword="false"/>.</returns>
    /// <remarks>
    /// The timeout is recognized by its parameter name, <c>millisecondsTimeout</c> or <c>timeout</c>, which is what
    /// every waiting method in the framework calls it. <c>Thread.Sleep</c> also names its parameter
    /// <c>millisecondsTimeout</c>, but a sleep is not a poll, and the caller excludes it. An argument beyond the
    /// method's parameter list belongs to a <c>params</c> array (<c>Task.WaitAll(first, second)</c>), which is never
    /// a timeout.
    /// </remarks>
    internal static bool HasZeroTimeoutArgument(SemanticModel semanticModel, InvocationExpressionSyntax invocation, IMethodSymbol method)
    {
        SeparatedSyntaxList<ArgumentSyntax> arguments = invocation.ArgumentList.Arguments;
        for (int i = 0; i < arguments.Count && i < method.Parameters.Length; i++)
        {
            ArgumentSyntax argument = arguments[i];
            string parameterName = argument.NameColon is null ? method.Parameters[i].Name : argument.NameColon.Name.Identifier.ValueText;
            if (parameterName is not ("millisecondsTimeout" or "timeout"))
            {
                continue;
            }

            Optional<object?> constantValue = semanticModel.GetConstantValue(argument.Expression);
            return (constantValue.HasValue && Equals(constantValue.Value, 0))
                || (semanticModel.GetSymbolInfo(argument.Expression).Symbol is IFieldSymbol field && field.ToDisplayString() == "System.TimeSpan.Zero");
        }

        return false;
    }

    /// <summary>
    /// Gets a predicate telling whether a node of an <c>async</c> handler's body runs before the handler first
    /// yields, which is to say on the thread that invoked the handler.
    /// </summary>
    /// <param name="handlerBody">The body of the <c>async</c> handler.</param>
    /// <returns>A predicate that is <see langword="true"/> for a node that runs before the handler first yields.</returns>
    /// <remarks>
    /// <para>
    /// An <c>async</c> handler runs synchronously until it awaits something that has not completed, and
    /// <c>RunHandlerAsynchronously</c> detaches only what follows. Whether a particular <c>await</c> completes
    /// synchronously is decided at run time, so the first one is taken as the point the handler yields. Its
    /// operand is evaluated before it yields, so a node inside the operand runs before that point, while a node
    /// that contains the <c>await</c> (a call taking the awaited value as an argument, say) runs only after it.
    /// An <c>await foreach</c> yields once its collection expression has been evaluated.
    /// </para>
    /// <para>
    /// The first yield is the one whose operand is finished earliest, which for an <c>await</c> nested inside
    /// another is the inner one. An <c>await</c> inside a nested function does not make the handler yield, and
    /// an <c>await using</c> declaration yields only when its scope ends, so neither counts. The first
    /// <c>await</c> may sit in a branch that does not run, in which case code after it is treated as offloaded
    /// although it may not be; that can only miss a report, never make a wrong one.
    /// </para>
    /// </remarks>
    internal static Func<SyntaxNode, bool> GetRunsBeforeFirstYield(SyntaxNode handlerBody)
    {
        SyntaxNode? firstYield = null;
        int firstYieldPosition = int.MaxValue;
        foreach (SyntaxNode node in handlerBody.DescendantNodesAndSelf(DoesNotBeginNestedFunction))
        {
            int yieldPosition = node switch
            {
                AwaitExpressionSyntax awaitExpression => awaitExpression.Span.End,
                CommonForEachStatementSyntax forEachStatement when forEachStatement.AwaitKeyword.IsKind(SyntaxKind.AwaitKeyword) => forEachStatement.Expression.Span.End,
                _ => int.MaxValue,
            };

            if (yieldPosition < firstYieldPosition)
            {
                firstYield = node;
                firstYieldPosition = yieldPosition;
            }
        }

        if (firstYield is null)
        {
            return static _ => true;
        }

        TextSpan firstYieldSpan = firstYield.Span;
        return node => node.SpanStart < firstYieldPosition && !node.Span.Contains(firstYieldSpan);
    }

    /// <summary>
    /// Determines whether a node is an operation whose right operand is evaluated only on some paths: a conditional
    /// logical operator (<c>&amp;&amp;</c> or <c>||</c>), the null-coalescing operator (<c>??</c>), or a
    /// null-coalescing assignment (<c>??=</c>).
    /// </summary>
    /// <param name="node">The node to inspect.</param>
    /// <returns><see langword="true"/> if the node is such an operation; otherwise <see langword="false"/>.</returns>
    /// <remarks>
    /// The rules that track a driver's state walk the right operand of these operations as a path that may not run,
    /// exactly as they walk the branch of an <c>if</c> statement without an <c>else</c>.
    /// </remarks>
    internal static bool IsShortCircuitOperation(SyntaxNode node)
    {
        return node.IsKind(SyntaxKind.LogicalAndExpression)
            || node.IsKind(SyntaxKind.LogicalOrExpression)
            || node.IsKind(SyntaxKind.CoalesceExpression)
            || node.IsKind(SyntaxKind.CoalesceAssignmentExpression);
    }

    /// <summary>
    /// Gets the body syntax node for a handler expression passed to AddObserver.
    /// Returns the body of an anonymous function, or resolves a method reference to its body.
    /// </summary>
    /// <param name="context">The analysis context.</param>
    /// <param name="expression">The handler expression.</param>
    /// <returns>The body syntax node, or <see langword="null"/> if it cannot be resolved.</returns>
    /// <remarks>
    /// Matching <see cref="AnonymousFunctionExpressionSyntax"/> covers all three spellings of an inline
    /// handler at once: a simple lambda, a parenthesized lambda, and an anonymous method written with the
    /// <c>delegate</c> keyword. All three declare their body on that base type. Matching the two lambda
    /// forms individually would silently exempt <c>delegate (…) { … }</c> handlers from every rule that
    /// inspects a handler body, which is a legal spelling with exactly the same hazards.
    /// </remarks>
    internal static SyntaxNode? GetHandlerBody(SyntaxNodeAnalysisContext context, ExpressionSyntax expression)
    {
        return expression switch
        {
            AnonymousFunctionExpressionSyntax anonymousFunction => anonymousFunction.Body,
            IdentifierNameSyntax identifierName => GetMethodBodyFromSymbol(context, identifierName),
            MemberAccessExpressionSyntax memberAccess => GetMethodBodyFromSymbol(context, memberAccess),
            _ => null,
        };
    }

    /// <summary>
    /// Gets the semantic model that can answer questions about a node, which is the context's own model
    /// when the node is in the tree being analyzed and the compilation's model for the node's tree
    /// otherwise.
    /// </summary>
    /// <param name="context">The analysis context.</param>
    /// <param name="node">The node to be queried.</param>
    /// <returns>The semantic model for the node's syntax tree.</returns>
    /// <remarks>
    /// <see cref="GetHandlerBody"/> resolves a method-group handler to its declaration, which may live in
    /// another file: a partial-class part, a base class, or a static helper. A semantic model answers only
    /// for nodes of its own tree — asking it about a node from another tree throws — so a rule that walks
    /// a body obtained that way must query the model for the body's tree rather than its own.
    /// </remarks>
    internal static SemanticModel GetSemanticModelFor(SyntaxNodeAnalysisContext context, SyntaxNode node)
    {
        return ReferenceEquals(node.SyntaxTree, context.Node.SyntaxTree)
            ? context.SemanticModel
            : context.Compilation.GetSemanticModel(node.SyntaxTree);
    }

    /// <summary>
    /// Gets the identifier at the root of a member access chain, looking through the wrappers a receiver may
    /// carry: <c>driver</c> for <c>driver.StartAsync</c>, <c>driver!.StartAsync</c>, <c>(driver).StartAsync</c>,
    /// <c>((IBiDiDriverLifecycleManager)driver).StartAsync</c>, <c>driver?.StartAsync</c> and
    /// <c>driver?.Session.StatusAsync</c>.
    /// </summary>
    /// <param name="expression">
    /// The member access to resolve, typically an invocation's <see cref="InvocationExpressionSyntax.Expression"/>.
    /// </param>
    /// <param name="memberDepth">
    /// When this method returns, the number of member accesses between the root identifier and the end of the chain:
    /// 1 for <c>driver.StartAsync</c>, 2 for <c>driver.Session.StatusAsync</c>.
    /// </param>
    /// <returns>
    /// The root identifier, or <see langword="null"/> when the expression is not a member access, or its chain does not
    /// root in a simple identifier.
    /// </returns>
    /// <remarks>
    /// Every wrapper names the same object as the bare identifier, so a rule that tracks a variable's state must
    /// read <c>driver!.StartAsync()</c> as it reads <c>driver.StartAsync()</c>. A call made through a null-conditional
    /// access happens only when the receiver is not null, which is also the only case in which a later call on it
    /// can succeed, so it is read as happening. A member binding (<c>.StartAsync</c> in <c>driver?.StartAsync</c>)
    /// takes its receiver from the nearest enclosing conditional access whose non-null branch contains it.
    /// </remarks>
    internal static IdentifierNameSyntax? GetMemberChainRoot(ExpressionSyntax expression, out int memberDepth)
    {
        memberDepth = 0;
        ExpressionSyntax current = expression;
        while (true)
        {
            switch (current)
            {
                case MemberAccessExpressionSyntax memberAccess:
                    memberDepth++;
                    current = memberAccess.Expression;
                    break;
                case MemberBindingExpressionSyntax memberBinding:
                    memberDepth++;
                    current = memberBinding.Ancestors()
                        .OfType<ConditionalAccessExpressionSyntax>()
                        .First(conditionalAccess => conditionalAccess.WhenNotNull.Span.Contains(memberBinding.Span))
                        .Expression;
                    break;
                case ParenthesizedExpressionSyntax parenthesized:
                    current = parenthesized.Expression;
                    break;
                case CastExpressionSyntax cast:
                    current = cast.Expression;
                    break;
                case PostfixUnaryExpressionSyntax postfix:
                    current = postfix.Operand;
                    break;
                case IdentifierNameSyntax identifier when memberDepth > 0:
                    return identifier;
                default:
                    return null;
            }
        }
    }

    /// <summary>
    /// Gets the member binding a conditional access applies directly to its receiver: <c>.StartAsync</c> in
    /// <c>driver?.StartAsync(url)</c> and in <c>driver?.StartAsync(url).ConfigureAwait(false)</c>.
    /// </summary>
    /// <param name="conditionalAccess">The conditional access.</param>
    /// <returns>
    /// The member binding on the conditional access's receiver, or <see langword="null"/> when the receiver is
    /// indexed instead (<c>items?[0]</c>).
    /// </returns>
    /// <remarks>
    /// The non-null branch always begins with the binding on the receiver, so the branch's first token, the
    /// <c>.</c> of a member binding or the <c>[</c> of an element binding, identifies it. A later binding in the
    /// branch belongs to a conditional access nested inside it.
    /// </remarks>
    internal static MemberBindingExpressionSyntax? GetReceiverMemberBinding(ConditionalAccessExpressionSyntax conditionalAccess)
    {
        return conditionalAccess.WhenNotNull.GetFirstToken().Parent as MemberBindingExpressionSyntax;
    }

    /// <summary>
    /// Peels the wrappers a mention of a variable may carry before it reaches the construct that decides
    /// what happens to it: parentheses, a cast, a conditional expression, and the null-forgiving operator.
    /// </summary>
    /// <param name="expression">The mention of the variable.</param>
    /// <returns>The outermost wrapper, or <paramref name="expression"/> itself when it is not wrapped.</returns>
    /// <remarks>
    /// <c>Helper((driver))</c>, <c>Helper(driver!)</c>, <c>Helper((IBiDiDriverLifecycleManager)driver)</c> and
    /// <c>Helper(flag ? driver : other)</c> all hand the driver to the helper exactly as <c>Helper(driver)</c>
    /// does. A classification that looked only at the identifier's immediate parent would read each of them
    /// as a mere use. Any postfix operator qualifies: the null-forgiving operator is the only one a driver
    /// or an observer can carry, since neither has an increment or decrement.
    /// </remarks>
    internal static SyntaxNode PeelExpressionWrappers(SyntaxNode expression)
    {
        SyntaxNode current = expression;
        while (current.Parent is ParenthesizedExpressionSyntax
            or CastExpressionSyntax
            or ConditionalExpressionSyntax
            or PostfixUnaryExpressionSyntax)
        {
            current = current.Parent;
        }

        return current;
    }

    /// <summary>
    /// Determines whether a type is a library module: it derives from the abstract <c>Module</c> base
    /// class, whatever it is named — a custom <c>class GoogleCdp : Module</c> is registered and used
    /// exactly as one named <c>GoogleCdpModule</c> would be — or it is a <c>"*Module"</c> type
    /// declared within the <c>WebDriverBiDi</c> namespace. Unrelated user types that merely end in
    /// <c>"Module"</c> neither derive from <c>Module</c> nor live in the library's namespace, and are
    /// not matched.
    /// </summary>
    /// <param name="type">The type to inspect.</param>
    /// <returns><see langword="true"/> if the type is a library module; otherwise <see langword="false"/>.</returns>
    internal static bool IsLibraryModuleType(ITypeSymbol? type)
    {
        return type is INamedTypeSymbol named
            && (IsModuleSubclass(named) || (named.Name.EndsWith("Module", System.StringComparison.Ordinal) && IsInWebDriverBiDiNamespace(named)));
    }

    /// <summary>
    /// Determines whether a type belongs to the WebDriverBiDi library (declared within the
    /// <c>WebDriverBiDi</c> namespace). Used both to recognize library modules and to avoid matching a
    /// user's own type that happens to share a member name or shape with a library type.
    /// </summary>
    /// <param name="type">The type to inspect.</param>
    /// <returns><see langword="true"/> if the type is declared in the WebDriverBiDi namespace; otherwise <see langword="false"/>.</returns>
    internal static bool IsInWebDriverBiDiNamespace(INamedTypeSymbol type)
    {
        // The library's root namespace exactly, or one of its sub-namespaces. A bare prefix match
        // would also claim a user's own namespace that merely begins with the same characters
        // (WebDriverBiDiExtensions, for example), branding the user's types with this library's
        // diagnostics; asking whether the outermost enclosing namespace *is* WebDriverBiDi draws
        // that distinction exactly, and does so without formatting the fully qualified name.
        // Composing the name (ContainingNamespace.ToString()) allocates a string on every call, and
        // this runs once per base type and per implemented interface of every symbol the analyzers
        // inspect, so the walk is the cheaper of the two identical tests.
        //
        // A named type always has a containing namespace (the global namespace at worst), and only
        // the global namespace has no container, so the loop always terminates.
        INamespaceSymbol containingNamespace = type.ContainingNamespace!;
        if (containingNamespace.IsGlobalNamespace)
        {
            return false;
        }

        while (!containingNamespace.ContainingNamespace.IsGlobalNamespace)
        {
            containingNamespace = containingNamespace.ContainingNamespace;
        }

        return containingNamespace.Name == "WebDriverBiDi";
    }

    /// <summary>
    /// Determines whether a type is the library's type of the given simple name, rather than an
    /// unrelated type that merely shares the name.
    /// </summary>
    /// <param name="type">The type to inspect.</param>
    /// <param name="name">The simple name of the library type.</param>
    /// <returns><see langword="true"/> if the type is the library's; otherwise <see langword="false"/>.</returns>
    /// <remarks>
    /// Matching on the simple name alone claims types this library has nothing to do with: a user class
    /// deriving from <c>Autofac.Module</c>, for example, would be treated as a WebDriver BiDi module and
    /// reported by BIDI010 at Error severity. Requiring the WebDriverBiDi namespace as well is the same
    /// guard <see cref="IsInWebDriverBiDiNamespace"/> already applies to <c>BiDiDriver</c> and
    /// its driver interfaces.
    /// </remarks>
    internal static bool IsLibraryTypeNamed(ITypeSymbol? type, string name)
    {
        return type is INamedTypeSymbol named
            && named.Name == name
            && IsInWebDriverBiDiNamespace(named);
    }

    /// <summary>
    /// Determines whether an invocation could be a call to one of the named methods, judged from
    /// syntax alone. Used as a pre-filter ahead of the semantic model, whose <c>GetSymbolInfo</c> is
    /// far more expensive than a name comparison and would otherwise be run for every invocation in
    /// every compiled file.
    /// </summary>
    /// <param name="invocation">The invocation to inspect.</param>
    /// <param name="methodNames">The method names of interest.</param>
    /// <returns><see langword="false"/> only when the invocation definitely names none of the methods; otherwise <see langword="true"/>.</returns>
    /// <remarks>
    /// This is a conservative filter, never an answer: it returns <see langword="true"/> whenever the
    /// invoked name is not syntactically evident (a delegate produced by another expression, say), so
    /// the caller still resolves the symbol and applies its own authoritative name test. Names are
    /// compared by <c>ValueText</c> rather than <c>Text</c> so that a verbatim identifier
    /// (<c>@ExecuteCommandAsync</c>) is not filtered out here and silently robbed of its diagnostic.
    /// </remarks>
    internal static bool CouldInvokeAnyOf(InvocationExpressionSyntax invocation, string[] methodNames)
    {
        SimpleNameSyntax? invokedName = invocation.Expression switch
        {
            MemberAccessExpressionSyntax memberAccess => memberAccess.Name,
            MemberBindingExpressionSyntax memberBinding => memberBinding.Name,
            SimpleNameSyntax simpleName => simpleName,
            _ => null,
        };

        return invokedName is null || methodNames.Contains(invokedName.Identifier.ValueText);
    }

    /// <summary>
    /// The syntax kinds that carry an executable body the intra-procedural analyzers examine: a method
    /// declaration, a constructor declaration, and a compilation unit (whose global statements form the
    /// body of a top-level program). Registering an analyzer for all three lets it fire in constructors
    /// and top-level programs, not only in methods.
    /// </summary>
    internal static readonly SyntaxKind[] ExecutableBodyKinds =
    [
        SyntaxKind.MethodDeclaration,
        SyntaxKind.ConstructorDeclaration,
        SyntaxKind.CompilationUnit,
    ];

    /// <summary>
    /// Gets the block containing a member's executable statements: a method's or constructor's body.
    /// Returns <see langword="null"/> for a compilation unit (its statements are global statements) or
    /// a body-less member.
    /// </summary>
    /// <param name="node">The declaration node.</param>
    /// <returns>The body block, or <see langword="null"/>.</returns>
    internal static BlockSyntax? GetBodyBlock(SyntaxNode node)
    {
        return node switch
        {
            MethodDeclarationSyntax method => method.Body,
            ConstructorDeclarationSyntax constructor => constructor.Body,
            _ => null,
        };
    }

    /// <summary>
    /// Gets the top-level executable statements of a method or constructor body, or the global
    /// statements of a top-level program, in source order.
    /// </summary>
    /// <param name="node">The declaration or compilation-unit node.</param>
    /// <returns>The top-level statements.</returns>
    internal static IReadOnlyList<StatementSyntax> GetTopLevelStatements(SyntaxNode node)
    {
        if (GetBodyBlock(node) is { } body)
        {
            return body.Statements;
        }

        if (node is CompilationUnitSyntax compilationUnit)
        {
            return compilationUnit.Members.OfType<GlobalStatementSyntax>().Select(globalStatement => globalStatement.Statement).ToArray();
        }

        return [];
    }

    /// <summary>
    /// Gets every executable statement (including nested statements) of a method or constructor body,
    /// or of a top-level program's global statements, in source order.
    /// </summary>
    /// <param name="node">The declaration or compilation-unit node.</param>
    /// <returns>The statements.</returns>
    internal static IEnumerable<StatementSyntax> GetAllStatements(SyntaxNode node)
    {
        if (GetBodyBlock(node) is { } body)
        {
            return body.DescendantNodes().OfType<StatementSyntax>();
        }

        if (node is CompilationUnitSyntax compilationUnit)
        {
            return compilationUnit.Members
                .OfType<GlobalStatementSyntax>()
                .SelectMany(globalStatement => globalStatement.Statement.DescendantNodesAndSelf().OfType<StatementSyntax>());
        }

        return [];
    }

    /// <summary>
    /// Determines whether descending into the children of the given node stays within the code that
    /// executes at the node's textual position; returns <see langword="false"/> for nodes that begin
    /// a nested function (lambdas, anonymous methods, and local functions), whose bodies run only
    /// when the delegate is invoked.
    /// </summary>
    /// <param name="node">The node being considered for descent.</param>
    /// <returns><see langword="true"/> to descend into the node's children; otherwise, <see langword="false"/>.</returns>
    internal static bool DoesNotBeginNestedFunction(SyntaxNode node)
    {
        return node is not (
            SimpleLambdaExpressionSyntax or
            ParenthesizedLambdaExpressionSyntax or
            AnonymousMethodExpressionSyntax or
            LocalFunctionStatementSyntax);
    }

    /// <summary>
    /// Gets every descendant node of a method or constructor body, or of a top-level program's global
    /// statements, in source order. Used by analyzers that search the whole body for specific node
    /// kinds (invocations, declarations, and so on) rather than iterating statements.
    /// </summary>
    /// <param name="node">The declaration or compilation-unit node.</param>
    /// <returns>The descendant nodes.</returns>
    internal static IEnumerable<SyntaxNode> GetBodyDescendantNodes(SyntaxNode node)
    {
        if (GetBodyBlock(node) is { } body)
        {
            return body.DescendantNodes();
        }

        // Expression-bodied members: the single arrow expression is the executable body.
        ArrowExpressionClauseSyntax? expressionBody = node switch
        {
            MethodDeclarationSyntax method => method.ExpressionBody,
            ConstructorDeclarationSyntax constructor => constructor.ExpressionBody,
            _ => null,
        };

        if (expressionBody is not null)
        {
            return expressionBody.Expression.DescendantNodesAndSelf();
        }

        if (node is CompilationUnitSyntax compilationUnit)
        {
            return compilationUnit.Members
                .OfType<GlobalStatementSyntax>()
                .SelectMany(globalStatement => globalStatement.Statement.DescendantNodesAndSelf());
        }

        return [];
    }

    /// <summary>
    /// Resolves a local that holds one of a driver's modules back to that driver's variable name, so
    /// that a command called through the alias is judged against the driver it belongs to.
    /// </summary>
    /// <param name="receiver">The identifier the command was invoked on.</param>
    /// <param name="body">The member body being analyzed.</param>
    /// <param name="semanticModel">The semantic model for the body.</param>
    /// <returns>The driver variable's name, or <see langword="null"/> when the identifier is not an alias of a module of a local driver.</returns>
    /// <remarks>
    /// Only an alias whose value is fixed is resolved: a local declared as
    /// <c>BrowsingContextModule context = driver.BrowsingContext;</c> and never assigned again. An
    /// alias that is rebound may hold a different driver's module by the time it is used, and the
    /// rules that call this report at Error severity, so an alias that is ever assigned is left
    /// unresolved rather than guessed at. The driver name this returns is looked up in the caller's
    /// own per-position state, so the alias decides <em>which</em> driver a call belongs to while that
    /// driver's lifecycle state is still judged where the call appears.
    /// </remarks>
    internal static string? GetDriverOfModuleAlias(IdentifierNameSyntax receiver, SyntaxNode body, SemanticModel semanticModel)
    {
        // Only a local declared with an initializer can be an alias. A field, a parameter, or a
        // foreach variable is declared by syntax this cannot read a module expression out of.
        if (semanticModel.GetSymbolInfo(receiver).Symbol is not ILocalSymbol local
            || local.DeclaringSyntaxReferences[0].GetSyntax() is not VariableDeclaratorSyntax { Initializer: not null } declarator)
        {
            return null;
        }

        // The initializer has to read a member of a driver-typed identifier: `driver.BrowsingContext`, or
        // `driver!.BrowsingContext` through a wrapped receiver.
        if (GetMemberChainRoot(declarator.Initializer.Value, out int memberDepth) is not IdentifierNameSyntax driverIdentifier
            || memberDepth != 1
            || !IsCommandExecutorType(semanticModel.GetTypeInfo(driverIdentifier).Type))
        {
            return null;
        }

        string aliasName = receiver.Identifier.ValueText;
        bool isRebound = GetBodyDescendantNodes(body)
            .OfType<AssignmentExpressionSyntax>()
            .Any(assignment => assignment.Left is IdentifierNameSyntax target && target.Identifier.ValueText == aliasName);

        return isRebound ? null : driverIdentifier.Identifier.ValueText;
    }

    /// <summary>
    /// Collects the names of variables that the given body hands to other code, so that a rule which
    /// tracks a variable's state across a single member can stop tracking them.
    /// </summary>
    /// <param name="body">The member body being analyzed.</param>
    /// <param name="includeArguments">
    /// Whether passing a variable as an argument counts as handing it on. A rule that judges each call it is passed to
    /// on its own terms (the command that sends it, say) passes <see langword="false"/>.
    /// </param>
    /// <returns>The set of names whose state cannot be known from this member alone.</returns>
    /// <remarks>
    /// A variable passed to a method, returned, stored elsewhere, or used to initialize another
    /// variable can be operated on by code the analyzer cannot see, so its state after that point is
    /// unknown. Rules that walk a member textually collect these names up front rather than at the
    /// point of escape, because the other code may run before or after the tracked call textually.
    /// Being on the left of an assignment is not an escape: that rebinds the name rather than handing
    /// the object out, and a rule that tracks assignments handles it directly.
    /// </remarks>
    internal static HashSet<string> FindVariablesHandedToOtherCode(SyntaxNode body, bool includeArguments = true)
    {
        HashSet<string> escapedNames = [];
        foreach (IdentifierNameSyntax identifier in GetBodyDescendantNodes(body).OfType<IdentifierNameSyntax>())
        {
            if (IsHandedToOtherCode(identifier, includeArguments))
            {
                escapedNames.Add(identifier.Identifier.ValueText);
            }
        }

        return escapedNames;
    }

    /// <summary>
    /// Determines whether the value of an expression is handed to code the enclosing member cannot see:
    /// returned, stored, passed as an argument, placed in a collection or initializer, or used to initialize
    /// another variable.
    /// </summary>
    /// <param name="expression">The expression whose value is examined.</param>
    /// <param name="includeArguments">Whether passing the value as an argument counts as handing it on.</param>
    /// <returns><see langword="true"/> if the value is handed to other code; otherwise <see langword="false"/>.</returns>
    internal static bool IsHandedToOtherCode(SyntaxNode expression, bool includeArguments = true)
    {
        // The value may be wrapped (parenthesized, cast, null-forgiven, or one arm of a conditional) before it
        // reaches the construct that hands it out.
        SyntaxNode mention = PeelExpressionWrappers(expression);
        return mention.Parent switch
        {
            // Returned to the caller: return observer; or yield return observer;
            ReturnStatementSyntax or YieldStatementSyntax => true,

            // Stored somewhere this member does not own: this.observer = observer;
            AssignmentExpressionSyntax assignment => assignment.Right == mention,

            // Passed to a method or constructor that may operate on it: BeginCapture(observer);
            ArgumentSyntax => includeArguments,

            // Placed in a collection expression or an initializer, or used to initialize another
            // variable that may be operated on under its own name.
            ExpressionElementSyntax or InitializerExpressionSyntax or EqualsValueClauseSyntax => true,

            _ => false,
        };
    }

    /// <summary>
    /// Collects the names of variables on which one of the given methods is called from inside a
    /// nested function (a lambda, an anonymous method, or a local function).
    /// </summary>
    /// <param name="body">The member body being analyzed.</param>
    /// <param name="methodNames">The names of the methods that change the state being tracked.</param>
    /// <returns>The set of names whose state a nested function can change.</returns>
    /// <remarks>
    /// A nested function runs when its delegate is invoked, not where it is written, so a call inside
    /// one changes the variable's state at a point a textual walk cannot place. Only calls that change
    /// the tracked state count: treating every capture as unknown would stop a rule reporting a genuine
    /// problem elsewhere in the same member.
    /// </remarks>
    internal static HashSet<string> FindVariablesChangedInsideNestedFunctions(SyntaxNode body, string[] methodNames)
    {
        HashSet<string> changedNames = [];
        foreach (InvocationExpressionSyntax invocation in GetBodyDescendantNodes(body).OfType<InvocationExpressionSyntax>())
        {
            if (invocation.Expression is not MemberAccessExpressionSyntax memberAccess ||
                memberAccess.Expression is not IdentifierNameSyntax receiver ||
                !methodNames.Contains(memberAccess.Name.Identifier.ValueText))
            {
                continue;
            }

            // The invocation came from the body's descendants, so the body is always an ancestor and
            // always stops the walk.
            bool insideNestedFunction = invocation.Ancestors()
                .TakeWhile(ancestor => ancestor != body)
                .Any(ancestor => !DoesNotBeginNestedFunction(ancestor));

            if (insideNestedFunction)
            {
                changedNames.Add(receiver.Identifier.ValueText);
            }
        }

        return changedNames;
    }

    /// <summary>
    /// The names of the methods that hand back a disposable handle for an event subscription.
    /// </summary>
    internal static readonly string[] EventSubscriptionHandleMethodNames = ["AddObserver", "AddDataCollector", "Subscribe"];

    /// <summary>
    /// Gets the name of the disposable handle type an invocation returns, for the three calls that
    /// hand one back.
    /// </summary>
    /// <param name="semanticModel">The semantic model for the document.</param>
    /// <param name="invocation">The invocation to inspect.</param>
    /// <returns>The handle type's name and the name of the method that returned it, or <see langword="null"/> when the call returns no handle.</returns>
    /// <remarks>
    /// <c>AddObserver</c> and <c>AddDataCollector</c> are recognised by their return types.
    /// <c>Subscribe</c> is not: <see cref="IObservable{T}"/> declares it as returning
    /// <see cref="IDisposable"/>, so the receiver is what identifies the call — an
    /// <see cref="IObservable{T}"/> of a library event-args type can only have come from this
    /// library's <c>ToObservable</c>.
    /// </remarks>
    internal static (string HandleTypeName, string MethodName)? GetEventSubscriptionHandle(SemanticModel semanticModel, InvocationExpressionSyntax invocation)
    {
        // Callers ask this of every invocation that initializes a local or is assigned to one, nearly none of which
        // name one of these methods; the syntactic check spares the semantic bind for all of them.
        if (!CouldInvokeAnyOf(invocation, EventSubscriptionHandleMethodNames)
            || semanticModel.GetSymbolInfo(invocation).Symbol is not IMethodSymbol method)
        {
            return null;
        }

        if (method.Name == "AddObserver" && IsLibraryTypeNamed(method.ReturnType, "EventObserver"))
        {
            return ("EventObserver", method.Name);
        }

        if (method.Name == "AddDataCollector" && IsLibraryTypeNamed(method.ReturnType, "EventDataCollector"))
        {
            return ("EventDataCollector", method.Name);
        }

        return method.Name == "Subscribe" && IsLibraryEventObservable(semanticModel, invocation)
            ? ("ObservableEventSubscription", method.Name)
            : null;
    }

    /// <summary>
    /// Determines whether an invocation's receiver is an observable over this library's event
    /// arguments.
    /// </summary>
    /// <param name="semanticModel">The semantic model for the document.</param>
    /// <param name="invocation">The invocation to inspect.</param>
    /// <returns><see langword="true"/> if the receiver is such an observable; otherwise <see langword="false"/>.</returns>
    private static bool IsLibraryEventObservable(SemanticModel semanticModel, InvocationExpressionSyntax invocation)
    {
        return invocation.Expression is MemberAccessExpressionSyntax memberAccess
            && semanticModel.GetTypeInfo(memberAccess.Expression).Type is INamedTypeSymbol { Name: "IObservable", TypeArguments.Length: 1 } observable
            && observable.TypeArguments[0] is INamedTypeSymbol eventArgsType
            && IsInWebDriverBiDiNamespace(eventArgsType);
    }

    /// <summary>
    /// Determines whether a Module type has <c>Module</c> anywhere in its base-type chain.
    /// </summary>
    /// <param name="type">The type to inspect.</param>
    /// <returns><see langword="true"/> if the type inherits from <c>Module</c>; otherwise <see langword="false"/>.</returns>
    internal static bool IsModuleSubclass(INamedTypeSymbol? type)
    {
        INamedTypeSymbol? current = type!.BaseType;
        while (current != null)
        {
            if (IsLibraryTypeNamed(current, "Module"))
            {
                return true;
            }

            current = current.BaseType;
        }

        return false;
    }

    private static SyntaxNode? GetMethodBodyFromSymbol(SyntaxNodeAnalysisContext context, ExpressionSyntax expression)
    {
        ISymbol? symbol = context.SemanticModel.GetSymbolInfo(expression).Symbol;
        if (symbol is ILocalSymbol local)
        {
            return GetLambdaHeldInLocal(local)?.Body;
        }

        if (symbol is not IMethodSymbol methodSymbol)
        {
            return null;
        }

        // A method group naming a partial method binds to its defining declaration, which has no body. The
        // implementation part carries the body, and may be declared in another part of the type, even in another
        // file or a source-generated one.
        methodSymbol = methodSymbol.PartialImplementationPart ?? methodSymbol;

        SyntaxReference? syntaxReference = methodSymbol.DeclaringSyntaxReferences.FirstOrDefault();
        if (syntaxReference == null)
        {
            return null;
        }

        // A method group can bind to a symbol whose declaring syntax is neither an ordinary method nor a
        // local function. The clearest case is a delegate type declared in the same compilation: its
        // implicit Invoke member reports the DelegateDeclarationSyntax as its declaration, so
        // `handler.Invoke` passed to AddObserver arrives here as a delegate declaration. There is no
        // body to inspect in that case, and saying so lets the caller decline to analyze the handler
        // rather than the analyzer throwing and suppressing itself for the whole file.
        SyntaxNode methodDeclaration = syntaxReference.GetSyntax();
        return methodDeclaration switch
        {
            MethodDeclarationSyntax method => method.Body ?? (SyntaxNode?)method.ExpressionBody?.Expression,
            LocalFunctionStatementSyntax localFunction => localFunction.Body ?? (SyntaxNode?)localFunction.ExpressionBody?.Expression,
            _ => null,
        };
    }

    /// <summary>
    /// Determines whether a type, one of its base types, or an interface one of them implements is a library type with
    /// one of the given names.
    /// </summary>
    /// <param name="type">The type to inspect.</param>
    /// <param name="typeNames">The names of the library types to look for.</param>
    /// <returns><see langword="true"/> if a library type with one of the names is found; otherwise <see langword="false"/>.</returns>
    internal static bool HasTypeOrBaseOrInterface(ITypeSymbol? type, params string[] typeNames)
    {
        for (ITypeSymbol? current = type; current != null; current = current.BaseType)
        {
            // Require the matched type to be declared in the WebDriverBiDi namespace so a user's own
            // type that merely shares a name (for example a class named BiDiDriver, or an interface
            // named IBiDiModuleHost, in another namespace) is not treated as the library type.
            if (current is not INamedTypeSymbol namedCurrent)
            {
                continue;
            }

            if (typeNames.Contains(namedCurrent.Name) && IsInWebDriverBiDiNamespace(namedCurrent))
            {
                return true;
            }

            if (namedCurrent.AllInterfaces.Any(interfaceType => typeNames.Contains(interfaceType.Name) && IsInWebDriverBiDiNamespace(interfaceType)))
            {
                return true;
            }
        }

        return false;
    }
}
