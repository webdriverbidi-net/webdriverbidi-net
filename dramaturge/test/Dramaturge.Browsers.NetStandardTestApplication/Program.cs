// <copyright file="Program.cs" company="WebDriverBiDi.NET Committers">
// Copyright (c) WebDriverBiDi.NET Committers. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.
// </copyright>

// Runs the netstandard2.0-only code of Dramaturge.Browsers against the fake browser whose path is given.
if (args.Length < 1)
{
    Console.Error.WriteLine("Usage: Dramaturge.Browsers.NetStandardTestApplication <fake-browser-path>");
    return 1;
}

try
{
    await BrowsersScenario.RunAsync(args[0]);
    Console.WriteLine("PASS: the netstandard2.0 build of Dramaturge.Browsers ran its netstandard2.0-only code.");
    return 0;
}
catch (Exception ex)
{
    Console.Error.WriteLine($"FAIL: {ex.GetType().Name}: {ex.Message}");
    Console.Error.WriteLine(ex.StackTrace);
    return 1;
}
