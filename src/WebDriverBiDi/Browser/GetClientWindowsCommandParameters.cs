// <copyright file="GetClientWindowsCommandParameters.cs" company="WebDriverBiDi.NET Committers">
// Copyright (c) WebDriverBiDi.NET Committers. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.
// </copyright>

namespace WebDriverBiDi.Browser;

using System.Text.Json.Serialization;

/// <summary>
/// Provides parameters for the browser.getClientWindows command.
/// </summary>
public class GetClientWindowsCommandParameters : CommandParameters<GetClientWindowsCommandResult>
{
    /// <summary>
    /// Initializes a new instance of the <see cref="GetClientWindowsCommandParameters"/> class.
    /// </summary>
    public GetClientWindowsCommandParameters()
    {
    }

    /// <summary>
    /// Gets the method name of the command.
    /// </summary>
    [JsonIgnore]
    public override string MethodName => "browser.getClientWindows";
}
