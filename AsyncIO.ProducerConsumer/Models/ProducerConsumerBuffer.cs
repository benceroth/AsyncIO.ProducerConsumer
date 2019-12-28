// <copyright file="ProducerConsumerBuffer.cs" company="PlaceholderCompany">
// Copyright (c) PlaceholderCompany. All rights reserved.
// </copyright>

namespace AsyncIO.ProducerConsumer.Models
{
    using System;
    using System.Collections.Concurrent;
    using System.Threading;
    using System.Threading.Tasks;
    using HellBrick.Collections;

    /// <summary>
    /// Provides a buffer producers consumers.
    /// </summary>
    internal class ProducerConsumerBuffer
    {
        private readonly AsyncQueue<object> requests;

        /// <summary>
        /// Initializes a new instance of the <see cref="ProducerConsumerBuffer"/> class.
        /// </summary>
        internal ProducerConsumerBuffer()
        {
            this.requests = new AsyncQueue<object>();
        }

        /// <summary>
        /// Gets element count in the buffer.
        /// </summary>
        internal int Count => this.requests.Count;

        /// <summary>
        /// Gets an item from the buffer.
        /// </summary>
        /// <returns>Item.</returns>
        /// <param name="token">Cancellation token.</param>
        internal async Task<object> GetItem(CancellationToken token)
        {
            try
            {
                return await this.requests.TakeAsync(token);
            }
            catch (OperationCanceledException)
            {
                return null;
            }
        }

        /// <summary>
        /// Adds an item to the buffer.
        /// </summary>
        /// <param name="item">Item.</param>
        /// <param name="token">Cancellation token.</param>
        internal void AddItem(object item, CancellationToken token)
        {
            this.requests.Add(item);
        }
    }
}
