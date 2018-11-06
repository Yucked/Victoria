using System;
using System.Collections.Generic;

namespace Victoria
{
    public sealed class LavaQueue<T> where T : class
    {
        private readonly LinkedList<T> _linky;

        public LavaQueue()
        {
            var lazyList = new Lazy<LinkedList<T>>();
            _linky = lazyList.Value;
        }

        /// <summary>
        /// Adds a song at the end of the queue.
        /// </summary>
        /// <param name="value"></param>
        public void Enqueue(T value)
        {
            _linky.AddLast(value);
        }

        /// <summary>
        /// Dequeues the first item.
        /// </summary>
        /// <returns><see cref="T"/></returns>
        public T Dequeue()
        {
            var result = _linky.First.Value;
            _linky.RemoveFirst();
            return result;
        }

        /// <summary>
        /// Safely dequeues an item from queue.
        /// </summary>
        /// <param name="value"></param>
        /// <returns><see cref="T"/></returns>
        public bool TryDequeue(out T value)
        {
            var result = _linky.First?.Value;
            if (result == null)
            {
                value = default;
                return false;
            }

            _linky.RemoveFirst();
            value = result;
            return true;
        }

        /// <summary>
        /// Returns the first item in queue without removing it.
        /// </summary>
        /// <returns><see cref="T"/></returns>
        public T Peek()
        {
            return _linky.First.Value;
        }

        /// <summary>
        /// Removes an item from queue.
        /// </summary>
        /// <param name="value"></param>
        public void Remove(T value)
        {
            _linky.Remove(value);
        }

        /// <summary>
        /// Clears the queue.
        /// </summary>
        public void Clear()
        {
            _linky.Clear();
        }

        /// <summary>
        /// All the items in queue.
        /// </summary>
        public int Count => _linky.Count;

        /// <summary>
        /// Returns all the items in queue.
        /// </summary>
        public IEnumerable<T> Items
        {
            get
            {
                for (var node = _linky.First; node != null; node = node.Next)
                    yield return node.Value;
            }
        }
    }
}