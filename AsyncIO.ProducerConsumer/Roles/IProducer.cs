// <copyright file="IProducer.cs" company="PlaceholderCompany">
// Copyright (c) PlaceholderCompany. All rights reserved.
// </copyright>

namespace AsyncIO.ProducerConsumer.Roles
{
    /// <summary>
    /// Provides interface for producers.
    /// </summary>
    public interface IProducer
    {
        /// <summary>
        /// Produces an item.
        /// </summary>
        /// <returns>Item.</returns>
        object Produce();
    }
}
