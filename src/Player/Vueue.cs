using System;
using System.Collections;
using System.Collections.Generic;

namespace Victoria.Player {
    /// <summary>
    ///     A queue based off of <see cref="LinkedList{T}" />.
    /// </summary>
    /// <typeparam name="T">
    ///     <see cref="LavaTrack" />
    /// </typeparam>
    public class Vueue<T> : IEnumerable<T> where T : LavaTrack {
        /// <summary>
        /// 
        /// </summary>
        protected readonly LinkedList<T> List;

        /// <summary>
        ///     Returns the total count of items.
        /// </summary>
        public int Count
        {
            get
            {
                lock (List) {
                    return List.Count;
                }
            }
        }

        /// <inheritdoc cref="Vueue{T}" />
        public Vueue() {
            List = new LinkedList<T>();
        }

        /// <inheritdoc />
        public IEnumerator<T> GetEnumerator() {
            lock (List) {
                for (var node = List.First; node != null; node = node.Next) {
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
        ///     Any object that inherits <see cref="LavaTrack" />.
        /// </param>
        public void Enqueue(T value) {
            if (value == null) {
                throw new ArgumentNullException(nameof(value));
            }

            lock (List) {
                List.AddLast(value);
            }
        }

        /// <summary>
        /// Adds several objects of <typeparamref name="T"/>.
        /// </summary>
        /// <param name="values">
        /// Any object that inherits <typeparamref name="T"/> />.
        /// </param>
        /// <exception cref="ArgumentNullException">Throws <see cref="ArgumentNullException"/> if <paramref name="values"/> is null.</exception>
        public void Enqueue(IEnumerable<T> values) {
            if (values == null) {
                throw new ArgumentNullException(nameof(values));
            }

            lock (List) {
                foreach (var value in values) {
                    List.AddLast(value);
                }
            }
        }

        /// <summary>
        ///     Safe way to dequeue an item.
        /// </summary>
        /// <param name="value">First object of type <see cref="LavaTrack" />.</param>
        /// <returns>
        ///     <see cref="bool" />
        /// </returns>
        public bool TryDequeue(out T value) {
            lock (List) {
                if (List.Count < 1) {
                    value = default;
                    return false;
                }

                if (List.First == null) {
                    value = default;
                    return true;
                }

                var result = List.First.Value;
                if (result == null) {
                    value = default;
                    return false;
                }

                List.RemoveFirst();
                value = result;

                return true;
            }
        }

        /// <summary>
        ///     Sneaky peaky the first time in list.
        /// </summary>
        /// <returns>
        ///     Returns first item of type <see cref="LavaTrack" />.
        /// </returns>
        public T Peek() {
            lock (List) {
                if (List.First == null) {
                    throw new Exception("Returned value is null.");
                }

                return List.First.Value;
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

            lock (List) {
                List.Remove(value);
            }
        }

        /// <summary>
        ///     Clears the queue.
        /// </summary>
        public void Clear() {
            lock (List) {
                List.Clear();
            }
        }

        /// <summary>
        ///     Shuffles all the items in the queue.
        /// </summary>
        public void Shuffle() {
            lock (List) {
                if (List.Count < 2) {
                    return;
                }

                var shadow = new T[List.Count];
                var i = 0;
                for (var node = List.First; node is not null; node = node.Next) {
                    var j = Random.Shared.Next(i + 1);
                    if (i != j) {
                        shadow[i] = shadow[j];
                    }

                    shadow[j] = node.Value;
                    i++;
                }

                List.Clear();
                foreach (var value in shadow) {
                    List.AddLast(value);
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
            lock (List) {
                var currentNode = List.First;

                for (var i = 0; i <= index && currentNode != null; i++) {
                    if (i != index) {
                        currentNode = currentNode.Next;
                        continue;
                    }

                    List.Remove(currentNode);
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
                throw new ArgumentOutOfRangeException(nameof(index));
            }

            var tempIndex = 0;
            var removed = new T[count];
            lock (List) {
                var currentNode = List.First;
                while (tempIndex != index && currentNode != null) {
                    tempIndex++;
                    currentNode = currentNode.Next;
                }

                var nextNode = currentNode?.Next;
                for (var i = 0; i < count && currentNode != null; i++) {
                    var tempValue = currentNode.Value;
                    removed[i] = tempValue;

                    List.Remove(currentNode);
                    currentNode = nextNode;
                    nextNode = nextNode?.Next;
                }

                return removed;
            }
        }
    }
}