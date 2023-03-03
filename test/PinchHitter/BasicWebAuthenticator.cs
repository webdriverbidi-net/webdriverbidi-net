// <copyright file="BasicWebAuthenticator.cs" company="PinchHitter Committers">
// Copyright (c) PinchHitter Committers. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.
// </copyright>

namespace PinchHitter;

using System.Text;
using System.Text.RegularExpressions;

/// <summary>
/// A class that implements checking for Basic authentication.
/// </summary>
public class BasicWebAuthenticator : WebAuthenticator
{
    private readonly string authenticatedValue;

    /// <summary>
    /// Initializes a new instance of the <see cref="BasicWebAuthenticator"/> class.
    /// </summary>
    /// <param name="userName">The user name for the resource.</param>
    /// <param name="password">The password for the resource.</param>
    public BasicWebAuthenticator(string userName, string password)
    {
        this.authenticatedValue = Convert.ToBase64String(Encoding.UTF8.GetBytes($"{userName}:{password}"));
    }

    /// <summary>
    /// Gets a value indicating whether the candidate string is authenticated.
    /// </summary>
    /// <param name="authCandidate">The candidate string to test.</param>
    /// <returns><see langword="true" /> if the candidate string is authenticated; otherwise, <see langword="false" />.</returns>
    public override bool IsAuthenticated(string authCandidate)
    {
        Match regexMatch = Regex.Match(authCandidate, @"Basic\s*(.*)", RegexOptions.IgnoreCase);
        if (!regexMatch.Success)
        {
            return false;
        }

        return regexMatch.Groups[1].Value == this.authenticatedValue;
    }
}