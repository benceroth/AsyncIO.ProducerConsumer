// <copyright file="IProducer.cs" company="PlaceholderCompany">
// Copyright (c) PlaceholderCompany. All rights reserved.
// </copyright>

namespace AsyncIO.ProducerConsumer.Roles
{
    using System;
    using System.Threading;

    /// <summary>
    /// Provides interface for producers.
    /// </summary>
    public interface IProducer
    {
        /// <summary>
        /// Event that fires when producing completed.
        /// </summary>
        event EventHandler OnCompleted;

        /// <summary>
        /// Produces an item.
        /// </summary>
        /// <returns>Item.</returns>
        /// <param name="token">Cancellation token.</param>
        object Produce(CancellationToken token);
    }
}
