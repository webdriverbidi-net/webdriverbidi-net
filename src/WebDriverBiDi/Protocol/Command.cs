// <copyright file="Command.cs" company="WebDriverBiDi.NET Committers">
// Copyright (c) WebDriverBiDi.NET Committers. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.
// </copyright>

namespace WebDriverBiDi.Protocol;

using System.Diagnostics.CodeAnalysis;
using System.Text.Json.Serialization;
using WebDriverBiDi.Internal;
using WebDriverBiDi.JsonConverters;

/// <summary>
/// Object containing data about a WebDriver BiDi command.
/// </summary>
[JsonConverter(typeof(CommandJsonConverter))]
public class Command
{
    private readonly TaskCompletionSource<CommandResult> taskCompletionSource = new(TaskCreationOptions.RunContinuationsAsynchronously);
    private readonly TimeProvider timeProvider;

    private long startTimestamp = ElapsedTimeUtilities.TimestampNotSet;
    private long stopTimestamp = ElapsedTimeUtilities.TimestampNotSet;

    /// <summary>
    /// Initializes a new instance of the <see cref="Command" /> class whose completion timeout is
    /// measured by <see cref="TimeProvider.System"/>.
    /// </summary>
    /// <param name="commandId">The ID of the command.</param>
    /// <param name="commandData">The settings for the command, including parameters.</param>
    /// <exception cref="ArgumentNullException">Thrown when the command parameters are null.</exception>
    public Command(long commandId, CommandParameters commandData)
        : this(commandId, commandData, TimeProvider.System)
    {
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="Command" /> class.
    /// </summary>
    /// <param name="commandId">The ID of the command.</param>
    /// <param name="commandData">The settings for the command, including parameters.</param>
    /// <param name="timeProvider">
    /// The <see cref="TimeProvider"/> whose clock measures the timeout passed to
    /// <see cref="WaitForCompletionAsync"/>. A <see cref="Transport"/> passes its own provider, so a
    /// transport whose waits run on virtual time (in a test, say) times its commands out on that same
    /// clock. <see cref="ElapsedMilliseconds"/> is a diagnostic measured against real time regardless.
    /// </param>
    /// <exception cref="ArgumentNullException">Thrown when the command parameters or the time provider are null.</exception>
    public Command(long commandId, CommandParameters commandData, TimeProvider timeProvider)
    {
        if (commandData is null)
        {
            throw new ArgumentNullException(nameof(commandData), "Command parameters must not be null");
        }

        this.CommandId = commandId;
        this.CommandParameters = commandData;
        this.CommandName = commandData.MethodName;
        this.timeProvider = timeProvider ?? throw new ArgumentNullException(nameof(timeProvider), "Time provider must not be null");
    }

    /// <summary>
    /// Gets the ID of the command.
    /// </summary>
    [JsonPropertyName("id")]
    public long CommandId { get; }

    /// <summary>
    /// Gets the method name of the command.
    /// </summary>
    [JsonPropertyName("method")]
    public string CommandName { get; }

    /// <summary>
    /// Gets the parameters of the command.
    /// </summary>
    [JsonPropertyName("params")]
    public CommandParameters CommandParameters { get; }

    /// <summary>
    /// Gets additional properties to be serialized with this command envelope.
    /// Note carefully this serializes these additional properties at the top
    /// level of the command; additional properties to be serialized with the
    /// command parameters should use the <see cref="CommandParameters.AdditionalData"/>
    /// property.
    /// </summary>
    /// <remarks>
    /// An entry may not use <c>id</c>, <c>method</c> or <c>params</c> as its name: the envelope writes those
    /// itself, so the message would carry the name twice, and a JSON object with a duplicate name has no
    /// defined meaning. Sending such a command throws <see cref="WebDriverBiDiSerializationException"/> rather
    /// than emitting the ambiguous payload.
    /// </remarks>
    [JsonExtensionData]
    public Dictionary<string, object?> AdditionalCommandProperties { get; } = [];

    /// <summary>
    /// Gets the type of the response for this command.
    /// </summary>
    [JsonIgnore]
    public Type ResponseType => this.CommandParameters.ResponseType;

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
    /// <remarks>
    /// This is zero until the command is sent, runs while the command is in flight, and freezes at the
    /// point a response, an error or a send failure stopped it.
    /// </remarks>
    [JsonIgnore]
    public long ElapsedMilliseconds
    {
        get
        {
            long start = Interlocked.Read(ref this.startTimestamp);
            if (start == ElapsedTimeUtilities.TimestampNotSet)
            {
                return 0;
            }

            long stop = Interlocked.Read(ref this.stopTimestamp);
            long end = stop == ElapsedTimeUtilities.TimestampNotSet ? ElapsedTimeUtilities.GetTimestamp() : stop;
            return ElapsedTimeUtilities.GetElapsedMilliseconds(start, end);
        }
    }

    /// <summary>
    /// Waits for the command to complete or until the specified timeout elapses.
    /// </summary>
    /// <param name="timeout">
    /// The timeout to wait for the command to complete. Must be non-negative and no greater than the maximum
    /// timer duration supported by the runtime, or <see cref="Timeout.InfiniteTimeSpan"/> to wait indefinitely.
    /// </param>
    /// <param name="cancellationToken">A cancellation token used to propagate notification that the operation should be canceled.</param>
    /// <returns><see langword="true"/> if the command completes before the timeout; otherwise <see langword="false"/>.</returns>
    /// <exception cref="ArgumentOutOfRangeException">Thrown when <paramref name="timeout"/> is negative (other than <see cref="Timeout.InfiniteTimeSpan"/>) or exceeds the maximum supported timer duration.</exception>
    /// <exception cref="OperationCanceledException">Thrown when <paramref name="cancellationToken"/> is canceled.</exception>
    public virtual async Task<bool> WaitForCompletionAsync(TimeSpan timeout, CancellationToken cancellationToken = default)
    {
        if (!TimeoutUtilities.IsValidTimeout(timeout))
        {
            throw new ArgumentOutOfRangeException(nameof(timeout), TimeoutUtilities.GetInvalidTimeoutMessage("Timeout"));
        }

        // A command that has already completed needs no wait at all, and no allocation to perform one. This
        // is also what a zero timeout requires: a completed command is reported as completed.
        Task<CommandResult> commandTask = this.taskCompletionSource.Task;
        if (commandTask.IsCompleted)
        {
            return true;
        }

        // The wait arms one timer against this command's TimeProvider and, only when the token can be
        // canceled, one registration on it. Racing the command against a delay with Task.WhenAny would
        // also need a linked CancellationTokenSource to cancel the delay once the command won, and a
        // delay task and a WhenAny task beside it, on every command.
        try
        {
            await this.WaitForOutcomeAsync(timeout, cancellationToken).ConfigureAwait(false);
        }
        catch (Exception) when (commandTask.IsCompleted)
        {
            // A command that faulted or was canceled has completed: the wait rethrows the command's own
            // outcome, which the caller reads from ThrownException or IsCanceled rather than from this
            // method. The filter is tested first so that the command's own cancellation is never mistaken
            // for the caller's, and so that a command that completes as the timeout elapses, or as the
            // caller's token is canceled, is reported as completed, which is the outcome the caller can act on.
        }
        catch (TimeoutException)
        {
            return false;
        }

        // Cancellation of the caller's token while the command is still pending falls through neither catch,
        // and propagates to the caller as the OperationCanceledException this method documents.
        return true;
    }

    /// <summary>
    /// Attempts to get the result of the command if it has completed successfully.
    /// </summary>
    /// <param name="commandResult">When this method returns, contains the result of the command, or <see langword="null"/> if the command has not completed successfully.</param>
    /// <returns><see langword="true"/> if the result was retrieved successfully; otherwise, <see langword="false"/>.</returns>
    public virtual bool TryGetResult([NotNullWhen(true)] out CommandResult? commandResult)
    {
        Task<CommandResult> task = this.taskCompletionSource.Task;
        if (task.IsCompleted && !task.IsFaulted && !task.IsCanceled)
        {
            commandResult = task.Result;
            return true;
        }

        commandResult = null;
        return false;
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
    /// Cancels the task used to wait for completion of this command, reporting
    /// whether the cancellation took effect.
    /// </summary>
    /// <returns>
    /// <see langword="true"/> if this call transitioned the command to the canceled state;
    /// <see langword="false"/> if the command had already completed with a result, faulted, or
    /// been canceled, in which case the existing outcome stands and this call has no effect.
    /// </returns>
    public virtual bool Cancel()
    {
        return this.taskCompletionSource.TrySetCanceled();
    }

    /// <summary>
    /// Records the point from which the execution of this command is timed. This should be
    /// called when the command is sent by a <see cref="Transport"/>.
    /// </summary>
    internal void StartTiming()
    {
        Interlocked.Exchange(ref this.startTimestamp, ElapsedTimeUtilities.GetTimestamp());
    }

    /// <summary>
    /// Records the point at which the timing of this command stops. This should be called by a
    /// <see cref="Transport"/> when a response or error is received for the command, or when sending
    /// it failed. Every caller reaches it having just taken the command out of the pending-command
    /// collection, so it runs at most once for a given command.
    /// </summary>
    internal void StopTiming()
    {
        Interlocked.Exchange(ref this.stopTimestamp, ElapsedTimeUtilities.GetTimestamp());
    }

    /// <summary>
    /// Asynchronously waits for this command's outcome, for the timeout to elapse, or for the token to be canceled,
    /// whichever happens first.
    /// </summary>
    /// <param name="timeout">
    /// The timeout, measured by this command's <see cref="TimeProvider"/>, or <see cref="Timeout.InfiniteTimeSpan"/> to wait
    /// until the command completes or the token is canceled.
    /// </param>
    /// <param name="cancellationToken">A cancellation token used to propagate notification that the wait should be canceled.</param>
    /// <returns>
    /// A task that completes as the command does if the command finishes first, rethrowing a fault or cancellation of
    /// the command itself; faults with a <see cref="TimeoutException"/> if the timeout elapses first; or is canceled if
    /// the token is canceled first.
    /// </returns>
    /// <remarks>
    /// <para>
    /// <see cref="WaitForCompletionAsync"/> calls this method only for a command that has not yet completed, and
    /// interprets how the returned task ends. A task that ends in any way at all, once the command has completed, is
    /// reported as completion: the command may complete in the moment between the timeout elapsing, or the token being
    /// canceled, and that outcome being examined, and the completed command is the outcome the caller can act on.
    /// </para>
    /// <para>
    /// This method is <see langword="protected virtual"/> to allow test doubles to fix the order in which the command
    /// completes and the wait ends, which in normal operation is decided by the thread pool.
    /// </para>
    /// </remarks>
    protected virtual Task WaitForOutcomeAsync(TimeSpan timeout, CancellationToken cancellationToken)
    {
        return TimeoutUtilities.WaitAsync(this.taskCompletionSource.Task, this.timeProvider, timeout, cancellationToken);
    }
}
