using Database.Contracts;
using Database.Extensions;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Database
{
    public class Cache<T> : ICache<T>, IClearable
    {
        private readonly ConcurrentDictionary<string, T> _cache = new();

        public Cache() { }

        public void Store(string key, T value)
        {
            _cache.AddOrUpdate(key, value, (k, v) => value);
        }

        public T? Get(string key)
        {
            return _cache.TryGetValue(key, out var value) ? value : default;
        }

        public void Remove(string key)
        {
            _cache.TryRemove(key, out _);
        }

        public void Clear()
        {
            _cache.Clear();
        }

        public bool Contains(string key)
        {
            return _cache.ContainsKey(key);
        }
    }
}
