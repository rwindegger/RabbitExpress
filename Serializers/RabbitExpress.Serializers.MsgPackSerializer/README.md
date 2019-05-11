# MsgPack Serializer

This library provides an implementation of a very simple serializer.

## Add the reference

In your csproj add a PackageReference to this package.

```xml
<ItemGroup>
    <PackageReference Include="RabbitExpress.Serializers.MsgPackSerializer" Version="1.*" />
</ItemGroup>
```

## The serializer

This is a very simple serializer to be used in the [RabbitExpress.QueueClient](../../RabbitExpress/README.md). The implementation uses [MsgPack.Cli](https://msgpack.org/index.html) to serialize and deserialize the transfered messages.

```c-sharp
    public class MsgPackSerializer : IExpressSerializer
    {
        private static readonly ConcurrentDictionary<Type, MessagePackSerializer> SerializerCache =
            new ConcurrentDictionary<Type, MessagePackSerializer>();

        public TObject Deserialize<TObject>(byte[] data)
        {
            var responseSerializer = SerializerCache.GetOrAdd(typeof(TObject), MessagePackSerializer.Get<TObject>()) as MessagePackSerializer<TObject>;
            if (responseSerializer != null)
                return responseSerializer.UnpackSingleObject(data);

            return default(TObject);
        }

        public byte[] Serialize<TObject>(TObject value)
        {
            var responseSerializer = SerializerCache.GetOrAdd(typeof(TObject), MessagePackSerializer.Get<TObject>()) as MessagePackSerializer<TObject>;
            if (responseSerializer != null)
                return responseSerializer.PackSingleObject(value);

            return new byte[0];
        }
    }
```
