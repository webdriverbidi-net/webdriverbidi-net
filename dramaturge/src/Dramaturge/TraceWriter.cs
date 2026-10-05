// <copyright file="TraceWriter.cs" company="WebDriverBiDi.NET Committers">
// Copyright (c) WebDriverBiDi.NET Committers. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.
// </copyright>

namespace Dramaturge;

using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using System.IO.Compression;
using System.Runtime.InteropServices;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;

/// <summary>
/// Writes the events of a trace, and the files they refer to, in version 9 of the format of Playwright's traces,
/// which Playwright's trace viewer reads.
/// </summary>
internal sealed class TraceWriter
{
    /// <summary>
    /// The version of the trace format written.
    /// </summary>
    public const int Version = 9;

    private readonly object lockObject = new();
    private readonly List<byte[]> events = [];
    private readonly Dictionary<string, byte[]> resources = [];

    /// <summary>
    /// Gets the time on the clock the trace's events are timed by: milliseconds, shared by every trace in the process.
    /// </summary>
    public static double Now => Math.Round(Stopwatch.GetTimestamp() * 1000.0 / Stopwatch.Frequency, 3);

    /// <summary>
    /// Writes the event that starts a trace.
    /// </summary>
    /// <param name="browserName">The name of the browser recorded.</param>
    /// <param name="title">The trace's title, or <see langword="null"/>.</param>
    /// <param name="testIdAttribute">The attribute test IDs are read from.</param>
    public void WriteContextOptions(string browserName, string? title, string testIdAttribute)
    {
        this.Write(writer =>
        {
            writer.WriteNumber("version", Version);
            writer.WriteString("type", "context-options");
            writer.WriteString("origin", "library");
            writer.WriteString("browserName", browserName);
            writer.WriteString("platform", Platform());
            writer.WriteNumber("wallTime", DateTimeOffset.UtcNow.ToUnixTimeMilliseconds());
            writer.WriteNumber("monotonicTime", Now);
            if (title is not null)
            {
                writer.WriteString("title", title);
            }

            writer.WriteStartObject("options");
            writer.WriteEndObject();
            writer.WriteString("sdkLanguage", "csharp");
            writer.WriteString("testIdAttributeName", testIdAttribute);
        });
    }

    /// <summary>
    /// Writes the event that starts an action.
    /// </summary>
    /// <param name="callId">The action's ID.</param>
    /// <param name="call">What the action is.</param>
    /// <param name="stack">The frames of the code that called it.</param>
    public void WriteBefore(string callId, TracedCall call, IReadOnlyList<TraceStackFrame> stack)
    {
        this.Write(writer =>
        {
            writer.WriteString("type", "before");
            writer.WriteString("callId", callId);
            writer.WriteNumber("startTime", Now);
            writer.WriteString("title", call.Title);
            if (call.Subtitle is not null)
            {
                writer.WriteString("subtitle", call.Subtitle);
            }

            writer.WriteString("class", call.ClassName);
            writer.WriteString("method", call.Method);
            writer.WriteStartObject("params");
            foreach (KeyValuePair<string, object> parameter in call.Parameters)
            {
                writer.WritePropertyName(parameter.Key);
                WriteValue(writer, parameter.Value);
            }

            writer.WriteEndObject();
            if (stack.Count > 0)
            {
                writer.WriteStartArray("stack");
                foreach (TraceStackFrame frame in stack)
                {
                    writer.WriteStartObject();
                    writer.WriteString("file", frame.File);
                    writer.WriteNumber("line", frame.Line);
                    writer.WriteNumber("column", frame.Column);
                    writer.WriteString("function", frame.Function);
                    writer.WriteEndObject();
                }

                writer.WriteEndArray();
            }
        });
    }

    /// <summary>
    /// Writes the event that ends an action.
    /// </summary>
    /// <param name="callId">The action's ID.</param>
    /// <param name="error">The exception the action threw, or <see langword="null"/>.</param>
    public void WriteAfter(string callId, Exception? error)
    {
        this.Write(writer =>
        {
            writer.WriteString("type", "after");
            writer.WriteString("callId", callId);
            writer.WriteNumber("endTime", Now);
            if (error is not null)
            {
                writer.WritePropertyName("error");
                WriteError(writer, error.GetType().Name, error.Message, error.ToString());
            }
        });
    }

    /// <summary>
    /// Writes a line of an action's log.
    /// </summary>
    /// <param name="callId">The action's ID.</param>
    /// <param name="message">The line.</param>
    public void WriteLog(string callId, string message)
    {
        this.Write(writer =>
        {
            writer.WriteString("type", "log");
            writer.WriteString("callId", callId);
            writer.WriteNumber("time", Now);
            writer.WriteString("message", message);
        });
    }

    /// <summary>
    /// Writes that a page opened.
    /// </summary>
    /// <param name="pageId">The page's ID.</param>
    /// <param name="openerPageId">The ID of the page that opened it, or <see langword="null"/>.</param>
    public void WritePageOpened(string pageId, string? openerPageId)
    {
        this.WriteEvent("page", null, writer =>
        {
            writer.WriteString("pageId", pageId);
            if (openerPageId is not null)
            {
                writer.WriteString("openerPageId", openerPageId);
            }
        });
    }

    /// <summary>
    /// Writes that a page closed.
    /// </summary>
    /// <param name="pageId">The page's ID.</param>
    public void WritePageClosed(string pageId)
    {
        this.WriteEvent("pageClosed", null, writer => writer.WriteString("pageId", pageId));
    }

