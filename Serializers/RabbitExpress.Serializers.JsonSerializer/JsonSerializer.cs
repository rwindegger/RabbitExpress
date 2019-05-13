// ***********************************************************************
// Assembly         : RabbitExpress.Serializers.JsonSerializer
// Author           : Rene Windegger
// Created          : 05-01-2019
//
// Last Modified By : Rene Windegger
// Last Modified On : 05-01-2019
// ***********************************************************************
// <copyright file="JsonSerializer.cs" company="Rene Windegger">
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
    using Newtonsoft.Json;
    using System;
    using System.Text;

    /// <summary>
    /// Class JsonSerializer.
    /// Implements the <see cref="RabbitExpress.IExpressSerializer" />
    /// </summary>
    /// <seealso cref="RabbitExpress.IExpressSerializer" />
    public class JsonSerializer : IExpressSerializer
    {
        /// <summary>
        /// Deserializes the specified data.
        /// </summary>
        /// <typeparam name="TObject">The type of the object.</typeparam>
        /// <param name="data">The data.</param>
        /// <returns>TObject.</returns>
        public TObject Deserialize<TObject>(byte[] data)
        {
            var raw = Encoding.UTF8.GetString(data);
            return JsonConvert.DeserializeObject<TObject>(raw,
                new JsonSerializerSettings() { TypeNameHandling = TypeNameHandling.Auto });
        }

        /// <summary>
        /// Deserializes the specified type.
        /// </summary>
        /// <param name="type">The type.</param>
        /// <param name="data">The data.</param>
        /// <returns>System.Object.</returns>
        public object Deserialize(Type type, byte[] data)
        {
            var raw = Encoding.UTF8.GetString(data);
            return JsonConvert.DeserializeObject(raw, type, new JsonSerializerSettings() { TypeNameHandling = TypeNameHandling.Auto });
        }

        /// <summary>
        /// Serializes the specified value.
        /// </summary>
        /// <typeparam name="TObject">The type of the object.</typeparam>
        /// <param name="value">The value.</param>
        /// <returns>System.Byte[].</returns>
        public byte[] Serialize<TObject>(TObject value)
        {
            var ser = JsonConvert.SerializeObject(value, new JsonSerializerSettings { TypeNameHandling = TypeNameHandling.Auto });
            return Encoding.UTF8.GetBytes(ser);
        }
    }
}
