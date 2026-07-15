// <copyright file="RunProcessResult.cs" company="WebDriverBiDi.NET Committers">
// Copyright (c) WebDriverBiDi.NET Committers. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.
// </copyright>

namespace WebDriverBiDi.Integration.Tests;

public record RunProcessResult
{
    public string FileName { get; init; } = string.Empty;

    public int ExitCode { get; init; } = 0;

    public string StandardOutputConsoleContent { get; init; } = string.Empty;

    public string StandardErrorConsoleContent { get; init; } = string.Empty;
}
