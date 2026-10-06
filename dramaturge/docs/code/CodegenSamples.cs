// <copyright file="CodegenSamples.cs" company="WebDriverBiDi.NET Committers">
// Copyright (c) WebDriverBiDi.NET Committers. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.
// </copyright>
// Code snippets for docs/articles/codegen.md

namespace Dramaturge.Docs.Code;

using Dramaturge.Browsers;

/// <summary>
/// Snippets for the code generation guide. Compiled at build time to prevent API drift.
/// </summary>
public static class CodegenSamples
{
    /// <summary>
    /// Recording a user's actions as C# from code.
    /// </summary>
    /// <returns>A task representing the asynchronous operation.</returns>
    public static async Task RecordCode()
    {
        #region RecordCode
        await using BrowserGroup group = await BrowserGroup.LaunchAsync(BrowserLauncher.Configure(BrowserKind.Firefox).WithHeadlessOption(false));
        Browser browser = group.DefaultBrowser;
        Page page = browser.Pages.Count > 0 ? browser.Pages[0] : await browser.NewPageAsync();

        await using CodeRecording recording = await browser.RecordCodeAsync(new CodeRecordingOptions() { Target = CodeTarget.Xunit });
        recording.OnStatement.AddObserver(e => Console.WriteLine(e.Statement));
        recording.OnLocatorPicked.AddObserver(e => Console.WriteLine($"Picked: {e.Code}"));
        await page.NavigateAsync("https://example.com/");

        // The user acts in the browser until the program decides to stop, here when a key is pressed.
        Console.ReadKey();
        string code = await recording.StopAsync();
        await File.WriteAllTextAsync("RecordedTests.cs", code);
        #endregion
    }
}
