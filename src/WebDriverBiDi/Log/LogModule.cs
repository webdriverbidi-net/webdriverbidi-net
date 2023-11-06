// <copyright file="LogModule.cs" company="WebDriverBiDi.NET Committers">
// Copyright (c) WebDriverBiDi.NET Committers. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.
// </copyright>

namespace WebDriverBiDi.Log;

/// <summary>
/// The Log module contains functionality and events related to logging.
/// </summary>
public sealed class LogModule : Module
{
    /// <summary>
    /// The name of the log module.
    /// </summary>
    public const string LogModuleName = "log";

    /// <summary>
    /// Initializes a new instance of the <see cref="LogModule"/> class.
    /// </summary>
    /// <param name="driver">The <see cref="BiDiDriver"/> used in the module commands and events.</param>
    public LogModule(BiDiDriver driver)
        : base(driver)
    {
        this.RegisterEventInvoker<LogEntry>("log.entryAdded", this.OnEntryAdded);
    }

    /// <summary>
    /// Occurs when an entry is added to the log.
    /// </summary>
    public event EventHandler<EntryAddedEventArgs>? EntryAdded;

    /// <summary>
    /// Gets the module name.
    /// </summary>
    public override string ModuleName => LogModuleName;

    private void OnEntryAdded(EventInfo<LogEntry> eventData)
    {
        // Special case here. The specification indicates that the parameters
        // for this event are a LogEntry object, so rather than duplicate the
        // properties to directly deserialize the EntryAddedEventArgs instance,
        // the protocol transport will deserialize to a LogEntry, then use that
        // here to create the appropriate EventArgs instance.
        if (this.EntryAdded is not null)
        {
            EntryAddedEventArgs eventArgs = eventData.ToEventArgs<EntryAddedEventArgs>();
            this.EntryAdded(this, eventArgs);
        }
    }
}
