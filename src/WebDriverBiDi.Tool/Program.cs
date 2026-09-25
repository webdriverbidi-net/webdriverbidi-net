// <copyright file="Program.cs" company="WebDriverBiDi.NET Committers">
// Copyright (c) WebDriverBiDi.NET Committers. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.
// </copyright>

namespace WebDriverBiDi.Tool;

using System.Diagnostics.CodeAnalysis;

/// <summary>
/// The entry point of the webdriverbidi tool.
/// </summary>
[ExcludeFromCodeCoverage] // Passes the console to WebDriverBiDiTool, which the tests drive directly.
internal static class Program
{
    /// <summary>
    /// Runs the tool.
    /// </summary>
    /// <param name="args">The command-line arguments.</param>
    /// <returns>A task whose result is the exit code.</returns>
    public static Task<int> Main(string[] args) => WebDriverBiDiTool.RunAsync(args, Console.Out, Console.Error);
}
