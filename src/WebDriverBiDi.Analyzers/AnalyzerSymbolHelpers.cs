// <copyright file="AnalyzerSymbolHelpers.cs" company="WebDriverBiDi.NET Committers">
// Copyright (c) WebDriverBiDi.NET Committers. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.
// </copyright>

namespace WebDriverBiDi.Analyzers;

using System.Collections.Generic;
using System.Linq;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;

/// <summary>
/// Helper methods for identifying WebDriver BiDi driver-related symbols.
/// </summary>
internal static class AnalyzerSymbolHelpers
{
    /// <summary>
    /// Determines whether the symbol represents a command executor capability.
    /// </summary>
    /// <param name="type">The symbol to inspect.</param>
    /// <returns><see langword="true"/> if the symbol represents a command executor capability; otherwise <see langword="false"/>.</returns>
    internal static bool IsCommandExecutorType(ITypeSymbol? type)
    {
        return HasTypeOrBaseOrInterface(type, "BiDiDriver", "IBiDiCommandExecutor");
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
    /// Determines whether the handler passed to an <c>AddObserver</c> invocation will actually
    /// execute off the dispatching thread when <c>RunHandlerAsynchronously</c> is specified.
    /// </summary>
    /// <param name="context">The analysis context.</param>
    /// <param name="invocation">The AddObserver invocation to inspect.</param>
    /// <param name="addObserverMethod">The resolved AddObserver overload.</param>
    /// <returns><see langword="true"/> if the handler is asynchronous; otherwise <see langword="false"/>.</returns>
    /// <remarks>
    /// The option only affects what happens with the <c>Task</c> a handler returns; the code that
    /// runs before the handler returns still executes on the dispatching thread. A handler is
    /// therefore considered asynchronous when it is bound to the <c>Action&lt;T&gt;</c> overload
    /// (the library queues the whole action to the thread pool in that case), when it is an
    /// <c>async</c> lambda or anonymous method, or when it is a method group that resolves to an
    /// <c>async</c> method. A non-<c>async</c> <c>Task</c>-returning handler is not offloaded.
    /// Callers only invoke this when an options argument is present, so the invocation always has
    /// at least one argument and the resolved overload at least one parameter.
    /// </remarks>
    internal static bool IsHandlerAsynchronous(SyntaxNodeAnalysisContext context, InvocationExpressionSyntax invocation, IMethodSymbol addObserverMethod)
    {
        if (addObserverMethod.Parameters[0].Type.Name == "Action")
        {
            return true;
        }

        ExpressionSyntax handler = invocation.ArgumentList.Arguments[0].Expression;
        return handler switch
        {
            AnonymousFunctionExpressionSyntax anonymousFunction => anonymousFunction.AsyncKeyword.IsKind(SyntaxKind.AsyncKeyword),
            IdentifierNameSyntax or MemberAccessExpressionSyntax => context.SemanticModel.GetSymbolInfo(handler).Symbol is IMethodSymbol { IsAsync: true },
            _ => false,
        };
    }

    /// <summary>
    /// Gets the body syntax node for a handler expression passed to AddObserver.
    /// Returns the lambda body, or resolves a method reference to its body.
    /// </summary>
    /// <param name="context">The analysis context.</param>
    /// <param name="expression">The handler expression.</param>
    /// <returns>The body syntax node, or <see langword="null"/> if it cannot be resolved.</returns>
    internal static SyntaxNode? GetHandlerBody(SyntaxNodeAnalysisContext context, ExpressionSyntax expression)
    {
        return expression switch
        {
            SimpleLambdaExpressionSyntax simpleLambda => simpleLambda.Body,
            ParenthesizedLambdaExpressionSyntax parenthesizedLambda => parenthesizedLambda.Body,
            IdentifierNameSyntax identifierName => GetMethodBodyFromSymbol(context, identifierName),
            MemberAccessExpressionSyntax memberAccess => GetMethodBodyFromSymbol(context, memberAccess),
            _ => null,
        };
    }

    /// <summary>
    /// Determines whether a type is a library module: its name ends in <c>"Module"</c> and it either
    /// derives from the abstract <c>Module</c> base class or is declared within the
    /// <c>WebDriverBiDi</c> namespace. Requiring more than the <c>"*Module"</c> name avoids matching
    /// unrelated user types that merely end in <c>"Module"</c> (they neither derive from <c>Module</c>
    /// nor live in the library's namespace).
    /// </summary>
    /// <param name="type">The type to inspect.</param>
    /// <returns><see langword="true"/> if the type is a library module; otherwise <see langword="false"/>.</returns>
    internal static bool IsLibraryModuleType(ITypeSymbol? type)
    {
        return type is INamedTypeSymbol named
            && named.Name.EndsWith("Module", System.StringComparison.Ordinal)
            && (IsModuleSubclass(named) || IsInWebDriverBiDiNamespace(named));
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
    /// <c>IBiDiCommandExecutor</c>.
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
    /// Unwraps the task-chaining wrappers around an invocation, returning the innermost call the
    /// chain is built on. <c>driver.StartAsync(url).ConfigureAwait(false)</c> yields
    /// <c>driver.StartAsync(url)</c>, as do <c>.Wait()</c> and
    /// <c>.ConfigureAwait(false).GetAwaiter().GetResult()</c>.
    /// </summary>
    /// <param name="invocation">The outermost invocation.</param>
    /// <returns>The innermost invocation the chain wraps, or the original invocation when it wraps nothing.</returns>
    /// <remarks>
    /// Only the names in <see cref="TaskChainingMethodNames"/> are unwrapped. Descending through any
    /// invocation-receivered member access would be wrong: in <c>GetDriver().StartAsync(url)</c> the
    /// receiver is also an invocation, and unwrapping it would discard the <c>StartAsync</c> call
    /// that the analyzers exist to find.
    /// </remarks>
    internal static InvocationExpressionSyntax UnwrapTaskChain(InvocationExpressionSyntax invocation)
    {
        InvocationExpressionSyntax current = invocation;
        while (current.Expression is MemberAccessExpressionSyntax { Expression: InvocationExpressionSyntax inner } memberAccess
            && TaskChainingMethodNames.Contains(memberAccess.Name.Identifier.ValueText))
        {
            current = inner;
        }

        return current;
    }

    /// <summary>
    /// The methods that wrap a task-returning call without changing which call was made: awaiting
    /// and blocking adapters. Hoisted to a static field to avoid allocating on every unwrap.
    /// </summary>
    private static readonly string[] TaskChainingMethodNames =
    [
        "ConfigureAwait",
        "Wait",
        "GetAwaiter",
        "GetResult",
        "AsTask",
    ];

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
        if (symbol is not IMethodSymbol methodSymbol)
        {
            return null;
        }

        SyntaxReference? syntaxReference = methodSymbol.DeclaringSyntaxReferences.FirstOrDefault();
        if (syntaxReference == null)
        {
            return null;
        }

        SyntaxNode methodDeclaration = syntaxReference.GetSyntax();
        return methodDeclaration is MethodDeclarationSyntax methodDecl
            ? methodDecl.Body ?? (SyntaxNode?)methodDecl.ExpressionBody?.Expression
            : ((LocalFunctionStatementSyntax)methodDeclaration).Body ?? (SyntaxNode?)((LocalFunctionStatementSyntax)methodDeclaration).ExpressionBody?.Expression;
    }

    private static bool HasTypeOrBaseOrInterface(ITypeSymbol? type, params string[] typeNames)
    {
        for (ITypeSymbol? current = type; current != null; current = current.BaseType)
        {
            // Require the matched type to be declared in the WebDriverBiDi namespace so a user's own
            // type that merely shares a name (for example a class named BiDiDriver, or an interface
            // named IBiDiCommandExecutor, in another namespace) is not treated as the library type.
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
