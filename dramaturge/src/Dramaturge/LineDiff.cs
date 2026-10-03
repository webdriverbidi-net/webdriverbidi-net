// <copyright file="LineDiff.cs" company="WebDriverBiDi.NET Committers">
// Copyright (c) WebDriverBiDi.NET Committers. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.
// </copyright>

namespace Dramaturge;

/// <summary>
/// Compares two texts line by line, for failure messages.
/// </summary>
internal static class LineDiff
{
    /// <summary>
    /// Describes how one text differs from another: every line of both, in order, each marked with <c>- </c> if it is
    /// only in the expected text, <c>+ </c> if it is only in the received text, or two spaces if it is in both.
    /// </summary>
    /// <param name="expected">The expected text.</param>
    /// <param name="received">The received text.</param>
    /// <returns>The lines, joined by line breaks.</returns>
    public static string Describe(string expected, string received)
    {
        string[] expectedLines = expected.Split('\n');
        string[] receivedLines = received.Split('\n');

        // The length of the longest common subsequence of each pair of suffixes, from which the diff is read.
        int[,] common = new int[expectedLines.Length + 1, receivedLines.Length + 1];
        for (int i = expectedLines.Length - 1; i >= 0; i--)
        {
            for (int j = receivedLines.Length - 1; j >= 0; j--)
            {
                common[i, j] = expectedLines[i] == receivedLines[j] ? common[i + 1, j + 1] + 1 : Math.Max(common[i + 1, j], common[i, j + 1]);
            }
        }

        List<string> lines = [];
        int expectedIndex = 0;
        int receivedIndex = 0;
        while (expectedIndex < expectedLines.Length || receivedIndex < receivedLines.Length)
        {
            if (expectedIndex < expectedLines.Length && receivedIndex < receivedLines.Length && expectedLines[expectedIndex] == receivedLines[receivedIndex])
            {
                lines.Add(Mark("  ", expectedLines[expectedIndex++]));
                receivedIndex++;
            }
            else if (receivedIndex == receivedLines.Length || (expectedIndex < expectedLines.Length && common[expectedIndex + 1, receivedIndex] >= common[expectedIndex, receivedIndex + 1]))
            {
                lines.Add(Mark("- ", expectedLines[expectedIndex++]));
            }
            else
            {
                lines.Add(Mark("+ ", receivedLines[receivedIndex++]));
            }
        }

        return string.Join("\n", lines);
    }

    // An empty line is its marker alone, without trailing white space.
    private static string Mark(string marker, string line)
    {
        return line.Length == 0 ? marker.TrimEnd() : marker + line;
    }
}
