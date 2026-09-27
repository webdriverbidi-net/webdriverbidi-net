// <copyright file="PageEventArgs.cs" company="WebDriverBiDi.NET Committers">
// Copyright (c) WebDriverBiDi.NET Committers. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.
// </copyright>

namespace WebDriverBiDi.Automation;

/// <summary>
/// The data of an event about a <see cref="Automation.Page"/>, such as its creation.
/// </summary>
/// <param name="Page">The page.</param>
public record PageEventArgs(Page Page) : WebDriverBiDiEventArgs;
