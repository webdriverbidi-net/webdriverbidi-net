// <copyright file="DisposalRules.cs" company="WebDriverBiDi.NET Committers">
// Copyright (c) WebDriverBiDi.NET Committers. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.
// </copyright>

namespace WebDriverBiDi.Analyzers;

using System;
using System.Collections.Generic;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;

/// <summary>
/// What a use-after-disposal rule tracks: the type whose locals it follows, the calls that dispose one,
/// the receiver a call is made through, and the members that throw once it is disposed.
/// </summary>
internal sealed class DisposalRules
{
    private readonly Func<ITypeSymbol?, bool> isTrackedType;

    private readonly ReceiverResolver resolveReceiverName;

    private readonly Func<IMethodSymbol, bool> throwsAfterDisposal;

    /// <summary>
    /// Initializes a new instance of the <see cref="DisposalRules"/> class.
    /// </summary>
    /// <param name="isTrackedType">Determines whether a local's type is the one the rule follows.</param>
    /// <param name="resolveReceiverName">Names the tracked variable a call is made through.</param>
    /// <param name="disposalMethodNames">The methods that dispose the instance they are called on.</param>
    /// <param name="throwsAfterDisposal">Determines whether a method throws once its instance is disposed.</param>
    public DisposalRules(
        Func<ITypeSymbol?, bool> isTrackedType,
        ReceiverResolver resolveReceiverName,
        IReadOnlyCollection<string> disposalMethodNames,
        Func<IMethodSymbol, bool> throwsAfterDisposal)
    {
        this.isTrackedType = isTrackedType;
        this.resolveReceiverName = resolveReceiverName;
        this.DisposalMethodNames = disposalMethodNames;
        this.throwsAfterDisposal = throwsAfterDisposal;
    }

    /// <summary>
    /// Names the tracked variable an invocation is made through.
    /// </summary>
    /// <param name="invocation">The invocation.</param>
    /// <param name="body">The member body being walked.</param>
    /// <param name="semanticModel">The semantic model for the member.</param>
    /// <param name="isDirectCall">
    /// When this method returns a name, whether the call is made on the tracked variable itself rather
    /// than on something reached through it. Only a direct call can be the disposal.
    /// </param>
    /// <returns>The variable's name, or <see langword="null"/> when the call is made through nothing the rule tracks.</returns>
    internal delegate string? ReceiverResolver(InvocationExpressionSyntax invocation, SyntaxNode body, SemanticModel semanticModel, out bool isDirectCall);

    /// <summary>
    /// Gets the methods that dispose the instance they are called on.
    /// </summary>
    public IReadOnlyCollection<string> DisposalMethodNames { get; }

    /// <summary>
    /// Determines whether a local's type is the one this rule follows.
    /// </summary>
    /// <param name="type">The local's type.</param>
    /// <returns><see langword="true"/> if the local is tracked.</returns>
    public bool IsTrackedType(ITypeSymbol? type) => this.isTrackedType(type);

    /// <summary>
    /// Names the tracked variable an invocation is made through.
    /// </summary>
    /// <param name="invocation">The invocation.</param>
    /// <param name="body">The member body being walked.</param>
    /// <param name="semanticModel">The semantic model for the member.</param>
    /// <param name="isDirectCall">Whether the call is made on the tracked variable itself.</param>
    /// <returns>The variable's name, or <see langword="null"/>.</returns>
    public string? ResolveReceiverName(InvocationExpressionSyntax invocation, SyntaxNode body, SemanticModel semanticModel, out bool isDirectCall)
    {
        return this.resolveReceiverName(invocation, body, semanticModel, out isDirectCall);
    }

    /// <summary>
    /// Determines whether a method throws once the instance it belongs to is disposed.
    /// </summary>
    /// <param name="method">The resolved method.</param>
    /// <returns><see langword="true"/> if the method is guarded by a disposal check.</returns>
    public bool ThrowsAfterDisposal(IMethodSymbol method) => this.throwsAfterDisposal(method);
}
