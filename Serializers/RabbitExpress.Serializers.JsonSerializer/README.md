# JSON Serializer

This library provides an implementation of a very simple serializer.

## Add the reference

In your csproj add a PackageReference to this package.

```xml
<ItemGroup>
    <PackageReference Include="RabbitExpress.Serializers.JsonSerializer" Version="1.*" />
</ItemGroup>
```

## The serializer

This is a very simple serializer to be used in the [RabbitExpress.QueueClient](../../RabbitExpress/README.md). The implementation uses [Newtonsoft.Json](https://www.newtonsoft.com/json) to serialize and deserialize the transfered messages.

```c-sharp
public class JsonSerializer : IExpressSerializer
{
    public TObject Deserialize<TObject>(byte[] data)
    {
        var raw = Encoding.UTF8.GetString(data);
        return JsonConvert.DeserializeObject<TObject>(raw,
            new JsonSerializerSettings() { TypeNameHandling = TypeNameHandling.Auto });
    }

    public byte[] Serialize<TObject>(TObject value)
    {
        var ser = JsonConvert.SerializeObject(value, new JsonSerializerSettings { TypeNameHandling = TypeNameHandling.Auto });
        return Encoding.UTF8.GetBytes(ser);
    }
}
```
