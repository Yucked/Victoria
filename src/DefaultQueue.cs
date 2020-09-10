using System;
using System.Collections;
using System.Collections.Generic;

namespace Victoria {
    /// <summary>
    ///     A queue based off of <see cref="LinkedList{T}" />.
    /// </summary>
    /// <typeparam name="T">
    ///     <see cref="LavaTrack" />
    /// </typeparam>
    public sealed class DefaultQueue<T> : IEnumerable<T> where T : LavaTrack {
        private readonly LinkedList<T> _list;
        private readonly Random _random;

        /// <summary>
        ///     Returns the total count of items.
        /// </summary>
        public int Count
        {
            get
            {
                lock (_list) {
                    return _list.Count;
                }
            }
        }

        /// <inheritdoc cref="DefaultQueue{T}" />
        public DefaultQueue() {
            _list = new LinkedList<T>();
            _random = new Random();
        }

        /// <inheritdoc />
        public IEnumerator<T> GetEnumerator() {
            lock (_list) {
                for (var node = _list.First; node != null; node = node.Next) {
                    yield return node.Value;
                }
            }
        }

        IEnumerator IEnumerable.GetEnumerator() {
            return GetEnumerator();
        }

        /// <summary>
        ///     Adds an object.
        /// </summary>
        /// <param name="value">
        ///     Any object that inherits <see cref="IQueueable" />.
        /// </param>
        public void Enqueue(T value) {
            if (value == null) {
                throw new ArgumentNullException(nameof(value));
            }

            lock (_list) {
                _list.AddLast(value);
            }
        }

        /// <summary>
        ///     Safe way to dequeue an item.
        /// </summary>
        /// <param name="value">First object of type <see cref="IQueueable" />.</param>
        /// <returns>
        ///     <see cref="bool" />
        /// </returns>
        public bool TryDequeue(out T value) {
            lock (_list) {
                if (_list.Count < 1) {
                    value = default;
                    return false;
                }

                if (_list.First == null) {
                    value = default;
                    return true;
                }

                var result = _list.First.Value;
                if (result == null) {
                    value = default;
                    return false;
                }

                _list.RemoveFirst();
                value = result;

                return true;
            }
        }

        /// <summary>
        ///     Sneaky peaky the first time in list.
        /// </summary>
        /// <returns>
        ///     Returns first item of type <see cref="IQueueable" />.
        /// </returns>
        public T Peek() {
            lock (_list) {
                if (_list.First == null) {
                    throw new Exception("Returned value is null.");
                }

                return _list.First.Value;
            }
        }

        /// <summary>
        ///     Removes an item from queue.
        /// </summary>
        /// <param name="value">Item to remove.</param>
        public void Remove(T value) {
            if (value == null) {
                throw new ArgumentNullException(nameof(value));
            }

            lock (_list) {
                _list.Remove(value);
            }
        }

        /// <summary>
        ///     Clears the queue.
        /// </summary>
        public void Clear() {
            lock (_list) {
                _list.Clear();
            }
        }

        /// <summary>
        ///     Shuffles all the items in the queue.
        /// </summary>
        public void Shuffle() {
            lock (_list) {
                if (_list.Count < 2) {
                    return;
                }

                var shadow = new T[_list.Count];
                var i = 0;
                for (var node = _list.First; !(node is null); node = node.Next) {
                    var j = _random.Next(i + 1);
                    if (i != j) {
                        shadow[i] = shadow[j];
                    }

                    shadow[j] = node.Value;
                    i++;
                }

                _list.Clear();
                foreach (var value in shadow) {
                    _list.AddLast(value);
                }
            }
        }

        /// <summary>
        ///     Removes an item based on the given index.
        /// </summary>
        /// <param name="index">Index of item.</param>
        /// <returns>
        ///     Returns the removed item.
        /// </returns>
        public T RemoveAt(int index) {
            lock (_list) {
                var currentNode = _list.First;

                for (var i = 0; i <= index && currentNode != null; i++) {
                    if (i != index) {
                        currentNode = currentNode.Next;
                        continue;
                    }

                    _list.Remove(currentNode);
                    break;
                }

                if (currentNode == null) {
                    throw new Exception("Node was null.");
                }

                return currentNode.Value;
            }
        }

        /// <summary>
        ///     Removes a item from given range.
        /// </summary>
        /// <param name="index">The start index.</param>
        /// <param name="count">How many items to remove after the specified index.</param>
        public ICollection<T> RemoveRange(int index, int count) {
            if (index < 0) {
                throw new ArgumentOutOfRangeException(nameof(index));
            }

            if (count < 0) {
                throw new ArgumentOutOfRangeException(nameof(count));
            }

            if (Count - index < count) {
                throw new ArgumentOutOfRangeException();
            }

            var tempIndex = 0;
            var removed = new T[count];
            lock (_list) {
                var currentNode = _list.First;
                while (tempIndex != index && currentNode != null) {
                    tempIndex++;
                    currentNode = currentNode.Next;
                }

                var nextNode = currentNode?.Next;
                for (var i = 0; i < count && currentNode != null; i++) {
                    var tempValue = currentNode.Value;
                    removed[i] = tempValue;

                    _list.Remove(currentNode);
                    currentNode = nextNode;
                    nextNode = nextNode?.Next;
                }

                return removed;
            }
        }
    }
}