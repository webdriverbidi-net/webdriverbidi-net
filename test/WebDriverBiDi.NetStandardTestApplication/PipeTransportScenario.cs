// <copyright file="PipeTransportScenario.cs" company="WebDriverBiDi.NET Committers">
// Copyright (c) WebDriverBiDi.NET Committers. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.
// </copyright>

using System.Diagnostics;
using System.Text;
using WebDriverBiDi.Protocol;

/// <summary>
/// Exercises the netstandard2.0 build's <see cref="PipeConnection"/> code — specifically the
/// <c>#else</c> (non-<c>NET5_0_OR_GREATER</c>) arms of its send path and its RECV trace — by running a
/// single round trip against the NamedPipeTestApplication pipe peer. The provider here implements the
/// netstandard2.0 <see cref="IPipeServerProcessProvider"/> directly rather than reusing the main test
/// project's TestPipeServer, which is compiled against the net10.0 build of the library.
/// </summary>
internal static class PipeTransportScenario
{
    public static async Task RunAsync(string pipePeerDllPath)
    {
        if (!File.Exists(pipePeerDllPath))
        {
            throw new FileNotFoundException($"Pipe peer application not found at '{pipePeerDllPath}'.");
        }

        const string expectedResponse = "netstandard2.0 pipe acknowledgement";
        using NamedPipePeerProcessProvider provider = new(pipePeerDllPath, expectedResponse);
        PipeConnection connection = new(provider);

        TaskCompletionSource<string> dataReceived = new(TaskCreationOptions.RunContinuationsAsynchronously);
        connection.OnDataReceived.AddObserver(e =>
        {
            dataReceived.TrySetResult(Encoding.UTF8.GetString(e.Data.ToArray()));
            return Task.CompletedTask;
        });

        // A log observer must be attached so PipeConnection's RECV trace runs when data is received;
        // that trace is one of the netstandard2.0-specific (#else) branches this scenario covers.
        connection.OnLogMessage.AddObserver(_ => { });

        provider.Start(connection.ReadPipeHandle, connection.WritePipeHandle);
        try
        {
            await connection.StartAsync("pipe://local");

            // Sending exercises the netstandard2.0 WritePipeDataAsync #else branch; the response the
            // peer pushes back is received while a log observer is attached, exercising the RECV trace.
            await connection.SendDataAsync(Encoding.UTF8.GetBytes("netstandard2.0 pipe ping"));

            string received = await dataReceived.Task.WaitAsync(TimeSpan.FromSeconds(10));
            if (received != expectedResponse)
            {
                throw new InvalidOperationException($"Pipe peer returned unexpected data: '{received}'.");
            }
        }
        finally
        {
            provider.Stop();
            await connection.StopAsync();
        }
    }

    private sealed class NamedPipePeerProcessProvider : IPipeServerProcessProvider, IDisposable
    {
        private readonly string peerDllPath;
        private readonly string response;

        public NamedPipePeerProcessProvider(string peerDllPath, string response)
        {
            this.peerDllPath = peerDllPath;
            this.response = response;
        }

        public Process? PipeServerProcess { get; private set; }

        public void Start(string readPipeHandle, string writePipeHandle)
        {
            ProcessStartInfo startInfo = new()
            {
                FileName = "dotnet",
                RedirectStandardInput = true,
                RedirectStandardOutput = true,
                RedirectStandardError = true,
                UseShellExecute = false,
                CreateNoWindow = true,
            };
            startInfo.ArgumentList.Add(this.peerDllPath);
            startInfo.ArgumentList.Add(readPipeHandle);
            startInfo.ArgumentList.Add(writePipeHandle);
            startInfo.ArgumentList.Add(this.response);

            Process process = new() { StartInfo = startInfo };
            process.Start();
            this.PipeServerProcess = process;
        }

        public void Stop()
        {
            if (this.PipeServerProcess is Process process && !process.HasExited)
            {
                try
                {
                    process.StandardInput.Write('\n');
                }
                catch (IOException)
                {
                    // The peer may have already exited; nothing to signal.
                }

                if (!process.WaitForExit(TimeSpan.FromSeconds(5)))
                {
                    process.Kill();
                }
            }
        }

        public void Dispose()
        {
            this.PipeServerProcess?.Dispose();
        }
    }
}
