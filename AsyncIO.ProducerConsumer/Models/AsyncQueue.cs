// <copyright file="AsyncQueue.cs" company="PlaceholderCompany">
// Copyright (c) PlaceholderCompany. All rights reserved.
// </copyright>

namespace AsyncIO.ProducerConsumer.Models
{
    using System.Collections;
    using System.Collections.Concurrent;
    using System.Collections.Generic;
    using System.Threading;
    using System.Threading.Tasks;

    /// <inheritdoc />
    public class AsyncQueue<T> : IAsyncQueue<T>
    {
        private readonly ConcurrentQueue<T> queue = new();
        private readonly SemaphoreSlim semaphoreDequeue = new(0);

        /// <inheritdoc />
        public int Count => this.queue.Count;

        /// <inheritdoc />
        public void Add(T item)
        {
            this.queue.Enqueue(item);
            this.semaphoreDequeue.Release();
        }

        /// <inheritdoc />
        public IEnumerator<T> GetEnumerator()
        {
            return this.queue.GetEnumerator();
        }

        /// <inheritdoc />
        public async ValueTask<T> TakeAsync(CancellationToken cancellationToken)
        {
            await this.semaphoreDequeue.WaitAsync(cancellationToken).ConfigureAwait(false);
            T result;
            while (!this.queue.TryDequeue(out result) && !cancellationToken.IsCancellationRequested)
            {
                await Task.Delay(0, cancellationToken);
            }

            return result;
        }

        /// <inheritdoc />
        IEnumerator IEnumerable.GetEnumerator()
        {
            return this.GetEnumerator();
        }
    }
}
