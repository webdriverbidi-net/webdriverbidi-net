// Stands in for a browser or driver executable in launcher tests, then waits to be killed:
// - Given Chrome's "--remote-debugging-port=<port>" or Firefox's "--remote-debugging-port <port>",
//   it reports readiness the way that browser does, choosing a free port if given port 0.
// - Given a driver's "--port=<port>", it answers HTTP requests on that port as a ready driver that
//   creates sessions, exiting with code 1 if the port is in use.
// - Given only "/ExtractDir=<directory>", it acts as the Firefox installer for Windows does, writing
//   core/firefox.exe into the directory, and exits.
// - Given only "--version", it writes "Fake Browser <version>" as a browser does, and exits; the version is also its
//   file version on Windows.
// Environment variables:
// - DRAMATURGE_FAKE_BROWSER_MODE: "exit:<code>" writes a line to stderr and exits at once with that code, first
//   writing numbered lines if given as "exit:<code>:<lines>"; "silent" never reports readiness; "ignore-term" ignores SIGTERM;
//   "hang-version" never answers "--version".
// - DRAMATURGE_FAKE_BROWSER_LOG: a file to which each launch appends a JSON line with its arguments
//   and the value of DRAMATURGE_FAKE_BROWSER_ECHO, and a driver appends one with each new session
//   request body.
// - DRAMATURGE_FAKE_BROWSER_CHILD_PID_FILE: starts a child process that never exits, writing its
//   ID to this file; a graceful exit kills the child, as a real browser ends its own processes.
// - DRAMATURGE_FAKE_BROWSER_EXIT_FILE: a file written with "graceful" when asked to exit.
using System.Diagnostics;
using System.Net;
using System.Net.Sockets;
using System.Runtime.InteropServices;
using System.Text;
using System.Text.Json;

if (args is [var extractArgument] && extractArgument.StartsWith("/ExtractDir=", StringComparison.Ordinal))
{
    string coreDirectory = Directory.CreateDirectory(Path.Combine(extractArgument["/ExtractDir=".Length..], "core")).FullName;
    File.WriteAllText(Path.Combine(coreDirectory, "firefox.exe"), string.Empty);
    return 0;
}

const string VariablePrefix = "DRAMATURGE_FAKE_BROWSER_";
string? mode = Environment.GetEnvironmentVariable(VariablePrefix + "MODE");
if (args is ["--version"])
{
    if (mode == "hang-version")
    {
        Thread.Sleep(Timeout.Infinite);
    }

    Console.WriteLine("Fake Browser 130.0.2849.80");
    return 0;
}

string? logFile = Environment.GetEnvironmentVariable(VariablePrefix + "LOG");
if (logFile is not null)
{
    string? echo = Environment.GetEnvironmentVariable(VariablePrefix + "ECHO");
    File.AppendAllText(logFile, JsonSerializer.Serialize(new Dictionary<string, object?>() { ["arguments"] = args, ["echo"] = echo }) + "\n");
}

if (mode is not null && mode.StartsWith("exit:", StringComparison.Ordinal))
{
    string[] exitParts = mode["exit:".Length..].Split(':');
    for (int line = 1; exitParts.Length > 1 && line <= int.Parse(exitParts[1]); line++)
    {
        Console.Error.WriteLine($"Output line {line}");
    }

    Console.Error.WriteLine($"Fake browser exiting with code {exitParts[0]}");
    return int.Parse(exitParts[0]);
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
        _ = Task.Run(() => AnswerDriverRequestsAsync(listener, logFile));
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

// Answers requests as a ready WebDriver classic driver: a POST creates a session, recording the
// request body, a DELETE ends one, and anything else is answered as a "status" request.
static async Task AnswerDriverRequestsAsync(TcpListener listener, string? logFile)
{
    while (true)
    {
        using TcpClient client = await listener.AcceptTcpClientAsync();
        NetworkStream stream = client.GetStream();
        byte[] buffer = new byte[65536];
        int length = 0;
        int headerEnd;
        while ((headerEnd = Encoding.ASCII.GetString(buffer, 0, length).IndexOf("\r\n\r\n", StringComparison.Ordinal)) < 0)
        {
            int read = await stream.ReadAsync(buffer.AsMemory(length));
            if (read == 0)
            {
                break;
            }

            length += read;
        }

        string headers = Encoding.ASCII.GetString(buffer, 0, Math.Max(headerEnd, 0));
        int contentLength = headers.Split("\r\n")
            .Where(line => line.StartsWith("Content-Length:", StringComparison.OrdinalIgnoreCase))
            .Select(line => int.Parse(line["Content-Length:".Length..].Trim()))
            .FirstOrDefault();
        int bodyStart = headerEnd + 4;
        while (headerEnd >= 0 && length < bodyStart + contentLength)
        {
            length += await stream.ReadAsync(buffer.AsMemory(length));
        }

        string response;
        if (headers.StartsWith("POST", StringComparison.Ordinal))
        {
            if (logFile is not null)
            {
                string requestBody = Encoding.UTF8.GetString(buffer, bodyStart, contentLength);
                File.AppendAllText(logFile, JsonSerializer.Serialize(new Dictionary<string, object?>() { ["sessionRequest"] = requestBody }) + "\n");
            }

            response = "{\"value\":{\"sessionId\":\"fake-session\",\"capabilities\":{\"webSocketUrl\":\"ws://127.0.0.1:9/session/fake-session\"}}}";
        }
        else if (headers.StartsWith("DELETE", StringComparison.Ordinal))
        {
            response = "{\"value\":null}";
        }
        else
        {
            response = "{\"value\":{\"ready\":true,\"message\":\"fake driver ready\"}}";
        }

        byte[] body = Encoding.UTF8.GetBytes(response);
        await stream.WriteAsync(Encoding.ASCII.GetBytes($"HTTP/1.1 200 OK\r\nContent-Type: application/json; charset=utf-8\r\nContent-Length: {body.Length}\r\nConnection: close\r\n\r\n"));
        await stream.WriteAsync(body);
    }
}
