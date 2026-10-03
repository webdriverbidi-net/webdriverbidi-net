// <copyright file="TestEnding.cs" company="WebDriverBiDi.NET Committers">
// Copyright (c) WebDriverBiDi.NET Committers. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.
// </copyright>

namespace Dramaturge.NUnit;

using global::NUnit.Framework.Interfaces;
using global::NUnit.Framework.Internal;

/// <summary>
/// Ends a test object as a test with a given result ends, and reports what it attached and wrote.
/// </summary>
public sealed record TestEnding(IReadOnlyList<string> Attachments, string Output)
{
    public static async Task<TestEnding> EndAsync(BrowserTest test, ResultState state)
    {
        TestResult result = TestExecutionContext.CurrentContext.CurrentResult;
        int attachmentCount = result.TestAttachments.Count;
        int outputLength = result.Output.Length;
        result.SetResult(state);
        try
        {
            await test.TearDownBrowsersAsync();
            return new TestEnding([.. result.TestAttachments.Skip(attachmentCount).Select(attachment => attachment.FilePath)], result.Output[outputLength..]);
        }
        finally
        {
            result.SetResult(ResultState.Inconclusive);
        }
    }
}
