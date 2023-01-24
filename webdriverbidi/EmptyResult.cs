// <copyright file="EmptyResult.cs" company="WebDriverBidi.NET Committers">
// Copyright (c) WebDriverBidi.NET Committers. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.
// </copyright>

namespace WebDriverBidi;

using Newtonsoft.Json;

/// <summary>
/// Represents an empty result from a command.
/// </summary>
[JsonObject(MemberSerialization.OptIn)]
public class EmptyResult : CommandResult
{
}