// <copyright file="ExpectedEnding.cs" company="WebDriverBiDi.NET Committers">
// Copyright (c) WebDriverBiDi.NET Committers. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.
// </copyright>

namespace Dramaturge.TUnit;

using System.Collections.Concurrent;
using global::TUnit.Core;
using global::TUnit.Core.Interfaces;

/// <summary>
/// Checks, once a test has ended, what its browsers' teardown did, so that a test can fail on purpose to be captured.
/// A test registers its check while it runs; the test passes if the check finds nothing wrong, and fails with what
/// the check found otherwise.
/// </summary>
[AttributeUsage(AttributeTargets.Class)]
public sealed class ExpectedEndingAttribute : Attribute, ITestEndEventReceiver
{
    private static readonly ConcurrentDictionary<string, Func<TestContext, string?>> Checks = new();

    public int Order => 0;

    /// <summary>
    /// Registers the running test's check.
    /// </summary>
    /// <param name="check">Returns what is wrong, or <see langword="null"/>.</param>
    public static void Expect(Func<TestContext, string?> check)
    {
        Checks[TestContext.Current!.Id] = check;
    }

    /// <inheritdoc/>
    public ValueTask OnTestEnd(TestContext context)
    {
        if (Checks.TryRemove(context.Id, out Func<TestContext, string?>? check))
        {
            string? problem = check(context);
            context.Execution.OverrideResult(problem is null ? TestState.Passed : TestState.Failed, problem ?? "The test ended as expected.");
        }

        return default;
    }
}
