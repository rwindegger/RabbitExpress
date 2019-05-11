// ***********************************************************************
// Assembly         : RabbitExpress.MsgPackSerializer
// Author           : ReneWindegger
// Created          : 05-11-2019
//
// Last Modified By : ReneWindegger
// Last Modified On : 05-11-2019
// ***********************************************************************
// <copyright file="MsgPackSerializer.cs" company="Rene Windegger">
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
    using System;
    using System.Collections.Concurrent;

    /// <summary>
    /// Class MsgPackSerializer.
    /// Implements the <see cref="RabbitExpress.IExpressSerializer" />
    /// </summary>
    /// <seealso cref="RabbitExpress.IExpressSerializer" />
    public class MsgPackSerializer : IExpressSerializer
    {
        /// <summary>
        /// The serializer cache
        /// </summary>
        private static readonly ConcurrentDictionary<Type, MessagePackSerializer> SerializerCache =
            new ConcurrentDictionary<Type, MessagePackSerializer>();

        /// <summary>
        /// Deserializes the specified data.
        /// </summary>
        /// <typeparam name="TObject">The type of the object.</typeparam>
        /// <param name="data">The data.</param>
        /// <returns>TObject.</returns>
        public TObject Deserialize<TObject>(byte[] data)
        {
            var responseSerializer = SerializerCache.GetOrAdd(typeof(TObject), MessagePackSerializer.Get<TObject>()) as MessagePackSerializer<TObject>;
            if (responseSerializer != null)
                return responseSerializer.UnpackSingleObject(data);

            return default(TObject);
        }

        /// <summary>
        /// Serializes the specified value.
        /// </summary>
        /// <typeparam name="TObject">The type of the object.</typeparam>
        /// <param name="value">The value.</param>
        /// <returns>System.Byte[].</returns>
        public byte[] Serialize<TObject>(TObject value)
        {
            var responseSerializer = SerializerCache.GetOrAdd(typeof(TObject), MessagePackSerializer.Get<TObject>()) as MessagePackSerializer<TObject>;
            if (responseSerializer != null)
                return responseSerializer.PackSingleObject(value);

            return new byte[0];
        }
    }
}
