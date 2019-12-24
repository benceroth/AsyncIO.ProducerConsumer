using AsyncIO.ProducerConsumer.Factories;
using AsyncIO.ProducerConsumer.Roles;
using AsyncIO.DemoConsole.Roles;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;

namespace AsyncIO.DemoConsole.Factories
{
    class ProducerFactory : IProducerFactory
    {
        private static int count;

        public IProducer GetProducer()
        {
            int num = Interlocked.Increment(ref count);

            return (num % 3) switch
            {
                0 => new HidrogenProducer(),
                1 => new OxigenProducer(),
                2 => new WaterProducer(),
                _ => throw new InvalidOperationException(),
            };
        }
    }
}
