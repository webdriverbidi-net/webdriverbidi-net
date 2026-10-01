// <copyright file="Dialog.cs" company="WebDriverBiDi.NET Committers">
// Copyright (c) WebDriverBiDi.NET Committers. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.
// </copyright>

namespace Dramaturge;

using WebDriverBiDi.BrowsingContext;
using WebDriverBiDi.Session;

/// <summary>
/// A dialog a page opened: an alert, a confirmation, a prompt, or a warning before leaving the page. The browser
/// handles it as the user prompt handler configured for it says; only when that handler is
/// <see cref="UserPromptHandlerType.Ignore"/> does the dialog wait to be accepted or dismissed here. Answer it from
/// the event's observer, without first awaiting the action that opened it, which cannot finish while it is open.
/// </summary>
public sealed class Dialog
{
    /// <summary>
    /// Initializes a new instance of the <see cref="Dialog"/> class.
    /// </summary>
    /// <param name="frame">The frame whose document opened the dialog.</param>
    /// <param name="type">The kind of dialog.</param>
    /// <param name="message">The dialog's message.</param>
    /// <param name="defaultValue">The prompt's default text, if any.</param>
    /// <param name="handler">What the browser does with the dialog.</param>
    internal Dialog(Frame frame, UserPromptType type, string message, string? defaultValue, UserPromptHandlerType handler)
    {
        this.Frame = frame;
        this.Type = type;
        this.Message = message;
        this.DefaultValue = defaultValue;
        this.Handler = handler;
    }

    /// <summary>
    /// Gets the frame whose document opened the dialog.
    /// </summary>
    public Frame Frame { get; }

    /// <summary>
    /// Gets the page that opened the dialog.
    /// </summary>
    public Page Page => this.Frame.Page;

    /// <summary>
    /// Gets the kind of dialog.
    /// </summary>
    public UserPromptType Type { get; }

    /// <summary>
    /// Gets the dialog's message.
    /// </summary>
    public string Message { get; }

    /// <summary>
    /// Gets the default text of a prompt, or <see langword="null"/> if it has none.
    /// </summary>
    public string? DefaultValue { get; }

    /// <summary>
    /// Gets what the browser does with the dialog: accept or dismiss it at once, or leave it open to be answered.
    /// </summary>
    public UserPromptHandlerType Handler { get; }

    /// <summary>
    /// Accepts the dialog, entering text first if it is a prompt.
    /// </summary>
    /// <param name="promptText">The text to enter in a prompt, or <see langword="null"/> to leave its text as it is.</param>
    /// <param name="cancellationToken">A token that cancels the command.</param>
    /// <returns>A task that completes when the dialog is closed.</returns>
    /// <exception cref="InvalidOperationException">Thrown when the browser handled the dialog itself.</exception>
    public Task AcceptAsync(string? promptText = null, CancellationToken cancellationToken = default)
    {
        return this.HandleAsync(true, promptText, cancellationToken);
    }

    /// <summary>
    /// Dismisses the dialog.
    /// </summary>
    /// <param name="cancellationToken">A token that cancels the command.</param>
    /// <returns>A task that completes when the dialog is closed.</returns>
    /// <exception cref="InvalidOperationException">Thrown when the browser handled the dialog itself.</exception>
    public Task DismissAsync(CancellationToken cancellationToken = default)
    {
        return this.HandleAsync(false, null, cancellationToken);
    }

    private Task HandleAsync(bool accept, string? promptText, CancellationToken cancellationToken)
    {
        if (this.Handler != UserPromptHandlerType.Ignore)
        {
            string handled = this.Handler == UserPromptHandlerType.Accept ? "accepted" : "dismissed";
            throw new InvalidOperationException($"The browser {handled} the dialog itself, as its user prompt handler says; to answer dialogs, set the handler to ignore, such as with {nameof(BrowserOptions)}.{nameof(BrowserOptions.UnhandledPromptBehavior)}.");
        }

        HandleUserPromptCommandParameters parameters = new(this.Frame.Id) { Accept = accept, UserText = promptText };
        return this.Page.Browser.Group.Driver.BrowsingContext.HandleUserPromptAsync(parameters, cancellationToken: cancellationToken);
    }
}
