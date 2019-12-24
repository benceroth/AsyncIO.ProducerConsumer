// <copyright file="Scheduler.cs" company="PlaceholderCompany">
// Copyright (c) PlaceholderCompany. All rights reserved.
// </copyright>

namespace AsyncIO.ProducerConsumer
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading;
    using System.Threading.Tasks;
    using AsyncIO.ProducerConsumer.Factories;
    using AsyncIO.ProducerConsumer.Models;
    using AsyncIO.ProducerConsumer.Roles;
    using Microsoft.Extensions.Logging;

    /// <summary>
    /// Producer-Consumer scheduler.
    /// </summary>
    internal class Scheduler
    {
        private readonly ILogger logger;
        private readonly IProducerFactory producerFactory;
        private readonly IConsumerFactory consumerFactory;
        private readonly IEnumerable<IProducer> producers;
        private readonly IEnumerable<IConsumer> consumers;
        private readonly object operations = new object();

        private long discardCount = 0;
        private long producerCount = 0;
        private long consumerCount = 0;
        private CancellationTokenSource cancellationTokenSource;

        /// <summary>
        /// Initializes a new instance of the <see cref="Scheduler"/> class.
        /// </summary>
        /// <param name="producerFactory">Producer factory that creates producers.</param>
        /// <param name="consumerFactory">Consumer factory that creates consumers.</param>
        /// <param name="logger">Injectable logger.</param>
        internal Scheduler(IProducerFactory producerFactory, IConsumerFactory consumerFactory, ILogger logger)
        {
            this.logger = logger;
            this.producerFactory = producerFactory;
            this.consumerFactory = consumerFactory;
            this.Configuration = new Configuration();
            this.Configuration.LogName = $"#{this.GetHashCode()}";
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="Scheduler"/> class.
        /// </summary>
        /// <param name="producers">Producers.</param>
        /// <param name="consumers">Consumers.</param>
        /// <param name="logger">Injectable logger.</param>
        internal Scheduler(IEnumerable<IProducer> producers, IEnumerable<IConsumer> consumers, ILogger logger)
        {
            this.logger = logger;
            this.producers = producers;
            this.consumers = consumers;
            this.Configuration = new Configuration();
            this.Configuration.LogName = $"#{this.GetHashCode()}";
        }

        /// <summary>
        /// Gets configuration.
        /// </summary>
        internal Configuration Configuration { get; }

        private string LogName => this.Configuration.LogName;

        /// <summary>
        /// Starts scheduler.
        /// </summary>
        internal void Start()
        {
            lock (this.operations)
            {
                this.logger?.LogInformation($"{this.LogName}: Starting scheduler.");

                if (this.cancellationTokenSource != null)
                {
                    throw new InvalidOperationException($"{this.LogName}: Scheduler has already been started!");
                }

                this.cancellationTokenSource = new CancellationTokenSource();
                if (this.producerFactory == null)
                {
                    Task.Run(async () => await this.StartSchedule(this.producers, this.consumers, this.cancellationTokenSource.Token));
                }
                else
                {
                    Task.Run(async () => await this.StartSchedule(this.producerFactory, this.consumerFactory, this.cancellationTokenSource.Token));
                }
            }
        }

        /// <summary>
        /// Stops scheduler.
        /// </summary>
        internal void Stop()
        {
            lock (this.operations)
            {
                this.logger?.LogInformation($"{this.LogName}: Stopping scheduler.");

                if (this.cancellationTokenSource == null)
                {
                    throw new InvalidOperationException($"{this.LogName}: Scheduler has to be started first!");
                }

                this.cancellationTokenSource.Cancel();
                this.cancellationTokenSource = null;
            }
        }

        private async Task StartSchedule(IEnumerable<IProducer> producers, IEnumerable<IConsumer> consumers, CancellationToken token)
        {
            this.logger?.LogInformation($"{this.LogName}: {producers.Count()} Producers found!");
            this.logger?.LogInformation($"{this.LogName}: {consumers.Count()} Consumers found!");

            int alsoConsumers = producers.Where(x => x is IConsumer).Select(x => x as IConsumer).Count();
            int alsoProducers = consumers.Where(x => x is IProducer).Select(x => x as IProducer).Count();
            if (alsoConsumers > 0 || alsoProducers > 0)
            {
                var count = alsoConsumers + alsoProducers;
                this.logger?.LogInformation($"{this.LogName}: {count} detected as both!");
            }

            await this.Schedule(producers, consumers, token);
        }

        private async Task StartSchedule(IProducerFactory producerFactory, IConsumerFactory consumerFactory, CancellationToken token)
        {
            var producers = Enumerable.Range(0, this.Configuration.ProducerCount).Select(_ => producerFactory.GetProducer()).ToList();
            this.logger?.LogInformation($"{this.LogName}: {producers.Count} Producers has created!");

            var consumers = Enumerable.Range(0, this.Configuration.ConsumerCount).Select(_ => consumerFactory.GetConsumer()).ToList();
            this.logger?.LogInformation($"{this.LogName}: {consumers.Count} Consumers has created!");

            var alsoConsumers = producers.Where(x => x is IConsumer).Select(x => x as IConsumer).ToList();
            var alsoProducers = consumers.Where(x => x is IProducer).Select(x => x as IProducer).ToList();
            if (alsoConsumers.Count > 0 || alsoProducers.Count > 0)
            {
                var count = alsoConsumers.Count + alsoProducers.Count;
                this.logger?.LogInformation($"{this.LogName}: {count} detected as both!");
            }

            await this.Schedule(
                producers.Concat(alsoProducers),
                consumers.Concat(alsoConsumers),
                token);
        }

        private async Task Schedule(IEnumerable<IProducer> producers, IEnumerable<IConsumer> consumers, CancellationToken token)
        {
            this.logger?.LogInformation($"{this.LogName}: Starting producers and consumers!");
            var buffer = new ProducerConsumerBuffer(this.Configuration.MaxBufferedElements);
            var producerTasks = producers.Select(x => this.StartProducer(x, buffer, token));
            var consumerTasks = consumers.Select(x => this.StartConsumer(x, buffer, token));

            this.StartLogPerformance(buffer, token);
            await Task.WhenAll(producerTasks.Concat(consumerTasks));
        }

        private async Task StartProducer(IProducer producer, ProducerConsumerBuffer buffer, CancellationToken token)
        {
            await Task.Run(() =>
            {
                while (!token.IsCancellationRequested)
                {
                    buffer.AddItem(producer.Produce());
                    Interlocked.Increment(ref this.producerCount);
                }
            }).ConfigureAwait(false);
        }

        private async Task StartConsumer(IConsumer consumer, ProducerConsumerBuffer buffer, CancellationToken token)
        {
            await Task.Run(() =>
            {
                while (!token.IsCancellationRequested || buffer.Count > 0)
                {
                    if (buffer.GetItem() is object item && consumer.CanConsume(item))
                    {
                        consumer.Consume(item);
                        Interlocked.Increment(ref this.consumerCount);
                    }
                    else
                    {
                        Interlocked.Increment(ref this.discardCount);
                    }
                }
            }).ConfigureAwait(false);
        }

        private void StartLogPerformance(ProducerConsumerBuffer buffer, CancellationToken token)
        {
            var log = new Thread(() => this.LogPerformance(buffer, token))
            {
                IsBackground = true,
                Priority = ThreadPriority.Highest,
            };

            log.Start();
        }

        private void LogPerformance(ProducerConsumerBuffer buffer, CancellationToken token)
        {
            if (this.Configuration.LogPerfomance)
            {
                int sleep = this.Configuration.LogPerfomanceMs;
                long lastDiscard = 0, lastProducer = 0, lastConsumer = 0;
                long perDiscard = 0, perProducer = 0, perConsumer = 0;
                double ratio = 1000.0 / sleep;

                while (!token.IsCancellationRequested)
                {
                    lastDiscard = this.discardCount;
                    lastProducer = this.producerCount;
                    lastConsumer = this.consumerCount;

                    Thread.Sleep(sleep);
                    perDiscard = (long)(ratio * (this.discardCount - lastDiscard));
                    perProducer = (long)(ratio * (this.producerCount - lastProducer));
                    perConsumer = (long)(ratio * (this.consumerCount - lastConsumer));

                    this.logger?.LogInformation($"{this.LogName}: Produce/s: {perProducer} Consume/s: {perConsumer} Discard/s: {perDiscard} Buffer: {buffer.Count}");
                }

                this.logger?.LogInformation($"{this.LogName}: Produce/s: {perProducer} Consume/s: {perConsumer} Discard/s: {perDiscard} Buffer: {buffer.Count}");
            }
        }
    }
}