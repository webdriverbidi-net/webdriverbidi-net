// <copyright file="Command.cs" company="WebDriverBiDi.NET Committers">
// Copyright (c) WebDriverBiDi.NET Committers. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.
// </copyright>

namespace WebDriverBiDi.Protocol;

using System.Diagnostics;
using System.Text.Json.Serialization;
using WebDriverBiDi.JsonConverters;

/// <summary>
/// Object containing data about a WebDriver Bidi command.
/// </summary>
[JsonConverter(typeof(CommandJsonConverter))]
public class Command
{
    private readonly Stopwatch commandStopwatch = new();
    private readonly CommandParameters commandData;
    private readonly long commandId;
    private readonly TaskCompletionSource<CommandResult> taskCompletionSource = new(TaskCreationOptions.RunContinuationsAsynchronously);

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
    /// Gets the result of the command, or <see langword="null"/> if the command has not
    /// completed successfully.
    /// </summary>
    [JsonIgnore]
    public virtual CommandResult? Result
    {
        get
        {
            Task<CommandResult> task = this.taskCompletionSource.Task;
            if (task.IsCompleted && !task.IsFaulted && !task.IsCanceled)
            {
                return task.Result;
            }

            return null;
        }
    }

    /// <summary>
    /// Gets the exception thrown during execution of the command, or <see langword="null"/>
    /// if the command has not faulted.
    /// </summary>
    [JsonIgnore]
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
    }

    /// <summary>
    /// Gets a value indicating whether this command has been canceled.
    /// </summary>
    [JsonIgnore]
    public bool IsCanceled => this.taskCompletionSource.Task.IsCanceled;

    /// <summary>
    /// Gets the elapsed time in milliseconds since the command was sent by a <see cref="Transport"/>.
    /// </summary>
    public long ElapsedMilliseconds => this.commandStopwatch.ElapsedMilliseconds;

    /// <summary>
    /// Waits for the command to complete or until the specified timeout elapses.
    /// </summary>
    /// <param name="timeout">The timeout to wait for the command to complete.</param>
    /// <param name="cancellationToken">A cancellation token used to propagate notification that the operation should be canceled.</param>
    /// <returns><see langword="true"/> if the command completes before the timeout; otherwise <see langword="false"/>.</returns>
    /// <exception cref="OperationCanceledException">Thrown when <paramref name="cancellationToken"/> is canceled.</exception>
    public virtual async Task<bool> WaitForCompletionAsync(TimeSpan timeout, CancellationToken cancellationToken = default)
    {
        // Task.WhenAny returns when any of the tasks passed in completes, and
        // returns the task that completes first. If that task is the task from
        // our TaskCompletionSource, the command completed. Otherwise, it timed
        // out or was externally canceled.
        using CancellationTokenSource linkedTokenSource = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);
        Task timeoutTask = Task.Delay(timeout, linkedTokenSource.Token);
        Task completedTask = await Task.WhenAny(this.taskCompletionSource.Task, timeoutTask).ConfigureAwait(false);
        bool commandTaskCompleted = completedTask == this.taskCompletionSource.Task;
        if (commandTaskCompleted)
        {
            linkedTokenSource.Cancel();
        }
        else
        {
            cancellationToken.ThrowIfCancellationRequested();
        }

        return commandTaskCompleted;
    }

    /// <summary>
    /// Sets the result of the command, completing the underlying task. If the command has
    /// already been completed, faulted, or canceled, this method is a safe no-op.
    /// </summary>
    /// <param name="result">The result of the command.</param>
    public virtual void SetResult(CommandResult result)
    {
        this.taskCompletionSource.TrySetResult(result);
    }

    /// <summary>
    /// Faults the command with the specified exception, completing the underlying task.
    /// If the command has already been completed, faulted, or canceled, this method is
    /// a safe no-op.
    /// </summary>
    /// <param name="exception">The exception that caused the command to fail.</param>
    public virtual void SetException(Exception exception)
    {
        this.taskCompletionSource.TrySetException(exception);
    }

    /// <summary>
    /// Cancels the task used to wait for completion of this command.
    /// </summary>
    public virtual void Cancel()
    {
        this.taskCompletionSource.TrySetCanceled();
    }

    /// <summary>
    /// Starts the stopwatch used to time the execution of this command. This should be
    /// called when the command is sent by a <see cref="Transport"/>.
    /// </summary>
    internal void StartTiming()
    {
        this.commandStopwatch.Start();
    }

    /// <summary>
    /// Stops the stopwatch used to time the execution of this command. This should be
    /// called by a <see cref="Transport"/> when a response or error is received for the
    /// command.
    /// </summary>
    internal void StopTiming()
    {
        this.commandStopwatch.Stop();
    }
}
