// <copyright file="SharedWorkerRealmInfo.cs" company="WebDriverBiDi.NET Committers">
// Copyright (c) WebDriverBiDi.NET Committers. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.
// </copyright>

namespace WebDriverBiDi.Script;

using System.Text.Json.Serialization;

/// <summary>
/// Object representing a shared worker realm for executing script.
/// </summary>
public class SharedWorkerRealmInfo : RealmInfo
{
    private List<string> owners = new();

    /// <summary>
    /// Initializes a new instance of the <see cref="SharedWorkerRealmInfo"/> class.
    /// </summary>
    internal SharedWorkerRealmInfo()
        : base()
    {
    }

    /// <summary>
    /// Gets the read-only list of IDs of realms that are owners of this realm.
    /// </summary>
    public IList<string> Owners => this.owners.AsReadOnly();

    /// <summary>
    /// Gets or sets the list of IDs of realms that are owners of this realm for serialization purposes.
    /// </summary>
    [JsonPropertyName("owners")]
    internal List<string> SerializableOwners { get => this.owners; set => this.owners = value; }
}
