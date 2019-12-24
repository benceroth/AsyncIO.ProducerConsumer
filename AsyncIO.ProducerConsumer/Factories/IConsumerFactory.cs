// <copyright file="IConsumerFactory.cs" company="PlaceholderCompany">
// Copyright (c) PlaceholderCompany. All rights reserved.
// </copyright>

namespace AsyncIO.ProducerConsumer.Factories
{
    using AsyncIO.ProducerConsumer.Roles;

    /// <summary>
    /// Provides interface for a factory that creates consumers.
    /// </summary>
    public interface IConsumerFactory
    {
        /// <summary>
        /// Creates a consumer.
        /// </summary>
        /// <returns>Item.</returns>
        IConsumer GetConsumer();
    }
}
