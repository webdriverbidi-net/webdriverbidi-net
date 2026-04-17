using System.IO.Pipes;
using System.Text;

if (args.Length < 2)
{
    Console.Error.WriteLine("Usage: TestPipeServer <readPipeHandle> <writePipeHandle> [response1] [response2] ...");
    return 1;
}

string readPipeHandle = args[0];
string writePipeHandle = args[1];

CancellationTokenSource cancellationSource = new();
using PipeStream pipeReader = new AnonymousPipeClientStream(PipeDirection.In, readPipeHandle);
using PipeStream pipeWriter = new AnonymousPipeClientStream(PipeDirection.Out, writePipeHandle);

// Sets up ability to quit process by sending a keystroke to the stdin of the console.
_ = Task.Run(() =>
{
    Console.Read();
    cancellationSource.Cancel();
});

// Define a reasonable buffer size.
byte[] buffer = new byte[16 * 1024]; // 16 KB buffer

using MemoryStream memoryStream = new();
_ = Task.Run(async () =>
{
    while (!cancellationSource.IsCancellationRequested)
    {
        try
        {
            int bytesRead = await pipeReader.ReadAsync(buffer, cancellationSource.Token);
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
});

// Send responses if provided
if (args.Length > 2)
{
    for (int index = 2; index < args.Length; index++)
    {
        string toSend = args[index].Replace("\\0", "\0");
        byte[] output = Encoding.UTF8.GetBytes(toSend);
        await pipeWriter.WriteAsync(output);
        if (Array.IndexOf(output, Convert.ToByte(0)) < 0)
        {
            byte[] nullTerminator = [0];
            await pipeWriter.WriteAsync(nullTerminator);
        }

        await pipeWriter.FlushAsync();
    }
}

// Wait for cancellation
await Task.Run(() => cancellationSource.Token.WaitHandle.WaitOne());

return 0;
