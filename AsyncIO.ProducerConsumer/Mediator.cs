// <copyright file="Mediator.cs" company="PlaceholderCompany">
// Copyright (c) PlaceholderCompany. All rights reserved.
// </copyright>

namespace AsyncIO.ProducerConsumer
{
    using System;
    using System.Collections.Generic;
    using AsyncIO.ProducerConsumer.Factories;
    using AsyncIO.ProducerConsumer.Models;
    using AsyncIO.ProducerConsumer.Roles;
    using Microsoft.Extensions.Logging;

    /// <summary>
    /// Mediator that handles all communications between producers and consumers.
    /// </summary>
    public class Mediator
    {
        private readonly Scheduler scheduler;

        /// <summary>
        /// Initializes a new instance of the <see cref="Mediator"/> class.
        /// </summary>
        /// <param name="producerFactory">Producer factory that creates producers.</param>
        /// <param name="consumerFactory">Consumer factory that creates consumers.</param>
        /// <param name="logger">Injectable logger.</param>
        public Mediator(IProducerFactory producerFactory, IConsumerFactory consumerFactory, ILogger logger = null)
        {
            producerFactory = producerFactory ?? throw new ArgumentNullException(nameof(producerFactory));
            consumerFactory = consumerFactory ?? throw new ArgumentNullException(nameof(consumerFactory));

            this.scheduler = new Scheduler(producerFactory, consumerFactory, logger);
            this.scheduler.OnCompleted += this.Scheduler_OnCompleted;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="Mediator"/> class.
        /// </summary>
        /// <param name="producers">Producers.</param>
        /// <param name="consumers">Consumers.</param>
        /// <param name="logger">Injectable logger.</param>
        public Mediator(IEnumerable<IProducer> producers, IEnumerable<IConsumer> consumers, ILogger logger = null)
        {
            producers = producers ?? throw new ArgumentNullException(nameof(producers));
            consumers = consumers ?? throw new ArgumentNullException(nameof(consumers));

            this.scheduler = new Scheduler(producers, consumers, logger);
            this.scheduler.OnCompleted += this.Scheduler_OnCompleted;
        }

        /// <summary>
        /// Event that fires when producers consumers completed.
        /// </summary>
        public event EventHandler OnCompleted;

        /// <summary>
        /// Gets configuration.
        /// </summary>
        public Configuration Configuration => this.scheduler.Configuration;

        /// <summary>
        /// Starts processing.
        /// </summary>
        public void Start()
        {
            this.scheduler.Start();
        }

        /// <summary>
        /// Stops processing.
        /// </summary>
        public void Stop()
        {
            this.scheduler.Stop();
        }

        private void Scheduler_OnCompleted(object sender, EventArgs e)
        {
            this.OnCompleted?.Invoke(this, e);
        }
    }
}
