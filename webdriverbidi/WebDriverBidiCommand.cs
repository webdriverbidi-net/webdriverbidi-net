// <copyright file="WebDriverBidiCommand.cs" company="WebDriverBidi.NET Committers">
// Copyright (c) WebDriverBidi.NET Committers. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.
// </copyright>

namespace WebDriverBidi;

using Newtonsoft.Json;
using WebDriverBidi.JsonConverters;

/// <summary>
/// Object containing data about a WebDriver Bidi command.
/// </summary>
[JsonObject(MemberSerialization.OptIn)]
public class WebDriverBidiCommand
{
    private readonly CommandData commandData;
    private readonly long commandId;
    private readonly ManualResetEvent synchronizationEvent = new(false);

    /// <summary>
    /// Initializes a new instance of the <see cref="WebDriverBidiCommand" /> class.
    /// </summary>
    /// <param name="commandId">The ID of the command.</param>
    /// <param name="commandData">The settings for the command, including parameters.</param>
    public WebDriverBidiCommand(long commandId, CommandData commandData)
    {
        this.commandId = commandId;
        this.commandData = commandData;
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
    public string CommandName => this.commandData.MethodName;

    /// <summary>
    /// Gets the parameters of the command.
    /// </summary>
    [JsonProperty("params")]
    public CommandData CommandParameters => this.commandData;

    /// <summary>
    /// Gets additional properties to be serialized with this command.
    /// </summary>
    [JsonExtensionData]
    [JsonConverter(typeof(ResponseValueJsonConverter))]
    public Dictionary<string, object?> AdditionalData => this.commandData.AdditionalData;

    /// <summary>
    /// Gets the type of the response for this command.
    /// </summary>
    public Type ResponseType => this.commandData.ResponseType;

    /// <summary>
    /// Gets a synchronization object used to wait for completion of the command.
    /// </summary>
    public ManualResetEvent SynchronizationEvent => this.synchronizationEvent;

    /// <summary>
    /// Gets or sets the result of the command.
    /// </summary>
    public ResponseData? Result { get; set; }

    /// <summary>
    /// Gets or sets the exception thrown during execution of the command, if any.
    /// </summary>
    public Exception? ThrownException { get; set; }
}