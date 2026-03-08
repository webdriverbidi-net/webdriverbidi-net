// <copyright file="BiDiDriver018_UnsafeRemoteValueValueAsAnalyzer.cs" company="WebDriverBiDi.NET Committers">
// Copyright (c) WebDriverBiDi.NET Committers. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.
// </copyright>

namespace WebDriverBiDi.Analyzers;

using System.Collections.Immutable;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.Text;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;

/// <summary>
/// Analyzer that detects unsafe use of ValueAs&lt;Dictionary&lt;string, object&gt;&gt; or ValueAs&lt;List&lt;object&gt;&gt;
/// on RemoteValue instances. JavaScript objects and arrays return RemoteValueDictionary and RemoteValueList respectively.
/// </summary>
[DiagnosticAnalyzer(LanguageNames.CSharp)]
public class BiDiDriver018_UnsafeRemoteValueValueAsAnalyzer : DiagnosticAnalyzer
{
    /// <summary>
    /// The diagnostic ID for this analyzer.
    /// </summary>
    public const string DiagnosticId = "BIDI018";

    private const string Category = "Usage";

    private static readonly LocalizableString Title = "Use RemoteValueDictionary or RemoteValueList for ValueAs";

    private static readonly LocalizableString MessageFormat = "ValueAs<{0}> will throw at runtime. JavaScript objects use RemoteValueDictionary and arrays use RemoteValueList. Use ValueAs<RemoteValueDictionary>() or ValueAs<RemoteValueList>() and extract values with .ValueAs<T>().";

    private static readonly LocalizableString Description = "RemoteValue.ValueAs<T>() for JavaScript objects returns RemoteValueDictionary, not Dictionary<string, object>. For arrays it returns RemoteValueList, not List<object>. Using the wrong type will throw WebDriverBiDiException at runtime.";

    private static readonly DiagnosticDescriptor Rule = new(
        DiagnosticId,
        Title,
        MessageFormat,
        Category,
        DiagnosticSeverity.Warning,
        isEnabledByDefault: true,
        description: Description);

    /// <inheritdoc/>
    public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics => [Rule];

    /// <inheritdoc/>
    public override void Initialize(AnalysisContext context)
    {
        context.ConfigureGeneratedCodeAnalysis(GeneratedCodeAnalysisFlags.None);
        context.EnableConcurrentExecution();

        context.RegisterSyntaxNodeAction(AnalyzeInvocation, SyntaxKind.InvocationExpression);
    }

    private static void AnalyzeInvocation(SyntaxNodeAnalysisContext context)
    {
        InvocationExpressionSyntax invocation = (InvocationExpressionSyntax)context.Node;
        SemanticModel semanticModel = context.SemanticModel;

        // Check if this is a generic method call (ValueAs<T>())
        if (invocation.Expression is not MemberAccessExpressionSyntax memberAccess)
        {
            return;
        }

        if (memberAccess.Name is not GenericNameSyntax genericName)
        {
            return;
        }

        if (genericName.Identifier.Text != "ValueAs")
        {
            return;
        }

        if (genericName.TypeArgumentList.Arguments.Count != 1)
        {
            return;
        }

        // Get the type argument
        ITypeSymbol? typeArgument = semanticModel.GetTypeInfo(genericName.TypeArgumentList.Arguments[0], context.CancellationToken).Type;
        if (typeArgument == null)
        {
            return;
        }

        // Check if it's Dictionary<string, object> or List<object>
        bool isDictionary = BiDiDriver018_UnsafeRemoteValueValueAsAnalyzerHelper.IsDictionaryStringObject(typeArgument);
        bool isList = BiDiDriver018_UnsafeRemoteValueValueAsAnalyzerHelper.IsListObject(typeArgument);

        if (!isDictionary && !isList)
        {
            return;
        }

        // Get the receiver type (what ValueAs is being called on)
        ITypeSymbol? receiverType = semanticModel.GetTypeInfo(memberAccess.Expression, context.CancellationToken).Type;
        if (receiverType == null)
        {
            return;
        }

        // Check if receiver is RemoteValue
        if (!IsRemoteValueType(receiverType))
        {
            return;
        }

        string typeName = isDictionary ? "Dictionary<string, object>" : "List<object>";
        TextSpan span = TextSpan.FromBounds(genericName.Span.Start, invocation.Span.End);
        Location location = Location.Create(invocation.SyntaxTree, span);
        Diagnostic diagnostic = Diagnostic.Create(Rule, location, typeName);
        context.ReportDiagnostic(diagnostic);
    }

    private static bool IsRemoteValueType(ITypeSymbol type)
    {
        ITypeSymbol? current = type;
        while (current != null)
        {
            if (current.Name == "RemoteValue" && current.ContainingNamespace?.ToString() == "WebDriverBiDi.Script")
            {
                return true;
            }

            current = current.BaseType;
        }

        return false;
    }

}
