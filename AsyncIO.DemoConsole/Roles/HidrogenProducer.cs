using AsyncIO.DemoConsole.Models;
using AsyncIO.ProducerConsumer.Roles;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;

namespace AsyncIO.DemoConsole.Roles
{
    class HidrogenProducer : IProducer
    {
        object IProducer.Produce()
        {
            return new Hidrogen();
        }
    }
}
