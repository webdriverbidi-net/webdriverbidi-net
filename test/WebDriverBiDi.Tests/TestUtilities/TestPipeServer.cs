namespace WebDriverBiDi.TestUtilities;

using System.Diagnostics;
using System.IO.Pipes;
using System.Text;
using Tmds.Utils;

public class TestPipeServer
{
    public Process? ServerProcess { get; private set; }

    public List<string> Responses { get; } = [];

    public void Start(string readPipeHandle, string writePipeHandle)
    {
        Process childProc = ExecFunction.Start(async args =>
        {
            CancellationTokenSource cancellationSource = new();
            using PipeStream pipeReader = new AnonymousPipeClientStream(PipeDirection.In, args[0]);
            using PipeStream pipeWriter = new AnonymousPipeClientStream(PipeDirection.Out, args[1]);

            // Sets up ability to quit process by sending a keystroke to the stdin of the console.
            _ = Task.Run(() =>
            {
                Console.Read();
                cancellationSource.Cancel();
            });

            // Define a reasonable buffer size.
            byte[] buffer = new byte[16 * 1024]; // 16 KB buffer

            using MemoryStream memoryStream = new();
            while (!cancellationSource.IsCancellationRequested)
            {
                try
                {
                    int bytesRead = await pipeReader.ReadAsync(buffer, 0, buffer.Length, cancellationSource.Token);
                    int startIndex = 0;
                    for (int byteIndex = 0; byteIndex < bytesRead; byteIndex++)
                    {
                        if (buffer[byteIndex] == 0)
                        {
                            if (byteIndex > startIndex)
                            {
                                memoryStream.Write(buffer, startIndex, byteIndex - startIndex);
                            }

                            if (memoryStream.Length > 0)
                            {
                                byte[] messageData = memoryStream.ToArray();
                                memoryStream.SetLength(0);
                                Console.Write(Encoding.UTF8.GetString(messageData));
                            }

                            startIndex = byteIndex + 1;
                        }
                    }
                }
                catch (OperationCanceledException)
                {
                }
            }

            if (args.Length > 2)
            {
                for (int index = 2; index < args.Length; index++)
                {
                    string toSend = args[index].Replace("\\0", "\0");
                    byte[] output = Encoding.UTF8.GetBytes(toSend);
                    pipeWriter.Write(output, 0, output.Length);
                    if (Array.IndexOf(output, Convert.ToByte(0)) < 0)
                    {
                        byte[] nullTerminator = [0];
                        pipeWriter.Write(nullTerminator, 0, 1);
                    }
                    
                    pipeWriter.Flush();
                }
            }
        },
        [readPipeHandle, writePipeHandle, .. this.Responses],
        options =>
        {
            options.StartInfo.RedirectStandardOutput = true;
            options.StartInfo.RedirectStandardInput = true;
        });

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
