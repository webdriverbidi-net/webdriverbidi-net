// <copyright file="ProcessTermination.cs" company="WebDriverBiDi.NET Committers">
// Copyright (c) WebDriverBiDi.NET Committers. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.
// </copyright>

namespace WebDriverBiDi.Browsers;

using System.ComponentModel;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Runtime.InteropServices;

/// <summary>
/// Stops processes started by the launchers.
/// </summary>
internal static class ProcessTermination
{
    /// <summary>
    /// The time allowed for a killed process to be reported as exited.
    /// </summary>
    internal static readonly TimeSpan KilledProcessExitTimeout = TimeSpan.FromSeconds(5);

    /// <summary>
    /// Waits for a process to exit.
    /// </summary>
    /// <param name="process">The process.</param>
    /// <param name="timeout">The maximum time to wait.</param>
    /// <param name="cancellationToken">A token that cancels the wait.</param>
    /// <returns><see langword="true"/> if the process exited within the timeout; otherwise, <see langword="false"/>.</returns>
    public static async Task<bool> WaitForExitAsync(Process process, TimeSpan timeout, CancellationToken cancellationToken = default)
    {
        TaskCompletionSource<bool> exitedSource = new(TaskCreationOptions.RunContinuationsAsynchronously);
        void OnExited(object? sender, EventArgs e) => exitedSource.TrySetResult(true);
        process.EnableRaisingEvents = true;
        process.Exited += OnExited;
        try
        {
            // Checked after subscribing, so an exit between the two cannot be missed.
            if (process.HasExited)
            {
                return true;
            }

            using CancellationTokenSource timeoutSource = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);
            timeoutSource.CancelAfter(timeout);
            Task timeoutTask = Task.Delay(Timeout.Infinite, timeoutSource.Token);
            if (await Task.WhenAny(exitedSource.Task, timeoutTask).ConfigureAwait(false) == exitedSource.Task)
            {
                return true;
            }

            cancellationToken.ThrowIfCancellationRequested();
            return false;
        }
        finally
        {
            process.Exited -= OnExited;
        }
    }

    /// <summary>
    /// Stops a process: optionally asks it to exit and waits up to <paramref name="shutdownTimeout"/>, then
    /// kills it and every process it started. The process is always stopped; cancellation only cuts the
    /// wait short.
    /// </summary>
    /// <param name="process">The process.</param>
    /// <param name="requestExit">A value indicating whether to ask the process to exit before killing it.</param>
    /// <param name="shutdownTimeout">The maximum time to wait for the process to exit when asked.</param>
    /// <param name="cancellationToken">A token that cancels waiting for the process to exit when asked.</param>
    /// <returns><see langword="true"/> if the wait was cancelled; otherwise, <see langword="false"/>.</returns>
    public static async Task<bool> StopAsync(Process process, bool requestExit, TimeSpan shutdownTimeout, CancellationToken cancellationToken)
    {
        bool isCancelled = false;
        bool hasExited = process.HasExited;
        if (!hasExited && requestExit && RequestExit(process))
        {
            try
            {
                hasExited = await WaitForExitAsync(process, shutdownTimeout, cancellationToken).ConfigureAwait(false);
            }
            catch (OperationCanceledException)
            {
                isCancelled = true;
            }
        }

        if (!hasExited)
        {
            KillTree(process);
            await WaitForExitAsync(process, KilledProcessExitTimeout).ConfigureAwait(false);
        }

        return isCancelled;
    }

    /// <summary>
    /// Waits for the output of an exited process, read through its output events, to be fully delivered.
    /// A process that started others may leave its output streams open, so the wait is bounded.
    /// </summary>
    /// <param name="process">The exited process.</param>
    /// <returns>A task that completes when the output is delivered or the wait times out.</returns>
    public static async Task WaitForOutputAsync(Process process)
    {
        // Unlike its overloads, the parameterless WaitForExit waits for the output events to be raised.
        Task outputTask = Task.Run(() =>
        {
            try
            {
                process.WaitForExit();
            }
            catch (InvalidOperationException)
            {
                // The process was disposed after the wait timed out.
            }
        });
        await Task.WhenAny(outputTask, Task.Delay(TimeSpan.FromSeconds(1))).ConfigureAwait(false);
    }

    /// <summary>
    /// Asks a process to exit: SIGTERM on Unix, and a close message to its main window on Windows.
    /// </summary>
    /// <param name="process">The process.</param>
    /// <returns><see langword="true"/> if the request was delivered; otherwise, <see langword="false"/>.</returns>
    [ExcludeFromCodeCoverage] // Takes only the branch for the operating system it runs on.
    public static bool RequestExit(Process process)
    {
        try
        {
            if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
            {
                // A windowless (e.g., headless) process has no way to be asked, so it is killed.
                return process.CloseMainWindow();
            }

            using Process signaller = Process.Start(new ProcessStartInfo("kill", $"-TERM {process.Id}") { UseShellExecute = false, CreateNoWindow = true })!;
            signaller.WaitForExit();
            return signaller.ExitCode == 0;
        }
        catch (Exception ex) when (ex is InvalidOperationException || ex is Win32Exception)
        {
            // The process has already exited, or the signal could not be sent.
            return false;
        }
    }

    /// <summary>
    /// Kills a process and, where the platform allows, every process it started.
    /// </summary>
    /// <param name="process">The process.</param>
    public static void KillTree(Process process)
    {
        try
        {
#if NET5_0_OR_GREATER
            process.Kill(entireProcessTree: true);
#else
            if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
            {
                using Process taskkill = Process.Start(new ProcessStartInfo("taskkill", $"/PID {process.Id} /T /F") { UseShellExecute = false, CreateNoWindow = true })!;
                taskkill.WaitForExit();
            }
            else
            {
                // Without the .NET 5 API, the descendants of a process cannot be found on Unix.
                process.Kill();
            }
#endif
        }
        catch (InvalidOperationException)
        {
            // The process has already exited.
        }
    }
}
