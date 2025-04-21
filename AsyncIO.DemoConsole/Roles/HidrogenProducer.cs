using AsyncIO.DemoConsole.Models;
using AsyncIO.ProducerConsumer.Roles;
using System.Threading;
using System.Threading.Tasks;

namespace AsyncIO.DemoConsole.Roles
{
    class HidrogenProducer : Producer<Hidrogen>
    {
        public override async Task<Hidrogen> Produce(CancellationToken token)
        {
            await Task.Delay(0, token);
            return new Hidrogen();
        }
    }
}
