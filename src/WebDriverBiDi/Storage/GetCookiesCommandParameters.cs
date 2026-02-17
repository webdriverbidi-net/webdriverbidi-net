// <copyright file="GetCookiesCommandParameters.cs" company="WebDriverBiDi.NET Committers">
// Copyright (c) WebDriverBiDi.NET Committers. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.
// </copyright>

namespace WebDriverBiDi.Storage;

using System.Text.Json.Serialization;

/// <summary>
/// Provides parameters for the storage.getCookies command.
/// </summary>
public class GetCookiesCommandParameters : CommandParameters<GetCookiesCommandResult>
{
    /// <summary>
    /// Initializes a new instance of the <see cref="GetCookiesCommandParameters"/> class.
    /// </summary>
    public GetCookiesCommandParameters()
    {
    }

    /// <summary>
    /// Gets the method name of the command.
    /// </summary>
    [JsonIgnore]
    public override string MethodName => "storage.getCookies";

    /// <summary>
    /// Gets or sets the filter to use when getting the cookies.
    /// </summary>
    [JsonPropertyName("filter")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingDefault)]
    public CookieFilter? Filter { get; set; }

    /// <summary>
    /// Gets or sets the partition descriptor to use when getting the cookies.
    /// </summary>
    [JsonPropertyName("partition")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingDefault)]
    public PartitionDescriptor? Partition { get; set; }
}
