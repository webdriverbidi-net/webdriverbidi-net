// <copyright file="PageAssertions.cs" company="WebDriverBiDi.NET Committers">
// Copyright (c) WebDriverBiDi.NET Committers. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.
// </copyright>

namespace Dramaturge;

using System.Text.RegularExpressions;

/// <summary>
/// Expectations for a page's main frame, each checked again until it is met or its time runs out.
/// </summary>
public sealed class PageAssertions
{
    private readonly Page page;
    private readonly bool isNot;

    /// <summary>
    /// Initializes a new instance of the <see cref="PageAssertions"/> class.
    /// </summary>
    /// <param name="page">The page.</param>
    /// <param name="isNot">A value indicating whether the expectations are negated.</param>
    internal PageAssertions(Page page, bool isNot)
    {
        this.page = page;
        this.isNot = isNot;
    }

    /// <summary>
    /// Gets the negated expectations, each met when its condition does not hold.
    /// </summary>
    public PageAssertions Not => new(this.page, !this.isNot);

    /// <summary>
    /// Expects the page's URL to be a string. The URL is the one the browser last reported, by navigation, fragment
    /// navigation, or history update, so no command is sent.
    /// </summary>
    /// <param name="expected">The URL.</param>
    /// <param name="ignoreCase">A value indicating whether case is ignored.</param>
    /// <param name="timeout">The time to wait, or <see langword="null"/> for <see cref="DramaturgeOptions.ExpectTimeout"/>.</param>
    /// <param name="cancellationToken">A token that cancels the expectation.</param>
    /// <returns>A task that completes when the expectation is met.</returns>
    /// <exception cref="InvalidOperationException">Thrown when the page is closed while waiting.</exception>
    /// <exception cref="ExpectationFailedException">Thrown when the expectation is not met in time.</exception>
    public Task ToHaveUrlAsync(string expected, bool ignoreCase = false, TimeSpan? timeout = null, CancellationToken cancellationToken = default)
    {
        return this.ExpectUrlAsync(TextPattern.For(expected, ignoreCase), timeout, cancellationToken);
    }

    /// <summary>
    /// Expects the page's URL to match a regular expression, as the URL is reported for
    /// <see cref="ToHaveUrlAsync(string, bool, TimeSpan?, CancellationToken)"/>.
    /// </summary>
    /// <param name="expected">The regular expression, which may match anywhere in the URL.</param>
    /// <param name="timeout">The time to wait, or <see langword="null"/> for <see cref="DramaturgeOptions.ExpectTimeout"/>.</param>
    /// <param name="cancellationToken">A token that cancels the expectation.</param>
    /// <returns>A task that completes when the expectation is met.</returns>
    /// <exception cref="InvalidOperationException">Thrown when the page is closed while waiting.</exception>
    /// <exception cref="ExpectationFailedException">Thrown when the expectation is not met in time.</exception>
    public Task ToHaveUrlAsync(Regex expected, TimeSpan? timeout = null, CancellationToken cancellationToken = default)
    {
        return this.ExpectUrlAsync(TextPattern.For(expected), timeout, cancellationToken);
    }

    /// <summary>
    /// Expects the page's title, with white space normalized, to be a string.
    /// </summary>
    /// <param name="expected">The title; its white space is normalized too.</param>
    /// <param name="ignoreCase">A value indicating whether case is ignored.</param>
    /// <param name="timeout">The time to retry, or <see langword="null"/> for <see cref="DramaturgeOptions.ExpectTimeout"/>.</param>
    /// <param name="cancellationToken">A token that cancels the expectation.</param>
    /// <returns>A task that completes when the expectation is met.</returns>
    /// <exception cref="ExpectationFailedException">Thrown when the expectation is not met in time.</exception>
    public Task ToHaveTitleAsync(string expected, bool ignoreCase = false, TimeSpan? timeout = null, CancellationToken cancellationToken = default)
    {
        return this.ExpectTitleAsync(TextPattern.For(TextPattern.Normalize(expected), ignoreCase), timeout, cancellationToken);
    }

    /// <summary>
    /// Expects the page's title, with white space normalized, to match a regular expression.
    /// </summary>
    /// <param name="expected">The regular expression, which may match anywhere in the title.</param>
    /// <param name="timeout">The time to retry, or <see langword="null"/> for <see cref="DramaturgeOptions.ExpectTimeout"/>.</param>
    /// <param name="cancellationToken">A token that cancels the expectation.</param>
    /// <returns>A task that completes when the expectation is met.</returns>
    /// <exception cref="ExpectationFailedException">Thrown when the expectation is not met in time.</exception>
    public Task ToHaveTitleAsync(Regex expected, TimeSpan? timeout = null, CancellationToken cancellationToken = default)
    {
        return this.ExpectTitleAsync(TextPattern.For(expected), timeout, cancellationToken);
    }

    private string Describe(string expected)
    {
        return this.isNot ? $"not {expected}" : expected;
    }

    private Task ExpectUrlAsync(TextPattern pattern, TimeSpan? timeout, CancellationToken cancellationToken)
    {
        return this.page.MainFrame.ExpectUrlAsync(this.Describe($"to have URL {pattern.Description}"), this.isNot, pattern, timeout, cancellationToken);
    }

    private Task ExpectTitleAsync(TextPattern pattern, TimeSpan? timeout, CancellationToken cancellationToken)
    {
        return this.page.MainFrame.ExpectTitleAsync(this.Describe($"to have title {pattern.Description}"), this.isNot, pattern, timeout, cancellationToken);
    }
}