    /// <summary>
    /// Writes a message a page wrote to its console.
    /// </summary>
    /// <param name="pageId">The page's ID.</param>
    /// <param name="type">The message's type, such as log, warning, or error.</param>
    /// <param name="text">The message.</param>
    /// <param name="location">Where in the page's scripts the message was written.</param>
    public void WriteConsoleMessage(string pageId, string type, string text, TraceSourceLocation location)
    {
        this.Write(writer =>
        {
            writer.WriteString("type", "console");
            writer.WriteNumber("time", Now);
            writer.WriteString("pageId", pageId);
            writer.WriteString("messageType", type);
            writer.WriteString("text", text);
            writer.WriteStartObject("location");
            writer.WriteString("url", location.Url);
            writer.WriteNumber("lineNumber", location.Line);
            writer.WriteNumber("columnNumber", location.Column);
            writer.WriteEndObject();
        });
    }

    /// <summary>
    /// Writes an error a page's script did not catch.
    /// </summary>
    /// <param name="pageId">The page's ID.</param>
    /// <param name="message">The error's message.</param>
    /// <param name="stack">The error's stack, as text, or <see langword="null"/>.</param>
    /// <param name="location">Where in the page's scripts the error was thrown.</param>
    public void WritePageError(string pageId, string message, string? stack, TraceSourceLocation location)
    {
        this.WriteEvent("pageError", pageId, writer =>
        {
            writer.WriteStartObject("error");
            writer.WritePropertyName("error");
            WriteError(writer, "Error", message, stack);
            writer.WriteEndObject();
            writer.WriteStartObject("location");
            writer.WriteString("url", location.Url);
            writer.WriteNumber("line", location.Line);
            writer.WriteNumber("column", location.Column);
            writer.WriteEndObject();
        });
    }

    /// <summary>
    /// Adds a file the trace's events refer to, named for its content, once.
    /// </summary>
    /// <param name="content">The file's content.</param>
    /// <returns>The file's path in the trace.</returns>
    public string AddResource(byte[] content)
    {
        string path = $"resources/{Sha1(content)}";
        lock (this.lockObject)
        {
            this.resources[path] = content;
        }

        return path;
    }

    /// <summary>
    /// Writes the trace to a zip file, replacing any file there.
    /// </summary>
    /// <param name="path">The file's path.</param>
    /// <param name="network">The lines of the trace's network events.</param>
    public void Save(string path, IReadOnlyList<string> network)
    {
        Directory.CreateDirectory(System.IO.Path.GetDirectoryName(path)!);
        using FileStream file = new(path, FileMode.Create, FileAccess.Write);
        using ZipArchive zip = new(file, ZipArchiveMode.Create);
        lock (this.lockObject)
        {
            using (Stream trace = zip.CreateEntry("trace.trace").Open())
            {
                foreach (byte[] line in this.events)
                {
                    trace.Write(line, 0, line.Length);
                    trace.WriteByte((byte)'\n');
                }
            }

            using (StreamWriter writer = new(zip.CreateEntry("trace.network").Open(), new UTF8Encoding(false)))
            {
                foreach (string line in network)
                {
                    writer.Write(line);
                    writer.Write('\n');
                }
            }

            foreach (KeyValuePair<string, byte[]> resource in this.resources)
            {
                using Stream entry = zip.CreateEntry(resource.Key).Open();
                entry.Write(resource.Value, 0, resource.Value.Length);
            }
        }
    }

    // The platform names of Node.js, which the viewer expects.
    [ExcludeFromCodeCoverage] // Each platform's branch runs only on that platform.
    private static string Platform()
    {
        if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
        {
            return "win32";
        }

        return RuntimeInformation.IsOSPlatform(OSPlatform.OSX) ? "darwin" : "linux";
    }

    private static string Sha1(byte[] content)
    {
        using SHA1 sha1 = SHA1.Create();
        return string.Concat(sha1.ComputeHash(content).Select(value => value.ToString("x2", CultureInfo.InvariantCulture)));
    }

    private static void WriteError(Utf8JsonWriter writer, string name, string message, string? stack)
    {
        writer.WriteStartObject();
        writer.WriteString("message", message);
        writer.WriteString("name", name);
        if (stack is not null)
        {
            writer.WriteString("stack", stack);
        }

        writer.WriteEndObject();
    }

    // A parameter is a string or a list of them.
    private static void WriteValue(Utf8JsonWriter writer, object value)
    {
        if (value is string text)
        {
            writer.WriteStringValue(text);
            return;
        }

        writer.WriteStartArray();
        foreach (string item in (IEnumerable<string>)value)
        {
            writer.WriteStringValue(item);
        }

        writer.WriteEndArray();
    }

    private void WriteEvent(string method, string? pageId, Action<Utf8JsonWriter> writeParameters)
    {
        this.Write(writer =>
        {
            writer.WriteString("type", "event");
            writer.WriteNumber("time", Now);
            writer.WriteString("class", "BrowserContext");
            writer.WriteString("method", method);
            writer.WriteStartObject("params");
            writeParameters(writer);
            writer.WriteEndObject();
            if (pageId is not null)
            {
                writer.WriteString("pageId", pageId);
            }
        });
    }

    private void Write(Action<Utf8JsonWriter> writeProperties)
    {
        using MemoryStream line = new();
        using (Utf8JsonWriter writer = new(line))
        {
            writer.WriteStartObject();
            writeProperties(writer);
            writer.WriteEndObject();
        }

        lock (this.lockObject)
        {
            this.events.Add(line.ToArray());
        }
    }
}
