# RabbitExpress

[![Build status](https://ci.appveyor.com/api/projects/status/85tk2tr8y5cqg4l8/branch/master?svg=true)](https://checked.link/0000004b)
[![Published version](https://img.shields.io/nuget/v/RabbitExpress.svg)](https://checked.link/00000049)
[![License: LGPL v3](https://img.shields.io/badge/License-LGPL%20v3-blue.svg)](https://checked.link/0000004c)

An easy to use RabbitMQ Client for .Net.

## How to install

In the csproj add a PackageReference to the [RabbitExpress.JsonSerializer](../RabbitExpress.JsonSerializer/README.md) package.

```xml
<ItemGroup>
    <PackageReference Include="RabbitExpress.JsonSerializer" Version="*" />
</ItemGroup>
```


## Publisher usage

For a simple example of a publisher see [RabbitExpress.ExamplePublisher](RabbitExpress.ExamplePublisher/README.md).

## Worker usage

For a simple worker implementation see [RabbitExpress.ExampleWorker](RabbitExpress.ExampleWorker/README.md).

