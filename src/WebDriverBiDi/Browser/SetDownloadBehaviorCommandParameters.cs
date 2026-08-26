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
    /// Gets the list of user contexts for which to set the download behavior.
    /// </summary>
    /// <remarks>
    /// The protocol requires this property, when present, to contain at least one entry.
    /// An empty list therefore means "not specified": the property is omitted from the JSON
    /// payload entirely, and an empty array is never sent. Add entries to the list to scope
    /// the command.
    /// </remarks>
    [JsonIgnore]
    public List<string> UserContexts { get; } = [];

    /// <summary>
    /// Gets the list of user contexts for which to set the download behavior, for serialization purposes.
    /// </summary>
    [JsonPropertyName("userContexts")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    [JsonInclude]
    internal List<string>? SerializableUserContexts
    {
        get
        {
            if (this.UserContexts.Count == 0)
            {
                return null;
            }

            return this.UserContexts;
        }
    }
}
