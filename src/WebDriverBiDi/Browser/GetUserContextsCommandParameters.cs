// <copyright file="GetUserContextsCommandParameters.cs" company="WebDriverBiDi.NET Committers">
// Copyright (c) WebDriverBiDi.NET Committers. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.
// </copyright>

namespace WebDriverBiDi.Browser;

using System.Text.Json.Serialization;

/// <summary>
/// Provides parameters for the browser.getUserContexts command.
/// </summary>
public class GetUserContextsCommandParameters : CommandParameters<GetUserContextsCommandResult>
{
    /// <summary>
    /// Initializes a new instance of the <see cref="GetUserContextsCommandParameters"/> class.
    /// </summary>
    public GetUserContextsCommandParameters()
    {
    }

    /// <summary>
    /// Gets the method name of the command.
    /// </summary>
    [JsonIgnore]
    public override string MethodName => "browser.getUserContexts";
}
