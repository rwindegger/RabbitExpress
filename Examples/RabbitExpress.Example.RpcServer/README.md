# RPC Server Example

This is a simple example demonstrating the server side of the RPC implementation. A very simple interface is provided in [RabbitExpress.Example.Shared](../RabbitExpress.Example.Shared/README.md). This interface is used in the server.

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

## Using the RPC Server

Using the RPC Server is as simple as:

```c-sharp
using (var qc = new QueueClient<MsgPackSerializer>(new Uri(config["RabbitExpressConnection"])))
{
    qc.RpcServer<IExampleService>(x => x.Calculate(1, 2), new Func<int, int, string>((i1, i2) =>
    {
        Console.WriteLine($"Calculating {i1} + {i2}");
        return (i1 + i2).ToString();
    }));
    qc.RpcServer<IExampleService>(x => x.Process(new ExampleMessage()), new Action<ExampleMessage>(m =>
    {
        Console.WriteLine($"Process {m.Text}");
    }));
    qc.RpcServer<IExampleService>(x => x.EncodeMessage(new ExampleMessage()), new Func<ExampleMessage, ExampleMessage>(m =>
    {
        Console.WriteLine($"Encoding {m.Text}");
        return new ExampleMessage() { Text = Convert.ToBase64String(Encoding.UTF8.GetBytes(m.Text)) };
    }));
    qc.RpcServer<IExampleService>(x => x.DecodeMessage(new ExampleMessage()), new Func<ExampleMessage, ExampleMessage>(m =>
    {
        Console.WriteLine($"Decoding {m.Text}");
        return new ExampleMessage() { Text = Encoding.UTF8.GetString(Convert.FromBase64String(m.Text)) };
    }));
    Console.ReadLine();
}
```
