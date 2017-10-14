namespace airbnbmonitor.Code
{
    using System;
    using System.Runtime.Caching;
    using System.Collections.Generic;
    using airbnbmonitor.Code.Interfaces;

    public class MyCacheItem<T>
    {
        public T Item { get; set; }
        public DateTime Date { get; set; }
    }

    public class CacheService : ICacheService
    {
        private static readonly Dictionary<string, object> Lockers = new Dictionary<string, object>();

        private object GetLocker(string key)
        {
            lock (Lockers)
            {
                if (!Lockers.ContainsKey(key))
                {
                    var lockObj = new object();
                    Lockers[key] = lockObj;
                }

                return Lockers[key];
            }
        }

        public T Get<T>(string key, Func<T> getItemCallback) where T : class
        {
            var locker = GetLocker(key);
            lock (locker)
            {
                var cacheObj = MemoryCache.Default[key] as MyCacheItem<T>;
                if (cacheObj == null || DateTime.Now.Date > cacheObj.Date.Date)
                {
                    var item = new MyCacheItem<T>
                    {
                        Item = getItemCallback(),
                        Date = DateTime.Now
                    };
                    MemoryCache.Default[key] = item;
                    return item.Item;
                }

                return cacheObj.Item;
            }
        }
    }
}