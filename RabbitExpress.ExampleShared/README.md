# Example Shared

This library contains the shared data between publisher and queuer. In this example the Library is pretty easy. The example utilizes only one set of queues. And offers only one Message.

## Defining Queues

Queues are defined using enums. So for every set of queues you want to use, you need to create an enum.

In the example the following enum is used.

```c-sharp
namespace RabbitExpress.ExampleShared
{
    /// <summary>
    /// Enum Queues
    /// </summary>
    public enum Queues
    {
        /// <summary>
        /// The example queue
        /// </summary>
        EXAMPLE_QUEUE
    }
}
```

Just use that enum as type argument for the [RabbitExpress.QueueClient](../RabbitExpress/README.md) to make it aware of the possible queues. The QueueClient will create non existing queues on the fly.

## Using Messages

Messages are just regular .Net classes. In this example the message class looks like:

```c-sharp
public class ExampleMessage
{
    public string Text { get; set; }
}
```

In combination with the [RabbitExpress.JsonSerializer](../RabbitExpress.JsonSerializer/README.md) nothing else needs to be specified on the class.
