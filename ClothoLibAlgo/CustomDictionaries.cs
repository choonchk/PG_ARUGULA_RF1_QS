using System;
using System.Collections;
using System.Collections.Generic;


namespace ClothoLibAlgo
{

    public class Dictionary   // extend the Dictionary class to include double-key, triple-key, and quadruple key dictionaries
    {
        public class DoubleKey<T1, T2, V> : Dictionary<T1, Dictionary<T2, V>>   // the <> denote a user-defined generic collection.
        {         //We see data type variables T1, T2, and V instead of data types. This get replaced during compilation with whatever types were designated at instantiation code.

            public DoubleKey()
            {
            }

            public DoubleKey(DoubleKey<T1, T2, V> copyFromDict)   // for copying a DoubleKeyDictionary into another DoubleKeyDictionary
            {
                foreach (T1 key1 in copyFromDict.Keys)    // this is necessary so that inner dictionaries don't share memory with copyFromDict's inner dictionaries
                {
                    this[key1] = new Dictionary<T2, V>(copyFromDict[key1]);
                }
            }

            public V this[T1 key1, T2 key2]      // error-safe 2D indexer (provides index index ability for objects), returns type V
            {
                get
                {
                    V outv = default(V);
                    if (this.ContainsKey(key1))
                    {
                        if (this[key1].ContainsKey(key2))
                        {
                            outv = this[key1][key2];
                        }
                    }
                    return outv;   // returns the private dictionary value at the requested keys
                }
                set
                {
                    if (!this.ContainsKey(key1))
                    {
                        this[key1] = new Dictionary<T2, V>();
                    }
                    this[key1][key2] = value;    // set the value at the requested keys, will overwrite existing key & value if present. Note that .Add method generates fault if key exists
                }
            }
        }


        public class TripleKey<T1, T2, T3, V> : Dictionary<T1, DoubleKey<T2, T3, V>> // the <> denote a user-defined generic collection.
        {         //We see data type variables T1, T2, T3 and V instead of data types. This get replaced during compilation with whatever types were designated at instantiation code.

            public TripleKey()
            {
            }

            public TripleKey(TripleKey<T1, T2, T3, V> copyFromDict)   // for copying a TripleKeyDictionary into another TripleKeyDictionary
            {
                foreach (T1 key1 in copyFromDict.Keys)    // this is necessary so that inner dictionaries don't share memory with copyFromDict's inner dictionaries
                {
                    this[key1] = new DoubleKey<T2, T3, V>(copyFromDict[key1]);
                }
            }

            public V this[T1 key1, T2 key2, T3 key3]      // error-safe indexer (provides index index ability for objects), returns type V
            {
                get
                {
                    V outv = default(V);
                    if (this.ContainsKey(key1))
                    {
                        outv = this[key1][key2, key3];
                    }
                    return outv;   // returns the private dictionary value at the requested keys
                }
                set
                {
                    if (!this.ContainsKey(key1))
                    {
                        this[key1] = new DoubleKey<T2, T3, V>(); //Dictionary<T2, Dictionary<T3, V>>();   // create the new inner-dictionary if it doesn't already exist
                    }
                    this[key1][key2, key3] = value;    // set the value at the requested keys, will overwrite existing key & value if present. Note that .Add method generates fault if key exists
                }
            }
        }


        public class QuadKey<T1, T2, T3, T4, V> : Dictionary<T1, TripleKey<T2, T3, T4, V>>   // the <> denote a user-defined generic collection.
        {         //We see data type variables T1, T2, T3, T4 and V instead of data types. This get replaced during compilation with whatever types were designated at instantiation code.

            public QuadKey()
            {
            }

            public QuadKey(QuadKey<T1, T2, T3, T4, V> copyFromDict)   // for copying a QuadKeyDictionary into another QuadKeyDictionary
            {
                foreach (T1 key1 in copyFromDict.Keys)    // this is necessary so that inner dictionaries don't share memory with copyFromDict's inner dictionaries
                {
                    this[key1] = new TripleKey<T2, T3, T4, V>(copyFromDict[key1]);
                }
            }

