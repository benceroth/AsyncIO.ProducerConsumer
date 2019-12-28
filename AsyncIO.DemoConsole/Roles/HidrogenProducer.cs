using AsyncIO.DemoConsole.Models;
using AsyncIO.ProducerConsumer.Roles;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace AsyncIO.DemoConsole.Roles
{
    class HidrogenProducer : Producer<Hidrogen>
    {
        public override Task<Hidrogen> Produce(CancellationToken token)
        {
            return Task.FromResult(new Hidrogen());
        }
    }
}
