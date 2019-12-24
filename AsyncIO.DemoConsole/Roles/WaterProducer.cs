using AsyncIO.DemoConsole.Models;
using AsyncIO.ProducerConsumer.Roles;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;

namespace AsyncIO.DemoConsole.Roles
{
    class WaterProducer : ProducerConsumer<Water, Element>
    {
        private int oxigenCount = 0;
        private int hidrogenCount = 0;

        public override bool CanConsume(object item)
        {
            return item is Oxigen || item is Hidrogen;
        }

        public override void Consume(Element item, CancellationToken token)
        {
            if (item is Oxigen)
            {
                Interlocked.Increment(ref oxigenCount);
            }
            else if (item is Hidrogen)
            {
                Interlocked.Increment(ref hidrogenCount);
            }
        }

        public override Water Produce(CancellationToken token)
        {
            while(oxigenCount < 1 || hidrogenCount < 2)
            {
                if (token.IsCancellationRequested)
                {
                    return null;
                }
                Thread.Sleep(1);
            }

            Interlocked.Decrement(ref oxigenCount);
            Interlocked.Decrement(ref hidrogenCount);
            Interlocked.Decrement(ref hidrogenCount);

            return new Water();
        }
    }
}
