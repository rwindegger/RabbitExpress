using System;
using System.IO;
using Microsoft.Extensions.Configuration;
using RabbitExpress.ExampleShared;

namespace RabbitExpress.ExampleRpcClient
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
                var client = qc.RpcClient<IService>(Queues.RPC_QUEUE);
                Console.WriteLine(client.EncodeMessage(new ExampleMessage { Text = "Easy test" }).Text);
            }
        }
    }
}
