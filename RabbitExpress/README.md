# RabbitExpress

This library provides an easy way to use [RabbitMQ](https://www.rabbitmq.com) from C# or your other preferred .Net language.

## Add the reference

In the csproj add a PackageReference to the [RabbitExpress.JsonSerializer](../RabbitExpress.JsonSerializer/README.md) package.

```xml
<ItemGroup>
    <PackageReference Include="RabbitExpress.JsonSerializer" Version="*" />
</ItemGroup>
```

## Basic usage

Using the QueueClient requires two type parameters. One type parameter defines the queues that will be created by the queue client. The other parameter defines the used serializer. As argument for the constructor the connection string is required in Uri format.

```c-sharp
var client = new QueueClient<Queues, JsonSerializer>(new Uri("amqps://xxx:yyy@host/instance"));
```

To publish a message just use the Publish method. A full Publisher example can be found in [RabbitExpress.ExamplePublisher](../RabbitExpress.ExamplePublisher/README.md).

```c-sharp
client.Publish(new ExampleMessage { Text = message }, Queues.EXAMPLE_QUEUE);
```

To listen for new messages use the StartWatch method. A full Worker example can be found in [RabbitExpress.ExamplePublisher](../RabbitExpress.ExampleWorker/README.md).

```c-sharp
qc.WatchQueue<ExampleMessage>(Queues.EXAMPLE_QUEUE, m => {...});
```

## Implementing a serializer

Implementing a serializer is very straight forward. Just implement the ```IExpressSerializer``` interface and make sure that your class accepts an parameterless default constructor. The interface defines only 2 methods, ```Serialize``` and ```Deserialize```. For a full example see the [RabbitExpress.JsonSerializer](../RabbitExpress.JsonSerializer/README.md) project.

