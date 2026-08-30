// <copyright file="InputModuleSamples.cs" company="WebDriverBiDi.NET Committers">
// Copyright (c) WebDriverBiDi.NET Committers. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for license information.
// </copyright>
// Code snippets for docs/articles/modules/input.md

namespace WebDriverBiDi.Docs.Code.Modules;

using WebDriverBiDi;
using WebDriverBiDi.BrowsingContext;
using WebDriverBiDi.Input;
using WebDriverBiDi.Script;
using WebDriverBiDi.Session;

/// <summary>
/// Snippets for Input module documentation. Compiled at build time to prevent API drift.
/// </summary>
public static class InputModuleSamples
{
    /// <summary>
    /// Accessing the module.
    /// </summary>
    public static void AccessingModule(BiDiDriver driver)
    {
        #region AccessingModule
        InputModule input = driver.Input;
        #endregion
    }

    /// <summary>
    /// Mouse click using PointerSourceActions.
    /// </summary>
    public static async Task MouseClick(BiDiDriver driver, string contextId)
    {
        #region MouseClick
        PerformActionsCommandParameters parameters = new PerformActionsCommandParameters(contextId);

        // Create a pointer (mouse) action source
        PointerSourceActions mouseSource = new PointerSourceActions
        {
            Parameters = new PointerParameters { PointerType = PointerType.Mouse },
        };
        mouseSource.Actions.Add(new PointerMoveAction { X = 100, Y = 100 });
        mouseSource.Actions.Add(new PointerDownAction(0));
        mouseSource.Actions.Add(new PointerUpAction(0));

        parameters.Actions.Add(mouseSource);

        await driver.Input.PerformActionsAsync(parameters);
        #endregion
    }

    /// <summary>
    /// Keyboard input using KeySourceActions.
    /// </summary>
    public static async Task KeyboardInput(BiDiDriver driver, string contextId)
    {
        #region KeyboardInput
        PerformActionsCommandParameters parameters = new PerformActionsCommandParameters(contextId);

        // Create a keyboard action source
        KeySourceActions keySource = new KeySourceActions();

        // Type text
        keySource.Actions.Add(new KeyDownAction("H"));
        keySource.Actions.Add(new KeyUpAction("H"));
        keySource.Actions.Add(new KeyDownAction("i"));
        keySource.Actions.Add(new KeyUpAction("i"));

        parameters.Actions.Add(keySource);

        await driver.Input.PerformActionsAsync(parameters);
        #endregion
    }

    /// <summary>
    /// Click on element using LocateNodes and ToSharedReference.
    /// </summary>
    public static async Task ClickOnElement(BiDiDriver driver, string contextId)
    {
        #region ClickonElement
        // First locate the element
        LocateNodesCommandResult locateResult = await driver.BrowsingContext.LocateNodesAsync(
            new LocateNodesCommandParameters(contextId, new CssLocator("button")));

        NodeRemoteValue element = locateResult.Nodes[0];

        // Click the element
        PerformActionsCommandParameters parameters = new PerformActionsCommandParameters(contextId);

        PointerSourceActions mouseSource = new PointerSourceActions
        {
            Parameters = new PointerParameters { PointerType = PointerType.Mouse },
        };
        mouseSource.Actions.Add(new PointerMoveAction
        {
            X = 0,
            Y = 0,
            Origin = Origin.Element(element.ToSharedReference()),
        });
        mouseSource.Actions.Add(new PointerDownAction(0));
        mouseSource.Actions.Add(new PointerUpAction(0));

        parameters.Actions.Add(mouseSource);

        await driver.Input.PerformActionsAsync(parameters);
        #endregion
    }

    /// <summary>
    /// Send keys to element with Enter. Uses Unicode U+E007 for Enter key.
    /// </summary>
    public static async Task SendKeysToElement(BiDiDriver driver, string contextId)
    {
        #region SendKeystoElement
        // Click element first to focus it
        // ... (click code from above)

        // Then send keys
        PerformActionsCommandParameters parameters = new PerformActionsCommandParameters(contextId);

        KeySourceActions keySource = new KeySourceActions();
        string text = "Hello, World!";

        foreach (char c in text)
        {
            keySource.Actions.Add(new KeyDownAction(c.ToString()));
            keySource.Actions.Add(new KeyUpAction(c.ToString()));
        }

        // Press Enter
        keySource.Actions.Add(new KeyDownAction("\uE007"));
        keySource.Actions.Add(new KeyUpAction("\uE007"));

        parameters.Actions.Add(keySource);

        await driver.Input.PerformActionsAsync(parameters);
        #endregion
    }

    /// <summary>
    /// Modifier keys - Ctrl+A. Uses Unicode U+E009 for Control key.
    /// </summary>
    public static async Task ModifierKeys(BiDiDriver driver, string contextId)
    {
        #region ModifierKeys
        PerformActionsCommandParameters parameters = new PerformActionsCommandParameters(contextId);

        KeySourceActions keySource = new KeySourceActions();

        // Ctrl+A (Select All)
        keySource.Actions.Add(new KeyDownAction("\uE009"));
        keySource.Actions.Add(new KeyDownAction("a"));
        keySource.Actions.Add(new KeyUpAction("a"));
        keySource.Actions.Add(new KeyUpAction("\uE009"));

        parameters.Actions.Add(keySource);

        await driver.Input.PerformActionsAsync(parameters);
        #endregion
    }

