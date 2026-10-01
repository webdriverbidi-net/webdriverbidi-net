// <copyright file="CommandLine.cs" company="WebDriverBiDi.NET Committers">
// Copyright (c) WebDriverBiDi.NET Committers. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.
// </copyright>

namespace Dramaturge.Browsers;

using System.Text;

/// <summary>
/// Builds command lines for launched processes.
/// </summary>
internal static class CommandLine
{
    private static readonly char[] CharactersRequiringQuotes = [' ', '\t', '\n', '\v', '"'];

    /// <summary>
    /// Joins arguments into a string for <see cref="System.Diagnostics.ProcessStartInfo.Arguments"/>,
    /// quoted by the rules of the Windows C runtime. .NET parses that property by the same rules on
    /// every platform, so each argument reaches the process unchanged.
    /// </summary>
    /// <param name="arguments">The arguments.</param>
    /// <returns>The joined arguments.</returns>
    public static string JoinArguments(IEnumerable<string> arguments)
    {
        return string.Join(" ", arguments.Select(QuoteArgument));
    }

    /// <summary>
    /// Quotes an argument for a POSIX shell.
    /// </summary>
    /// <param name="argument">The argument.</param>
    /// <returns>The quoted argument.</returns>
    public static string QuotePosixShellArgument(string argument)
    {
        return "'" + argument.Replace("'", "'\\''") + "'";
    }

    private static string QuoteArgument(string argument)
    {
        if (argument.Length > 0 && argument.IndexOfAny(CharactersRequiringQuotes) < 0)
        {
            return argument;
        }

        // Backslashes are literal except before a quote, where each must be doubled; so must a
        // run of them before the closing quote.
        StringBuilder quoted = new("\"");
        int pendingBackslashes = 0;
        foreach (char character in argument)
        {
            if (character == '\\')
            {
                pendingBackslashes++;
                continue;
            }

            if (character == '"')
            {
                quoted.Append('\\', (pendingBackslashes * 2) + 1);
            }
            else
            {
                quoted.Append('\\', pendingBackslashes);
            }

            quoted.Append(character);
            pendingBackslashes = 0;
        }

        quoted.Append('\\', pendingBackslashes * 2);
        quoted.Append('"');
        return quoted.ToString();
    }
}
