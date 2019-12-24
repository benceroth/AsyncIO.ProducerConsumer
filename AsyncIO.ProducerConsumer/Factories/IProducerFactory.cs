// <copyright file="IProducerFactory.cs" company="PlaceholderCompany">
// Copyright (c) PlaceholderCompany. All rights reserved.
// </copyright>

namespace AsyncIO.ProducerConsumer.Factories
{
    using AsyncIO.ProducerConsumer.Roles;

    /// <summary>
    /// Provides an interface for a factory that creates producers.
    /// </summary>
    public interface IProducerFactory
    {
        /// <summary>
        /// Creates a producer.
        /// </summary>
        /// <returns>Producer.</returns>
        IProducer GetProducer();
    }
}
