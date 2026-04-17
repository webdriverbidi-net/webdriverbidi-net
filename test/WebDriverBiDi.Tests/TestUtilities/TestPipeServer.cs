namespace WebDriverBiDi.TestUtilities;

using System.Diagnostics;
using System.Reflection;
using WebDriverBiDi.Protocol;

public class TestPipeServer : IPipeServerProcessProvider
{
    private static readonly string TestPipeServerPath = GetTestPipeServerPath();

    public Process? ServerProcess { get; private set; }

    public List<string> Responses { get; } = [];

    public Process? PipeServerProcess => this.ServerProcess;

    private static string GetTestPipeServerPath()
    {
        string assemblyPath = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location)
            ?? throw new WebDriverBiDiException("Could not determine assembly location");

        string exePath = Path.Combine(assemblyPath, "..", "..", "..", "..", "WebDriverBiDi.TestPipeServer", "bin", "Debug", "net10.0", "WebDriverBiDi.TestPipeServer.dll");
        string normalizedPath = Path.GetFullPath(exePath);

        return !File.Exists(normalizedPath)
            ? throw new WebDriverBiDiException($"Test pipe server executable not found at: {normalizedPath}")
            : normalizedPath;
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

    public bool WaitForDataSent(TimeSpan timeout)
    {
        Task timeoutTask = Task.Delay(timeout);
        Task peekTask = Task.Run(async () =>
        {
            if (this.ServerProcess is not null)
            {
                while (this.ServerProcess.StandardOutput.Peek() < 0)
                {
                    await Task.Delay(TimeSpan.FromMilliseconds(50));
                }
            }
        });
        int completedTaskIndex = Task.WaitAny(peekTask, timeoutTask);
        return completedTaskIndex == 0;
    }

    public string GetSentData()
    {
        if (this.ServerProcess is not null)
        {
            return this.ServerProcess.StandardOutput.ReadToEnd();
        }

        return string.Empty;
    }
}
