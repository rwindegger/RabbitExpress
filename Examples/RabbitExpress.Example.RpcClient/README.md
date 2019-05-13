# RPC Client Example

This is a simple example demonstrating the client side of the RPC implementation. A very simple interface is provided in [RabbitExpress.Example.Shared](../RabbitExpress.Example.Shared/README.md). This interface is used in the client. After the proxy is acquired some calls are triggered to test functionallity.

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

## Using the RPC Client

Using the RPC Client is as simple as:

```c-sharp
using (var qc = new QueueClient<MsgPackSerializer>(new Uri(config["RabbitExpressConnection"])))
{
    IExampleService client = qc.RpcClient<IExampleService>();
    Console.WriteLine(client.Calculate(2, 4));
    var input = new ExampleMessage { Text = "RabbitExpress Test" };
    client.Process(input);
    ExampleMessage msg = client.EncodeMessage(input);
    Console.WriteLine(msg.Text);
    ExampleMessage decmsg = client.DecodeMessage(msg);
    Console.WriteLine(decmsg.Text);
    Console.ReadLine();
}
```
