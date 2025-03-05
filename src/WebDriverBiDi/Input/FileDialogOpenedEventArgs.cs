// <copyright file="FileDialogOpenedEventArgs.cs" company="WebDriverBiDi.NET Committers">
// Copyright (c) WebDriverBiDi.NET Committers. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.
// </copyright>

namespace WebDriverBiDi.Input;

using WebDriverBiDi.Script;

/// <summary>
/// Object containing event data for the input.fileDialogOpened event.
/// </summary>
public class FileDialogOpenedEventArgs : WebDriverBiDiEventArgs
{
    private readonly FileDialogInfo info;

    /// <summary>
    /// Initializes a new instance of the <see cref="FileDialogOpenedEventArgs"/> class.
    /// </summary>
    /// <param name="info">The <see cref="FileDialogInfo"/> object containing information about the file dialog.</param>
    public FileDialogOpenedEventArgs(FileDialogInfo info)
    {
        this.info = info;
    }

    /// <summary>
    /// Gets the ID of the browsing context displaying the file dialog.
    /// </summary>
    public string BrowsingContextId => this.info.BrowsingContextId;

    /// <summary>
    /// Gets a value indicating whether the file dialog supports multiple files.
    /// </summary>
    public bool IsMultiple => this.info.Multiple;

    /// <summary>
    /// Gets a reference to the element that displayed the file dialog, if any.
    /// </summary>
    public SharedReference? Element => this.info.Element;
}