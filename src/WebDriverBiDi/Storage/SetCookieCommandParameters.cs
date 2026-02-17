// <copyright file="SetCookieCommandParameters.cs" company="WebDriverBiDi.NET Committers">
// Copyright (c) WebDriverBiDi.NET Committers. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.
// </copyright>

namespace WebDriverBiDi.Storage;

using System.Text.Json.Serialization;

/// <summary>
/// Provides parameters for the storage.setCookie command.
/// </summary>
public class SetCookieCommandParameters : CommandParameters<SetCookieCommandResult>
{
    /// <summary>
    /// Initializes a new instance of the <see cref="SetCookieCommandParameters"/> class.
    /// </summary>
    /// <param name="cookie">The values of the cookie to set.</param>
    public SetCookieCommandParameters(PartialCookie cookie)
    {
        this.Cookie = cookie;
    }

    /// <summary>
    /// Gets the method name of the command.
    /// </summary>
    [JsonIgnore]
    public override string MethodName => "storage.setCookie";

    /// <summary>
    /// Gets or sets the filter to use when getting the cookies.
    /// </summary>
    [JsonPropertyName("cookie")]
    public PartialCookie Cookie { get; set; }

    /// <summary>
    /// Gets or sets the partition descriptor to use when getting the cookies.
    /// </summary>
    [JsonPropertyName("partition")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingDefault)]
    public PartitionDescriptor? Partition { get; set; }
}
