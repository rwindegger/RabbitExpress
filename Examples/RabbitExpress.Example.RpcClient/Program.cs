﻿// ***********************************************************************
// Assembly         : RabbitExpress.Example.RpcClient
// Author           : Rene Windegger
// Created          : 05-11-2019
//
// Last Modified By : Rene Windegger
// Last Modified On : 05-11-2019
// ***********************************************************************
// <copyright file="Program.cs" company="Rene Windegger">
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
namespace RabbitExpress.Example.RpcClient
{
    using Microsoft.Extensions.Configuration;
    using Shared;
    using System;
    using System.IO;
    using System.Threading.Tasks;

    /// <summary>
    /// Class Program.
    /// </summary>
    internal class Program
    {
        private static IConfigurationRoot _configuration;

        private static async Task Test()
        {
            var qConfig = new QueueConfig();
            _configuration.GetSection("RabbitExpress").Bind(qConfig);
            using (var qc = new QueueClient<MsgPackSerializer>(qConfig))
            {
                IExampleService client = qc.RpcClient<IExampleService>();
                Console.WriteLine(client.Calculate(2, 4));
                var input = new ExampleMessage { Text = "RabbitExpress Test" };
                client.Process(input);
                ExampleMessage msg = await client.EncodeMessage(input);
                Console.WriteLine(msg.Text);
                ExampleMessage decmsg = await client.DecodeMessage(msg);
                Console.WriteLine(decmsg.Text);
                Console.ReadLine();
            }
        }

        /// <summary>
        /// Defines the entry point of the application.
        /// </summary>
        /// <param name="args">The arguments.</param>
        private static void Main(string[] args)
        {
            _configuration = new ConfigurationBuilder()
                .SetBasePath(Directory.GetCurrentDirectory())
                .AddJsonFile("appsettings.json", true, true)
                .AddEnvironmentVariables()
                .Build();

            Task.WaitAll(Test());
        }
    }
}
