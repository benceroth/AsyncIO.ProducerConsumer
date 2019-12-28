using AsyncIO.DemoConsole.Models;
using AsyncIO.ProducerConsumer.Roles;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace AsyncIO.DemoConsole.Roles
{
    class OxigenProducer : Producer<Oxigen>
    {
        public override Task<Oxigen> Produce(CancellationToken token)
        {
            return Task.FromResult(new Oxigen());
        }
    }
}
