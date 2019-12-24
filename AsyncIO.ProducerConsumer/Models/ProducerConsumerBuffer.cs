// <copyright file="ProducerConsumerBuffer.cs" company="PlaceholderCompany">
// Copyright (c) PlaceholderCompany. All rights reserved.
// </copyright>

namespace AsyncIO.ProducerConsumer.Models
{
    using System.Collections.Concurrent;

    /// <summary>
    /// Provides a buffer producers consumers.
    /// </summary>
    internal class ProducerConsumerBuffer
    {
        private readonly BlockingCollection<object> requests;

        /// <summary>
        /// Initializes a new instance of the <see cref="ProducerConsumerBuffer"/> class.
        /// </summary>
        /// <param name="maxBufferedElements">Maximum amount of elements that can be stored in the buffer.</param>
        internal ProducerConsumerBuffer(int maxBufferedElements)
        {
            this.requests = new BlockingCollection<object>(maxBufferedElements);
        }

        /// <summary>
        /// Gets element count in the buffer.
        /// </summary>
        internal int Count => this.requests.Count;

        /// <summary>
        /// Gets an item from the buffer.
        /// </summary>
        /// <returns>Item.</returns>
        internal object GetItem()
        {
            return this.requests.Take();
        }

        /// <summary>
        /// Adds an item to the buffer.
        /// </summary>
        /// <param name="item">Item.</param>
        internal void AddItem(object item)
        {
            this.requests.Add(item);
        }
    }
}
