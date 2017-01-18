using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using DataMiner;

namespace DataMiner
{
    public class LoaderWithTimeout
    {
        private int _timeout = 15000;
        private ThreadSafeValue<bool> _done = new ThreadSafeValue<bool>(false);
        private ThreadSafeValue<bool> _canceled = new ThreadSafeValue<bool>(false);
        private ThreadSafeValue<Exception> _exception = new ThreadSafeValue<Exception>(null);
        private ThreadSafeValue<string> _result = new ThreadSafeValue<string>();

        private void OnLoadComplete(object sender, DownloadStringCompletedEventArgs e)
        {
            if (e.Cancelled)
            {
                _canceled.Value = true;
                return;
            }

            if (e.Error == null)
            {
                _result.Value = e.Result;
                _done.Value = true;
            }
            else
            {
                _exception.Value = e.Error;
            }
        }

        public string LoadUrl(string url, Proxy p)
        {
            using (var client = new WebClient())
            {
                var proxy = new WebProxy(p.Url, p.Port);
                client.Proxy = proxy;

                client.Encoding = Encoding.UTF8;
                client.Headers.Add("User-Agent: Other");
                client.DownloadStringAsync(new Uri(url));

                client.DownloadStringCompleted += OnLoadComplete;

                var _before = DateTime.Now;
                while (true)
                {
                    if ((DateTime.Now - _before).TotalMilliseconds >= _timeout)
                    {
                        client.CancelAsync();
                        throw new Exception("Loader: TIMEOUT");
                    }
                    
                    if (_canceled.Value)
                    {
                        throw new Exception("Loader: Cancelled");
                    }

                    if (_exception.Value != null)
                    {
                        throw _exception.Value;
                    }

                    if (_done.Value)
                    {
                        return _result.Value;
                    }

                    Thread.Sleep(50);
                }
            }
        }

    }

    static class Loader
    {
        private static ProxyCache _cache = new ProxyCache();
        public static void Init()
        {
            _cache.Init();
        }

        public static string Load(string url)
        {
            string html = null;

            bool done = false;
            while(!done)
            {
                var p = _cache.Get();
                if (p == null)
                {
                    Thread.Sleep(1000);
                    continue;
                }

                try
                {
                    var loader = new LoaderWithTimeout();
                    html = loader.LoadUrl(url, p);

                    _cache.Put(p, true);
                    done = true;
                }
                catch (Exception ex)
                {
                    _cache.Put(p, false);
                    Logger.LogError(ex, url);
                }
                /*catch (WebException ex)
                {
                    int timeout = 1000;
                    if (ex.Status == WebExceptionStatus.ProtocolError)
                    {
                        var response = ex.Response as HttpWebResponse;
                        if (response != null && 
                           (response.StatusCode == HttpStatusCode.ServiceUnavailable || response.StatusCode == HttpStatusCode.Forbidden))
                        {
                            timeout = 60000 * 45;
                        }
                       
                    }

                    Logger.LogError(ex, url);
                    Thread.Sleep(timeout);
                }*/
                
            }

            return html;
        }

        public static bool Test(string url, Proxy p)
        {
            try
            {
                var loader = new LoaderWithTimeout();
                var html = loader.LoadUrl(url, p);

                return true;
            }
            catch (Exception ex)
            {
                return false;
            }
        }

        private static void AddSettings(WebClient client, Proxy p)
        {
            var proxy = new WebProxy(p.GetUrl());
            client.Proxy = proxy;
            client.Encoding = Encoding.UTF8;
            client.Headers.Add("User-Agent: Other");
        }
    }
}
