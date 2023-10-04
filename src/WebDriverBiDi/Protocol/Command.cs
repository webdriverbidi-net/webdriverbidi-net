// <copyright file="Command.cs" company="WebDriverBiDi.NET Committers">
// Copyright (c) WebDriverBiDi.NET Committers. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.
// </copyright>

namespace WebDriverBiDi.Protocol;

using Newtonsoft.Json;
using WebDriverBiDi.JsonConverters;

/// <summary>
/// Object containing data about a WebDriver Bidi command.
/// </summary>
[JsonObject(MemberSerialization.OptIn)]
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
    public CommandParameters CommandParameters => this.commandData;

    /// <summary>
    /// Gets additional properties to be serialized with this command.
    /// </summary>
    [JsonExtensionData]
    [JsonConverter(typeof(ReceivedDataJsonConverter))]
    public Dictionary<string, object?> AdditionalData => this.commandData.AdditionalData;

    /// <summary>
    /// Gets the type of the response for this command.
    /// </summary>
    public Type ResponseType => this.commandData.ResponseType;

    /// <summary>
    /// Gets or sets the result of the command.
    /// </summary>
    public virtual CommandResult? Result
    {
        get
        {
            if (this.taskCompletionSource.Task.IsCompleted && !this.taskCompletionSource.Task.IsFaulted && !this.taskCompletionSource.Task.IsCanceled)
            {
                return this.taskCompletionSource.Task.Result;
            }

            return null;
        }

        set
        {
            this.taskCompletionSource.SetResult(value!);
        }
    }

    /// <summary>
    /// Gets or sets the exception thrown during execution of the command, if any.
    /// </summary>
    public virtual Exception? ThrownException
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
    public virtual async Task<bool> WaitForCompletionAsync(TimeSpan timeout)
    {
        // Task.WhenAny returns when any of the tasks passed in completes, and
        // returns the task that completes first. If that task is the task from
        // our TaskCompletionSource, the command completed. Otherwise, it timed
        // out.
        Task completedTask = await Task.WhenAny(this.taskCompletionSource.Task, Task.Delay(timeout)).ConfigureAwait(false);
        return completedTask == this.taskCompletionSource.Task;
    }

    /// <summary>
    /// Cancels the task used to wait for completion of this command.
    /// </summary>
    public virtual void Cancel()
    {
        this.taskCompletionSource.SetCanceled();
    }
}