    /// <summary>
    /// Reuse an input source across multiple performActions calls by giving it a stable ID.
    /// </summary>
    public static async Task ReusingInputSourceIds(BiDiDriver driver, string contextId)
    {
        #region ReusingInputSourceIds
        // The remote end tracks the state of each input source (pressed keys and buttons,
        // pointer position) by its ID, and that state persists across performActions calls
        // until ReleaseActionsAsync is called. Giving the source a stable ID lets a later
        // call continue where an earlier one left off.
        const string MouseId = "default mouse";

        // First call: move the pointer and press the primary button.
        PerformActionsCommandParameters pressParameters = new PerformActionsCommandParameters(contextId);
        PointerSourceActions mouseDown = new PointerSourceActions(MouseId)
        {
            Parameters = new PointerParameters { PointerType = PointerType.Mouse },
        };
        mouseDown.Actions.Add(new PointerMoveAction { X = 100, Y = 100 });
        mouseDown.Actions.Add(new PointerDownAction(0));
        pressParameters.Actions.Add(mouseDown);
        await driver.Input.PerformActionsAsync(pressParameters);

        // Second call: the same source ID, so the button is still pressed and the
        // pointer is still at (100, 100); this move performs a drag.
        PerformActionsCommandParameters dragParameters = new PerformActionsCommandParameters(contextId);
        PointerSourceActions mouseDrag = new PointerSourceActions(MouseId)
        {
            Parameters = new PointerParameters { PointerType = PointerType.Mouse },
        };
        mouseDrag.Actions.Add(new PointerMoveAction { X = 300, Y = 200 });
        mouseDrag.Actions.Add(new PointerUpAction(0));
        dragParameters.Actions.Add(mouseDrag);
        await driver.Input.PerformActionsAsync(dragParameters);

        // A source created without an ID gets a new random ID each time, so it never
        // shares state with sources from earlier calls.
        PointerSourceActions independentMouse = new PointerSourceActions();
        #endregion
    }

    /// <summary>
    /// Release actions.
    /// </summary>
    public static async Task ReleaseActions(BiDiDriver driver, string contextId)
    {
        #region ReleaseActions
        // Release all pressed keys/buttons
        ReleaseActionsCommandParameters parameters = new ReleaseActionsCommandParameters(contextId);
        await driver.Input.ReleaseActionsAsync(parameters);
        #endregion
    }

    /// <summary>
    /// Set files on a file input element.
    /// </summary>
    public static async Task SetFiles(BiDiDriver driver, string contextId)
    {
        #region SetFiles
        LocateNodesCommandResult locateResult = await driver.BrowsingContext.LocateNodesAsync(
            new LocateNodesCommandParameters(contextId, new CssLocator("input[type='file']")));

        NodeRemoteValue element = locateResult.Nodes[0];

        SetFilesCommandParameters parameters = new SetFilesCommandParameters(
            contextId,
            element.ToSharedReference());

        parameters.Files.Add("/path/to/file1.txt");
        parameters.Files.Add("/path/to/file2.png");

        await driver.Input.SetFilesAsync(parameters);
        #endregion
    }

    /// <summary>
    /// File dialog opened event - handle and optionally provide files via SetFilesAsync.
    /// </summary>
    public static async Task FileDialogOpened(BiDiDriver driver)
    {
        #region FileDialogOpened
        driver.Input.OnFileDialogOpened.AddObserver(async (FileDialogOpenedEventArgs e) =>
        {
            Console.WriteLine($"File dialog opened in context {e.BrowsingContextId}");
            Console.WriteLine($"Multiple files: {e.IsMultiple}");

            if (e.Element != null)
            {
                SetFilesCommandParameters parameters = new SetFilesCommandParameters(
                    e.BrowsingContextId,
                    e.Element.ToSharedReference());

                parameters.Files.Add("/path/to/upload.txt");
                await driver.Input.SetFilesAsync(parameters);
            }
        },
        ObservableEventHandlerOptions.RunHandlerAsynchronously);

        SubscribeCommandParameters subscribe =
            new SubscribeCommandParameters(driver.Input.OnFileDialogOpened.EventName);
        await driver.Session.SubscribeAsync(subscribe);
        #endregion
    }

    /// <summary>
    /// Common key constants - Unicode values for special keys.
    /// </summary>
    public static void CommonKeyConstants()
    {
        #region CommonKeyConstants
        // Enter: \uE007, Tab: \uE004, Backspace: \uE003, Delete: \uE017
        // Escape: \uE00C, Control: \uE009, Shift: \uE008, Alt: \uE00A
        // ArrowUp: \uE013, ArrowDown: \uE015, ArrowLeft: \uE012, ArrowRight: \uE014
        #endregion
    }
}
