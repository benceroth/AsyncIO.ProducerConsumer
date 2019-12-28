// <copyright file="Configuration.cs" company="PlaceholderCompany">
// Copyright (c) PlaceholderCompany. All rights reserved.
// </copyright>

namespace AsyncIO.ProducerConsumer.Models
{
    using System;

    /// <summary>
    /// Provides configurable information about the mediator.
    /// </summary>
    public class Configuration
    {
        /// <summary>
        /// Gets or sets logging name.
        /// </summary>
        public string LogName { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether log perfomance.
        /// Changes only take effect on start.
        /// Defaults to false.
        /// </summary>
        public bool LogPerfomance { get; set; } = false;

        /// <summary>
        /// Gets or sets logging interval in milliseconds.
        /// Changes only take effect on start.
        /// Defaults to 250.
        /// </summary>
        public int LogPerfomanceMs { get; set; } = 250;

        /// <summary>
        /// Gets or sets maximum amount of producer.
        /// Changes only take effect on start.
        /// Defaults to environment processor count.
        /// </summary>
        public int ProducerCount { get; set; } = Environment.ProcessorCount;

        /// <summary>
        /// Gets or sets maximum amount of consumers.
        /// Changes only take effect on start.
        /// Defaults to environment processor count.
        /// </summary>
        public int ConsumerCount { get; set; } = Environment.ProcessorCount;
    }
}
