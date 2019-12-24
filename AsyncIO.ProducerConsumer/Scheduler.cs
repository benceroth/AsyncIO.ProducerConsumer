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
        private readonly object operations = new object();

        private int producersLeft = 0;
        private long discardCount = 0;
        private long producerCount = 0;
        private long consumerCount = 0;
        private IEnumerable<IProducer> producers;
        private IEnumerable<IConsumer> consumers;
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
        /// Event that fires when producers consumers completed.
        /// </summary>
        internal event EventHandler OnCompleted;

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
                    Task.Run(async () => await this.StartSchedule(this.cancellationTokenSource.Token));
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

        private async Task StartSchedule(CancellationToken token)
        {
            this.logger?.LogInformation($"{this.LogName}: {this.producers.Count()} Producers found!");
            this.logger?.LogInformation($"{this.LogName}: {this.consumers.Count()} Consumers found!");

            int alsoConsumers = this.producers.Where(x => x is IConsumer).Select(x => x as IConsumer).Count();
            int alsoProducers = this.consumers.Where(x => x is IProducer).Select(x => x as IProducer).Count();
            if (alsoConsumers > 0 || alsoProducers > 0)
            {
                var count = alsoConsumers + alsoProducers;
                this.logger?.LogInformation($"{this.LogName}: {count} detected as both, but will not be added - if needed try with factories!");
            }

            await this.Schedule(token);
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
                this.logger?.LogInformation($"{this.LogName}: {count} detected as both and added!");
            }

            this.producers = producers.Concat(alsoProducers);
            this.consumers = consumers.Concat(alsoConsumers);

            await this.Schedule(token);
        }

        private async Task Schedule(CancellationToken token)
        {
            this.logger?.LogInformation($"{this.LogName}: Starting producers and consumers!");
            var buffer = new ProducerConsumerBuffer(this.Configuration.MaxBufferedElements);

            this.producersLeft = this.producers.Count();
            this.StartLogPerformance(buffer, token);
            var producerTasks = this.producers.Select(x => this.StartProducer(x, buffer, token));
            var consumerTasks = this.consumers.Select(x => this.StartConsumer(x, buffer, token));

            await Task.WhenAll(producerTasks.Concat(consumerTasks));

            this.logger?.LogInformation($"{this.LogName}: Completed execution by stop!");
            this.OnCompleted?.Invoke(this, EventArgs.Empty);
        }

        private async Task StartProducer(IProducer producer, ProducerConsumerBuffer buffer, CancellationToken token)
        {
            await Task.Run(() =>
            {
                producer.OnCompleted += this.Producer_OnCompleted;
                while (!token.IsCancellationRequested)
                {
                    buffer.AddItem(producer.Produce(token), token);
                    Interlocked.Increment(ref this.producerCount);
                }
            }).ConfigureAwait(false);
        }

        private async Task StartConsumer(IConsumer consumer, ProducerConsumerBuffer buffer, CancellationToken token)
        {
            await Task.Run(() =>
            {
                while (!token.IsCancellationRequested)
                {
                    consumer.State = ConsumerState.Waiting;
                    var item = buffer.GetItem(token);
                    consumer.State = ConsumerState.Busy;
                    if (consumer.CanConsume(item))
                    {
                        consumer.Consume(item, token);
                        Interlocked.Increment(ref this.consumerCount);
                    }
                    else
                    {
                        Interlocked.Increment(ref this.discardCount);
                    }
                }
            }).ConfigureAwait(false);
        }

        private async void Producer_OnCompleted(object sender, EventArgs e)
        {
            if (Interlocked.Decrement(ref this.producersLeft) == 0)
            {
                await this.DetectWaiting();
            }
        }

        private async Task DetectWaiting()
        {
            while (!this.consumers.All(x => x.State == ConsumerState.Waiting))
            {
                await Task.Delay(50);
            }

            foreach (var consumer in this.consumers)
            {
                consumer.State = ConsumerState.Completed;
            }

            this.logger?.LogInformation($"{this.LogName}: Completed execution as all producers and consumers has finished their job!");
            this.OnCompleted?.Invoke(this, EventArgs.Empty);
        }

        private void StartLogPerformance(ProducerConsumerBuffer buffer, CancellationToken token)
        {
            if (this.Configuration.LogPerfomance)
            {
                var log = new Thread(() => this.LogPerformance(buffer, token))
                {
                    IsBackground = true,
                    Priority = ThreadPriority.Highest,
                };

                log.Start();
            }
        }

        private void LogPerformance(ProducerConsumerBuffer buffer, CancellationToken token)
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
        }
    }
}