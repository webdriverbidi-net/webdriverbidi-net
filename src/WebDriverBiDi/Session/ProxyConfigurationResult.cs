// <copyright file="ProxyConfigurationResult.cs" company="WebDriverBiDi.NET Committers">
// Copyright (c) WebDriverBiDi.NET Committers. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.
// </copyright>

namespace WebDriverBiDi.Session;

using System.Diagnostics.CodeAnalysis;
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
                field = JsonConverterUtilities.ConvertIncomingExtensionData(this.proxy.AdditionalData);
            }

            return field;
        }

        internal set;
    }

    /// <summary>
    /// Casts this <see cref="ProxyConfigurationResult"/> to a type-specific proxy configuration result,
    /// throwing if it is not of that type. Use <see cref="TryAs{T}"/> to test without throwing.
    /// </summary>
    /// <typeparam name="T">A <see cref="ProxyConfigurationResult"/> type to cast to.</typeparam>
    /// <returns>This instance cast to the specified correct type.</returns>
    /// <exception cref="WebDriverBiDiException">Thrown if this ProxyConfigurationResult is not the specified type.</exception>
    public T As<T>()
        where T : ProxyConfigurationResult
    {
        if (this is T castValue)
        {
            return castValue;
        }

        throw new WebDriverBiDiException($"This ProxyConfigurationResult cannot be cast to {typeof(T)}");
    }

    /// <summary>
    /// Attempts to cast this <see cref="ProxyConfigurationResult"/> to a type-specific proxy
    /// configuration result, returning <see langword="false"/> rather than throwing when it
    /// is not of that type.
    /// </summary>
    /// <typeparam name="T">The specific type of ProxyConfigurationResult to return.</typeparam>
    /// <param name="result">When this method returns, contains the cast value or null if the cast failed.</param>
    /// <returns><see langword="true"/> if the cast was successful; otherwise, <see langword="false"/>.</returns>
    public bool TryAs<T>([NotNullWhen(true)] out T? result)
        where T : ProxyConfigurationResult
    {
        if (this is T converted)
        {
            result = converted;
            return true;
        }

        result = null;
        return false;
    }

    /// <summary>
    /// Gets the underlying proxy configuration as the type-specific proxy configuration type.
    /// </summary>
    /// <typeparam name="T">A <see cref="ProxyConfiguration"/> type.</typeparam>
    /// <returns>The proxy configuration as the type-specific proxy configuration type.</returns>
    protected T ProxyConfigurationAs<T>()
        where T : ProxyConfiguration
    {
        return (T)this.proxy;
    }
}
