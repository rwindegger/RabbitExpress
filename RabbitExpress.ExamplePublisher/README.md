# Example Publisher

This project makes use of the [RabbitExpress.QueueClient](../RabbitExpress/README.md) and utilizes the [RabbitExpress.JsonSerializer](../RabbitExpress.JsonSerializer/README.md) when communicating with the queue.

## Add the reference

In the csproj add a PackageReference to the [RabbitExpress.JsonSerializer](../RabbitExpress.JsonSerializer/README.md) package.

```xml
<ItemGroup>
    <PackageReference Include="RabbitExpress.JsonSerializer" Version="*" />
</ItemGroup>
```

## A simple publisher

The main code makes use of predefined messages and queues. See [RabbitExpress.ExampleShared](../RabbitExpress.ExampleShared/README.md) for details.

Making use of the the publisher is as simple as:

```c-sharp
            using (var qc = new QueueClient<Queues, JsonSerializer>(new Uri(config["RabbitExpressConnection"])))
            {
                string message;
                do
                {
                    Console.Write("Message: ");
                    message = Console.ReadLine();
                    qc.Publish(new ExampleMessage { Text = message }, Queues.EXAMPLE_QUEUE);
                } while (message != "exit");
            }
```

This simple code will publish an ExampleMessage to the [RabbitMQ](https://www.rabbitmq.com/) queue called EXAMPLE_QUEUE.
