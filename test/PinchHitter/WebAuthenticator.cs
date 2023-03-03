// <copyright file="WebAuthenticator.cs" company="PinchHitter Committers">
// Copyright (c) PinchHitter Committers. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.
// </copyright>

namespace PinchHitter;

/// <summary>
/// Abstract base class for authenticators for the web.
/// </summary>
public abstract class WebAuthenticator
{
    /// <summary>
    /// Gets a value indicating whether the candidate string is authenticated.
    /// </summary>
    /// <param name="authCandidate">The candidate string to test.</param>
    /// <returns><see langword="true" /> if the candidate string is authenticated; otherwise, <see langword="false" />.</returns>
    public abstract bool IsAuthenticated(string authCandidate);
}