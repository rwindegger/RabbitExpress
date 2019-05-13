// ***********************************************************************
// Assembly         : RabbitExpress
// Author           : Rene Windegger
// Created          : 05-13-2019
//
// Last Modified By : Rene Windegger
// Last Modified On : 05-13-2019
// ***********************************************************************
// <copyright file="ObjectPool.cs" company="Rene Windegger">
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
    using System;
    using System.Collections.Concurrent;
    using System.Collections.Generic;
    using System.Linq;

    /// <summary>
    /// Class ObjectPool.
    /// </summary>
    /// <typeparam name="TItem">The type of the t item.</typeparam>
    internal class ObjectPool<TItem>
        where TItem : class
    {
        /// <summary>
        /// The pool
        /// </summary>
        private readonly ConcurrentQueue<TItem> _pool = new ConcurrentQueue<TItem>();
        /// <summary>
        /// The factory
        /// </summary>
        private readonly Func<TItem> _factory;
        /// <summary>
        /// The backend
        /// </summary>
        private readonly List<PoolStore> _backend = new List<PoolStore>();

        /// <summary>
        /// Initializes a new instance of the <see cref="ObjectPool{TItem}"/> class.
        /// </summary>
        /// <param name="factory">The factory.</param>
        internal ObjectPool(Func<TItem> factory)
        {
            _factory = factory;
        }

        /// <summary>
        /// Expands the pool.
        /// </summary>
        private void ExpandPool()
        {
            _backend.Add(new PoolStore(this));
        }

        /// <summary>
        /// Gets this instance.
        /// </summary>
        /// <returns>PoolObject.</returns>
        public PoolObject Get()
        {
            if (_pool.TryDequeue(out TItem item))
                return new PoolObject(this, item);
            ExpandPool();
            return Get();
        }

        /// <summary>
        /// Releases the specified item.
        /// </summary>
        /// <param name="item">The item.</param>
        private void Release(TItem item)
        {
            if (_backend.Any(p => p.Contains(item)))
                _pool.Enqueue(item);
        }

        /// <summary>
        /// Class PoolStore.
        /// </summary>
        private class PoolStore
        {
            /// <summary>
            /// The parent
            /// </summary>
            private ObjectPool<TItem> _parent;
            /// <summary>
            /// The items
            /// </summary>
            private readonly TItem[] _items;

            /// <summary>
            /// Initializes a new instance of the <see cref="PoolStore"/> class.
            /// </summary>
            /// <param name="parent">The parent.</param>
            internal PoolStore(ObjectPool<TItem> parent)
            {
                _parent = parent;
                _items = Enumerable.Range(0, 25).Select(_ => _parent._factory()).ToArray();
                foreach (TItem i in _items)
                    _parent._pool.Enqueue(i);
            }

            /// <summary>
            /// Determines whether this instance contains the object.
            /// </summary>
            /// <param name="item">The item.</param>
            /// <returns><c>true</c> if [contains] [the specified item]; otherwise, <c>false</c>.</returns>
            internal bool Contains(TItem item)
            {
                return _items.Contains(item);
            }
        }

        /// <summary>
        /// Class PoolObject.
        /// Implements the <see cref="System.IDisposable" />
        /// </summary>
        /// <seealso cref="System.IDisposable" />
        public class PoolObject : IDisposable
        {
            /// <summary>
            /// The parent
            /// </summary>
            private ObjectPool<TItem> _parent;
            /// <summary>
            /// Initializes a new instance of the <see cref="PoolObject"/> class.
            /// </summary>
            /// <param name="parent">The parent.</param>
            /// <param name="item">The item.</param>
            internal PoolObject(ObjectPool<TItem> parent, TItem item)
            {
                _parent = parent;
                Item = item;
            }
            /// <summary>
            /// Gets the item.
            /// </summary>
            /// <value>The item.</value>
            public TItem Item { get; private set; }

            #region IDisposable Support
            /// <summary>
            /// The disposed value
            /// </summary>
            private bool _disposedValue = false; // To detect redundant calls

            /// <summary>
            /// Releases unmanaged and - optionally - managed resources.
            /// </summary>
            /// <param name="disposing"><c>true</c> to release both managed and unmanaged resources; <c>false</c> to release only unmanaged resources.</param>
            protected virtual void Dispose(bool disposing)
            {
                if (!_disposedValue)
                {
                    if (disposing)
                    {
                    }

                    _parent.Release(Item);
                    Item = null;
                    _parent = null;

                    _disposedValue = true;
                }
            }

            /// <summary>
            /// Finalizes an instance of the <see cref="PoolObject"/> class.
            /// </summary>
            ~PoolObject()
            {
                // Do not change this code. Put cleanup code in Dispose(bool disposing) above.
                Dispose(false);
            }

            // This code added to correctly implement the disposable pattern.
            /// <summary>
            /// Performs application-defined tasks associated with freeing, releasing, or resetting unmanaged resources.
            /// </summary>
            public void Dispose()
            {
                // Do not change this code. Put cleanup code in Dispose(bool disposing) above.
                Dispose(true);
                GC.SuppressFinalize(this);
            }
            #endregion
        }
    }
}
