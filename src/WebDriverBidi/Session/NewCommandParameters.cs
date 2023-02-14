// <copyright file="NewCommandParameters.cs" company="WebDriverBidi.NET Committers">
// Copyright (c) WebDriverBidi.NET Committers. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.
// </copyright>

namespace WebDriverBidi.Session;

using Newtonsoft.Json;

/// <summary>
/// Provides parameters for the session.new command.
/// </summary>
[JsonObject(MemberSerialization.OptIn)]
public class NewCommandParameters : CommandParameters<NewCommandResult>
{
    private readonly List<CapabilitiesRequest> firstMatch = new();
    private CapabilitiesRequest? alwaysMatch;

    /// <summary>
    /// Gets the method name of the command.
    /// </summary>
    public override string MethodName => "session.new";

    /// <summary>
    /// Gets or sets the set of capabilities that must be matched to create a new session.
    /// </summary>
    [JsonProperty("alwaysMatch", NullValueHandling = NullValueHandling.Ignore)]
    public CapabilitiesRequest? AlwaysMatch { get => this.alwaysMatch; set => this.alwaysMatch = value; }

    /// <summary>
    /// Gets the list of sets of capabilities any of which may be matched to create a new session.
    /// </summary>
    public List<CapabilitiesRequest> FirstMatch => this.firstMatch;

    /// <summary>
    /// Gets the list of sets of capabilities any of which may be matched to create a new session for serialization purposes.
    /// </summary>
    [JsonProperty("firstMatch", NullValueHandling = NullValueHandling.Ignore)]
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