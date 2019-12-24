using AsyncIO.DemoConsole.Models;
using AsyncIO.ProducerConsumer.Roles;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;

namespace AsyncIO.DemoConsole.Roles
{
    class OxigenProducer : Producer<Oxigen>
    {
        public override Oxigen Produce(CancellationToken token)
        {
            return new Oxigen();
        }
    }
}
