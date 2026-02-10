// <copyright file="ProxyConfigurationResult.cs" company="WebDriverBiDi.NET Committers">
// Copyright (c) WebDriverBiDi.NET Committers. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.
// </copyright>

namespace WebDriverBiDi.Session;

using System.Text.Json;
using WebDriverBiDi.Internal;

/// <summary>
/// Object representing a read-only proxy settings object that is being used by the browser in this session.
/// </summary>
public record ProxyConfigurationResult
{
    private readonly ProxyConfiguration proxy;

    /// <summary>
    /// Initializes a new instance of the <see cref="ProxyConfigurationResult"/> class.
    /// </summary>
    /// <param name="proxy">The <see cref="ProxyConfiguration"/> returned by the new session command.</param>
    internal ProxyConfigurationResult(ProxyConfiguration proxy)
    {
        this.proxy = proxy;
        this.AdditionalData = ReceivedDataDictionary.EmptyDictionary;
    }

    /// <summary>
    /// Gets the type of proxy configuration.
    /// </summary>
    public ProxyType ProxyType => this.proxy.ProxyType;

    /// <summary>
    /// Gets a read-only dictionary of additional properties deserialized with this proxy result.
    /// </summary>
    public ReceivedDataDictionary AdditionalData
    {
        get
        {
            if (this.proxy.AdditionalData.Count > 0 && field.Count == 0)
            {
                field = JsonConverterUtilities.ConvertIncomingExtensionData(this.ConvertIncomingExtensionData());
            }

            return field;
        }

        internal set;
    }

    /// <summary>
    /// Gets the ProxyConfigurationResult as the type-specific type.
    /// </summary>
    /// <typeparam name="T">A <see cref="ProxyConfigurationResult"/> type to convert to.</typeparam>
    /// <returns>The <see cref="ProxyConfigurationResult"/> type to convert to.</returns>
    public T ProxyConfigurationResultAs<T>()
        where T : ProxyConfigurationResult
    {
        return (T)this;
    }

    /// <summary>
    /// Gets the underlying proxy configuration as the type-specific proxy configuration type.
    /// </summary>
    /// <typeparam name="T">A <see cref="ProxyConfiguration"/> type.</typeparam>
    /// <returns>The proxy configuration ad the type-specific proxy configuration type.</returns>
    protected T ProxyConfigurationAs<T>()
        where T : ProxyConfiguration
    {
        return (T)this.proxy;
    }

    private Dictionary<string, JsonElement> ConvertIncomingExtensionData()
    {
        // ASSUMPTION: Every object in the deserialized ProxyConfiguration is a
        // JsonElement. Since we control the deserialization, this should always
        // be true. If not, this will throw.
        Dictionary<string, JsonElement> convertedData = [];
        foreach (KeyValuePair<string, object?> pair in this.proxy.AdditionalData)
        {
            convertedData[pair.Key] = (JsonElement)pair.Value!;
        }

        return convertedData;
    }
}
