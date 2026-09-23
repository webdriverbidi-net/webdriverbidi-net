// Stands in for a browser or driver executable in launcher tests, then waits to be killed:
// - Given Chrome's "--remote-debugging-port=<port>" or Firefox's "--remote-debugging-port <port>",
//   it reports readiness the way that browser does, choosing a free port if given port 0.
// - Given a driver's "--port=<port>", it answers HTTP requests on that port as a ready driver,
//   exiting with code 1 if the port is in use.
// Environment variables:
// - WEBDRIVERBIDI_FAKE_BROWSER_MODE: "exit:<code>" exits at once with that code; "silent" never
//   reports readiness; "ignore-term" ignores SIGTERM.
// - WEBDRIVERBIDI_FAKE_BROWSER_LOG: a file to which each launch appends its arguments as a JSON line.
// - WEBDRIVERBIDI_FAKE_BROWSER_CHILD_PID_FILE: starts a child process that never exits, writing its
//   ID to this file; a graceful exit kills the child, as a real browser ends its own processes.
// - WEBDRIVERBIDI_FAKE_BROWSER_EXIT_FILE: a file written with "graceful" when asked to exit.
using System.Diagnostics;
using System.Net;
using System.Net.Sockets;
using System.Runtime.InteropServices;
using System.Text;
using System.Text.Json;

const string VariablePrefix = "WEBDRIVERBIDI_FAKE_BROWSER_";
string? mode = Environment.GetEnvironmentVariable(VariablePrefix + "MODE");
string? logFile = Environment.GetEnvironmentVariable(VariablePrefix + "LOG");
if (logFile is not null)
{
    File.AppendAllText(logFile, JsonSerializer.Serialize(args) + "\n");
}

if (mode is not null && mode.StartsWith("exit:", StringComparison.Ordinal))
{
    return int.Parse(mode["exit:".Length..]);
}

Process? child = null;
string? childProcessIdFile = Environment.GetEnvironmentVariable(VariablePrefix + "CHILD_PID_FILE");
if (childProcessIdFile is not null)
{
    ProcessStartInfo childStartInfo = new(Environment.ProcessPath!) { UseShellExecute = false };
    foreach (string name in childStartInfo.Environment.Keys.Where(name => name.StartsWith(VariablePrefix, StringComparison.Ordinal)).ToList())
    {
        childStartInfo.Environment.Remove(name);
    }

    child = Process.Start(childStartInfo)!;
    File.WriteAllText(childProcessIdFile, child.Id.ToString());
}

// Handled explicitly: without a handler, SIGTERM ends a .NET console app without raising ProcessExit.
string? exitFile = Environment.GetEnvironmentVariable(VariablePrefix + "EXIT_FILE");
using PosixSignalRegistration terminationHandler = PosixSignalRegistration.Create(PosixSignal.SIGTERM, context =>
{
    if (mode == "ignore-term")
    {
        context.Cancel = true;
        return;
    }

    child?.Kill();
    if (exitFile is not null)
    {
        File.WriteAllText(exitFile, "graceful");
    }
});

const string DriverPortPrefix = "--port=";
string? driverPort = args.FirstOrDefault(arg => arg.StartsWith(DriverPortPrefix, StringComparison.Ordinal))?[DriverPortPrefix.Length..];
if (driverPort is not null)
{
    TcpListener listener = new(IPAddress.Loopback, int.Parse(driverPort));
    try
    {
        listener.Start();
    }
    catch (SocketException)
    {
        return 1;
    }

    if (mode != "silent")
    {
        _ = Task.Run(() => AnswerStatusRequestsAsync(listener));
    }
}
else if (mode != "silent")
{
    const string ChromePortPrefix = "--remote-debugging-port=";
    string? chromePort = args.FirstOrDefault(arg => arg.StartsWith(ChromePortPrefix, StringComparison.Ordinal))?[ChromePortPrefix.Length..];
    int firefoxPortIndex = Array.IndexOf(args, "--remote-debugging-port") + 1;
    if (chromePort is not null)
    {
        Console.Error.WriteLine($"DevTools listening on ws://127.0.0.1:{ChoosePort(chromePort)}/devtools/browser/fake-browser");
    }
    else if (firefoxPortIndex > 0 && firefoxPortIndex < args.Length)
    {
        Console.Error.WriteLine($"WebDriver BiDi listening on ws://127.0.0.1:{ChoosePort(args[firefoxPortIndex])}");
    }
}

Thread.Sleep(Timeout.Infinite);
return 0;

static int ChoosePort(string requestedPort)
{
    if (requestedPort != "0")
    {
        return int.Parse(requestedPort);
    }

    TcpListener listener = new(IPAddress.Loopback, 0);
    listener.Start();
    int port = ((IPEndPoint)listener.LocalEndpoint).Port;
    listener.Stop();
    return port;
}

// Answers every request as a WebDriver classic "status" request from a ready driver.
static async Task AnswerStatusRequestsAsync(TcpListener listener)
{
    byte[] body = Encoding.UTF8.GetBytes("{\"value\":{\"ready\":true,\"message\":\"fake driver ready\"}}");
    byte[] headers = Encoding.ASCII.GetBytes($"HTTP/1.1 200 OK\r\nContent-Type: application/json; charset=utf-8\r\nContent-Length: {body.Length}\r\nConnection: close\r\n\r\n");
    while (true)
    {
        using TcpClient client = await listener.AcceptTcpClientAsync();
        NetworkStream stream = client.GetStream();
        byte[] buffer = new byte[4096];
        StringBuilder request = new();
        while (!request.ToString().Contains("\r\n\r\n"))
        {
            int read = await stream.ReadAsync(buffer);
            if (read == 0)
            {
                break;
            }

            request.Append(Encoding.ASCII.GetString(buffer, 0, read));
        }

        await stream.WriteAsync(headers);
        await stream.WriteAsync(body);
    }
}
