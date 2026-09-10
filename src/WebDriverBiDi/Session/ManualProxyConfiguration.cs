// <copyright file="ManualProxyConfiguration.cs" company="WebDriverBiDi.NET Committers">
// Copyright (c) WebDriverBiDi.NET Committers. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.
// </copyright>

namespace WebDriverBiDi.Session;

using System.Text.Json.Serialization;
using WebDriverBiDi.JsonConverters;

/// <summary>
/// Object representing a manual proxy to be used by the browser.
/// </summary>
public class ManualProxyConfiguration : ProxyConfiguration
{
    /// <summary>
    /// Initializes a new instance of the <see cref="ManualProxyConfiguration"/> class.
    /// </summary>
    [JsonConstructor]
    public ManualProxyConfiguration()
        : base(ProxyType.Manual)
    {
    }

    /// <summary>
    /// Gets or sets the address to be used to proxy HTTP commands.
    /// </summary>
    [JsonPropertyName("httpProxy")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public string? HttpProxy { get; set; }

    /// <summary>
    /// Gets or sets the address to be used to proxy HTTPS commands.
    /// </summary>
    [JsonPropertyName("sslProxy")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public string? SslProxy { get; set; }

    /// <summary>
    /// Gets or sets the address of a SOCKS proxy used to proxy commands.
    /// </summary>
    /// <remarks>
    /// The specification groups this member with <see cref="SocksVersion"/>: whenever one is
    /// present, the other must be present as well. The pairing is not validated here; a
    /// conforming remote end rejects a configuration providing only one of the two.
    /// </remarks>
    [JsonPropertyName("socksProxy")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public string? SocksProxy { get; set; }

    /// <summary>
    /// Gets or sets the version of the SOCKS proxy to be used.
    /// </summary>
    /// <remarks>
    /// Valid values for this property range from 0 to 255, inclusive. The specification groups
    /// this member with <see cref="SocksProxy"/>: whenever one is present, the other must be
    /// present as well. Neither the range nor the pairing is validated here; a value outside
    /// the range, or a configuration providing only one of the two members, is sent as-is, and
    /// a conforming remote end rejects it when the command is executed.
    /// </remarks>
    [JsonPropertyName("socksVersion")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    [SpecRange(0.0, 255.0)]
    public int? SocksVersion { get; set; }

    /// <summary>
    /// Gets a list of addresses to be bypassed by the proxy.
    /// </summary>
    /// <remarks>
    /// This property is optional in the protocol, and omitting it has the same meaning as sending an
    /// empty array: the bypass list is the addresses this member names, and naming none is the same as
    /// naming nothing. An empty list therefore means "not specified": the property is omitted from the
    /// JSON payload entirely. Add entries to the list to populate it. When this configuration is read
    /// back from a session's capabilities, a payload that omits the member and one that sends an empty
    /// array both produce an empty list.
    /// </remarks>
    [JsonIgnore]
    public List<string> NoProxyAddresses { get; } = [];

    /// <summary>
    /// Gets or sets the list of addresses to be bypassed by the proxy, for serialization purposes.
    /// </summary>
    [JsonPropertyName("noProxy")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    [JsonInclude]
    [JsonConverter(typeof(NonNullElementListJsonConverter<string>))]
    internal List<string>? SerializableNoProxyAddresses
    {
        get
        {
            if (this.NoProxyAddresses.Count == 0)
            {
                return null;
            }

            return this.NoProxyAddresses;
        }

        set
        {
            // This type is sent as a capability request and received back in a session's capabilities,
            // so the shim is assignable as well as readable; the public list is the single storage in
            // both directions.
            this.NoProxyAddresses.Clear();
            if (value is not null)
            {
                this.NoProxyAddresses.AddRange(value);
            }
        }
    }
}
