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
/// properties inside the otherwise empty <c>result</c> object. Those are exposed through
/// <see cref="CommandResult.AdditionalData"/>, and extension properties found on the response
/// envelope through <see cref="CommandResult.AdditionalResponseProperties"/>, exactly as for every
/// other command result. The two positions are never merged, and neither stands in for the other:
/// an empty <c>result</c> object leaves <see cref="CommandResult.AdditionalData"/> empty however
/// many extension properties the envelope carries.
/// </remarks>
public record EmptyResult : CommandResult
{
}
