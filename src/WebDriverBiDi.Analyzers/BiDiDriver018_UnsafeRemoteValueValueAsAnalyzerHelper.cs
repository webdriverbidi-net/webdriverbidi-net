// <copyright file="BiDiDriver018_UnsafeRemoteValueValueAsAnalyzerHelper.cs" company="WebDriverBiDi.NET Committers">
// Copyright (c) WebDriverBiDi.NET Committers. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.
// </copyright>

namespace WebDriverBiDi.Analyzers;

using Microsoft.CodeAnalysis;

/// <summary>
/// Helper methods for BIDI018 analyzer and code fix provider.
/// </summary>
public static class BiDiDriver018_UnsafeRemoteValueValueAsAnalyzerHelper
{
    /// <summary>
    /// Checks if the type is Dictionary&lt;string, object&gt;.
    /// </summary>
    public static bool IsDictionaryStringObject(ITypeSymbol type)
    {
        if (type is not INamedTypeSymbol namedType || !namedType.IsGenericType)
        {
            return false;
        }

        INamedTypeSymbol? originalDefinition = namedType.OriginalDefinition;
        if (originalDefinition == null || originalDefinition.Name != "Dictionary")
        {
            return false;
        }

        if (originalDefinition.ContainingNamespace?.ToString() != "System.Collections.Generic")
        {
            return false;
        }

        if (namedType.TypeArguments.Length != 2)
        {
            return false;
        }

        return namedType.TypeArguments[0].SpecialType == SpecialType.System_String
            && namedType.TypeArguments[1].SpecialType == SpecialType.System_Object;
    }

    /// <summary>
    /// Checks if the type is List&lt;object&gt;.
    /// </summary>
    public static bool IsListObject(ITypeSymbol type)
    {
        if (type is not INamedTypeSymbol namedType || !namedType.IsGenericType)
        {
            return false;
        }

        INamedTypeSymbol? originalDefinition = namedType.OriginalDefinition;
        if (originalDefinition == null || originalDefinition.Name != "List")
        {
            return false;
        }

        if (originalDefinition.ContainingNamespace?.ToString() != "System.Collections.Generic")
        {
            return false;
        }

        if (namedType.TypeArguments.Length != 1)
        {
            return false;
        }

        return namedType.TypeArguments[0].SpecialType == SpecialType.System_Object;
    }
}
