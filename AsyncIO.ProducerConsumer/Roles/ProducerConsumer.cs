// <copyright file="ProducerConsumer.cs" company="PlaceholderCompany">
// Copyright (c) PlaceholderCompany. All rights reserved.
// </copyright>

namespace AsyncIO.ProducerConsumer.Roles
{
    using System;
    using System.Collections.Generic;
    using System.Text;
    using System.Threading;
    using System.Threading.Tasks;
    using AsyncIO.ProducerConsumer.Models;

    /// <inheritdoc />
    public abstract class ProducerConsumer<TProduce, TConsume> : IProducer, IConsumer
        where TProduce : class
        where TConsume : class
    {
        /// <inheritdoc />
        public State ProducerState { get; set; } = State.Busy;

        /// <inheritdoc />
        public State ConsumerState { get; set; } = State.Busy;

        /// <inheritdoc />
        async Task<object> IProducer.Produce(CancellationToken token)
        {
            return await this.Produce(token);
        }

        /// <inheritdoc />
        public virtual bool CanConsume(object item)
        {
            return item is TConsume;
        }

        /// <inheritdoc />
        public virtual void Consume(object item, CancellationToken token)
        {
            this.Consume(item as TConsume, token);
        }

        /// <summary>
        /// Produces an item.
        /// </summary>
        /// <returns>Item.</returns>
        /// <param name="token">Cancellation token.</param>
        public abstract Task<TProduce> Produce(CancellationToken token);

        /// <summary>
        /// Consumes an item.
        /// </summary>
        /// <param name="item">T Item.</param>
        /// <param name="token">Cancellation token.</param>
        public abstract void Consume(TConsume item, CancellationToken token);

        public abstract void Finish();
    }
}
