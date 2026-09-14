// <copyright file="AuthChallengeCredentials.cs" company="WebDriverBiDi.NET Committers">
// Copyright (c) WebDriverBiDi.NET Committers. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.
// </copyright>

namespace WebDriverBiDi.Client.Network;

using WebDriverBiDi.Network;

/// <summary>
/// Credentials to supply for an authentication challenge, optionally restricted to a challenge scheme and realm.
/// </summary>
public class AuthChallengeCredentials
{
    /// <summary>
    /// Initializes a new instance of the <see cref="AuthChallengeCredentials"/> class.
    /// </summary>
    /// <param name="userName">The user name to supply.</param>
    /// <param name="password">The password to supply.</param>
    public AuthChallengeCredentials(string userName, string password)
    {
        this.Credentials = new AuthCredentials(userName, password);
    }

    /// <summary>
    /// Gets the credentials to supply.
    /// </summary>
    public AuthCredentials Credentials { get; }

    /// <summary>
    /// Gets or sets the authentication scheme these credentials answer, such as <c>Basic</c>, compared without regard
    /// to case, or <see langword="null"/> to answer a challenge of any scheme.
    /// </summary>
    public string? Scheme { get; set; }

    /// <summary>
    /// Gets or sets the realm these credentials answer, compared exactly, or <see langword="null"/> to answer a
    /// challenge for any realm.
    /// </summary>
    public string? Realm { get; set; }

    /// <summary>
    /// Determines whether these credentials answer one of the challenges a response carries.
    /// </summary>
    /// <param name="challenges">The challenges the response carries, or <see langword="null"/> when it lists none.</param>
    /// <returns><see langword="true"/> if these credentials answer one of the challenges; otherwise, <see langword="false"/>.</returns>
    /// <remarks>
    /// A response that lists no challenges can only be answered by credentials that restrict neither the scheme nor
    /// the realm, since there is nothing to compare a restriction against.
    /// </remarks>
    internal bool Matches(IList<AuthChallenge>? challenges)
    {
        if (challenges is null || challenges.Count == 0)
        {
            return this.Scheme is null && this.Realm is null;
        }

        return challenges.Any(challenge =>
            (this.Scheme is null || string.Equals(this.Scheme, challenge.Scheme, StringComparison.OrdinalIgnoreCase)) &&
            (this.Realm is null || string.Equals(this.Realm, challenge.Realm, StringComparison.Ordinal)));
    }
}
