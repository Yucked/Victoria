using System;
using System.Collections;
using System.Collections.Generic;

namespace Victoria {
    /// <summary>
    /// 
    /// </summary>
    /// <typeparam name="T"></typeparam>
    // ReSharper disable once ClassNeverInstantiated.Global
    public class LavaQueue<T> : IEnumerable<T>
        where T : LavaTrack {
        private readonly LinkedList<T> _list;

        /// <summary>
        /// 
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

        /// <summary>
        /// 
        /// </summary>
        public LavaQueue() {
            _list = new LinkedList<T>();
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
        /// 
        /// </summary>
        /// <param name="value"></param>
        /// <exception cref="ArgumentNullException"></exception>
        public void Enqueue(T value) {
            ArgumentNullException.ThrowIfNull(value);

            lock (_list) {
                _list.AddLast(value);
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="value"></param>
        /// <returns></returns>
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
        /// 
        /// </summary>
        /// <param name="value"></param>
        /// <returns></returns>
        public bool TryPeek(out T value) {
            lock (_list) {
                if (_list.First == null) {
                    value = default;
                    return false;
                }

                value = _list.First.Value;
                return true;
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="value"></param>
        /// <exception cref="ArgumentNullException"></exception>
        public void Remove(T value) {
            ArgumentNullException.ThrowIfNull(value);

            lock (_list) {
                _list.Remove(value);
            }
        }

        /// <summary>
        /// 
        /// </summary>
        public void Clear() {
            lock (_list) {
                _list.Clear();
            }
        }

        /// <summary>
        /// 
        /// </summary>
        public void Shuffle() {
            lock (_list) {
                if (_list.Count < 2) {
                    return;
                }

                var shadow = new T[_list.Count];
                var i = 0;
                for (var node = _list.First; !(node is null); node = node.Next) {
                    var j = Random.Shared.Next(i + 1);
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
        /// 
        /// </summary>
        /// <param name="index"></param>
        /// <returns></returns>
        /// <exception cref="Exception"></exception>
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
        /// 
        /// </summary>
        /// <param name="index"></param>
        /// <param name="count"></param>
        /// <returns></returns>
        /// <exception cref="ArgumentOutOfRangeException"></exception>
        public ICollection<T> RemoveRange(int index, int count) {
            ArgumentOutOfRangeException.ThrowIfNegative(index);
            ArgumentOutOfRangeException.ThrowIfNegative(count);
            ArgumentOutOfRangeException.ThrowIfGreaterThan(count, Count - index);

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