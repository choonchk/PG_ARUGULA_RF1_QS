using System.Collections.Generic;


namespace GuCal
{
    public partial class GU
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
        }
    }
}



