using AsyncIO.ProducerConsumer.Models;
using AsyncIO.ProducerConsumer.Roles;
using AsyncIO.DemoConsole.Models;
using System;
using System.Collections.Generic;
using System.Text;

namespace AsyncIO.DemoConsole.Roles
{
    class HidrogenConsumer : IConsumer
    {
        bool IConsumer.CanConsume(object item)
        {
            return item is Hidrogen;
        }

        void IConsumer.Consume(object item)
        {
        }
    }
}
