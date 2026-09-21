// <copyright file="Program.cs" company="WebDriverBiDi.NET Committers">
// Copyright (c) WebDriverBiDi.NET Committers. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.
// </copyright>

using BenchmarkDotNet.Reports;
using BenchmarkDotNet.Running;

namespace WebDriverBiDi.Benchmarks;

/// <summary>
/// Main entry point for the benchmarks application.
/// </summary>
public class Program
{
    /// <summary>
    /// Main method that runs the benchmarks.
    /// </summary>
    /// <param name="args">Command line arguments.</param>
    /// <returns>Zero when every benchmark ran, or one when any failed to.</returns>
    /// <remarks>
    /// A benchmark that throws, or that BenchmarkDotNet rejects as invalid, produces no results for the run to
    /// compare. Reporting that through the exit code makes the failure fail the step that ran it, rather than
    /// leaving the comparison to report a missing benchmark, or a baseline run to record a result-less baseline.
    /// </remarks>
    public static int Main(string[] args)
    {
        IEnumerable<Summary> summaries = BenchmarkSwitcher.FromAssembly(typeof(Program).Assembly).Run(args);
        bool anyFailed = false;
        foreach (Summary summary in summaries)
        {
            if (summary.HasCriticalValidationErrors)
            {
                anyFailed = true;
                continue;
            }

            foreach (BenchmarkReport report in summary.Reports)
            {
                if (!report.Success)
                {
                    anyFailed = true;
                }
            }
        }

        return anyFailed ? 1 : 0;
    }
}
