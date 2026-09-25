// <copyright file="ToolResult.cs" company="WebDriverBiDi.NET Committers">
// Copyright (c) WebDriverBiDi.NET Committers. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.
// </copyright>

namespace WebDriverBiDi.Tool.TestUtilities;

/// <summary>
/// What a run of the tool returned and wrote.
/// </summary>
/// <param name="ExitCode">The exit code.</param>
/// <param name="Output">What the tool wrote to its output.</param>
/// <param name="Error">What the tool wrote to its error output.</param>
public sealed record ToolResult(int ExitCode, string Output, string Error)
{
    /// <summary>
    /// Gets the lines of the output.
    /// </summary>
    public IReadOnlyList<string> OutputLines => this.Output.Split(Environment.NewLine, StringSplitOptions.RemoveEmptyEntries);
}
