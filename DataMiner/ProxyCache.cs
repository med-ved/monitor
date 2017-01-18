using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace DataMiner
{
    public enum ProxyStatus { New, DontUpdate, Ready, Working, InTest, Offline }

    public class Proxy
    {
        public Guid Uid { get; set; }
        public string Url { get; set; }
        public int Port { get; set; }
        public DateTime? LastUsageTime { get; set; }
        public ProxyStatus Status { get; set; }
        public int Ping { get; set; }
        public DateTime? LastErrorTime { get; set; }
        public string LastError { get; set; }

        public string GetUrl()
        {
            return String.Format("{0}:{1}", Url, Port);
        }
    }

    public class ProxyCache
    {
        private object _lock = new object();

        private ProxyList _ready = null;
        private Dictionary<Guid, Proxy> _used = null;
        private Dictionary<Guid, Proxy> _inTest = null;

        private const string _testUrl = "https://www.airbnb.ru";
        private const int _proxyTestInterval = 5 * 60 * 1000;
        private const int _proxyUpdateFormDbInterval = 30 * 60 * 1000;

        private bool _loading = true;

        public void Init()
        {
            var comparer = new ProxyComparer();
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
                Database.IncProxyUseCount(first.Uid);

                return first;
            }
        }

        public void Put(Proxy proxy, bool result)
        {
            lock (_lock)
            {
                proxy.LastUsageTime = DateTime.Now;
                
                _used.Remove(proxy.Uid);
                if (result)
                {
                    Database.UpdateProxyPing(proxy.Uid);
                    _ready.Add(proxy);
                }
                else
                {
                    Database.UpdateProxyPing(proxy.Uid, ProxyStatus.InTest);
                    //Logger.WriteLine("Proxy", string.Format("Proxy {0} is OFFLINE and added to IN TEST", proxy.GetUrl()));
                    _inTest.Add(proxy.Uid, proxy);
                }
            }
        }

        #region Testing thread

        private void TestProxy(Proxy p)
        {
            try
            {
                Database.SetProxyStatus(p.Uid, lastUsageTime: DateTime.Now);
                if (!Loader.Test(_testUrl, p))
                {
                    return;
                }

                lock (_lock)
                {
                    Database.SetProxyStatus(p.Uid, ProxyStatus.Ready);
                    //Logger.WriteLine("Proxy", string.Format("Proxy {0} added to READY. Count={1}", p.GetUrl(), _ready.Count));
                    _inTest.Remove(p.Uid);
                    p.LastUsageTime = DateTime.Now;
                    _ready.Add(p);
                   
                }
            }
            catch(Exception e)
            {
                Logger.LogError(e);
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
                            Task.Factory.StartNew(() => TestProxy(_inTest[key]));
                        }
                    }

                    Thread.Sleep(_proxyTestInterval);
                }
                catch (Exception e)
                {
                    Logger.LogError(e);
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
                    var proxies = Database.GetProxies();
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
                        Task.Factory.StartNew(() => TestingThread());
                    }
                    
                    Thread.Sleep(_proxyUpdateFormDbInterval);
                }
                catch (Exception e)
                {
                    Logger.LogError(e);
                }
            }
        }


        private void UpdateProxy(Proxy p, Proxy newData)
        {
            //Logger.WriteLine("Proxy", string.Format("Proxy {0} UPDATED", p.GetUrl()));

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
                var proxy = _used.FirstOrDefault(r => r.Key== p.Uid);
                UpdateProxy(proxy.Value, p);
                return;
            }

            if (_inTest.ContainsKey(p.Uid))
            {
                var proxy = _inTest.FirstOrDefault(r => r.Key == p.Uid);
                UpdateProxy(proxy.Value, p);
                return;
            }

            //Logger.WriteLine("Proxy", string.Format("Proxy {0} added to TESTING", p.GetUrl()));
            Database.SetProxyStatus(p.Uid, ProxyStatus.InTest);
            _inTest.Add(p.Uid, p);
        }

        private void RemoveProxies(IEnumerable<Proxy> proxies)
        {
            var proxiesToDelete = _ready.NotInList(proxies); //_ready.Where(p => !proxies.Any(x => x.Uid == p.Value)).ToList();
            foreach (var p in proxiesToDelete)
            {
                //Logger.WriteLine("Proxy", string.Format("Proxy {0} REMOVED", p.GetUrl()));
                _ready.Remove(p);
            }

            var proxiesToDelete2 = _inTest.Where(p => !proxies.Any(x => x.Uid == p.Key)).ToList();
            foreach (var p in proxiesToDelete2)
            {
                //Logger.WriteLine("Proxy", string.Format("Proxy {0} REMOVED", p.Value.GetUrl()));
                _inTest.Remove(p.Key);
            }

            var proxiesToDelete3 = _used.Where(p => !proxies.Any(x => x.Uid == p.Key)).ToList();
            foreach (var p in proxiesToDelete3)
            {
                //Logger.WriteLine("Proxy", string.Format("Proxy {0} REMOVED", p.Value.GetUrl()));
                _used.Remove(p.Key);
            }
        }

        #endregion
    }

    public class ProxyComparer : IComparer<Proxy>
    {
        public int Compare(Proxy x, Proxy y)
        {
            return Convert.ToInt32(x.LastUsageTime > y.LastUsageTime);
        }
    }
}
