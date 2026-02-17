// <copyright file="ObservableEventHandlerOptions.cs" company="WebDriverBiDi.NET Committers">
// Copyright (c) WebDriverBiDi.NET Committers. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.
// </copyright>

namespace WebDriverBiDi;

/// <summary>
/// Enumerated value describing options for the execution of a handler for an ObservableEvent.
/// </summary>
[Flags]
public enum ObservableEventHandlerOptions
{
    /// <summary>
    /// No options, meaning handlers attempt to run synchronously, awaiting the completion of execution. This is the default.
    /// </summary>
    None = 0,

    /// <summary>
    /// The handler will execute asynchronously. Order of multiple executions of the handler is not guaranteed.
    /// </summary>
    RunHandlerAsynchronously = 1,
}
