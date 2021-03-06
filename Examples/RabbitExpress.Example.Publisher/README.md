# Example Publisher

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

## A simple publisher

The main code makes use of predefined messages and queues. See [RabbitExpress.Example.Shared](../RabbitExpress.Example.Shared/README.md) for details.

Making use of the the publisher is as simple as:

```c-sharp
using (var qc = new QueueClient<JsonSerializer>(new Uri(config["RabbitExpressConnection"])))
{
    string message;
    do
    {
        Console.Write("Message: ");
        message = Console.ReadLine();
        qc.Publish(Queues.EXAMPLE_QUEUE, new ExampleMessage { Text = message });
    } while (message != "exit");
}
```

This simple code will publish an ExampleMessage to the [RabbitMQ](https://www.rabbitmq.com/) queue called EXAMPLE_QUEUE.
