# RabbitExpress

This library provides an easy way to use [RabbitMQ](https://www.rabbitmq.com) from C# or your other preferred .Net language.

## Add the reference

In the csproj add a PackageReference to the [RabbitExpress.Serializers.JsonSerializer](../Serializers/RabbitExpress.Serializers.JsonSerializer/README.md)

```xml
<ItemGroup>
    <PackageReference Include="RabbitExpress.Serializers.JsonSerializer" Version="1.*" />
</ItemGroup>
```
 or the [RabbitExpress.Serializers.MsgPackSerializer](../Serializers/RabbitExpress.Serializers.MsgPackSerializer/README.md) package.
```xml
<ItemGroup>
    <PackageReference Include="RabbitExpress.Serializers.MsgPackSerializer" Version="1.*" />
</ItemGroup>
```

## Basic usage

Using the QueueClient requires two type parameters. One type parameter defines the queues that will be created by the queue client. The other parameter defines the used serializer. As argument for the constructor the connection string is required in Uri format.

```c-sharp
var client = new QueueClient<JsonSerializer>(new Uri("amqps://xxx:yyy@host/instance"));
```

To publish a message just use the Publish method. A full Publisher example can be found in [RabbitExpress.ExamplePublisher](../Examples/RabbitExpress.Example.Publisher/README.md).

```c-sharp
client.Publish(Queues.EXAMPLE_QUEUE, new ExampleMessage { Text = message });
```

To listen for new messages use the StartWatch method. A full Worker example can be found in [RabbitExpress.ExamplePublisher](../Examples/RabbitExpress.Example.Worker/README.md).

```c-sharp
qc.WatchQueue<Queues, ExampleMessage>(Queues.EXAMPLE_QUEUE, m => {...});
```

## RPC usage

Using the QueueClient for RPC is straight forward. You need to define an interface for your RPC proxies. 

```c-sharp
public interface IExampleService
{
    string Calculate(int number1, int number2);
}
```

Using void as return type makes the proxy behave like the worker/publisher example. Other return types cause the proxy to block until the result is available. Make sure the return types and parameters are serializeable. Failing to do so will result in unexpected behavior.

Consuming a method from the proxy is easy, you simply get a proxy from the queue client and start to call methods on it.

```c-sharp
IExampleService client = qc.RpcClient<IExampleService>();
Console.WriteLine(client.Calculate(2, 4));
```

Without a service to handle the Request the code above will block forever. Implementing the server just requires another method call on the QueueClient.

```c-sharp
qc.RpcServer<IExampleService>(x => x.Calculate(1, 2), new Func<int, int, string>((i1, i2) =>
{
    Console.WriteLine($"Calculating {i1} + {i2}");
    return (i1 + i2).ToString();
}));
```

For a full example of the RPC pattern see [RabbitExpress.Example.RpcClient](../Examples/RabbitExpress.Example.RpcClient/README.md) and [RabbitExpress.Example.RpcServer](../Examples/RabbitExpress.Example.RpcServer/README.md).

## Implementing a serializer

Implementing a serializer is very straight forward. Just implement the ```IExpressSerializer``` interface and make sure that your class accepts a parameterless default constructor. The interface defines only 3 methods, ```Serialize``` and ```Deserialize```. For a full example see the [RabbitExpress.JsonSerializer](../Serializers/RabbitExpress.Serializers.JsonSerializer/README.md) project.
