// <copyright file="SetGeolocationOverrideCommandParameters.cs" company="WebDriverBiDi.NET Committers">
// Copyright (c) WebDriverBiDi.NET Committers. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.
// </copyright>

namespace WebDriverBiDi.Emulation;

using System.Text.Json.Serialization;

/// <summary>
/// Provides parameters for the emulation.setGeolocationOverride command.
/// </summary>
public class SetGeolocationOverrideCommandParameters : CommandParameters<SetGeolocationOverrideCommandResult>
{
    /// <summary>
    /// Initializes a new instance of the <see cref="SetGeolocationOverrideCommandParameters"/> class.
    /// </summary>
    protected SetGeolocationOverrideCommandParameters()
    {
    }

    /// <summary>
    /// Gets a pre-initialized instance of <see cref="SetGeolocationOverrideCommandParameters"/>
    /// with the <see cref="SetGeolocationOverrideCoordinatesCommandParameters.Coordinates"/> property
    /// set to <see langword="null"/> to clear any existing geolocation override. Returns a new
    /// instance on each access to allow for modification of the properties without affecting other uses.
    /// Functionally equivalent to using the parameterless constructor of
    /// <see cref="SetGeolocationOverrideCoordinatesCommandParameters"/>, but provided as a named property
    /// to make the intent of clearing the override more explicit in code that uses this property.
    /// </summary>
    public static SetGeolocationOverrideCommandParameters ResetGeolocationOverride => new SetGeolocationOverrideCoordinatesCommandParameters();

    /// <summary>
    /// Gets the method name of the command.
    /// </summary>
    [JsonIgnore]
    public override string MethodName => "emulation.setGeolocationOverride";

    /// <summary>
    /// Gets the browsing contexts for which to set the geolocation override.
    /// </summary>
    /// <remarks>
    /// The protocol requires this property, when present, to contain at least one entry.
    /// An empty list therefore means "not specified": the property is omitted from the JSON
    /// payload entirely, and an empty array is never sent. Add entries to the list to scope
    /// the command.
    /// </remarks>
    [JsonIgnore]
    public List<string> Contexts { get; } = [];

    /// <summary>
    /// Gets the user contexts for which to set the geolocation override.
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
    /// Gets the browsing contexts for which to set the geolocation override, for serialization purposes.
    /// </summary>
    [JsonPropertyName("contexts")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    [JsonInclude]
    internal List<string>? SerializableContexts
    {
        get
        {
            if (this.Contexts.Count == 0)
            {
                return null;
            }

            return this.Contexts;
        }
    }

    /// <summary>
    /// Gets the user contexts for which to set the geolocation override, for serialization purposes.
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
