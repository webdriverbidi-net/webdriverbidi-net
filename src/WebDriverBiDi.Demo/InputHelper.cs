namespace WebDriverBiDi.Demo;

using WebDriverBiDi.Client.Inputs;
using WebDriverBiDi.Input;
using WebDriverBiDi.Script;

/// <summary>
/// A helper class for adding input sequences for execution in the browser.
/// </summary>
public static class InputHelper
{
    /// <summary>
    /// Adds an action to click on a specified element.
    /// </summary>
    /// <param name="builder">The <see cref="InputBuilder"/> used to create proper payloads for action types.</param>
    /// <param name="elementReference">The <see cref="SharedReference"/> representing the element to click on.</param>
    public static void AddClickOnElementAction(InputBuilder builder, SharedReference elementReference)
    {
        builder.AddAction(builder.DefaultPointerInputSource.CreatePointerMove(0, 0, Origin.Element(new ElementOrigin(elementReference))))
            .AddAction(builder.DefaultPointerInputSource.CreatePointerDown(PointerButton.Left))
            .AddAction(builder.DefaultPointerInputSource.CreatePointerUp());
    }

    /// <summary>
    /// Adds an action to send a series of keystrokes to a specified element.
    /// </summary>
    /// <param name="builder">The <see cref="InputBuilder"/> used to create proper payloads for action types.</param>
    /// <param name="elementReference">The <see cref="SharedReference"/> representing the element to send the keystrokes to.</param>
    public static void AddSendKeysToActiveElementAction(InputBuilder builder, string keysToSend)
    {
        foreach (char character in keysToSend)
        {
            builder.AddAction(builder.DefaultKeyInputSource.CreateKeyDown(character))
                .AddAction(builder.DefaultKeyInputSource.CreateKeyUp(character));
        }
    }
}
