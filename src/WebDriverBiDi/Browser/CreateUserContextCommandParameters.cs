// <copyright file="CreateUserContextCommandParameters.cs" company="WebDriverBiDi.NET Committers">
// Copyright (c) WebDriverBiDi.NET Committers. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.
// </copyright>

namespace WebDriverBiDi.Browser;

using System.Text.Json.Serialization;
using WebDriverBiDi.Session;

/// <summary>
/// Provides parameters for the browser.createUserContext command.
/// </summary>
public class CreateUserContextCommandParameters : CommandParameters<CreateUserContextCommandResult>
{
    private bool? acceptInsecureCerts;
    private ProxyConfiguration? proxyConfiguration;
    private UserPromptHandler? unhandledPromptBehavior;

    /// <summary>
    /// Initializes a new instance of the <see cref="CreateUserContextCommandParameters"/> class.
    /// </summary>
    public CreateUserContextCommandParameters()
    {
    }

    /// <summary>
    /// Gets the method name of the command.
    /// </summary>
    [JsonIgnore]
    public override string MethodName => "browser.createUserContext";

    /// <summary>
    /// Gets or sets a value indicating whether to accept insecure certificates.
    /// </summary>
    [JsonPropertyName("acceptInsecureCerts")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public bool? AcceptInsecureCerts { get => this.acceptInsecureCerts; set => this.acceptInsecureCerts = value; }

    /// <summary>
    /// Gets or sets the proxy configuration used with the created user context.
    /// </summary>
    [JsonPropertyName("proxy")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public ProxyConfiguration? Proxy { get => this.proxyConfiguration; set => this.proxyConfiguration = value; }

    /// <summary>
    /// Gets or sets the handler for unhandled user prompts.
    /// </summary>
    [JsonPropertyName("unhandledPromptBehavior")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public UserPromptHandler? UnhandledPromptBehavior { get => this.unhandledPromptBehavior; set => this.unhandledPromptBehavior = value; }
}
