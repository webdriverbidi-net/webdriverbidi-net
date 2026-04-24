// <copyright file="IBiDiDriverConfiguration.cs" company="WebDriverBiDi.NET Committers">
// Copyright (c) WebDriverBiDi.NET Committers. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.
// </copyright>

namespace WebDriverBiDi;

using System.Text.Json.Serialization.Metadata;

/// <summary>
/// Interface for configuring a WebDriver BiDi driver. This interface is implemented by <see cref="BiDiDriver"/>
/// and can be used for testing, or to allow users to implement their own driver classes. It provides methods for
/// registering events, modules, and JSON type info resolvers.
/// </summary>
/// <remarks>
/// This interface is not intended to be implemented by users of this library. It is exposed publicly
/// to allow for testing and to allow users to implement their own driver classes if they choose.
/// Normal usage of this library should involve using the <see cref="BiDiDriver"/> class, which
/// provides a complete implementation. This interface should be used only by advanced users who
/// are implementing custom driver behavior or for testing purposes.
/// </remarks>
public interface IBiDiDriverConfiguration
{
    /// <summary>
    /// Registers a module for use with this driver.
    /// </summary>
    /// <param name="module">The module object.</param>
    void RegisterModule(Module module);

    /// <summary>
    /// Registers an additional <see cref="IJsonTypeInfoResolver"/> for JSON serialization
    /// and deserialization. This allows custom types, such as those from user-defined modules,
    /// to be serialized in AOT scenarios where reflection-based serialization is unavailable.
    /// </summary>
    /// <param name="resolver">The type info resolver to add.</param>
    /// <param name="cancellationToken">A cancellation token that can be used to cancel the asynchronous operation.</param>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
    Task RegisterTypeInfoResolverAsync(IJsonTypeInfoResolver resolver, CancellationToken cancellationToken = default);
}
