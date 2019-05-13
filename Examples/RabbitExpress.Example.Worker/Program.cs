// ***********************************************************************
// Assembly         : RabbitExpress.Example.Worker
// Author           : Rene Windegger
// Created          : 04-30-2019
//
// Last Modified By : Rene Windegger
// Last Modified On : 04-30-2019
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
namespace RabbitExpress.Example.Worker
{
    using Microsoft.Extensions.Configuration;
    using Shared;
    using System;
    using System.IO;

    /// <summary>
    /// Class Program.
    /// </summary>
    internal class Program
    {
        /// <summary>
        /// Defines the entry point of the application.
        /// </summary>
        /// <param name="args">The arguments.</param>
        private static void Main(string[] args)
        {
            IConfiguration config = new ConfigurationBuilder()
                .SetBasePath(Directory.GetCurrentDirectory())
                .AddJsonFile("appsettings.json", true, true)
                .AddEnvironmentVariables()
                .Build();

            var r = new Random();
            using (var qc = new QueueClient<JsonSerializer>(new Uri(config["RabbitExpressConnection"])))
            {
                qc.RegisterWorker<Queues, ExampleMessage>(Queues.EXAMPLE_QUEUE, m =>
                {
                    try
                    {
                        if (string.IsNullOrWhiteSpace(m.Message?.Text))
                        {
                            Console.WriteLine("Rejecting empty message.");
                            return WorkerResult.Failed;
                        }

                        if (r.Next(100) % 3 == 0)
                        {
                            throw new ApplicationException("Simulated recoverable error.");
                        }

                        Console.WriteLine($"Acknowledging {m.Message.Text}");
                        return WorkerResult.Success;
                    }
                    catch (Exception e)
                    {
                        Console.WriteLine($"Rejecting {m.Message?.Text} with reason: {e}");
                        return WorkerResult.Requeue;
                    }
                });
                Console.ReadLine();
            }
        }
    }
}
