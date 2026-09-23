// <copyright file="ProcessOutputTail.cs" company="WebDriverBiDi.NET Committers">
// Copyright (c) WebDriverBiDi.NET Committers. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.
// </copyright>

namespace WebDriverBiDi.Browsers;

/// <summary>
/// The most recent lines a launched process wrote to its standard output and error streams, kept
/// to explain a failed launch.
/// </summary>
internal sealed class ProcessOutputTail
{
    private const int MaximumLineCount = 20;

    private readonly Queue<string> lines = new();

    /// <summary>
    /// Records a line of output, discarding the oldest once more than the maximum are kept.
    /// </summary>
    /// <param name="line">The line, or <see langword="null"/> at the end of a stream.</param>
    public void Add(string? line)
    {
        if (line is null)
        {
            return;
        }

        lock (this.lines)
        {
            this.lines.Enqueue(line);
            if (this.lines.Count > MaximumLineCount)
            {
                this.lines.Dequeue();
            }
        }
    }

    /// <summary>
    /// Discards every recorded line.
    /// </summary>
    public void Clear()
    {
        lock (this.lines)
        {
            this.lines.Clear();
        }
    }

    /// <summary>
    /// Gets a copy of the recorded lines, oldest first.
    /// </summary>
    /// <returns>The recorded lines.</returns>
    public IReadOnlyList<string> ToList()
    {
        lock (this.lines)
        {
            return [.. this.lines];
        }
    }
}
