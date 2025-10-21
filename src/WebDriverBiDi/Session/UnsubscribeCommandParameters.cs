// <copyright file="UnsubscribeCommandParameters.cs" company="WebDriverBiDi.NET Committers">
// Copyright (c) WebDriverBiDi.NET Committers. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.
// </copyright>

namespace WebDriverBiDi.Session;

using System.Text.Json.Serialization;

/// <summary>
/// Provides parameters for the session.unsubscribe command.
/// </summary>
public class UnsubscribeCommandParameters : CommandParameters<UnsubscribeCommandResult>
{
    private readonly List<string> eventList = [];

    private readonly List<string> contextList = [];

    /// <summary>
    /// Initializes a new instance of the <see cref="UnsubscribeCommandParameters"/> class.
    /// </summary>
    protected UnsubscribeCommandParameters()
    {
    }

    /// <summary>
    /// Gets the method name of the command.
    /// </summary>
    [JsonIgnore]
    public override string MethodName => "session.unsubscribe";
}
