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
    internal sealed class Scheduler
    {
        private readonly ILogger logger;
        private readonly IProducerFactory producerFactory;
        private readonly IConsumerFactory consumerFactory;
        private readonly object operations = new();

        private long producerCount = 0;
        private long consumerCount = 0;
        private long discardProducerCount = 0;
        private long discardConsumerCount = 0;
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
            this.Configuration = new Configuration
            {
                LogName = $"#{this.GetHashCode()}",
            };
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
            this.Configuration = new Configuration
            {
                LogName = $"#{this.GetHashCode()}",
            };
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
                    this.StartSchedule(this.cancellationTokenSource.Token);
                }
                else
                {
                    this.StartSchedule(this.producerFactory, this.consumerFactory, this.cancellationTokenSource.Token);
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

        private void StartSchedule(CancellationToken token)
        {
            if (this.logger != null)
            {
                this.logger.LogInformation($"{this.LogName}: {this.producers.Count()} Producers found!");
                this.logger.LogInformation($"{this.LogName}: {this.consumers.Count()} Consumers found!");

                int alsoConsumers = this.producers.Where(x => x is IConsumer).Count();
                int alsoProducers = this.consumers.Where(x => x is IProducer).Count();
                if (alsoConsumers > 0 || alsoProducers > 0)
                {
                    var count = alsoConsumers + alsoProducers;
                    this.logger.LogInformation($"{this.LogName}: {count} detected as both, but will not be added - if needed try with factories!");
                }
            }

            this.Schedule(token);
        }

        private void StartSchedule(IProducerFactory producerFactory, IConsumerFactory consumerFactory, CancellationToken token)
        {
            var producers = Enumerable.Range(0, this.Configuration.ProducerCount).Select(_ => producerFactory.GetProducer()).ToList();
            var consumers = Enumerable.Range(0, this.Configuration.ConsumerCount).Select(_ => consumerFactory.GetConsumer()).ToList();
            var alsoConsumers = producers.Where(x => x is IConsumer).Select(x => x as IConsumer).ToList();
            var alsoProducers = consumers.Where(x => x is IProducer).Select(x => x as IProducer).ToList();

            if (this.logger != null)
            {
                this.logger.LogInformation($"{this.LogName}: {producers.Count} Producers has created!");
                this.logger.LogInformation($"{this.LogName}: {consumers.Count} Consumers has created!");
                if (alsoConsumers.Count > 0 || alsoProducers.Count > 0)
                {
                    var count = alsoConsumers.Count + alsoProducers.Count;
                    this.logger.LogInformation($"{this.LogName}: {count} detected as both and added!");
                }
            }

            this.producers = producers.Concat(alsoProducers);
            this.consumers = consumers.Concat(alsoConsumers);
            this.Schedule(token);
        }

        private void Schedule(CancellationToken token)
        {
            this.logger?.LogInformation($"{this.LogName}: Starting producers and consumers!");

            var buffer = new ProducerConsumerBuffer();
            this.StartDetectCompletion(token);
            this.StartLogPerformance(buffer, token);
            this.StartProducerConsumers(buffer, token);
        }

        private void StartProducerConsumers(ProducerConsumerBuffer buffer, CancellationToken token)
        {
            new Thread(async () =>
            {
                var tasks = new List<Task>();

                tasks.AddRange(this.consumers
                    .AsParallel()
                    .Select(x => this.StartConsumer(x, buffer, token))
                    .ToList());

                tasks.AddRange(this.producers
                    .AsParallel()
                    .Select(x => this.StartProducer(x, buffer, token))
                    .ToList());

                await Task.WhenAll();
            })
            {
                IsBackground = true,
                Priority = ThreadPriority.AboveNormal,
            }.Start();
        }

        private async Task StartProducer(IProducer producer, ProducerConsumerBuffer buffer, CancellationToken token)
        {
            while (!token.IsCancellationRequested && producer.ProducerState != State.Completed)
            {
                if (await producer.Produce(token) is object item)
                {
                    buffer.AddItem(item, token);
                    Interlocked.Increment(ref this.producerCount);
                }
                else
                {
                    Interlocked.Increment(ref this.discardProducerCount);
                }
            }
        }

        private async Task StartConsumer(IConsumer consumer, ProducerConsumerBuffer buffer, CancellationToken token)
        {
            while (!token.IsCancellationRequested && consumer.ConsumerState != State.Completed)
            {
                var item = await buffer.GetItem(token);
                if (consumer.CanConsume(item))
                {
                    consumer.Consume(item, token);
                    Interlocked.Increment(ref this.consumerCount);
                }
                else
                {
                    Interlocked.Increment(ref this.discardConsumerCount);
                }
            }
        }

        private void StartDetectCompletion(CancellationToken token)
        {
            new Thread(() => this.DetectCompletion(token))
            {
                IsBackground = true,
                Priority = ThreadPriority.AboveNormal,
            }.Start();
        }

        private void DetectCompletion(CancellationToken token)
        {
            while ((this.producers.Any(x => x.ProducerState != State.Completed) ||
                    (this.producerCount > this.consumerCount + this.discardConsumerCount &&
                    this.consumers.Any(x => x.ConsumerState != State.Completed))) &&
                !token.IsCancellationRequested)
            {
                Thread.Sleep(1);
            }

            if (token.IsCancellationRequested)
            {
                this.logger?.LogInformation($"{this.LogName}: Completed execution by stop!");
            }
            else
            {
                this.logger?.LogInformation($"{this.LogName}: Completed execution as all producers and consumers has finished their job!");
            }

            foreach (var consumer in this.consumers)
            {
                consumer.ConsumerState = State.Completed;
                consumer.Cleanup();
            }

            this.OnCompleted?.Invoke(this, EventArgs.Empty);
        }

        private void StartLogPerformance(ProducerConsumerBuffer buffer, CancellationToken token)
        {
            if (this.logger != null && this.Configuration.LogPerfomance)
            {
                new Thread(() => this.LogPerformance(buffer, token))
                {
                    IsBackground = true,
                    Priority = ThreadPriority.Lowest,
                }.Start();
            }
        }

        private void LogPerformance(ProducerConsumerBuffer buffer, CancellationToken token)
        {
            int sleep = this.Configuration.LogPerfomanceMs;
            double ratio = 1000.0 / sleep;

            long lastProducer, lastDiscardProducer,
                lastConsumer, lastDiscardConsumer,
                perProducer, perProducerDiscard,
                perConsumer, perConsumerDiscard;

            while ((this.producers.Any(x => x.ProducerState != State.Completed) ||
                    (this.producerCount > this.consumerCount + this.discardConsumerCount &&
                    this.consumers.Any(x => x.ConsumerState != State.Completed))) &&
                !token.IsCancellationRequested)
            {
                lastProducer = this.producerCount;
                lastConsumer = this.consumerCount;
                lastDiscardProducer = this.discardProducerCount;
                lastDiscardConsumer = this.discardConsumerCount;

                Thread.Sleep(sleep);
                perProducer = (long)(ratio * (this.producerCount - lastProducer));
                perConsumer = (long)(ratio * (this.consumerCount - lastConsumer));
                perProducerDiscard = (long)(ratio * (this.discardProducerCount - lastDiscardProducer));
                perConsumerDiscard = (long)(ratio * (this.discardConsumerCount - lastDiscardConsumer));

                this.logger.LogInformation($"{this.LogName}: Produce/s: {perProducer} Discard/s: {perProducerDiscard} Consume/s: {perConsumer} Discard/s: {perConsumerDiscard}  Buffer: {buffer.Count}");
            }
        }
    }
}