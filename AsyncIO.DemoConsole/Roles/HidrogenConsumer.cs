using AsyncIO.ProducerConsumer.Roles;
using AsyncIO.DemoConsole.Models;
using System;
using System.Threading;

namespace AsyncIO.DemoConsole.Roles
{
    class HidrogenConsumer : Consumer<Hidrogen>
    {
        public override void Consume(Hidrogen item, CancellationToken token)
        {
        }

        public override void Cleanup()
        {
            throw new NotImplementedException();
        }
    }
}
