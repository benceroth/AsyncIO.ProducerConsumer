using AsyncIO.ProducerConsumer.Factories;
using AsyncIO.ProducerConsumer.Roles;
using AsyncIO.DemoConsole.Roles;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;

namespace AsyncIO.DemoConsole.Factories
{
    class ConsumerFactory : IConsumerFactory
    {
        private static int count; 

        public IConsumer GetConsumer()
        {
            int num = Interlocked.Increment(ref count);

            return (num % 3) switch
            {
                0 => new HidrogenConsumer(),
                1 => new OxigenConsumer(),
                2 => new WaterConsumer(),
                _ => throw new InvalidOperationException(),
            };
        }
    }
}
