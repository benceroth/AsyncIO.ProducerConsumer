// <copyright file="IAsyncQueue.cs" company="PlaceholderCompany">
// Copyright (c) PlaceholderCompany. All rights reserved.
// </copyright>

namespace AsyncIO.ProducerConsumer.Models
{
    using System.Collections.Generic;
    using System.Threading;
    using System.Threading.Tasks;

    /// <summary>
    /// IAsyncQueue is an interface designed for an awaitable queue.
    /// </summary>
    /// <typeparam name="T">Item type.</typeparam>
    public interface IAsyncQueue<T> : IReadOnlyCollection<T>
    {
        /// <summary>
        /// Add an item to the queue.
        /// </summary>
        /// <param name="item">Item to be queued.</param>
        void Add(T item);

        /// <summary>
        /// Take an item from the queue. Wait until there is at least one item or the token is cancelled.
        /// </summary>
        /// <param name="cancellationToken">Token that can cancel the action.</param>
        /// <returns>A queued item.</returns>
        ValueTask<T> TakeAsync(CancellationToken cancellationToken);
    }
}
