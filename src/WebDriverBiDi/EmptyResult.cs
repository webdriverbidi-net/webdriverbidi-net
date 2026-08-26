// <copyright file="EmptyResult.cs" company="WebDriverBiDi.NET Committers">
// Copyright (c) WebDriverBiDi.NET Committers. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.
// </copyright>

namespace WebDriverBiDi;

/// <summary>
/// Represents an empty result from a command.
/// </summary>
/// <remarks>
/// The protocol defines <c>EmptyResult</c> as extensible, so a remote end may place additional
/// properties inside the otherwise empty <c>result</c> object. When it does, those properties are
/// exposed through <see cref="CommandResult.AdditionalData"/> in preference to any extension
/// properties found on the response envelope; when the result object is empty, the envelope's
/// extension properties are exposed instead, as for every other command result.
/// </remarks>
public record EmptyResult : CommandResult
{
}
