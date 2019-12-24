// <copyright file="Producer.cs" company="PlaceholderCompany">
// Copyright (c) PlaceholderCompany. All rights reserved.
// </copyright>

namespace AsyncIO.ProducerConsumer.Roles
{
    using System;
    using System.Collections.Generic;
    using System.Text;
    using System.Threading;

    /// <inheritdoc />
    public abstract class Producer<TProduce> : IProducer
    {
        /// <inheritdoc />
        public event EventHandler OnCompleted;

        /// <inheritdoc />
        object IProducer.Produce(CancellationToken token)
        {
            return this.Produce(token);
        }

        /// <summary>
        /// Produces an item.
        /// </summary>
        /// <returns>Item.</returns>
        /// <param name="token">Cancellation token.</param>
        public abstract TProduce Produce(CancellationToken token);
    }
}
