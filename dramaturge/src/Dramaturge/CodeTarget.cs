// <copyright file="CodeTarget.cs" company="WebDriverBiDi.NET Committers">
// Copyright (c) WebDriverBiDi.NET Committers. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.
// </copyright>

namespace Dramaturge;

/// <summary>
/// The kind of file a code recording writes.
/// </summary>
public enum CodeTarget
{
    /// <summary>
    /// A program of top-level statements that launches the recorded browser.
    /// </summary>
    Program,

    /// <summary>
    /// An xUnit test class built on Dramaturge.Xunit's PageTest.
    /// </summary>
    Xunit,

    /// <summary>
    /// An NUnit test class built on Dramaturge.NUnit's PageTest.
    /// </summary>
    NUnit,

    /// <summary>
    /// An MSTest test class built on Dramaturge.MSTest's PageTest.
    /// </summary>
    MSTest,

    /// <summary>
    /// A TUnit test class built on Dramaturge.TUnit's PageTest.
    /// </summary>
    TUnit,
}
