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

    private const string FileDialogOpenedEventName = $"{InputModuleName}.fileDialogOpened";

    /// <summary>
    /// Initializes a new instance of the <see cref="InputModule"/> class.
    /// </summary>
    /// <param name="driver">The <see cref="BiDiDriver"/> used in the module commands and events.</param>
    public InputModule(BiDiDriver driver)
        : base(driver)
    {
        this.RegisterAsyncEventInvoker<FileDialogInfo>(FileDialogOpenedEventName, this.OnFileDialogOpenedAsync);
     }

    /// <summary>
    /// Gets the module name.
    /// </summary>
    public override string ModuleName => InputModuleName;

    /// <summary>
    /// Gets an observable event that notifies when a file dialog is opened.
    /// </summary>
    public ObservableEvent<FileDialogOpenedEventArgs> OnFileDialogOpened { get; } = new(FileDialogOpenedEventName);

    /// <summary>
    /// Performs a set of actions.
    /// </summary>
    /// <param name="commandParameters">The parameters for the command.</param>
    /// <returns>An empty command result.</returns>
    public Task<PerformActionsCommandResult> PerformActionsAsync(PerformActionsCommandParameters commandParameters)
    {
        return this.Driver.ExecuteCommandAsync(commandParameters);
    }

    /// <summary>
    /// Releases pending actions.
    /// </summary>
    /// <param name="commandParameters">The parameters for the command.</param>
    /// <returns>An empty command result.</returns>
    public Task<ReleaseActionsCommandResult> ReleaseActionsAsync(ReleaseActionsCommandParameters commandParameters)
    {
        return this.Driver.ExecuteCommandAsync(commandParameters);
    }

    /// <summary>
    /// Sets the files on a file upload element. The element must be of type {input type="file"}.
    /// </summary>
    /// <param name="commandParameters">The parameters for the command.</param>
    /// <returns>An empty command result.</returns>
    public Task<SetFilesCommandResult> SetFilesAsync(SetFilesCommandParameters commandParameters)
    {
        return this.Driver.ExecuteCommandAsync<SetFilesCommandResult>(commandParameters);
    }

    private async Task OnFileDialogOpenedAsync(EventInfo<FileDialogInfo> eventData)
    {
        FileDialogOpenedEventArgs eventArgs = eventData.ToEventArgs((info) => new FileDialogOpenedEventArgs(info));
        await this.OnFileDialogOpened.NotifyObserversAsync(eventArgs);
    }
}
