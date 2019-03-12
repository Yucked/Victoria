using System;
using System.Collections.Generic;

namespace Victoria.Queue
{
    /// <summary>
    /// Queue based on <see cref="LinkedList{T}" />. Follows FIFO.
    /// </summary>
    /// <typeparam name="T">
    /// <see cref="IQueueObject" />
    /// </typeparam>
    public sealed class LavaQueue<T> where T : IQueueObject
    {
        private readonly LinkedList<T> _linked;
        private readonly Random _random;
        private readonly object _lockObj;

        /// <inheritdoc cref="LavaQueue{T}" />
        public LavaQueue()
        {
            _random = new Random();
            _linked = new LinkedList<T>();
            _lockObj = new object();
        }

        /// <summary>
        /// Returns the total count of items.
        /// </summary>
        public int Count
        {
            get
            {
                lock (_lockObj)
                {
                    return _linked.Count;
                }
            }
        }

        /// <inheritdoc cref="IEnumerable{T}" />
        public IEnumerable<T> Items
        {
            get
            {
                lock (_lockObj)
                {
                    for (var node = _linked.First; node != null; node = node.Next)
                        yield return node.Value;
                }
            }
        }

        /// <summary>
        /// Adds an object.
        /// </summary>
        /// <param name="value">
        /// <see cref="IQueueObject" />
        /// </param>
        public void Enqueue(T value)
        {
            lock (_lockObj)
            {
                _linked.AddLast(value);
            }
        }

        /// <summary>
        /// Removes the first item from queue.
        /// </summary>
        /// <returns>
        /// <see cref="IQueueObject" />
        /// </returns>
        public T Dequeue()
        {
            lock (_lockObj)
            {
                var result = _linked.First.Value;
                _linked.RemoveFirst();
                return result;
            }
        }

        /// <summary>
        /// Safely removes the first item from queue.
        /// </summary>
        /// <param name="value">
        /// <see cref="IQueueObject" />
        /// </param>
        /// <returns><see cref="bool" /> based on if dequeue-ing was successful.</returns>
        public bool TryDequeue(out T value)
        {
            lock (_lockObj)
            {
                if (_linked.Count < 1)
                {
                    value = default;
                    return false;
                }

                var result = _linked.First.Value;
                if (result == null)
                {
                    value = default;
                    return false;
                }

                _linked.RemoveFirst();
                value = result;
                return true;
            }
        }

        /// <summary>
        /// Sneaky peaky the first time in list.
        /// </summary>
        /// <returns>
        /// <see cref="IQueueObject" />
        /// </returns>
        public T Peek()
        {
            lock (_lockObj)
            {
                return _linked.First.Value;
            }
        }

        /// <summary>
        /// Removes an item from queue.
        /// </summary>
        /// <param name="value">
        /// <see cref="IQueueObject" />
        /// </param>
        public void Remove(T value)
        {
            lock (_lockObj)
            {
                _linked.Remove(value);
            }
        }

        /// <summary>
        /// Clears the queue.
        /// </summary>
        public void Clear()
        {
            lock (_lockObj)
            {
                _linked.Clear();
            }
        }

        /// <summary>
        /// Shuffles the queue.
        /// </summary>
        public void Shuffle()
        {
            lock (_lockObj)
            {
                if (_linked.Count < 2)
                    return;

                var shadow = new T[_linked.Count];
                var i = 0;
                for (var node = _linked.First; !(node is null); node = node.Next)
                {
                    var j = _random.Next(i + 1);
                    if (i != j)
                        shadow[i] = shadow[j];
                    shadow[j] = node.Value;
                    i++;
                }

                _linked.Clear();
                foreach (var value in shadow)
                    _linked.AddLast(value);
            }
        }

        /// <summary>
        /// Removes an item based on the given index.
        /// </summary>
        /// <param name="index">Index of item.</param>
        /// <returns>
        /// <see cref="IQueueObject" />
        /// </returns>
        public T RemoveAt(int index)
        {
            lock (_lockObj)
            {
                var currentNode = _linked.First;

                for (var i = 0; i <= index && currentNode != null; i++)
                {
                    if (i != index)
                    {
                        currentNode = currentNode.Next;
                        continue;
                    }

                    _linked.Remove(currentNode);
                    break;
                }

                return currentNode.Value;
            }
        }

        /// <summary>
        /// Removes a item from given range.
        /// </summary>
        /// <param name="from">Start index.</param>
        /// <param name="to">End index.</param>
        public void RemoveRange(int from, int to)
        {
            lock (_lockObj)
            {
                var currentNode = _linked.First;
                for (var i = 0; i <= to && currentNode != null; i++)
                {
                    if (from <= i)
                    {
                        _linked.Remove(currentNode);
                        currentNode = currentNode.Next;
                        continue;
                    }

                    _linked.Remove(currentNode);
                }
            }
        }
    }
}