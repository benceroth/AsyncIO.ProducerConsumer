// <copyright file="IConsumer.cs" company="PlaceholderCompany">
// Copyright (c) PlaceholderCompany. All rights reserved.
// </copyright>

namespace AsyncIO.ProducerConsumer.Roles
{
    using System.Threading;
    using AsyncIO.ProducerConsumer.Models;

    /// <summary>
    /// Provides interface for consumers.
    /// </summary>
    public interface IConsumer
    {
        /// <summary>
        /// Gets or sets consumer state.
        /// </summary>
        State ConsumerState { get; set; }

        /// <summary>
        /// Decides whether item can be consumed, otherwise discarded.
        /// </summary>
        /// <param name="item">Item.</param>
        /// <returns>Indicating whether item can be consumed or discarded.</returns>
        bool CanConsume(object item);

        /// <summary>
        /// Consumes an item.
        /// </summary>
        /// <param name="item">Item.</param>
        /// <param name="token">Cancellation token.</param>
        void Consume(object item, CancellationToken token);

        /// <summary>
        /// Finishes consuming.
        /// </summary>
        void Cleanup();
    }
}
