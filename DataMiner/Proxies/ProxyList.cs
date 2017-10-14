namespace DataMiner.Proxies
{
    using System;
    using System.Collections.Generic;

    public class ProxyList
    {
        private readonly List<Proxy> _proxyes = null;
        private readonly Dictionary<Guid, Proxy> _keys = null;

        public int Count
        {
            get
            {
                return _proxyes.Count;
            }
        }

        public ProxyList()
        {
            _proxyes = new List<Proxy>();
            _keys = new Dictionary<Guid, Proxy>();
        }

        public Proxy Get()
        {
            if (_proxyes.Count == 0)
            {
                return null;
            }

            var first = _proxyes[0];
            _proxyes.RemoveAt(0);
            _keys.Remove(first.Uid);
            return first;
        }

        public void Update(Proxy newData, Action<Proxy, Proxy> updater)
        {
            if (!_keys.ContainsKey(newData.Uid))
            {
                return;
            }

            var p = _keys[newData.Uid];
            updater(p, newData);
            _proxyes.Sort(Comparator);
        }

        public void Add(Proxy p)
        {
            if (_keys.ContainsKey(p.Uid))
            {
                return;
            }

            p.UpdateLastUsageTime();
            _keys.Add(p.Uid, p);
            _proxyes.Add(p);
            _proxyes.Sort(Comparator);
        }

        public bool Contains(Proxy p)
        {
            return _keys.ContainsKey(p.Uid);
        }

        public void Remove(Proxy p)
        {
            if (!_keys.ContainsKey(p.Uid))
            {
                return;
            }

            var proxy = _keys[p.Uid];
            _keys.Remove(p.Uid);
            _proxyes.Remove(proxy);
        }

        public IEnumerable<Proxy> NotInList(IEnumerable<Proxy> list)
        {
            var inList = new Dictionary<Guid, bool>();
            foreach (var p in list)
            {
                inList.Add(p.Uid, true);
            }

            var result = new List<Proxy>();
            foreach (var p in _proxyes)
            {
                if (!inList.ContainsKey(p.Uid))
                {
                    result.Add(p);
                }
            }

            return result;
        }

        private int Comparator(Proxy a, Proxy b)
        {
            if (a.LastUsageTime == null && b.LastUsageTime == null)
            {
                return 0;
            }

            if (a.LastUsageTime == null && b.LastUsageTime != null)
            {
                return 1;
            }

            if (a.LastUsageTime != null && b.LastUsageTime == null)
            {
                return -1;
            }

            if (a.LastUsageTime == b.LastUsageTime)
            {
                return 0;
            }

            return a.LastUsageTime < b.LastUsageTime ? -1 : 1;
        }
    }
}
