// <copyright file="BoundingBox.cs" company="WebDriverBiDi.NET Committers">
// Copyright (c) WebDriverBiDi.NET Committers. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.
// </copyright>

namespace WebDriverBiDi.Automation;

/// <summary>
/// The box an element occupies, in CSS pixels relative to the top left corner of its frame's viewport.
/// </summary>
/// <param name="Frame">The frame whose viewport the box is measured in.</param>
/// <param name="X">The distance of the box's left edge from the viewport's left edge.</param>
/// <param name="Y">The distance of the box's top edge from the viewport's top edge.</param>
/// <param name="Width">The width of the box.</param>
/// <param name="Height">The height of the box.</param>
public sealed record BoundingBox(Frame Frame, double X, double Y, double Width, double Height)
{
    /// <summary>
    /// Converts the box to the coordinates of its page's main frame, adding the position of each frame element the
    /// box is nested in. The positions are read now, so a box read before something scrolled is out of date, and a
    /// frame element that is transformed, such as scaled or rotated, makes the result wrong.
    /// </summary>
    /// <param name="origin">What the result is measured from: the main frame's viewport, as <see cref="Page.Mouse"/> coordinates are, or its document.</param>
    /// <param name="timeout">The time the conversion may take, or <see langword="null"/> for <see cref="AutomationOptions.ActionTimeout"/>.</param>
    /// <param name="cancellationToken">A token that cancels the conversion.</param>
    /// <returns>The box, belonging to the page's main frame. A box of the main frame measured from the viewport is returned as it is.</returns>
    /// <exception cref="InvalidOperationException">Thrown when a frame's element cannot be found in its parent, such as one within a shadow root.</exception>
    public async Task<BoundingBox> ToTopLevelAsync(CoordinateOrigin origin = CoordinateOrigin.Viewport, TimeSpan? timeout = null, CancellationToken cancellationToken = default)
    {
        AutomationOptions options = this.Frame.Page.Browser.Group.Options;
        TimeBudget budget = new(timeout ?? options.ActionTimeout, options.TimeProvider, cancellationToken);
        double x = this.X;
        double y = this.Y;
        for (Frame frame = this.Frame; frame.ParentFrame is not null; frame = frame.ParentFrame)
        {
            (double offsetX, double offsetY) = await frame.GetOffsetInParentAsync(budget).ConfigureAwait(false);
            x += offsetX;
            y += offsetY;
        }

        Frame mainFrame = this.Frame.Page.MainFrame;
        if (origin == CoordinateOrigin.Document)
        {
            (double scrollX, double scrollY) = await mainFrame.GetScrollPositionAsync(budget).ConfigureAwait(false);
            x += scrollX;
            y += scrollY;
        }

        return mainFrame == this.Frame && origin == CoordinateOrigin.Viewport ? this : new BoundingBox(mainFrame, x, y, this.Width, this.Height);
    }
}
