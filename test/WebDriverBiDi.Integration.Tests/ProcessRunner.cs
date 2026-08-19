// <copyright file="ProcessRunner.cs" company="WebDriverBiDi.NET Committers">
// Copyright (c) WebDriverBiDi.NET Committers. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.
// </copyright>

namespace WebDriverBiDi.Integration.Tests;

using System.Diagnostics;
using Xunit.Sdk;

public static class ProcessRunner
{
    public static async Task<RunProcessResult> RunProcessAsync(string fileName, string arguments, string workingDirectory, int timeoutSeconds)
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

        // Read stdout/stderr concurrently to avoid deadlocks.
        Task<string> stdoutTask = process.StandardOutput.ReadToEndAsync();
        Task<string> stderrTask = process.StandardError.ReadToEndAsync();

        bool exited = await Task.Run(() => process.WaitForExit(timeoutSeconds * 1000));

        string stdout = await stdoutTask;
        string stderr = await stderrTask;

        RunProcessResult result = new()
        {
            FileName = fileName,
            ExitCode = process.ExitCode,
            StandardOutputConsoleContent = stdout,
            StandardErrorConsoleContent = stderr,
        };

        TestContext.Current.SendDiagnosticMessage($"[{Path.GetFileName(fileName)}] stdout:\n{stdout}");
        if (!string.IsNullOrWhiteSpace(stderr))
        {
            TestContext.Current.SendDiagnosticMessage($"[{Path.GetFileName(fileName)}] stderr:\n{stderr}");
        }

        if (!exited)
        {
            process.Kill(entireProcessTree: true);
            throw new XunitException($"Process '{fileName}' timed out after {timeoutSeconds}s.");
        }

        return result;
    }
}
