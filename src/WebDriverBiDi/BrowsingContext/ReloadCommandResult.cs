// <copyright file="ReloadCommandResult.cs" company="WebDriverBiDi.NET Committers">
// Copyright (c) WebDriverBiDi.NET Committers. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.
// </copyright>

namespace WebDriverBiDi.BrowsingContext;

using System.Text.Json.Serialization;

/// <summary>
/// Result reloading the browsing context using the browserContext.reload command.
/// </summary>
public record ReloadCommandResult : NavigateCommandResult
{
    [JsonConstructor]
    internal ReloadCommandResult()
        : base()
    {
    }
}
