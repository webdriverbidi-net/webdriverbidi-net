// <copyright file="FileExtractor.cs" company="WebDriverBiDi.NET Committers">
// Copyright (c) WebDriverBiDi.NET Committers. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.
// </copyright>

namespace WebDriverBiDi.Browsers;

using System.Diagnostics;

/// <summary>
/// Base class for extracting downloaded binary files from downloaded installers to obtain the executable.
/// </summary>
public abstract class FileExtractor
{
    /// <summary>
    /// Extracts the file from the downloaded installer to the specified directory,
    /// and returns the path to the extracted browser executable.
    /// </summary>
    /// <param name="installerPath">The path to the downloaded installer.</param>
    /// <param name="extractDir">The directory where the file should be extracted.</param>
    /// <returns>A task representing the asynchronous operation.</returns>
    public abstract Task ExtractFileContentsAsync(string installerPath, string extractDir);

    /// <summary>
    /// Runs a process with the specified file name and arguments, and waits for it to complete.
    /// If the process and its output do not complete within the specified timeout, the process is
    /// killed. If the process exits with a non-zero exit code, an exception is thrown with the
    /// standard output and error included in the message.
    /// </summary>
    /// <param name="fileName">The file name of the process to run.</param>
    /// <param name="arguments">The arguments for the process.</param>
    /// <param name="timeout">The timeout for the process. If omitted, a default timeout of 10 minutes is used.</param>
    /// <returns>A task representing the asynchronous operation.</returns>
    /// <exception cref="InvalidOperationException">Thrown when the process exits with a non-zero exit code.</exception>
    /// <exception cref="WebDriverBiDiTimeoutException">Thrown when the process does not complete within the timeout.</exception>
    protected async Task RunProcessAsync(string fileName, string arguments, TimeSpan? timeout = null)
    {
        TimeSpan processTimeout = timeout ?? TimeSpan.FromMinutes(10);
        using Process process = new();
        process.StartInfo.FileName = fileName;
        process.StartInfo.Arguments = arguments;
        process.StartInfo.UseShellExecute = false;
        process.StartInfo.RedirectStandardOutput = true;
        process.StartInfo.RedirectStandardError = true;
        process.StartInfo.CreateNoWindow = true;
        process.EnableRaisingEvents = true;
        TaskCompletionSource<bool> exitedSource = new(TaskCreationOptions.RunContinuationsAsynchronously);
        process.Exited += (sender, e) => exitedSource.TrySetResult(true);
        process.Start();

        // Drained concurrently: a process blocked writing to a full stderr pipe never closes stdout.
        Task<string> stdoutTask = process.StandardOutput.ReadToEndAsync();
        Task<string> stderrTask = process.StandardError.ReadToEndAsync();

        // A child process that inherits the output pipes can hold them open after the process exits,
        // so reading the output is bounded by the same timeout.
        using CancellationTokenSource timeoutSource = new(processTimeout);
        Task timeoutTask = Task.Delay(Timeout.Infinite, timeoutSource.Token);
        Task completedTask = Task.WhenAll(exitedSource.Task, stdoutTask, stderrTask);
        if (await Task.WhenAny(completedTask, timeoutTask).ConfigureAwait(false) != completedTask)
        {
            ProcessTermination.KillTree(process);
            throw new WebDriverBiDiTimeoutException($"Process '{fileName} {arguments}' did not complete within {processTimeout.TotalSeconds} seconds.");
        }

        timeoutSource.Cancel();
        string stdout = await stdoutTask.ConfigureAwait(false);
        string stderr = await stderrTask.ConfigureAwait(false);
        if (process.ExitCode != 0)
        {
            throw new InvalidOperationException($"Process '{fileName} {arguments}' exited with code {process.ExitCode}.\nstdout: {stdout}\nstderr: {stderr}");
        }
    }
}