            public V this[T1 key1, T2 key2, T3 key3, T4 key4]      // error-safe indexer (provides index index ability for objects), returns type V
            {
                get
                {
                    V outv = default(V);
                    if (this.ContainsKey(key1))
                    {
                        outv = this[key1][key2, key3, key4];
                    }
                    return outv;   // returns the private dictionary value at the requested keys
                }
                set
                {
                    if (!this.ContainsKey(key1))
                    {
                        this[key1] = new TripleKey<T2, T3, T4, V>(); //Dictionary<T2, Dictionary<T3, Dictionary<T4, V>>>();   // create the new inner-dictionary if it doesn't already exist
                    }
                    this[key1][key2, key3, key4] = value;    // set the value at the requested keys, will overwrite existing key & value if present. Note that .Add method generates fault if key exists
                }
            }
        }

        public class Ordered<TKey, TValue> : IDictionary<TKey, TValue>, ICollection<KeyValuePair<TKey, TValue>>, IEnumerable<KeyValuePair<TKey, TValue>>, IEnumerable
        {
            private Dictionary<TKey, TValue> _dictionary;
            private List<TKey> _keys;
            private List<TValue> _values;
            public int Count
            {
                get
                {
                    return this._dictionary.Count;
                }
            }
            public ICollection<TKey> Keys
            {
                get
                {
                    return this._keys.AsReadOnly();
                }
            }
            public TValue this[TKey key]
            {
                get
                {
                    return this._dictionary[key];
                }
                set
                {
                    this.RemoveFromLists(key);
                    this._dictionary[key] = value;
                    this._keys.Add(key);
                    this._values.Add(value);
                }
            }
            public ICollection<TValue> Values
            {
                get
                {
                    return this._values.AsReadOnly();
                }
            }
            bool ICollection<KeyValuePair<TKey, TValue>>.IsReadOnly
            {
                get
                {
                    return ((ICollection<KeyValuePair<TKey, TValue>>)this._dictionary).IsReadOnly;
                }
            }
            //[TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
            public Ordered()
                : this(0)
            {
            }
            public Ordered(int capacity)
            {
                this._dictionary = new Dictionary<TKey, TValue>(capacity);
                this._keys = new List<TKey>(capacity);
                this._values = new List<TValue>(capacity);
            }
            public void Add(TKey key, TValue value)
            {
                this._dictionary.Add(key, value);
                this._keys.Add(key);
                this._values.Add(value);
            }
            public void Clear()
            {
                this._dictionary.Clear();
                this._keys.Clear();
                this._values.Clear();
            }
            public bool ContainsKey(TKey key)
            {
                return this._dictionary.ContainsKey(key);
            }
            public bool ContainsValue(TValue value)
            {
                return this._dictionary.ContainsValue(value);
            }
            public IEnumerator<KeyValuePair<TKey, TValue>> GetEnumerator()
            {
                int num = 0;
                foreach (TKey current in this._keys)
                {
                    yield return new KeyValuePair<TKey, TValue>(current, this._values[num]);
                    num++;
                }
                yield break;
            }
            private void RemoveFromLists(TKey key)
            {
                int num = this._keys.IndexOf(key);
                if (num != -1)
                {
                    this._keys.RemoveAt(num);
                    this._values.RemoveAt(num);
                }
            }
            public bool Remove(TKey key)
            {
                this.RemoveFromLists(key);
                return this._dictionary.Remove(key);
            }
            public bool TryGetValue(TKey key, out TValue value)
            {
                return this._dictionary.TryGetValue(key, out value);
            }
            void ICollection<KeyValuePair<TKey, TValue>>.Add(KeyValuePair<TKey, TValue> item)
            {
                this.Add(item.Key, item.Value);
            }
            bool ICollection<KeyValuePair<TKey, TValue>>.Contains(KeyValuePair<TKey, TValue> item)
            {
                return ((ICollection<KeyValuePair<TKey, TValue>>)this._dictionary).Contains(item);
            }
            void ICollection<KeyValuePair<TKey, TValue>>.CopyTo(KeyValuePair<TKey, TValue>[] array, int arrayIndex)
            {
                ((ICollection<KeyValuePair<TKey, TValue>>)this._dictionary).CopyTo(array, arrayIndex);
            }
            bool ICollection<KeyValuePair<TKey, TValue>>.Remove(KeyValuePair<TKey, TValue> item)
            {
                bool flag = ((ICollection<KeyValuePair<TKey, TValue>>)this._dictionary).Remove(item);
                if (flag)
                {
                    this.RemoveFromLists(item.Key);
                }
                return flag;
            }
            //[TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
            IEnumerator IEnumerable.GetEnumerator()
            {
                return this.GetEnumerator();
            }
        }

    }

}