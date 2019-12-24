using AsyncIO.DemoConsole.Models;
using AsyncIO.ProducerConsumer.Roles;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;

namespace AsyncIO.DemoConsole.Roles
{
    class HidrogenProducer : Producer<Hidrogen>
    {
        public override Hidrogen Produce(CancellationToken token)
        {
            return new Hidrogen();
        }
    }
}
