namespace DataMiner.Proxies
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading;
    using System.Threading.Tasks;
    using DataMiner.Database;
    using DataMiner.Logger;
    using DataMiner.UrlLoader;

    public enum ProxyStatus { New, DontUpdate, Ready, Working, InTest, Offline }

    public class ProxyCache
    {
        private static readonly string TestUrl = "https://www.airbnb.ru";
        private static readonly int ProxyTestInterval = 5 * 60 * 1000;
        private static readonly int ProxyUpdateFormDbInterval = 30 * 60 * 1000;

        private readonly object _lock = new object();

        private ProxyList _ready = null;
        private Dictionary<Guid, Proxy> _used = null;
        private Dictionary<Guid, Proxy> _inTest = null;

        private bool _loading = true;

        private IDatabase _db;
        private ILogger _log;

        public void Init(IDatabase db, ILogger log)
        {
            _db = db;
            _log = log;
            _ready = new ProxyList();
            _used = new Dictionary<Guid, Proxy>();
            _inTest = new Dictionary<Guid, Proxy>();
            Task.Factory.StartNew(() => AutoUpdateThread());
        }

        public Proxy Get()
        {
            lock (_lock)
            {
                var first = _ready.Get();
                if (first == null)
                {
                    return null;
                }

                _used.Add(first.Uid, first);
                return first;
            }
        }

        public void Put(Proxy proxy, bool result)
        {
            lock (_lock)
            {
                _used.Remove(proxy.Uid);
                if (result)
                {
                    _ready.Add(proxy);
                }
                else
                {
                    _db.SetProxyStatus(proxy.Uid, ProxyStatus.InTest);
                    _inTest.Add(proxy.Uid, proxy);
                }
            }
        }

        #region Testing thread

        private void TestProxy(Proxy p)
        {
            try
            {
                if (!ObjectCreator.Get<Loader>().Test(TestUrl, p))
                {
                    return;
                }

                lock (_lock)
                {
                    _inTest.Remove(p.Uid);
                    _db.SetProxyStatus(p.Uid, ProxyStatus.Ready, Helpers.GetSpbCurrentTime());
                    p.LastUsageTime = DateTime.Now;
                    _ready.Add(p);
                }
            }
            catch (Exception e)
            {
                _log.LogError(e);
            }
        }

        private void TestingThread()
        {
            while (true)
            {
                try
                {
                    lock (_lock)
                    {
                        foreach (var key in _inTest.Keys)
                        {
                            var t = new Thread(() => TestProxy(_inTest[key]));
                            t.Start();
                        }
                    }

                    Thread.Sleep(ProxyTestInterval);
                }
                catch (Exception e)
                {
                    _log.LogError(e);
                }
            }
        }

        #endregion

        #region AutoUpdate

        private void AutoUpdateThread()
        {
            while (true)
            {
                try
                {
                    var proxies = _db.GetProxies();
                    lock (_lock)
                    {
                        RemoveProxies(proxies);
                        foreach (Proxy p in proxies)
                        {
                            CheckProxy(p);
                        }
                    }

                    if (_loading)
                    {
                        _loading = false;
                        var t = new Thread(TestingThread);
                        t.Start();
                    }
                    
                    Thread.Sleep(ProxyUpdateFormDbInterval);
                }
                catch (Exception e)
                {
                    _log.LogError(e);
                }
            }
        }
        
        private void UpdateProxy(Proxy p, Proxy newData)
        {
            p.Url = newData.Url;
            p.LastUsageTime = newData.LastUsageTime;
            p.Ping = newData.Ping;
            p.Port = newData.Port;
            p.Status = newData.Status;
        }

        private void CheckProxy(Proxy p)
        {
            if (_ready.Contains(p))
            {
                _ready.Update(p, UpdateProxy);
                return;
            }

            if (_used.ContainsKey(p.Uid))
            {
                var proxy = _used.FirstOrDefault(r => r.Key == p.Uid);
                UpdateProxy(proxy.Value, p);
                return;
            }

            if (_inTest.ContainsKey(p.Uid))
            {
                var proxy = _inTest.FirstOrDefault(r => r.Key == p.Uid);
                UpdateProxy(proxy.Value, p);
                return;
            }

            _db.SetProxyStatus(p.Uid, ProxyStatus.InTest);
            _inTest.Add(p.Uid, p);
        }

        private void RemoveProxies(IEnumerable<Proxy> proxies)
        {
            var proxiesToDelete = _ready.NotInList(proxies); 
            foreach (var p in proxiesToDelete)
            {
                _ready.Remove(p);
            }

            var proxiesToDeleteInTest = _inTest.Where(p => !proxies.Any(x => x.Uid == p.Key)).ToList();
            foreach (var p in proxiesToDeleteInTest)
            {
                _inTest.Remove(p.Key);
            }

            var proxiesToDeleteUsed = _used.Where(p => !proxies.Any(x => x.Uid == p.Key)).ToList();
            foreach (var p in proxiesToDeleteUsed)
            {
                _used.Remove(p.Key);
            }
        }

        #endregion
    }
}
