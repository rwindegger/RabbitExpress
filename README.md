# RabbitExpress

[![Build status](https://ci.appveyor.com/api/projects/status/85tk2tr8y5cqg4l8/branch/master?svg=true)](https://checked.link/0000004b)
[![Published version](https://img.shields.io/nuget/v/RabbitExpress.svg)](https://checked.link/00000049)
[![License: LGPL v3](https://img.shields.io/badge/License-LGPL%20v3-blue.svg)](https://checked.link/0000004c)

An easy to use RabbitMQ Client for .Net.

## How to install

In the csproj add a PackageReference to the [RabbitExpress.Serializers.JsonSerializer](Serializers/RabbitExpress.Serializers.JsonSerializer/README.md)

```xml
<ItemGroup>
    <PackageReference Include="RabbitExpress.Serializers.JsonSerializer" Version="1.*" />
</ItemGroup>
```
 or the [RabbitExpress.Serializers.MsgPackSerializer](Serializers/RabbitExpress.Serializers.MsgPackSerializer/README.md) package.
```xml
<ItemGroup>
    <PackageReference Include="RabbitExpress.Serializers.MsgPackSerializer" Version="1.*" />
</ItemGroup>
```

## Publisher usage

For a simple example of a publisher see [RabbitExpress.Example.Publisher](Examples/RabbitExpress.Example.Publisher/README.md).

## Worker usage

For a simple worker implementation see [RabbitExpress.Example.Worker](Examples/RabbitExpress.Example.Worker/README.md).

## RPC Client usage

For a simple rpc client implementation see [RabbitExpress.Example.RpcClient](Examples/RabbitExpress.Example.RpcClient/README.md).

## RPC Server usage

For a simple rpc client implementation see [RabbitExpress.Example.RpcServer](Examples/RabbitExpress.Example.RpcServer/README.md).
