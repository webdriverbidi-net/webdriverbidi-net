namespace WebDriverBiDi.TestUtilities;

using System.Diagnostics;
using WebDriverBiDi.Protocol;

public class TestPipeServer : IPipeServerProcessProvider, IDisposable
{
    private static readonly string TestPipeServerPath = GetTestPipeServerPath();

    private bool disposed;

    public Process? ServerProcess { get; private set; }

    public List<string> Responses { get; } = [];

    public Process? PipeServerProcess => this.ServerProcess;

    private static string GetTestPipeServerPath()
    {
        // The NamedPipeTestApplication executable is copied to the TestApplications subdirectory
        // of the test output directory during build. See the <None Include=...> items in the
        // WebDriverBiDi.Tests.csproj file for how this copy is configured.
        string baseDir = AppContext.BaseDirectory;
        string exePath = Path.Combine(baseDir, "TestApplications", "WebDriverBiDi.NamedPipeTestApplication.dll");

        return !File.Exists(exePath)
            ? throw new WebDriverBiDiException($"Test pipe server executable not found at: {exePath}")
            : exePath;
    }

    public void Start(string readPipeHandle, string writePipeHandle)
    {
        List<string> arguments = [readPipeHandle, writePipeHandle, .. this.Responses];

        ProcessStartInfo startInfo = new()
        {
            FileName = "dotnet",
            ArgumentList = { TestPipeServerPath },
            RedirectStandardOutput = true,
            RedirectStandardInput = true,
            RedirectStandardError = true,
            UseShellExecute = false,
            CreateNoWindow = true,
        };

        foreach (string arg in arguments)
        {
            startInfo.ArgumentList.Add(arg);
        }

        Process childProc = new()
        {
            StartInfo = startInfo,
        };

        childProc.Start();
        this.ServerProcess = childProc;
    }

    public void Stop()
    {
        if (this.ServerProcess is not null)
        {
            this.ServerProcess.StandardInput.Write('\n');
            bool exited = this.ServerProcess.WaitForExit(TimeSpan.FromSeconds(5));
            if (!exited)
            {
                throw new WebDriverBiDiException("Test pipe server did not exit within 5 seconds");
            }
        }
    }

    // Reads exactly the number of characters the child is expected to echo, completing as soon as they
    // arrive. The previous helper raced a fixed one-second delay against a 50 ms Peek() poll, so its pass
    // depended on the child process starting and echoing inside a budget that also had to cover dotnet
    // process startup. Awaiting the data itself removes the wall clock from the assertion; the caller's
    // cancellation token supplies a generous safety bound so a genuine hang still fails rather than hangs.
    public async Task<string> ReadSentDataAsync(int expectedLength, CancellationToken cancellationToken)
    {
        if (this.ServerProcess is null)
        {
            return string.Empty;
        }

        char[] buffer = new char[expectedLength];
        int totalRead = 0;
        while (totalRead < expectedLength)
        {
            int read = await this.ServerProcess.StandardOutput.ReadAsync(buffer.AsMemory(totalRead, expectedLength - totalRead), cancellationToken);
            if (read == 0)
            {
                // The child closed its output before sending everything expected.
                break;
            }

            totalRead += read;
        }

        return new string(buffer, 0, totalRead);
    }

    public void Dispose()
    {
        this.Dispose(true);
        GC.SuppressFinalize(this);
    }

    protected virtual void Dispose(bool disposing)
    {
        if (this.disposed)
        {
            return;
        }

        if (disposing && this.ServerProcess is Process process)
        {
            // Force the child process down even when a test bails out (an assertion failure) before
            // calling Stop(), so the dotnet child process is never leaked. Unlike Stop(), this is a
            // best-effort safety net and never throws.
            try
            {
                if (!process.HasExited)
                {
                    try
                    {
                        process.StandardInput.Write('\n');
                    }
                    catch (IOException)
                    {
                    }

                    if (!process.WaitForExit(TimeSpan.FromSeconds(5)))
                    {
                        process.Kill(entireProcessTree: true);
                    }
                }
            }
            catch (InvalidOperationException)
            {
                // The process was never started or has already been released; nothing to clean up.
            }
            finally
            {
                process.Dispose();
            }

            this.ServerProcess = null;
        }

        this.disposed = true;
    }
}
