// <copyright file="SetDownloadBehaviorCommandParameters.cs" company="WebDriverBiDi.NET Committers">
// Copyright (c) WebDriverBiDi.NET Committers. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.
// </copyright>

namespace WebDriverBiDi.Browser;

using System.Text.Json.Serialization;

/// <summary>
/// Provides parameters for the browser.setDownloadBehavior command.
/// </summary>
public class SetDownloadBehaviorCommandParameters : CommandParameters<SetDownloadBehaviorCommandResult>
{
    /// <summary>
    /// Initializes a new instance of the <see cref="SetDownloadBehaviorCommandParameters"/> class.
    /// </summary>
    public SetDownloadBehaviorCommandParameters()
    {
    }

    /// <summary>
    /// Gets a pre-initialized instance of <see cref="SetDownloadBehaviorCommandParameters"/>
    /// with the <see cref="DownloadBehavior"/> property set to <see langword="null"/> to clear
    /// any existing download behavior override. Returns a new instance on each access to allow for
    /// modification of the properties without affecting other uses. Functionally equivalent to
    /// using the parameterless constructor, but provided as a named property to make the intent of
    /// clearing the override more explicit in code that uses this property.
    /// </summary>
    public static SetDownloadBehaviorCommandParameters ResetDownloadBehavior => new();

    /// <summary>
    /// Gets the method name of the command.
    /// </summary>
    [JsonIgnore]
    public override string MethodName => "browser.setDownloadBehavior";

    /// <summary>
    /// Gets or sets the download behavior for the browser.
    /// Setting the value to <see langword="null"/> resets the download behavior to the default.
    /// </summary>
    [JsonPropertyName("downloadBehavior")]
    [JsonInclude]
    public DownloadBehavior? DownloadBehavior { get; set; }

    /// <summary>
    /// Gets or sets the list of user contexts for which to set the download behavior.
    /// </summary>
    /// <remarks>
    /// This property is nullable to distinguish between omitting the property from the JSON payload (null)
    /// and sending an empty array (empty list). When null, the property is not included in the command;
    /// when an empty list, an empty array is sent to the remote end.
    /// </remarks>
    [JsonPropertyName("userContexts")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public List<string>? UserContexts { get; set; }
}
