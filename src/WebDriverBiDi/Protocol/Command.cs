// <copyright file="Command.cs" company="WebDriverBiDi.NET Committers">
// Copyright (c) WebDriverBiDi.NET Committers. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.
// </copyright>

namespace WebDriverBiDi.Protocol;

using System.Text.Json.Serialization;
using WebDriverBiDi.JsonConverters;

/// <summary>
/// Object containing data about a WebDriver Bidi command.
/// </summary>
[JsonConverter(typeof(CommandJsonConverter))]
public class Command
{
    private readonly CommandParameters commandData;
    private readonly long commandId;
    private readonly TaskCompletionSource<CommandResult> taskCompletionSource = new();

    /// <summary>
    /// Initializes a new instance of the <see cref="Command" /> class.
    /// </summary>
    /// <param name="commandId">The ID of the command.</param>
    /// <param name="commandData">The settings for the command, including parameters.</param>
    public Command(long commandId, CommandParameters commandData)
    {
        this.commandId = commandId;
        this.commandData = commandData;
    }

    /// <summary>
    /// Gets the ID of the command.
    /// </summary>
    [JsonPropertyName("id")]
    public long CommandId => this.commandId;

    /// <summary>
    /// Gets the method name of the command.
    /// </summary>
    [JsonPropertyName("method")]
    public string CommandName => this.commandData.MethodName;

    /// <summary>
    /// Gets the parameters of the command.
    /// </summary>
    [JsonPropertyName("params")]
    public CommandParameters CommandParameters => this.commandData;

    /// <summary>
    /// Gets additional properties to be serialized with this command.
    /// </summary>
    [JsonExtensionData]
    public Dictionary<string, object?> AdditionalData => this.commandData.AdditionalData;

    /// <summary>
    /// Gets the type of the response for this command.
    /// </summary>
    [JsonIgnore]
    public Type ResponseType => this.commandData.ResponseType;

    /// <summary>
    /// Gets or sets the result of the command.
    /// </summary>
    [JsonIgnore]
    public CommandResult? Result
    {
        get
        {
            if (this.taskCompletionSource.Task.IsCompleted && !this.taskCompletionSource.Task.IsFaulted)
            {
                return this.taskCompletionSource.Task.Result;
            }

            return null;
        }

        set
        {
            if (value is not null)
            {
                this.taskCompletionSource.SetResult(value);
            }
        }
    }

    /// <summary>
    /// Gets or sets the exception thrown during execution of the command, if any.
    /// </summary>
    [JsonIgnore]
    public Exception? ThrownException
    {
        get
        {
            if (this.taskCompletionSource.Task.IsFaulted)
            {
                return this.taskCompletionSource.Task.Exception.InnerException;
            }

            return null;
        }

        set
        {
            if (value is not null)
            {
                this.taskCompletionSource.SetException(value);
            }
        }
    }

    /// <summary>
    /// Waits for the command to complete or until the specified timeout elapses.
    /// </summary>
    /// <param name="timeout">The timeout to wait for the command to complete.</param>
    /// <returns><see langword="true"/> if the command completes before the timeout; otherwise <see langword="false"/>.</returns>
    public async Task<bool> WaitForCompletionAsync(TimeSpan timeout)
    {
        Task completedTask = await Task.WhenAny(this.taskCompletionSource.Task, Task.Delay(timeout)).ConfigureAwait(false);
        return completedTask == this.taskCompletionSource.Task;
    }
}
