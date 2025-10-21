// <copyright file="RemoveUserContextCommandParameters.cs" company="WebDriverBiDi.NET Committers">
// Copyright (c) WebDriverBiDi.NET Committers. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.
// </copyright>

namespace WebDriverBiDi.Browser;

using System.Text.Json.Serialization;

/// <summary>
/// Provides parameters for the browser.removeUserContext command.
/// </summary>
public class RemoveUserContextCommandParameters : CommandParameters<RemoveUserContextCommandResult>
{
    private string userContextId;

    /// <summary>
    /// Initializes a new instance of the <see cref="RemoveUserContextCommandParameters"/> class.
    /// </summary>
    /// <param name="userContextId">The ID of the user context to remove.</param>
    public RemoveUserContextCommandParameters(string userContextId)
    {
        this.userContextId = userContextId;
    }

    /// <summary>
    /// Gets the method name of the command.
    /// </summary>
    [JsonIgnore]
    public override string MethodName => "browser.removeUserContext";

    /// <summary>
    /// Gets or sets the ID of the user context to remove.
    /// </summary>
    [JsonPropertyName("userContext")]
    public string UserContextId { get => this.userContextId; set => this.userContextId = value; }
}
