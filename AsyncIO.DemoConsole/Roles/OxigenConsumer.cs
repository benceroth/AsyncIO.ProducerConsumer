using AsyncIO.DemoConsole.Models;
using AsyncIO.ProducerConsumer.Roles;
using System.Threading;

namespace AsyncIO.DemoConsole.Roles
{
    class OxigenConsumer : Consumer<Oxigen>
    {
        public override void Consume(Oxigen item, CancellationToken token)
        {
        }

        public override void Cleanup()
        {
        }
    }
}
