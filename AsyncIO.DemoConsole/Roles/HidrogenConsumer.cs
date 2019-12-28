using AsyncIO.ProducerConsumer.Models;
using AsyncIO.ProducerConsumer.Roles;
using AsyncIO.DemoConsole.Models;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;

namespace AsyncIO.DemoConsole.Roles
{
    class HidrogenConsumer : Consumer<Hidrogen>
    {
        public override void Consume(Hidrogen item, CancellationToken token)
        {
        }

        public override void Finish()
        {
            throw new NotImplementedException();
        }
    }
}
