// <copyright file="Consumer.cs" company="PlaceholderCompany">
// Copyright (c) PlaceholderCompany. All rights reserved.
// </copyright>

namespace AsyncIO.ProducerConsumer.Roles
{
    using System.Threading;
    using AsyncIO.ProducerConsumer.Models;

    /// <inheritdoc />
    public abstract class Consumer<TConsume> : IConsumer
        where TConsume : class
    {
        /// <inheritdoc />
        public State ConsumerState { get; set; } = State.Busy;

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
        /// Consumes an item.
        /// </summary>
        /// <param name="item">Item.</param>
        /// <param name="token">Cancellation token.</param>
        public abstract void Consume(TConsume item, CancellationToken token);

        /// <summary>
        /// Called once at the end of the run for cleanup.
        /// </summary>
        public abstract void Cleanup();
    }
}
