using System;
using System.Collections.Generic;
using System.Linq;


namespace Victoria.Entities
{
    /// <summary>
    /// Based on linked list and uses FIFO.
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public sealed class LavaQueue<T> where T : class
    {
        private LinkedList<T> _linky;
        private readonly Random _random;

        /// <summary>
        /// Initializes a new instance of LavaQueue.
        /// </summary>
        public LavaQueue()
        {
            _random = new Random();
            _linky = new LinkedList<T>();
        }

        /// <summary>
        /// Adds a item at the end of the queue (FIFO).
        /// </summary>
        /// <param name="value"></param>
        public void Enqueue(T value)
        {
            _linky.AddLast(value);
        }

        /// <summary>
        /// Dequeues the first item from queue and returns it.
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
                value = null;
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
        /// Removes the first instance of <see cref="T"/> from queue.
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
        /// Count of all the items in queue.
        /// </summary>
        public int Count => _linky.Count;

        /// <summary>
        /// Returns a collection of all queue items.
        /// </summary>
        public IEnumerable<T> Items
        {
            get
            {
                for (var node = _linky.First; node != null; node = node.Next)
                    yield return node.Value;
            }
        }

        /// <summary>
        /// Shuffles the whole queue. Thanks to @WorkingRobot (Issue #9).
        /// </summary>
        public void Shuffle()
        {
            if (_linky.Count < 2)
                return;
            var shadow = new T[_linky.Count];
            var i = 0;
            for (var node = _linky.First; !(node is null); node = node.Next)
            {
                var j = _random.Next(i + 1);
                if (i != j)
                    shadow[i] = shadow[j];
                shadow[j] = node.Value;
                i++;
            }

            _linky.Clear();
            foreach (var value in shadow)
                _linky.AddLast(value);
        }
    }
}