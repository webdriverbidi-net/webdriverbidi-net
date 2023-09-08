// <copyright file="EmptyResult.cs" company="WebDriverBiDi.NET Committers">
// Copyright (c) WebDriverBiDi.NET Committers. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.
// </copyright>

namespace WebDriverBiDi;

using Newtonsoft.Json;

/// <summary>
/// Represents an empty result from a command.
/// </summary>
[JsonObject(MemberSerialization.OptIn)]
public class EmptyResult : CommandResult
{
}