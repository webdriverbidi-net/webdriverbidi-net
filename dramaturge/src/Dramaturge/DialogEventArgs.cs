// <copyright file="DialogEventArgs.cs" company="WebDriverBiDi.NET Committers">
// Copyright (c) WebDriverBiDi.NET Committers. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.
// </copyright>

namespace Dramaturge;

using WebDriverBiDi;

/// <summary>
/// Data about a dialog a page opened.
/// </summary>
/// <param name="Dialog">The dialog.</param>
public record DialogEventArgs(Dialog Dialog) : WebDriverBiDiEventArgs;
