using AsyncIO.DemoConsole.Models;
using AsyncIO.ProducerConsumer.Roles;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;

namespace AsyncIO.DemoConsole.Roles
{
    class WaterProducer : IProducer, IConsumer
    {
        private int oxigenCount = 0;
        private int hidrogenCount = 0;

        public bool CanConsume(object item)
        {
            return item is Oxigen || item is Hidrogen;
        }

        public void Consume(object item)
        {
            if(item is Oxigen)
            {
                Interlocked.Increment(ref oxigenCount);
            }
            else if(item is Hidrogen)
            {
                Interlocked.Increment(ref hidrogenCount);
            }
        }

        public object Produce()
        {
            while(oxigenCount < 1 || hidrogenCount < 2)
            {
                Thread.Sleep(1);
            }

            Interlocked.Decrement(ref oxigenCount);
            Interlocked.Decrement(ref hidrogenCount);
            Interlocked.Decrement(ref hidrogenCount);

            return new Water();
        }
    }
}
