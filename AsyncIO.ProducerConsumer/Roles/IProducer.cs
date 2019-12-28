// <copyright file="IProducer.cs" company="PlaceholderCompany">
// Copyright (c) PlaceholderCompany. All rights reserved.
// </copyright>

namespace AsyncIO.ProducerConsumer.Roles
{
    using System;
    using System.Threading;
    using System.Threading.Tasks;
    using AsyncIO.ProducerConsumer.Models;

    /// <summary>
    /// Provides interface for producers.
    /// </summary>
    public interface IProducer
    {
        /// <summary>
        /// Gets or sets consumer state.
        /// </summary>
        State ProducerState { get; set; }

        /// <summary>
        /// Produces an item.
        /// </summary>
        /// <returns>Item.</returns>
        /// <param name="token">Cancellation token.</param>
        Task<object> Produce(CancellationToken token);
    }
}
