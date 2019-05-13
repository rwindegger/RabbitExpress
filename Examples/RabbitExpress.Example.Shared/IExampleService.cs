// ***********************************************************************
// Assembly         : RabbitExpress.Example.Shared
// Author           : Rene Windegger
// Created          : 05-11-2019
//
// Last Modified By : Rene Windegger
// Last Modified On : 05-11-2019
// ***********************************************************************
// <copyright file="IExampleService.cs" company="Rene Windegger">
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
namespace RabbitExpress.Example.Shared
{
    /// <summary>
    /// Interface IExampleService
    /// </summary>
    public interface IExampleService
    {
        /// <summary>
        /// Encodes the message.
        /// </summary>
        /// <param name="message">The message.</param>
        /// <returns>ExampleMessage.</returns>
        ExampleMessage EncodeMessage(ExampleMessage message);
        /// <summary>
        /// Decodes the message.
        /// </summary>
        /// <param name="message">The message.</param>
        /// <returns>ExampleMessage.</returns>
        ExampleMessage DecodeMessage(ExampleMessage message);

        /// <summary>Calculates the specified number1.</summary>
        /// <param name="number1">The number1.</param>
        /// <param name="number2">The number2.</param>
        /// <returns>System.String.</returns>
        string Calculate(int number1, int number2);

        /// <summary>
        /// Processes the specified message.
        /// </summary>
        /// <param name="message">The message.</param>
        void Process(ExampleMessage message);
    }
}
