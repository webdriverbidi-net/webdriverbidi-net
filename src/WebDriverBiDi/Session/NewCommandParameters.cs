// <copyright file="NewCommandParameters.cs" company="WebDriverBiDi.NET Committers">
// Copyright (c) WebDriverBiDi.NET Committers. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.
// </copyright>

namespace WebDriverBiDi.Session;

using System.Text.Json.Serialization;

/// <summary>
/// Provides parameters for the session.new command.
/// </summary>
public class NewCommandParameters : CommandParameters<NewCommandResult>
{
    private readonly List<CapabilitiesRequest> firstMatch = new();
    private CapabilitiesRequest? alwaysMatch;

    /// <summary>
    /// Gets the method name of the command.
    /// </summary>
    [JsonIgnore]
    public override string MethodName => "session.new";

    /// <summary>
    /// Gets or sets the set of capabilities that must be matched to create a new session.
    /// </summary>
    [JsonPropertyName("alwaysMatch")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    [JsonInclude]
    public CapabilitiesRequest? AlwaysMatch { get => this.alwaysMatch; set => this.alwaysMatch = value; }

    /// <summary>
    /// Gets the list of sets of capabilities any of which may be matched to create a new session.
    /// </summary>
    [JsonIgnore]
    public List<CapabilitiesRequest> FirstMatch => this.firstMatch;

    /// <summary>
    /// Gets the list of sets of capabilities any of which may be matched to create a new session for serialization purposes.
    /// </summary>
    [JsonPropertyName("firstMatch")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    [JsonInclude]
    internal IList<CapabilitiesRequest>? SerializableFirstMatch
    {
        get
        {
            if (this.firstMatch.Count == 0)
            {
                return null;
            }

            return this.firstMatch.AsReadOnly();
        }
    }
}
