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
        /// Dequeues an item from the queue at a specific index.
        /// </summary>
        /// <param name="index">A 0-based index</param>
        /// <returns><see cref="T"/></returns>
        public T RemoveAt(int index)
        {
            LinkedListNode<T> currentNode = _linky.First;
            for (int i = 0; i <= index && currentNode != null; i++)
            {
                if (i != index)
                {
                    currentNode = currentNode.Next;
                    continue;
                }

                _linky.Remove(currentNode);
                return currentNode.Value;
            }

            throw new ArgumentOutOfRangeException("index");
        }

        /// <summary>
        /// Dequeues multiple items from the queue based on the index range.
        /// </summary>
        /// <param name="from">A 0-based inclusive index</param>
        /// <param name="to">A 0-based inclusive index</param>
        public void RemoveRange(int from, int to)
        {
            LinkedListNode<T> currentNode = _linky.First;
            for (int i = 0; i <= to && currentNode != null; i++)
            {
                if (from <= i)
                {
                    _linky.Remove(currentNode);
                    currentNode = currentNode.Next;
                    continue;
                }

                _linky.Remove(currentNode);
            }
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