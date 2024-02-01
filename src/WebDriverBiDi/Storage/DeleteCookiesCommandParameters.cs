// <copyright file="DeleteCookiesCommandParameters.cs" company="WebDriverBiDi.NET Committers">
// Copyright (c) WebDriverBiDi.NET Committers. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.
// </copyright>

namespace WebDriverBiDi.Storage;

using System.Text.Json.Serialization;

/// <summary>
/// Provides parameters for the storage.deleteCookies command.
/// </summary>
public class DeleteCookiesCommandParameters : CommandParameters<DeleteCookiesCommandResult>
{
    private CookieFilter? filter;
    private PartitionDescriptor? partition;

    /// <summary>
    /// Initializes a new instance of the <see cref="DeleteCookiesCommandParameters"/> class.
    /// </summary>
    public DeleteCookiesCommandParameters()
    {
    }

    /// <summary>
    /// Gets the method name of the command.
    /// </summary>
    [JsonIgnore]
    public override string MethodName => "storage.deleteCookies";

    /// <summary>
    /// Gets or sets the filter to use when getting the cookies.
    /// </summary>
    [JsonPropertyName("filter")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingDefault)]
    public CookieFilter? Filter { get => this.filter; set => this.filter = value; }

    /// <summary>
    /// Gets or sets the partition descriptor to use when getting the cookies.
    /// </summary>
    [JsonPropertyName("partition")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingDefault)]
    public PartitionDescriptor? Partition { get => this.partition; set => this.partition = value; }
}