using AsyncIO.ProducerConsumer;
using AsyncIO.ProducerConsumer.Factories;
using AsyncIO.ProducerConsumer.Roles;
using AsyncIO.DemoConsole.Adapters;
using AsyncIO.DemoConsole.Factories;
using AsyncIO.DemoConsole.Roles;
using Serilog;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace AsyncIO.DemoConsole
{
    class Program
    {
        static Microsoft.Extensions.Logging.ILogger Logger;

        static void Main()
        {
            Logger = new SerilogLogger(
                new LoggerConfiguration()
                    .MinimumLevel.Debug()
                    .WriteTo.Console()
                    .CreateLogger());

            StartLinkedMediator();
            //StartSimpleMediator();
            Console.ReadKey();
        }

        static void StartSimpleMediator()
        {
            var mediator = new Mediator(new ProducerFactory(), new ConsumerFactory(), Logger);
            mediator.Configuration.LogName = "SimpleMediator";
            mediator.Configuration.LogPerfomance = true;
            mediator.Configuration.LogPerfomanceMs = 250;
            mediator.Configuration.ProducerCount = 6;
            mediator.Configuration.ConsumerCount = 600;

            mediator.OnCompleted += Mediator_OnCompleted;
            mediator.Start();

            Task.Run(() =>
            {
                Thread.Sleep(5000);
                mediator.Stop();
            });
        }

        static void StartLinkedMediator()
        {
            var producers = new IProducer[] { new HidrogenProducer(), new HidrogenProducer(), new OxigenProducer() };
            var consumers = Enumerable.Range(0, 10).Select(x => new WaterProducer()).ToList();

            var producers2 = consumers.Select(x => x as IProducer);
            var consumers2 = new IConsumer[] { new WaterConsumer() };

            var mediator = CreateAndInitMediator(producers, consumers);
            mediator.Configuration.LogName = "ComponentFactory";

            var mediator2 = CreateAndInitMediator(producers2, consumers2);
            mediator2.Configuration.LogName = "WaterFactory";

            mediator.Start();
            mediator2.Start();

            Task.Run(async () =>
            {
                await Task.Delay(15000);
                mediator.Stop();
                mediator2.Stop();
            });
        }

        static Mediator CreateAndInitMediator(IEnumerable<IProducer> producers, IEnumerable<IConsumer> consumers)
        {
            var mediator = new Mediator(producers, consumers, Logger);
            mediator.Configuration.LogPerfomance = true;
            mediator.Configuration.LogPerfomanceMs = 1000;
            mediator.OnCompleted += Mediator_OnCompleted;
            return mediator;
        }

        private static void Mediator_OnCompleted(object sender, EventArgs e)
        {
            var mediator = sender as Mediator;
            Console.WriteLine($"Completed {mediator.Configuration.LogName}");
        }
    }
}
