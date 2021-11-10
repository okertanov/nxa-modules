using Neo.IO;
using Neo.Persistence;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Nxa.Plugins.Persistence.Cache
{
    public class StorageCache : IDisposable
    {
        private readonly Db.IStore store;

        public class DbItem
        {
            public StorageKey Key { get; set; }
            public byte[] Value { get; set; }
        }

        private readonly Dictionary<StorageKey, DbItem> dictionary = new();

        public StorageCache(Db.IStore store)
        {
            this.store = store;
        }

        /// <summary>
        /// Reads a specified entry from the cache. If the entry is not in the cache, it will be automatically loaded from the underlying storage.
        /// </summary>
        /// <param name="key">The key of the entry.</param>
        /// <returns>The cached data.</returns>
        /// <exception cref="KeyNotFoundException">If the entry doesn't exist.</exception>
        public byte[] this[StorageKey key]
        {
            get
            {
                lock (dictionary)
                {
                    if (!dictionary.TryGetValue(key, out DbItem dbItem))
                    {
                        dbItem = new DbItem
                        {
                            Key = key,
                            Value = store.TryGet(key.Key),
                        };
                        dictionary.Add(key, dbItem);
                    }

                    return dbItem.Value;
                }
            }
        }

        /// <summary>
        /// Adds a new entry to the cache.
        /// </summary>
        /// <param name="key">The key of the entry.</param>
        /// <param name="value">The data of the entry.</param>
        /// <exception cref="ArgumentException">The entry has already been cached.</exception>
        /// <remarks>Note: This method does not read the internal storage to check whether the record already exists.</remarks>
        public bool AddOrUpdate(StorageKey key, byte[] value)
        {
            lock (dictionary)
            {
                //if (dictionary.TryGetValue(key, out DbItem dbItem))
                //    return false;

                store?.Put(key.Key, value);

                dictionary[key] = new DbItem
                {
                    Key = key,
                    Value = value,
                };

                return true;
            }
        }


        /// <summary>
        /// Reads a specified entry from the cache. If the entry is not in the cache, it will be automatically loaded from the underlying storage.
        /// </summary>
        /// <param name="key">The key of the entry.</param>
        /// <returns>The cached data. Or <see langword="null"/> if it is neither in the cache nor in the underlying storage.</returns>
        public byte[] TryGet(StorageKey key)
        {
            lock (dictionary)
            {
                if (dictionary.TryGetValue(key, out DbItem dbItem))
                {
                    return dbItem.Value;
                }
                byte[] value = store.TryGet(key.Key);
                if (value == null) return null;
                dictionary.Add(key, new DbItem
                {
                    Key = key,
                    Value = value
                });
                return value;
            }
        }


        /// <summary>
        /// Deletes an entry from the cache and underlying storage.
        /// </summary>
        /// <param name="key">The key of the entry.</param>
        public void Delete(StorageKey key)
        {
            lock (dictionary)
            {
                if (dictionary.TryGetValue(key, out DbItem dbItem))
                {
                    dictionary.Remove(key);
                    store.Delete(key.Key);
                }
                else if (store.Contains(key.Key))
                {
                    store.Delete(key.Key);
                }
            }
        }

        /// <summary>
        /// Determines whether the cache or undelying storage contains the specified entry.
        /// </summary>
        /// <param name="key">The key of the entry.</param>
        /// <returns><see langword="true"/> if the cache contains an entry with the specified key; otherwise, <see langword="false"/>.</returns>
        public bool Contains(StorageKey key)
        {
            lock (dictionary)
            {
                if (dictionary.TryGetValue(key, out DbItem dbItem))
                {
                    return true;
                }
                return store.Contains(key.Key);
            }
        }

        /// <summary>
        /// Seeks to the entry with the specified key.
        /// </summary>
        /// <param name="keyOrPrefix">The key to be sought.</param>
        /// <param name="values">Check for prefix values.</param>
        /// <returns>An enumerator containing all the entries after seeking.</returns>
        public IEnumerable<(StorageKey Key, byte[] Value)> Seek(byte[] keyOrPrefix, byte[][] values = null)
        {
            IEnumerable<(byte[], StorageKey, byte[])> cached;
            HashSet<StorageKey> cachedKeySet;
            lock (dictionary)
            {
                cached = dictionary
                        .Where(p => (Helper.CheckForPrefix(p.Key.Key, keyOrPrefix) && (values == null || Helper.CompareByteArray(p.Value.Value, values))))
                        .Select(p =>
                        (
                            KeyBytes: p.Key.Key,
                            p.Key,
                            p.Value.Value
                        ))
                        .ToArray();
                cachedKeySet = new HashSet<StorageKey>(dictionary.Keys);
            }

            var uncached = store.Seek(keyOrPrefix ?? Array.Empty<byte>(), SeekDirection.Forward)
                .Select(p => (Key: new StorageKey(p.Key), Value: p.Value))
                .Where(p => !cachedKeySet.Contains(p.Key) && (values == null || Helper.CompareByteArray(p.Value, values)))
                .Select(p =>
                (
                    KeyBytes: p.Key.Key,
                    p.Key,
                    p.Value
                ));


            using var e1 = cached.GetEnumerator();
            using var e2 = uncached.GetEnumerator();
            (byte[] KeyBytes, StorageKey Key, byte[] Value) i1, i2;
            bool c1 = e1.MoveNext();
            bool c2 = e2.MoveNext();
            i1 = c1 ? e1.Current : default;
            i2 = c2 ? e2.Current : default;
            while (c1 || c2)
            {
                if (!c2 || ((c1 && Helper.CompareByteArray(i1.KeyBytes, i2.KeyBytes))))
                {
                    yield return (i1.Key, i1.Value);
                    c1 = e1.MoveNext();
                    i1 = c1 ? e1.Current : default;
                }
                else
                {
                    yield return (i2.Key, i2.Value);
                    c2 = e2.MoveNext();
                    i2 = c2 ? e2.Current : default;
                }
            }

        }

        #region Dispose

        private bool _disposedValue;

        ~StorageCache() => Dispose(false);

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (!_disposedValue)
            {
                if (disposing)
                {
                    store?.Dispose();
                }
                _disposedValue = true;
            }
        }

        #endregion
    }
}
