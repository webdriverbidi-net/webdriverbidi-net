// <copyright file="CreateCommandSettings.cs" company="WebDriverBidi.NET Committers">
// Copyright (c) WebDriverBidi.NET Committers. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.
// </copyright>

namespace WebDriverBidi.BrowsingContext;

using Newtonsoft.Json;

/// <summary>
/// Provides parameters for the browsingContext.create command.
/// </summary>
[JsonObject(MemberSerialization.OptIn)]
public class CreateCommandSettings : CommandSettings
{
    private BrowsingContextCreateType createType;

    private string? referenceContextId;

    /// <summary>
    /// Initializes a new instance of the <see cref="CreateCommandSettings" /> class.
    /// </summary>
    /// <param name="createType">The type of browsing context to create.</param>
    public CreateCommandSettings(BrowsingContextCreateType createType)
    {
        this.createType = createType;
    }

    /// <summary>
    /// Gets the method name of the command.
    /// </summary>
    public override string MethodName => "browsingContext.create";

    /// <summary>
    /// Gets the type of the result of the command.
    /// </summary>
    public override Type ResultType => typeof(CreateCommandResult);

    /// <summary>
    /// Gets or sets the type of browsing context (tab or window) to create.
    /// </summary>
    public BrowsingContextCreateType CreateType { get => this.createType; set => this.createType = value; }

    /// <summary>
    /// Gets or sets the ID of the browsing context to reference within the newly created context.
    /// </summary>
    [JsonProperty("referenceContext", NullValueHandling = NullValueHandling.Ignore)]
    public string? ReferenceContextId { get => this.referenceContextId; set => this.referenceContextId = value; }

    /// <summary>
    /// Gets the string value of the type of browsing context to be created for serialization purposes.
    /// </summary>
    [JsonProperty("type")]
    internal string SerializableCreateType
    {
        get => this.createType.ToString().ToLowerInvariant();
    }
}