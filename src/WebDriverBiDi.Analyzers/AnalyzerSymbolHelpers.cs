// <copyright file="AnalyzerSymbolHelpers.cs" company="WebDriverBiDi.NET Committers">
// Copyright (c) WebDriverBiDi.NET Committers. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.
// </copyright>

namespace WebDriverBiDi.Analyzers;

using System.Linq;
using Microsoft.CodeAnalysis;

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

    private static bool HasTypeOrBaseOrInterface(ITypeSymbol? type, params string[] typeNames)
    {
        for (ITypeSymbol? current = type; current != null; current = current.BaseType)
        {
            if (typeNames.Contains(current.Name))
            {
                return true;
            }

            if (current is INamedTypeSymbol namedType && namedType.AllInterfaces.Any(interfaceType => typeNames.Contains(interfaceType.Name)))
            {
                return true;
            }
        }

        return false;
    }
}
