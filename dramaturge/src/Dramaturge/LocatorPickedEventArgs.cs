// <copyright file="LocatorPickedEventArgs.cs" company="WebDriverBiDi.NET Committers">
// Copyright (c) WebDriverBiDi.NET Committers. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.
// </copyright>

namespace Dramaturge;

using WebDriverBiDi;

/// <summary>
/// The data of the event raised when the user of a code recording picks an element with the toolbar's Pick locator.
/// </summary>
/// <param name="Page">The page the element is in.</param>
/// <param name="Locator">The element's locator.</param>
/// <param name="Code">The locator as C#, as the recording's statements write it.</param>
public record LocatorPickedEventArgs(Page Page, ElementLocator Locator, string Code) : WebDriverBiDiEventArgs;
