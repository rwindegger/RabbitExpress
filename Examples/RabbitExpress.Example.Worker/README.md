# Example Worker

This project makes use of the [RabbitExpress.QueueClient](../../RabbitExpress/README.md) and utilizes the [RabbitExpress.Serializers.JsonSerializer](../../Serializers/RabbitExpress.Serializers.JsonSerializer/README.md) when communicating with the queue.

## Add the reference

In the csproj add a PackageReference to the [RabbitExpress.Serializers.JsonSerializer](../../Serializers/RabbitExpress.Serializers.JsonSerializer/README.md)

```xml
<ItemGroup>
    <PackageReference Include="RabbitExpress.Serializers.JsonSerializer" Version="1.*" />
</ItemGroup>
```
 or the [RabbitExpress.Serializers.MsgPackSerializer](../../Serializers/RabbitExpress.Serializers.MsgPackSerializer/README.md) package.
```xml
<ItemGroup>
    <PackageReference Include="RabbitExpress.Serializers.MsgPackSerializer" Version="1.*" />
</ItemGroup>
```

## A simple worker

The main code makes use of predefined messages and queues. See [RabbitExpress.Example.Shared](../RabbitExpress.Example.Shared/README.md) for details.

Making use of the the worker is a little more involved than [using the client for publishing](../RabbitExpress.Example.Publisher/README.md).

```c-sharp
var r = new Random();
using (var qc = new QueueClient<JsonSerializer>(new Uri(config["RabbitExpressConnection"])))
{
    qc.RegisterWorker<Queues, ExampleMessage>(Queues.EXAMPLE_QUEUE, m =>
    {
        try
        {
            if (string.IsNullOrWhiteSpace(m.Message?.Text))
            {
                Console.WriteLine("Rejecting empty message.");
                return WorkerResult.Failed;
            }

            if (r.Next(100) % 3 == 0)
            {
                throw new ApplicationException("Simulated recoverable error.");
            }

            Console.WriteLine($"Acknowledging {m.Message.Text}");
            return WorkerResult.Success;
        }
        catch (Exception e)
        {
            Console.WriteLine($"Rejecting {m.Message?.Text} with reason: {e}");
            return WorkerResult.Requeue;
        }
    });
    Console.ReadLine();
}
```

This simple code will continuously wait for an ExampleMessage on the [RabbitMQ](https://www.rabbitmq.com/) queue called EXAMPLE_QUEUE. The most important part is ```gc.WatchQueue<ExampleMessage>(Queues.<QueueName>, ...);```. The delegate passed to this function will be called for every valid received message.
