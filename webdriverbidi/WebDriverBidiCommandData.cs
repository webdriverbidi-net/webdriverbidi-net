// <copyright file="WebDriverBidiCommandData.cs" company="WebDriverBidi.NET Committers">
// Copyright (c) WebDriverBidi.NET Committers. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.
// </copyright>

namespace WebDriverBidi;

using Newtonsoft.Json;

/// <summary>
/// Object containing data about a WebDriver Bidi command.
/// </summary>
[JsonObject(MemberSerialization.OptIn)]
public class WebDriverBidiCommandData
{
    private readonly CommandSettings commandSettings;
    private readonly long commandId;
    private readonly ManualResetEvent synchronizationEvent = new(false);

    /// <summary>
    /// Initializes a new instance of the <see cref="WebDriverBidiCommandData" /> class.
    /// </summary>
    /// <param name="commandId">The ID of the command.</param>
    /// <param name="commandSettings">The settings for the command, including parameters.</param>
    public WebDriverBidiCommandData(long commandId, CommandSettings commandSettings)
    {
        this.commandId = commandId;
        this.commandSettings = commandSettings;
    }

    /// <summary>
    /// Gets the ID of the command.
    /// </summary>
    [JsonProperty("id")]
    public long CommandId => this.commandId;

    /// <summary>
    /// Gets the method name of the command.
    /// </summary>
    [JsonProperty("method")]
    public string CommandName => this.commandSettings.MethodName;

    /// <summary>
    /// Gets the parameters of the command.
    /// </summary>
    [JsonProperty("params")]
    public CommandSettings CommandParameters => this.commandSettings;

    /// <summary>
    /// Gets the type for the expected result of the command.
    /// </summary>
    public Type ResultType => this.commandSettings.ResultType;

    /// <summary>
    /// Gets a synchronization object used to wait for completion of the command.
    /// </summary>
    public ManualResetEvent SynchronizationEvent => this.synchronizationEvent;

    /// <summary>
    /// Gets or sets the result of the command.
    /// </summary>
    public CommandResult? Result { get; set; }

    /// <summary>
    /// Gets or sets the exception thrown during execution of the command, if any.
    /// </summary>
    public Exception? ThrownException { get; set; }
}