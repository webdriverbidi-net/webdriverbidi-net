// <copyright file="TracedCall.cs" company="WebDriverBiDi.NET Committers">
// Copyright (c) WebDriverBiDi.NET Committers. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.
// </copyright>

namespace Dramaturge;

/// <summary>
/// What an action recorded in a trace is: the title the viewer shows, the class and method it was called on, and its
/// parameters. A title or subtitle can name a parameter in braces, such as <c>Fill "{value}"</c>.
/// </summary>
/// <param name="Title">The title.</param>
/// <param name="Subtitle">The subtitle, or <see langword="null"/>.</param>
/// <param name="ClassName">The name of the class the action was called on.</param>
/// <param name="Method">The name of the method called.</param>
/// <param name="Parameters">The parameters, each a string or a list of strings.</param>
internal sealed record TracedCall(string Title, string? Subtitle, string ClassName, string Method, IReadOnlyDictionary<string, object> Parameters);
