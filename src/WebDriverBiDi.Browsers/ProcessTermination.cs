// <copyright file="ProcessTermination.cs" company="WebDriverBiDi.NET Committers">
// Copyright (c) WebDriverBiDi.NET Committers. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.
// </copyright>

namespace WebDriverBiDi.Browsers;

using System.ComponentModel;
using System.Diagnostics;
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
    /// <returns><see langword="true"/> if the process exited within the timeout; otherwise, <see langword="false"/>.</returns>
    public static async Task<bool> WaitForExitAsync(Process process, TimeSpan timeout)
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

            using CancellationTokenSource timeoutSource = new(timeout);
            Task timeoutTask = Task.Delay(Timeout.Infinite, timeoutSource.Token);
            return await Task.WhenAny(exitedSource.Task, timeoutTask).ConfigureAwait(false) == exitedSource.Task;
        }
        finally
        {
            process.Exited -= OnExited;
        }
    }

    /// <summary>
    /// Asks a process to exit: SIGTERM on Unix, and a close message to its main window on Windows.
    /// </summary>
    /// <param name="process">The process.</param>
    /// <returns><see langword="true"/> if the request was delivered; otherwise, <see langword="false"/>.</returns>
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
