using AsyncIO.DemoConsole.Models;
using AsyncIO.ProducerConsumer.Roles;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;

namespace AsyncIO.DemoConsole.Roles
{
    class WaterConsumer : Consumer<Water>
    {
        public override void Consume(Water item, CancellationToken token)
        {
        }
    }
}
