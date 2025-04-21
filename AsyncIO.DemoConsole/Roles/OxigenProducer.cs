using AsyncIO.DemoConsole.Models;
using AsyncIO.ProducerConsumer.Roles;
using System.Threading;
using System.Threading.Tasks;

namespace AsyncIO.DemoConsole.Roles
{
    class OxigenProducer : Producer<Oxigen>
    {
        public override async Task<Oxigen> Produce(CancellationToken token)
        {
            await Task.Delay(0, token);
            return new Oxigen();
        }
    }
}
