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
    using RabbitMQ.Client;
    using System;
    using System.Linq;
    using System.Threading;

    /// <summary>
    /// The QueueClient class provides a simple way to access RabbitMQ.
    /// Implements the <see cref="System.IDisposable" /> interface.
    /// </summary>
    /// <typeparam name="TQueues">The type that defines the queues.</typeparam>
    /// <typeparam name="TSerializer">The type that defines the serializer.</typeparam>
    /// <seealso cref="System.IDisposable" />
    public class QueueClient<TQueues, TSerializer>
        : IDisposable
        where TQueues : Enum
        where TSerializer : IExpressSerializer, new()
    {
        private readonly IConnection _connection;
        private readonly IModel _model;
        private readonly TSerializer _serializer;
        private bool _isRunning = true;

        /// <summary>
        /// Initializes a new instance of the <see cref="QueueClient{Queues, TSerializer}"/> class.
        /// </summary>
        /// <param name="connectionString">The connection string.</param>
        public QueueClient(Uri connectionString)
        {
            var factory = new ConnectionFactory()
            {
                Uri = connectionString
            };

            _connection = factory.CreateConnection();
            _model = _connection.CreateModel();

            Type t = typeof(TQueues);
            foreach (TQueues e in Enum.GetValues(t).Cast<TQueues>())
                _model.QueueDeclare(Enum.GetName(t, e), true, false, false, null);

            _serializer = new TSerializer();
        }

        /// <summary>
        /// Publishes the specified message.
        /// </summary>
        /// <typeparam name="TMessage">The type of the message.</typeparam>
        /// <param name="message">The message.</param>
        /// <param name="queue">The queue.</param>
        public void Publish<TMessage>(TMessage message, TQueues queue)
        {
            _model.BasicPublish("", Enum.GetName(typeof(TQueues), queue), null, _serializer.Serialize(message));
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
        /// Gets a single Message from the specified queue.
        /// </summary>
        /// <typeparam name="TMessage">The type of the message.</typeparam>
        /// <param name="queue">The queue.</param>
        /// <returns>QueuedMessage&lt;Message&gt;.</returns>
        public QueuedMessage<TMessage> Get<TMessage>(TQueues queue)
        {
            BasicGetResult data = _model.BasicGet(Enum.GetName(typeof(TQueues), queue), false);
            return data == null ? null : new QueuedMessage<TMessage>(this, data);
        }

        /// <summary>
        /// Watches the specified queue.
        /// </summary>
        /// <typeparam name="TMessage">The expected type of the message.</typeparam>
        /// <param name="queue">The queue that is watched.</param>
        /// <param name="callback">The callback that is executed when a new message is delivered.</param>
        /// <param name="timeout">The timeout that the watcher will wait when no new message is available.</param>
        public void WatchQueue<TMessage>(TQueues queue, Action<QueuedMessage<TMessage>> callback, int timeout = 1000)
        {
            _isRunning = true;
            while (_isRunning)
            {
                if (_model.MessageCount(Enum.GetName(typeof(TQueues), queue)) > 0)
                {
                    QueuedMessage<TMessage> data = Get<TMessage>(queue);
                    if (data == null) continue;
                    callback(data);
                }
                else
                {
                    Thread.Sleep(timeout);
                }
            }
        }

        /// <summary>
        /// Stops the queue watcher.
        /// </summary>
        public void StopWatch()
        {
            _isRunning = false;
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
        /// The QueuedMessage class is the message wrapper that is used for actual storage in RabbitMQ.
        /// Implements the <see cref="System.IDisposable" /> interface.
        /// </summary>
        /// <typeparam name="TMessage">The type of the message.</typeparam>
        /// <seealso cref="System.IDisposable" />
        public class QueuedMessage<TMessage> : IDisposable
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
            public QueueClient<TQueues, TSerializer> Client { get; }

            private readonly BasicGetResult _data;
            private volatile bool _isDisposed = false;

            /// <summary>
            /// Initializes a new instance of the <see cref="QueuedMessage{TMessage}"/> class.
            /// </summary>
            /// <param name="client">The client the message was delivered on.</param>
            /// <param name="data">The data delivered by RabbitMQ.</param>
            internal QueuedMessage(QueueClient<TQueues, TSerializer> client, BasicGetResult data)
            {
                Client = client;
                _data = data;
                Message = Client._serializer.Deserialize<TMessage>(data.Body);
            }

            /// <summary>
            /// Acknowledges this message.
            /// </summary>
            /// <exception cref="ObjectDisposedException">TMessage</exception>
            public void Acknowledge()
            {
                if (_isDisposed)
                    throw new ObjectDisposedException(nameof(QueuedMessage<TMessage>));
                Client.Acknowledge(_data.DeliveryTag);
                Dispose();
            }

            /// <summary>
            /// Rejects this message.
            /// </summary>
            /// <param name="requeue">if set to <c>true</c> the rejected message is requeued.</param>
            /// <exception cref="ObjectDisposedException">TMessage</exception>
            public void Reject(bool requeue = true)
            {
                if (_isDisposed)
                    throw new ObjectDisposedException(nameof(QueuedMessage<TMessage>));
                Client.Reject(_data.DeliveryTag, requeue);
                Dispose();
            }

            /// <summary>
            /// Performs application-defined tasks associated with freeing, releasing, or resetting unmanaged resources.
            /// </summary>
            public void Dispose()
            {
                _isDisposed = true;
            }
        }
    }
}
