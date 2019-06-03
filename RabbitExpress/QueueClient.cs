// ***********************************************************************
// Assembly         : RabbitExpress
// Author           : Rene Windegger
// Created          : 04-30-2019
//
// Last Modified By : Rene Windegger
// Last Modified On : 04-30-2019
// ***********************************************************************
// <copyright file="QueueClient.cs" company="Rene Windegger">
//     Copyright (c) Rene Windegger. All rights reserved.
// </copyright>
// <summary>
// This file is part of RabbitExpress.
//
// RabbitExpress is free software: you can redistribute it and/or modify
// it under the terms of the GNU Lesser General Public License as published by
// the Free Software Foundation, either version 3 of the License, or
// (at your option) any later version.
//
// RabbitExpress is distributed in the hope that it will be useful,
// but WITHOUT ANY WARRANTY; without even the implied warranty of
// MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the
// GNU Lesser General Public License for more details.
//
// You should have received a copy of the GNU Lesser General Public License
// along with this RabbitExpress. If not, see <http://www.gnu.org/licenses/>.
// </summary>
// ***********************************************************************
namespace RabbitExpress
{
    using MsgPack.Serialization;
    using RabbitMQ.Client;
    using RabbitMQ.Client.Events;
    using RabbitMQ.Client.Framing;
    using SexyProxy;
    using SexyProxy.Emit;
    using System;
    using System.Collections.Concurrent;
    using System.Collections.Generic;
    using System.Linq;
    using System.Linq.Expressions;
    using System.Reflection;
    using System.Security.Cryptography;
    using System.Text;
    using System.Threading;
    using System.Threading.Tasks;

