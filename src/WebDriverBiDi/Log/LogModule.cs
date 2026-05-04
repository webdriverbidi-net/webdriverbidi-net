// <copyright file="LogModule.cs" company="WebDriverBiDi.NET Committers">
// Copyright (c) WebDriverBiDi.NET Committers. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.
// </copyright>

namespace WebDriverBiDi.Log;

/// <summary>
/// The Log module contains functionality and events related to writing to the browser's console log.
/// </summary>
public sealed class LogModule : Module
{
    /// <summary>
    /// The name of the log module.
    /// </summary>
    public const string LogModuleName = "log";

    private const string EntryAddedEventName = $"{LogModuleName}.entryAdded";

    private readonly ObservableEventInvocable<EntryAddedEventArgs> invocableEntryAddedObservableEvent = new(EntryAddedEventName);

    /// <summary>
    /// Initializes a new instance of the <see cref="LogModule"/> class.
    /// </summary>
    /// <param name="driver">The <see cref="IBiDiCommandExecutor"/> used in the module commands and events.</param>
    public LogModule(IBiDiCommandExecutor driver)
        : base(driver)
    {
        this.RegisterObservableEvent<LogEntry, EntryAddedEventArgs>(this.invocableEntryAddedObservableEvent, entry => new EntryAddedEventArgs(entry));
    }

    /// <summary>
    /// Gets an observable event that notifies when an entry is added to the log.
    /// </summary>
    [ObservableEventName(EntryAddedEventName)]
    public ObservableEvent<EntryAddedEventArgs> OnEntryAdded => this.invocableEntryAddedObservableEvent;

    /// <summary>
    /// Gets the module name.
    /// </summary>
    public override string ModuleName => LogModuleName;
}
