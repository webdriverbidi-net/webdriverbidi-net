// <copyright file="Program.cs" company="WebDriverBiDi.NET Committers">
// Copyright (c) WebDriverBiDi.NET Committers. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.
// </copyright>

namespace Dramaturge.Tool;

using System.Diagnostics.CodeAnalysis;

/// <summary>
/// The entry point of the dramaturge tool.
/// </summary>
[ExcludeFromCodeCoverage] // Passes the console to DramaturgeTool, which the tests drive directly.
internal static class Program
{
    /// <summary>
    /// Runs the tool.
    /// </summary>
    /// <param name="args">The command-line arguments.</param>
    /// <returns>A task whose result is the exit code.</returns>
    public static Task<int> Main(string[] args) => DramaturgeTool.RunAsync(args, Console.Out, Console.Error);
}
