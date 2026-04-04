// <copyright file="BrowserExtractor.cs" company="WebDriverBiDi.NET Committers">
// Copyright (c) WebDriverBiDi.NET Committers. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.
// </copyright>

namespace WebDriverBiDi.Client.Launchers;

using System.Diagnostics;

/// <summary>
/// Base class for extracting downloaded browser binaries from downloaded installers to obtain the browser executable.
/// </summary>
public abstract class BrowserExtractor
{
    /// <summary>
    /// Extracts the browser from the downloaded installer to the specified directory,
    /// and returns the path to the extracted browser executable.
    /// </summary>
    /// <param name="installerPath">The path to the downloaded installer.</param>
    /// <param name="extractDir">The directory where the browser should be extracted.</param>
    /// <returns>A task representing the asynchronous operation.</returns>
    public abstract Task ExtractBrowserAsync(string installerPath, string extractDir);

    /// <summary>
    /// Runs a process with the specified file name and arguments, and waits for it to complete.
    /// If the process does not complete within the specified timeout, it is killed. If the process
    /// exits with a non-zero exit code, an exception is thrown with the standard output and error
    /// included in the message.
    /// </summary>
    /// <param name="fileName">The file name of the process to run.</param>
    /// <param name="arguments">The arguments for the process.</param>
    /// <param name="timeout">The timeout for the process. If omitted, a default timeout of 10 minutes is used.</param>
    /// <returns>A task representing the asynchronous operation.</returns>
    /// <exception cref="InvalidOperationException">Thrown when the process exits with a non-zero exit code.</exception>
    protected async Task RunProcessAsync(string fileName, string arguments, TimeSpan? timeout = null)
    {
        if (timeout == null)
        {
            timeout = TimeSpan.FromMinutes(10);
        }

        using Process process = new();
        process.StartInfo.FileName = fileName;
        process.StartInfo.Arguments = arguments;
        process.StartInfo.UseShellExecute = false;
        process.StartInfo.RedirectStandardOutput = true;
        process.StartInfo.RedirectStandardError = true;
        process.StartInfo.CreateNoWindow = true;
        process.Start();

        string stdout = await process.StandardOutput.ReadToEndAsync();
        string stderr = await process.StandardError.ReadToEndAsync();
        if (!process.WaitForExit(Convert.ToInt32(timeout.Value.TotalMilliseconds)))
        {
            process.Kill();
        }

        if (process.ExitCode != 0)
        {
            throw new InvalidOperationException($"Process '{fileName} {arguments}' exited with code {process.ExitCode}.\nstdout: {stdout}\nstderr: {stderr}");
        }
    }
}
