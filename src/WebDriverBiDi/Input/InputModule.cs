// <copyright file="InputModule.cs" company="WebDriverBiDi.NET Committers">
// Copyright (c) WebDriverBiDi.NET Committers. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.
// </copyright>

namespace WebDriverBiDi.Input;

/// <summary>
/// The Input module contains commands and events relating to simulating user input.
/// </summary>
public sealed class InputModule : Module
{
   /// <summary>
    /// The name of the input module.
    /// </summary>
    public const string InputModuleName = "input";

    private readonly ObservableEvent<FileDialogOpenedEventArgs> onFileDialogOpenedEvent = new();

     /// <summary>
    /// Initializes a new instance of the <see cref="InputModule"/> class.
    /// </summary>
    /// <param name="driver">The <see cref="BiDiDriver"/> used in the module commands and events.</param>
    public InputModule(BiDiDriver driver)
        : base(driver)
    {
        this.RegisterAsyncEventInvoker<FileDialogInfo>("input.fileDialogOpened", this.OnFileDialogOpenedAsync);
     }

    /// <summary>
    /// Gets the module name.
    /// </summary>
    public override string ModuleName => InputModuleName;

    /// <summary>
    /// Gets an observable event that notifies when a file dialog is opened.
    /// </summary>
    public ObservableEvent<FileDialogOpenedEventArgs> OnFileDialogOpened => this.onFileDialogOpenedEvent;

    /// <summary>
    /// Performs a set of actions.
    /// </summary>
    /// <param name="commandProperties">The parameters for the command.</param>
    /// <returns>An empty command result.</returns>
    public async Task<EmptyResult> PerformActionsAsync(PerformActionsCommandParameters commandProperties)
    {
        return await this.Driver.ExecuteCommandAsync<EmptyResult>(commandProperties).ConfigureAwait(false);
    }

    /// <summary>
    /// Releases pending actions.
    /// </summary>
    /// <param name="commandProperties">The parameters for the command.</param>
    /// <returns>An empty command result.</returns>
    public async Task<EmptyResult> ReleaseActionsAsync(ReleaseActionsCommandParameters commandProperties)
    {
        return await this.Driver.ExecuteCommandAsync<EmptyResult>(commandProperties).ConfigureAwait(false);
    }

    /// <summary>
    /// Sets the files on a file upload element. The element must be of type {input type="file"}.
    /// </summary>
    /// <param name="commandProperties">The parameters for the command.</param>
    /// <returns>An empty command result.</returns>
    public async Task<EmptyResult> SetFilesAsync(SetFilesCommandParameters commandProperties)
    {
        return await this.Driver.ExecuteCommandAsync<EmptyResult>(commandProperties).ConfigureAwait(false);
    }

    private async Task OnFileDialogOpenedAsync(EventInfo<FileDialogInfo> eventData)
    {
        // Special case here. The specification indicates that the parameters
        // for this event are a FileDialogInfo object, so rather than
        // duplicate the properties to directly deserialize the
        // FileDialogOpenedEventArgs instance, the protocol transport will
        // deserialize to a FileDialogInfo, then use that here to create
        // the appropriate EventArgs instance.
        // Note that the base class for a protocol module should not allow
        // eventData to be any other type than the expected type.
        FileDialogOpenedEventArgs eventArgs = eventData.ToEventArgs<FileDialogOpenedEventArgs>();
        await this.onFileDialogOpenedEvent.NotifyObserversAsync(eventArgs);
    }
}
