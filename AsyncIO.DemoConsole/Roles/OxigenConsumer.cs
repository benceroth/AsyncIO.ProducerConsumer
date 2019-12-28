using AsyncIO.DemoConsole.Models;
using AsyncIO.ProducerConsumer.Roles;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;

namespace AsyncIO.DemoConsole.Roles
{
    class OxigenConsumer : Consumer<Oxigen>
    {
        public override void Consume(Oxigen item, CancellationToken token)
        {
        }

        public override void Finish()
        {
        }
    }
}
