// <copyright file="Producer.cs" company="PlaceholderCompany">
// Copyright (c) PlaceholderCompany. All rights reserved.
// </copyright>

namespace AsyncIO.ProducerConsumer.Roles
{
    using System.Threading;
    using System.Threading.Tasks;
    using AsyncIO.ProducerConsumer.Models;

    /// <inheritdoc />
    public abstract class Producer<TProduce> : IProducer
    {
        /// <inheritdoc />
        public State ProducerState { get; set; } = State.Busy;

        /// <inheritdoc />
        async Task<object> IProducer.Produce(CancellationToken token)
        {
            return await this.Produce(token);
        }

        /// <summary>
        /// Produces an item.
        /// </summary>
        /// <returns>Item.</returns>
        /// <param name="token">Cancellation token.</param>
        public abstract Task<TProduce> Produce(CancellationToken token);
    }
}
