// <copyright file="Mouse.cs" company="WebDriverBiDi.NET Committers">
// Copyright (c) WebDriverBiDi.NET Committers. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.
// </copyright>

namespace Dramaturge;

using WebDriverBiDi.Input;

/// <summary>
/// A page's mouse, driven by coordinates in CSS pixels from the top left corner of the page's viewport. It is one
/// input source for the page's lifetime, so a button pressed in one call stays pressed until a later call releases
/// it, and a move starts from where the last one ended.
/// </summary>
public sealed class Mouse
{
    private const string PointerSourceId = "automation-mouse";
    private const string WheelSourceId = "automation-wheel";
    private readonly Page page;
    private readonly object lockObject = new();
    private double x;
    private double y;

    /// <summary>
    /// Initializes a new instance of the <see cref="Mouse"/> class.
    /// </summary>
    /// <param name="page">The page the mouse belongs to.</param>
    internal Mouse(Page page)
    {
        this.page = page;
    }

    private (double X, double Y) Position
    {
        get
        {
            lock (this.lockObject)
            {
                return (this.x, this.y);
            }
        }
    }

    /// <summary>
    /// Moves the mouse to a point, in equal steps from where it is.
    /// </summary>
    /// <param name="x">The distance of the point from the viewport's left edge.</param>
    /// <param name="y">The distance of the point from the viewport's top edge.</param>
    /// <param name="steps">The number of moves to make, which is at least 1.</param>
    /// <param name="cancellationToken">A token that cancels the command.</param>
    /// <returns>A task that completes when the mouse is at the point.</returns>
    /// <exception cref="ArgumentOutOfRangeException">Thrown when <paramref name="steps"/> is less than 1.</exception>
    public Task MoveAsync(double x, double y, int steps = 1, CancellationToken cancellationToken = default)
    {
        if (steps < 1)
        {
            throw new ArgumentOutOfRangeException(nameof(steps), steps, "The number of steps must be at least 1.");
        }

        PointerSourceActions source = CreatePointerSource();
        (double fromX, double fromY) = this.MoveTo(x, y);
        for (int step = 1; step <= steps; step++)
        {
            source.Actions.Add(new PointerMoveAction() { X = fromX + ((x - fromX) * step / steps), Y = fromY + ((y - fromY) * step / steps) });
        }

        return this.PerformAsync(source, cancellationToken);
    }

    /// <summary>
    /// Presses a mouse button where the mouse is.
    /// </summary>
    /// <param name="button">The button.</param>
    /// <param name="cancellationToken">A token that cancels the command.</param>
    /// <returns>A task that completes when the button is pressed.</returns>
    public Task DownAsync(PointerButton button = PointerButton.Left, CancellationToken cancellationToken = default)
    {
        PointerSourceActions source = CreatePointerSource();
        source.Actions.Add(new PointerDownAction((ulong)button));
        return this.PerformAsync(source, cancellationToken);
    }

    /// <summary>
    /// Releases a mouse button where the mouse is.
    /// </summary>
    /// <param name="button">The button.</param>
    /// <param name="cancellationToken">A token that cancels the command.</param>
    /// <returns>A task that completes when the button is released.</returns>
    public Task UpAsync(PointerButton button = PointerButton.Left, CancellationToken cancellationToken = default)
    {
        PointerSourceActions source = CreatePointerSource();
        source.Actions.Add(new PointerUpAction((ulong)button));
        return this.PerformAsync(source, cancellationToken);
    }

    /// <summary>
    /// Moves the mouse to a point and clicks a button there.
    /// </summary>
    /// <param name="x">The distance of the point from the viewport's left edge.</param>
    /// <param name="y">The distance of the point from the viewport's top edge.</param>
    /// <param name="button">The button.</param>
    /// <param name="clickCount">The number of times the button is pressed and released, which is at least 1.</param>
    /// <param name="cancellationToken">A token that cancels the command.</param>
    /// <returns>A task that completes when the click has been performed.</returns>
    /// <exception cref="ArgumentOutOfRangeException">Thrown when <paramref name="clickCount"/> is less than 1.</exception>
    public Task ClickAsync(double x, double y, PointerButton button = PointerButton.Left, int clickCount = 1, CancellationToken cancellationToken = default)
    {
        if (clickCount < 1)
        {
            throw new ArgumentOutOfRangeException(nameof(clickCount), clickCount, "The click count must be at least 1.");
        }

        PointerSourceActions source = CreatePointerSource();
        this.MoveTo(x, y);
        source.Actions.Add(new PointerMoveAction() { X = x, Y = y });
        for (int click = 0; click < clickCount; click++)
        {
            source.Actions.Add(new PointerDownAction((ulong)button));
            source.Actions.Add(new PointerUpAction((ulong)button));
        }

        return this.PerformAsync(source, cancellationToken);
    }

    /// <summary>
    /// Moves the mouse to a point and double-clicks a button there.
    /// </summary>
    /// <param name="x">The distance of the point from the viewport's left edge.</param>
    /// <param name="y">The distance of the point from the viewport's top edge.</param>
    /// <param name="button">The button.</param>
    /// <param name="cancellationToken">A token that cancels the command.</param>
    /// <returns>A task that completes when the double click has been performed.</returns>
    public Task DblClickAsync(double x, double y, PointerButton button = PointerButton.Left, CancellationToken cancellationToken = default)
    {
        return this.ClickAsync(x, y, button, 2, cancellationToken);
    }

    /// <summary>
    /// Turns the mouse wheel where the mouse is, scrolling what is under it.
    /// </summary>
    /// <param name="deltaX">The distance to scroll to the right, or to the left if negative.</param>
    /// <param name="deltaY">The distance to scroll down, or up if negative.</param>
    /// <param name="cancellationToken">A token that cancels the command.</param>
    /// <returns>A task that completes when the wheel has been turned.</returns>
    public Task WheelAsync(long deltaX, long deltaY, CancellationToken cancellationToken = default)
    {
        (double atX, double atY) = this.Position;
        WheelSourceActions source = new(WheelSourceId);
        source.Actions.Add(new WheelScrollAction() { X = (long)atX, Y = (long)atY, DeltaX = deltaX, DeltaY = deltaY });
        return this.PerformAsync(source, cancellationToken);
    }

    private static PointerSourceActions CreatePointerSource()
    {
        return new PointerSourceActions(PointerSourceId) { Parameters = new PointerParameters() { PointerType = PointerType.Mouse } };
    }

    // Records the mouse's new position, returning where it was.
    private (double X, double Y) MoveTo(double x, double y)
    {
        lock (this.lockObject)
        {
            (double X, double Y) from = (this.x, this.y);
            this.x = x;
            this.y = y;
            return from;
        }
    }

    private Task PerformAsync(SourceActions source, CancellationToken cancellationToken)
    {
        return this.page.Browser.Group.Driver.Input.PerformActionsAsync(new PerformActionsCommandParameters(this.page.Id) { Actions = { source } }, cancellationToken: cancellationToken);
    }
}
