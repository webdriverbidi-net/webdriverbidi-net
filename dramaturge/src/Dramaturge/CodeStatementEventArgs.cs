// <copyright file="CodeStatementEventArgs.cs" company="WebDriverBiDi.NET Committers">
// Copyright (c) WebDriverBiDi.NET Committers. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.
// </copyright>

namespace Dramaturge;

using WebDriverBiDi;

/// <summary>
/// The data of the event raised when a code recording's statement settles.
/// </summary>
/// <param name="Statement">The statement, as C# without indentation.</param>
public record CodeStatementEventArgs(string Statement) : WebDriverBiDiEventArgs;
