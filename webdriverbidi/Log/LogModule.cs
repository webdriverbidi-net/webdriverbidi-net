// <copyright file="LogModule.cs" company="WebDriverBidi.NET Committers">
// Copyright (c) WebDriverBidi.NET Committers. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.
// </copyright>

namespace WebDriverBidi.Log;

/// <summary>
/// The Log module.
/// </summary>
public sealed class LogModule : ProtocolModule
{
    /// <summary>
    /// The name of the log module.
    /// </summary>
    public const string LogModuleName = "log";

    /// <summary>
    /// Initializes a new instance of the <see cref="LogModule"/> class.
    /// </summary>
    /// <param name="driver">The <see cref="Driver"/> used in the module commands and events.</param>
    public LogModule(Driver driver)
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

    private void OnEntryAdded(EventInvocationData<LogEntry> eventData)
    {
        // Special case here. The specification indicates that the parameters
        // for this event are a LogEntry object, so rather than duplicate the
        // properties to directly deserialize the EntryAddedEventArgs instance,
        // the protocol transport will deserialize to a LogEntry, then use that
        // here to create the appropriate EventArgs instance.
        // Note that the base class for a protocol module should not allow
        // eventData to be any other type than the expected type.
        if (this.EntryAdded is not null)
        {
            EntryAddedEventArgs eventArgs = new(eventData.EventData)
            {
                AdditionalData = eventData.AdditionalData,
            };
            this.EntryAdded(this, eventArgs);
        }
    }
}