// <copyright file="CreateUserContextCommandParameters.cs" company="WebDriverBiDi.NET Committers">
// Copyright (c) WebDriverBiDi.NET Committers. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.
// </copyright>

namespace WebDriverBiDi.Browser;

using System.Text.Json.Serialization;

/// <summary>
/// Provides parameters for the browser.createUserContext command.
/// </summary>
public class CreateUserContextCommandParameters : CommandParameters<CreateUserContextCommandResult>
{
    private bool? acceptInsecureCerts;

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
}
