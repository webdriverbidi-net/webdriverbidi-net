// <copyright file="EndCommandParameters.cs" company="WebDriverBiDi.NET Committers">
// Copyright (c) WebDriverBiDi.NET Committers. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.
// </copyright>

namespace WebDriverBiDi.Session;

using System.Text.Json.Serialization;

/// <summary>
/// Provides parameters for the session.end command.
/// </summary>
public class EndCommandParameters : CommandParameters<EndCommandResult>
{
    /// <summary>
    /// Initializes a new instance of the <see cref="EndCommandParameters"/> class.
    /// </summary>
    public EndCommandParameters()
    {
    }

    /// <summary>
    /// Gets the method name of the command.
    /// </summary>
    [JsonIgnore]
    public override string MethodName => "session.end";
}
