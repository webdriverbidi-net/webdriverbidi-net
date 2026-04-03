// <copyright file="Program.cs" company="WebDriverBiDi.NET Committers">
// Copyright (c) WebDriverBiDi.NET Committers. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.
// </copyright>

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
    public static void Main(string[] args)
    {
        BenchmarkSwitcher.FromAssembly(typeof(Program).Assembly).Run(args);
    }
}
