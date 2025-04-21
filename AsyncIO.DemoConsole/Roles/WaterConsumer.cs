using AsyncIO.DemoConsole.Models;
using AsyncIO.ProducerConsumer.Roles;
using System.Threading;

namespace AsyncIO.DemoConsole.Roles
{
    class WaterConsumer : Consumer<Water>
    {
        public override void Consume(Water item, CancellationToken token)
        {
        }

        public override void Cleanup()
        {
        }
    }
}