    /// <summary>
    /// The QueueClient class provides a simple way to access RabbitMQ.
    /// Implements the <see cref="System.IDisposable" /> interface.
    /// </summary>
    /// <typeparam name="TSerializer">The type that defines the serializer.</typeparam>
    /// <seealso cref="System.IDisposable" />
    public class QueueClient<TSerializer>
        : IDisposable
        where TSerializer : IExpressSerializer, new()
    {
        private const string Exchange = "RabbitExpress";
        private readonly string _queue = Guid.NewGuid().ToString();

        private readonly ObjectPool<EventWaitHandle> _eventPool = new ObjectPool<EventWaitHandle>(() => new EventWaitHandle(false, EventResetMode.AutoReset));
        private readonly ConcurrentDictionary<string, Func<byte[], WorkerResult>> _pending = new ConcurrentDictionary<string, Func<byte[], WorkerResult>>();
        private readonly ConcurrentDictionary<string, Func<BasicDeliverEventArgs, WorkerResult>> _workers = new ConcurrentDictionary<string, Func<BasicDeliverEventArgs, WorkerResult>>();
        private readonly ConcurrentDictionary<string, Func<BasicDeliverEventArgs, WorkerResult>> _rpcHandlers = new ConcurrentDictionary<string, Func<BasicDeliverEventArgs, WorkerResult>>();
        private readonly HashAlgorithm _hasher = SHA256.Create();

        private readonly IConnection _connection;
        private readonly IModel _model;
        private readonly TSerializer _serializer;
        private readonly AsyncEventingBasicConsumer _consumer;

        private void HandleMessage(Func<byte[], WorkerResult> handler, BasicDeliverEventArgs @event)
        {
            WorkerResult res = handler(@event.Body);
            switch (res)
            {
                case WorkerResult.Success:
                    Acknowledge(@event.DeliveryTag);
                    break;
                case WorkerResult.Requeue:
                    Reject(@event.DeliveryTag);
                    break;
                case WorkerResult.Failed:
                    Reject(@event.DeliveryTag, false);
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }

        private void HandleMessage(Func<BasicDeliverEventArgs, WorkerResult> handler, BasicDeliverEventArgs @event)
        {
            WorkerResult res = handler(@event);
            switch (res)
            {
                case WorkerResult.Success:
                    Acknowledge(@event.DeliveryTag);
                    break;
                case WorkerResult.Requeue:
                    Reject(@event.DeliveryTag);
                    break;
                case WorkerResult.Failed:
                    Reject(@event.DeliveryTag, false);
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }

        private async Task Consumer_Received(object sender, BasicDeliverEventArgs @event)
        {
            try
            {
                await Task.Yield();

                // Handle pending RPC Requests
                if (@event.BasicProperties.CorrelationId != null && _pending.TryGetValue(@event.BasicProperties.CorrelationId, out Func<byte[], WorkerResult> handler))
                {
                    HandleMessage(handler, @event);
                    return;
                }

                await Task.Yield();

                // Handle workers
                if (@event.BasicProperties.Headers.ContainsKey("target"))
                {
                    var d = @event.BasicProperties.Headers["target"] as byte[] ?? new byte[0];
                    var s = Encoding.UTF8.GetString(d);

                    if (!string.IsNullOrWhiteSpace(s) && _workers.TryGetValue(s, out Func<BasicDeliverEventArgs, WorkerResult> worker))
                    {
                        HandleMessage(worker, @event);
                        return;
                    }
                }

                await Task.Yield();

                // Handle rpc workers
                var r = Encoding.UTF8.GetString(@event.BasicProperties.Headers["returnType"] as byte[] ?? new byte[0]);
                var n = Encoding.UTF8.GetString(@event.BasicProperties.Headers["signature"] as byte[] ?? new byte[0]);
                List<object> a = @event.BasicProperties.Headers["args"] as List<object> ?? new List<object>();
                var queueName = GetQueueIdentifier(r, n, a.Cast<byte[]>().Select(b => Encoding.UTF8.GetString(b)).ToArray());

                if (_rpcHandlers.TryGetValue(queueName, out Func<BasicDeliverEventArgs, WorkerResult> rpcHandler))
                {
                    HandleMessage(rpcHandler, @event);
                    return;
                }

                Reject(@event.DeliveryTag);
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
            }
        }

        private string GetQueueIdentifier(string ret, string name, string[] args)
        {
            var queueName = $"{ret} {name}({string.Join(", ", args)});";
            var buffer = _hasher.ComputeHash(Encoding.UTF8.GetBytes(queueName));
            return Convert.ToBase64String(buffer);
        }

        private void RegisterQueues<TInterface>()
        {
            var iType = typeof(TInterface);
            var methods = iType.GetMethods(BindingFlags.Public | BindingFlags.Instance | BindingFlags.InvokeMethod);
            foreach (var info in methods)
            {
                var h = new
                {
                    name = $"{info.DeclaringType?.FullName}.{info.Name}",
                    args = info.GetParameters().Select(a => a.ParameterType.FullName).ToArray(),
                    ret = info.ReturnType.FullName
                };
                var queueName = GetQueueIdentifier(h.ret, h.name, h.args);

                var res = _model.QueueDeclare(queueName, true, false);
                IDictionary<string, object> spec = new Dictionary<string, object>
                {
                    {"x-match", "all"},
                    {"returnType", h.ret},
                    {"signature", h.name},
                    {"args", h.args}
                };
                _model.QueueBind(queueName, Exchange, string.Empty, spec);
            }
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="QueueClient{TSerializer}"/> class.
        /// </summary>
        /// <param name="connectionString">The connection string.</param>
        public QueueClient(Uri connectionString)
        {
            var factory = new ConnectionFactory()
            {
                Uri = connectionString,
                DispatchConsumersAsync = true
            };

            _connection = factory.CreateConnection();
            _model = _connection.CreateModel();

            _consumer = new AsyncEventingBasicConsumer(_model);
            _consumer.Received += Consumer_Received;

            _model.ExchangeDeclare(Exchange, ExchangeType.Headers, true, true);

            _model.QueueDeclare(_queue, false, false);
            IDictionary<string, object> specs = new Dictionary<string, object>
            {
                { "x-match", "all" },
                { "target", _queue }
            };
            _model.QueueBind(_queue, Exchange, string.Empty, specs);
            _model.BasicConsume(_queue, false, _consumer);

            _serializer = new TSerializer();
        }

        /// <summary>
        /// Registers the worker.
        /// </summary>
        /// <typeparam name="TQueues">The type that defines the queues.</typeparam>
        /// <typeparam name="TMessage">The type of the t message.</typeparam>
        /// <param name="queue">The queue.</param>
        /// <param name="callback">The callback.</param>
        public void RegisterWorker<TQueues, TMessage>(TQueues queue, Func<QueuedMessage<TMessage>, WorkerResult> callback)
            where TQueues : Enum
        {
            var queueName = Enum.GetName(typeof(TQueues), queue);

            if (queueName != null && _workers.TryAdd(queueName, d =>
            {
                try
                {
                    return callback(new QueuedMessage<TMessage>(this, d));
                }
                catch (Exception e)
                {
                    Console.WriteLine(e);
                    return WorkerResult.Requeue;
                }
            }))
            {
                _model.QueueDeclare(queueName, true, false);
                IDictionary<string, object> spec = new Dictionary<string, object>
                {
                    { "x-match", "all" },
                    { "target", queueName }
                };
                _model.QueueBind(queueName, Exchange, string.Empty, spec);

                _model.BasicConsume(queueName, false, _consumer);
            }
        }

        /// <summary>
        /// Publishes the specified message.
        /// </summary>
        /// <typeparam name="TMessage">The type of the message.</typeparam>
        /// <param name="message">The message.</param>
        /// <param name="queue">The queue.</param>
        /// <typeparam name="TQueues">The type that defines the queues.</typeparam>
        public void Publish<TQueues, TMessage>(TQueues queue, TMessage message)
            where TQueues : Enum
        {
            var queueName = Enum.GetName(typeof(TQueues), queue);
            var props = new BasicProperties
            {
                Headers = new Dictionary<string, object>
                {
                    {"target", queueName}
                }
            };
            _model.BasicPublish(Exchange, string.Empty, props, _serializer.Serialize(message));
        }

        /// <summary>
        /// Acknowledges one or more delivered messages.
        /// </summary>
        /// <param name="deliveryTag">The delivery tag.</param>
        /// <param name="multiple">if set to <c>true</c> multiple messages will be acknowledged.</param>
        public void Acknowledge(ulong deliveryTag, bool multiple = false)
        {
            _model.BasicAck(deliveryTag, multiple);
        }

        /// <summary>
        /// Rejects a delivery message.
        /// </summary>
        /// <param name="deliveryTag">The delivery tag.</param>
        /// <param name="requeue">if set to <c>true</c> the message will be requeued.</param>
        public void Reject(ulong deliveryTag, bool requeue = true)
        {
            _model.BasicReject(deliveryTag, requeue);
        }

        /// <summary>
        /// Performs application-defined tasks associated with freeing, releasing, or resetting unmanaged resources.
        /// </summary>
        public void Dispose()
        {
            _model?.Dispose();
            _connection?.Dispose();
        }

        /// <summary>
        /// RPCs the client.
        /// </summary>
        /// <typeparam name="TInterface">The type of the t interface.</typeparam>
        /// <returns>TInterface.</returns>
        public TInterface RpcClient<TInterface>()
            where TInterface : class
        {
            RegisterQueues<TInterface>();
            return Proxy.CreateProxy<TInterface>(Intercept, asyncMode: AsyncInvocationMode.Wait);
        }

        /// <summary>
        /// RPCs the server.
        /// </summary>
        /// <typeparam name="TInterface">The type of the t interface.</typeparam>
        /// <param name="method">The method.</param>
        /// <param name="implementation">The implementation.</param>
        /// <exception cref="System.ArgumentException">method</exception>
        public void RpcServer<TInterface>(Expression<Action<TInterface>> method, Delegate implementation)
            where TInterface : class
        {
            RegisterQueues<TInterface>();
            if (method.Body is MethodCallExpression callExpression)
            {
                System.Reflection.MethodInfo info = callExpression.Method;
                var h = new
                {
                    name = $"{info.DeclaringType?.FullName}.{info.Name}",
                    args = info.GetParameters().Select(a => a.ParameterType.FullName).ToArray(),
                    ret = info.ReturnType.FullName
                };
                var queueName = GetQueueIdentifier(h.ret, h.name, h.args);

                if (_rpcHandlers.TryAdd(queueName, e =>
                {
                    RpcRequest request = _serializer.Deserialize<RpcRequest>(e.Body);
                    if (info.ReturnType == typeof(void))
                    {
                        implementation.DynamicInvoke(request.Arguments);
                        return WorkerResult.Success;
                    }

                    var result = implementation.DynamicInvoke(request.Arguments);
                    var p = new BasicProperties
                    {
                        CorrelationId = e.BasicProperties.CorrelationId,
                        Headers = new Dictionary<string, object> { ["target"] = e.BasicProperties.ReplyTo }
                    };
                    _model.BasicPublish(Exchange, string.Empty, p, _serializer.Serialize(result));
                    return WorkerResult.Success;
                }))
                {
                    _model.BasicConsume(queueName, false, _consumer);

                    return;
                }
            }
            throw new ArgumentException($"{nameof(method)} is not a method.", nameof(method));
        }

        /// <summary>
        /// Intercepts the specified invocation.
        /// </summary>
        /// <param name="invocation">The invocation.</param>
        public async Task<object> Intercept(Invocation invocation)
        {
            await Task.Yield();

            var correlationId = Guid.NewGuid().ToString();

            var h = new
            {
                name = $"{invocation.Method.DeclaringType?.FullName}.{invocation.Method.Name}",
                args = invocation.Method.GetParameters().Select(a => a.ParameterType.FullName).ToArray(),
                ret = invocation.Method.ReturnType.FullName
            };

            using (ObjectPool<EventWaitHandle>.PoolObject pooledEvent = _eventPool.Get())
            {
                EventWaitHandle @event = pooledEvent.Item;
                object result = null;
                var handler = new Func<byte[], WorkerResult>(d =>
                {
                    var type = invocation.Method.ReturnType;
                    if (type.IsTaskT())
                    {
                        type = invocation.Method.ReturnType.GenericTypeArguments[0];
                    }

                    result = _serializer.Deserialize(type, d);
                    @event.Set();
                    return WorkerResult.Success;
                });

                if (!_pending.TryAdd(correlationId, handler))
                    return null;

                var props = new BasicProperties
                {
                    CorrelationId = correlationId,
                    Headers = new Dictionary<string, object>
                    {
                        {"returnType", h.ret},
                        {"signature", h.name},
                        {"args", h.args}
                    }
                };

                if (invocation.Method.ReturnType != typeof(void))
                {
                    props.ReplyTo = _queue;
                }

                return await Task.Run(async () =>
                {
                    var req = new RpcRequest { Arguments = invocation.Arguments };
                    _model.BasicPublish(Exchange, string.Empty, props, _serializer.Serialize(req));

                    if (invocation.Method.ReturnType == typeof(void))
                    {
                        _pending.TryRemove(correlationId, out Func<byte[], WorkerResult> _);
                        return null;
                    }

                    await Task.Yield();
                    @event.WaitOne();
                    return result;
                });
            }
        }

        /// <summary>
        /// The QueuedMessage class is the message wrapper that is used for actual storage in RabbitMQ.
        /// Implements the <see cref="System.IDisposable" /> interface.
        /// </summary>
        /// <typeparam name="TMessage">The type of the message.</typeparam>
        public class QueuedMessage<TMessage>
        {
            /// <summary>
            /// Gets the message.
            /// </summary>
            /// <value>The message.</value>
            public TMessage Message { get; internal set; }

            /// <summary>
            /// Gets the client the message was received on.
            /// </summary>
            /// <value>The client.</value>
            public QueueClient<TSerializer> Client { get; }


            /// <summary>
            /// Initializes a new instance of the <see cref="QueuedMessage{TMessage}"/> class.
            /// </summary>
            /// <param name="client">The client the message was delivered on.</param>
            /// <param name="data">The data delivered by RabbitMQ.</param>
            internal QueuedMessage(QueueClient<TSerializer> client, BasicDeliverEventArgs data)
            {
                Client = client;
                Message = Client._serializer.Deserialize<TMessage>(data.Body);
            }
        }

        /// <summary>
        /// The RpcRequest class ia the message wrapper that is used to pass RPC Arguments.
        /// </summary>
        public class RpcRequest
        {
            /// <summary>
            /// Gets or sets the arguments.
            /// </summary>
            /// <value>The arguments.</value>
            [MessagePackRuntimeCollectionItemType]
            public object[] Arguments { get; set; }
        }
    }
}
