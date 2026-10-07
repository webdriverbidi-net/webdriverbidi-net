// <copyright file="HandlerOperation.cs" company="WebDriverBiDi.NET Committers">
// Copyright (c) WebDriverBiDi.NET Committers. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.
// </copyright>

namespace WebDriverBiDi.Analyzers;

using Microsoft.CodeAnalysis;

/// <summary>
/// An operation in an event handler body that a rule reports, with the name the diagnostic reports it under.
/// </summary>
internal sealed class HandlerOperation
{
    /// <summary>
    /// Initializes a new instance of the <see cref="HandlerOperation"/> class.
    /// </summary>
    /// <param name="node">The syntax node of the operation.</param>
    /// <param name="name">The name the diagnostic reports the operation under.</param>
    public HandlerOperation(SyntaxNode node, string name)
    {
        this.Node = node;
        this.Name = name;
    }

    /// <summary>
    /// Gets the syntax node of the operation.
    /// </summary>
    public SyntaxNode Node { get; }

    /// <summary>
    /// Gets the name the diagnostic reports the operation under.
    /// </summary>
    public string Name { get; }
}
