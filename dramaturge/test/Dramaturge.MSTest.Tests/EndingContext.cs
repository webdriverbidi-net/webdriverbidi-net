// <copyright file="EndingContext.cs" company="WebDriverBiDi.NET Committers">
// Copyright (c) WebDriverBiDi.NET Committers. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.
// </copyright>

namespace Dramaturge.MSTest;

using System.Text;

/// <summary>
/// The context of the running test, reporting another outcome, and keeping the files and output added to it, so that
/// a test object can be ended as a test with that outcome ends. MSTest gives it no display name, so it names the test
/// by its method, or by another name if given one.
/// </summary>
public sealed class EndingContext(TestContext running, UnitTestOutcome outcome, string? testName) : TestContext
{
    private readonly Dictionary<string, object?> properties = new(running.Properties) { ["TestName"] = testName ?? running.TestName };
    private readonly StringBuilder output = new();

    public List<string> ResultFiles { get; } = [];

    public string Output => this.output.ToString();

    public override UnitTestOutcome CurrentTestOutcome => outcome;

    public override IDictionary<string, object?> Properties => this.properties;

    public override void AddResultFile(string fileName) => this.ResultFiles.Add(fileName);

    public override void Write(string? message) => this.output.Append(message);

    public override void Write(string format, params object?[] args) => this.output.AppendFormat(format, args);

    public override void WriteLine(string? message) => this.output.AppendLine(message);

    public override void WriteLine(string format, params object?[] args) => this.output.AppendLine(string.Format(format, args));

    public override void DisplayMessage(MessageLevel messageLevel, string message) => this.output.AppendLine(message);

    public static async Task<EndingContext> EndAsync(BrowserTest test, TestContext running, UnitTestOutcome outcome, string? testName = null)
    {
        EndingContext context = new(running, outcome, testName);
        test.TestContext = context;
        await test.TearDownBrowsersAsync();
        return context;
    }
}
