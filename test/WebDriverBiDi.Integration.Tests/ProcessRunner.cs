// <copyright file="ProcessRunner.cs" company="WebDriverBiDi.NET Committers">
// Copyright (c) WebDriverBiDi.NET Committers. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.
// </copyright>

namespace WebDriverBiDi.Integration.Tests;

using System.Diagnostics;
using Xunit.Sdk;

public static class ProcessRunner
{
    // Once the process tree is gone, its ends of the redirected pipes are closed and the
    // pending reads complete immediately. This bounds the wait anyway, so that a descendant
    // that somehow escaped the kill can cost us the console output but never the test run.
    private static readonly TimeSpan ConsoleDrainTimeout = TimeSpan.FromSeconds(5);

    public static async Task<RunProcessResult> RunProcessAsync(string fileName, string arguments, string workingDirectory, TimeSpan timeout)
    {
        using Process process = new();
        process.StartInfo.FileName = fileName;
        process.StartInfo.Arguments = arguments;
        process.StartInfo.WorkingDirectory = workingDirectory;
        process.StartInfo.UseShellExecute = false;
        process.StartInfo.RedirectStandardOutput = true;
        process.StartInfo.RedirectStandardError = true;
        process.StartInfo.CreateNoWindow = true;

        process.Start();

        // Read stdout/stderr concurrently to avoid deadlocks. These tasks complete only when
        // their pipes reach end-of-stream, which cannot happen while the process still holds
        // the write ends open, so neither may be awaited until the process is known to be gone.
        Task<string> stdoutTask = process.StandardOutput.ReadToEndAsync();
        Task<string> stderrTask = process.StandardError.ReadToEndAsync();

        // Note the overload taking a timeout waits only for the process to exit, unlike the
        // parameterless overload, which additionally waits for the redirected streams to reach
        // end-of-stream. Draining those streams is handled explicitly below.
        bool exited = await Task.Run(() => process.WaitForExit(timeout));
        if (!exited)
        {
            // The process must be killed before its output can be collected, for the reason
            // given above.
            process.Kill(entireProcessTree: true);
            await ReportConsoleContentAsync(fileName, stdoutTask, stderrTask);
            throw new XunitException($"Process '{fileName}' timed out after {timeout.TotalSeconds}s.");
        }

        ConsoleOutputs outputs = await ReportConsoleContentAsync(fileName, stdoutTask, stderrTask);

        return new RunProcessResult()
        {
            FileName = fileName,
            ExitCode = process.ExitCode,
            StandardOutputConsoleContent = outputs.StandardOutput,
            StandardErrorConsoleContent = outputs.StandardError,
        };
    }

    private static async Task<ConsoleOutputs> ReportConsoleContentAsync(string fileName, Task<string> stdoutTask, Task<string> stderrTask)
    {
        string stdout = await DrainAsync(stdoutTask);
        string stderr = await DrainAsync(stderrTask);

        TestContext.Current.SendDiagnosticMessage($"[{Path.GetFileName(fileName)}] stdout:\n{stdout}");
        if (!string.IsNullOrWhiteSpace(stderr))
        {
            TestContext.Current.SendDiagnosticMessage($"[{Path.GetFileName(fileName)}] stderr:\n{stderr}");
        }

        return new ConsoleOutputs(stdout, stderr);
    }

    private static async Task<string> DrainAsync(Task<string> readTask)
    {
        Task completedTask = await Task.WhenAny(readTask, Task.Delay(ConsoleDrainTimeout));
        if (completedTask != readTask)
        {
            return $"<unavailable: the redirected pipe was still open {ConsoleDrainTimeout.TotalSeconds}s after the process was expected to be gone>";
        }

        return await readTask;
    }

    private record ConsoleOutputs
    {
        public ConsoleOutputs(string standardOutput, string standardError)
        {
            this.StandardOutput = standardOutput;
            this.StandardError = standardError;
        }

        public string StandardOutput { get; init; }

        public string StandardError { get; init; }
    }
}
