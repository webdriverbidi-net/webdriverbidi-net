// <copyright file="ItemDispatchedEventArgs{T}.cs" company="WebDriverBidi.NET Committers">
// Copyright (c) WebDriverBidi.NET Committers. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.
// </copyright>

namespace WebDriverBidi.Protocol;

/// <summary>
/// Object containing event data for events raised when a dispatcher dispatches an item.
/// </summary>
/// <typeparam name="T">The type of item to be dispatched.</typeparam>
public class ItemDispatchedEventArgs<T> : EventArgs
{
    private readonly T dispatchedItem;

    /// <summary>
    /// Initializes a new instance of the <see cref="ItemDispatchedEventArgs{T}"/> class.
    /// </summary>
    /// <param name="itemToDispatch">The item to be dispatched.</param>
    public ItemDispatchedEventArgs(T itemToDispatch)
    {
        this.dispatchedItem = itemToDispatch;
    }

    /// <summary>
    /// Gets the dispatched item.
    /// </summary>
    public T DispatchedItem => this.dispatchedItem;
}