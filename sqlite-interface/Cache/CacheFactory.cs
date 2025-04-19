using Database.Contracts;
using Database.Contracts.Cache;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Database.Cache
{
    public class CacheFactory
    {
        private static readonly ConcurrentDictionary<string, object> cacheObjects = new();

        private static readonly ConcurrentDictionary<string, DateTime> cacheExpiry = new();

        /// <summary>
        /// Creates or retrieves a cache object for the specified type.
        /// </summary>
        /// <typeparam name="T">The type of data to be cached</typeparam>
        /// <param name="cacheKey">Optional custom cache key. If not provided, type name will be used</param>
        /// <returns></returns>
        public static ICache<T> GetOrCreate<T>(string? cacheKey = null) where T : ICacheable
        {
            string key = cacheKey ?? typeof(T).Name;
            var cache = (ICache<T>)cacheObjects.GetOrAdd(key, _ => new Cache<T>());

            if (!cacheExpiry.ContainsKey(key))
            {
                cacheExpiry.TryAdd(key, DateTime.Now.AddMinutes(5));
            }

            if (DateTime.Now > cacheExpiry[key])
            {
                cache.Clear();
            }

            // Reset the timer any time the cache is accessed
            cacheExpiry[key] = DateTime.Now.AddMinutes(5);

            return cache;
        }

        /// <summary>
        /// Clears all cache objects.
        /// </summary>
        public static void ClearAll()
        {
            foreach (var cache in cacheObjects.Values)
            {
                if (cache is IClearable clearable)
                {
                    clearable.Clear();
                }
            }
        }

        /// <summary>
        /// Clears expired cache objects.
        /// </summary>
        public static void ClearExpired()
        {
            foreach (var key in cacheExpiry.Keys)
            {
                if (DateTime.Now > cacheExpiry[key])
                {
                    if (cacheObjects.TryGetValue(key, out var cache) && cache is IClearable clearable)
                    {
                        clearable.Clear();
                    }
                }
            }
        }

        /// <summary>
        /// Removes the cache object for the specified key.
        /// </summary>
        /// <param name="cacheKey"></param>
        public static void RemoveCache(string cacheKey)
        {
            cacheObjects.TryRemove(cacheKey, out _);
        }

        /// <summary>
        /// Clears the cache for the specified key.
        /// </summary>
        /// <param name="cacheKey"></param>
        public static void ClearCache(string cacheKey)
        {
            if (cacheObjects.TryGetValue(cacheKey, out var cache) && cache is IClearable clearable)
            {
                clearable.Clear();
            }
        }

        /// <summary>
        /// Clears the cache for the specified type.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        public static void ClearCache<T>()
        {
            ClearCache(typeof(T).Name);
        }

        /// <summary>
        /// Retrieves all cache keys.
        /// </summary>
        /// <returns></returns>
        public static IEnumerable<string> GetCacheKeys()
        {
            return cacheObjects.Keys;
        }

        /// <summary>
        /// Generates a cache key based on the specified data.
        /// </summary>
        /// <param name="data"></param>
        /// <returns>string</returns>
        public static string GenerateCacheKey(params string[] data)
        {
            return string.Join(":", data);
        }
    }
}
