using System;
using System.IO;
using Microsoft.Extensions.Configuration;
using RabbitExpress.ExampleShared;

namespace RabbitExpress.Example.RpcServer
{
    class Program
    {
        static void Main(string[] args)
        {
            IConfiguration config = new ConfigurationBuilder()
                .SetBasePath(Directory.GetCurrentDirectory())
                .AddJsonFile("appsettings.json", true, true)
                .AddEnvironmentVariables()
                .Build();

            using (var qc = new QueueClient<Queues, JsonSerializer>(new Uri(config["RabbitExpressConnection"])))
            {
                IService tmp = null;
                //qc.RpcServer(Queues.RPC_QUEUE, tmp.EncodeMessage,(ExampleMessage m) => m);
            }
        }
    }
}
