// <copyright file="StatusCommandSettings.cs" company="WebDriverBidi.NET Committers">
// Copyright (c) WebDriverBidi.NET Committers. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.
// </copyright>

namespace WebDriverBidi.Session;

using System;
using Newtonsoft.Json;

/// <summary>
/// Provides parameters for the session.new command.
/// </summary>
[JsonObject(MemberSerialization.OptIn)]
public class StatusCommandSettings : CommandSettings
{
    /// <summary>
    /// Initializes a new instance of the <see cref="StatusCommandSettings"/> class.
    /// </summary>
    public StatusCommandSettings()
    {
    }

    /// <summary>
    /// Gets the method name of the command.
    /// </summary>
    public override string MethodName => "session.status";

    /// <summary>
    /// Gets the type of the result of the command.
    /// </summary>
    public override Type ResultType => typeof(StatusCommandResult);
}