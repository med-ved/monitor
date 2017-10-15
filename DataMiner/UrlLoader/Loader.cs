namespace DataMiner.UrlLoader
{
    using System;
    using System.Net;
    using System.Threading;
    using DataMiner.Database;
    using DataMiner.Logger;
    using DataMiner.Proxies;

    public class Loader
    {
        private static ProxyCache _cache = new ProxyCache();
        private readonly IDatabase _db;
        private readonly ILogger _log;

        public static void Init(IDatabase db, ILogger log)
        {
            _cache.Init(db, log);
        }

        public Loader(IDatabase db, ILogger log)
        {
            _db = db;
            _log = log;
        }

        public string Load(string url)
        {
            while (true)
            {
                var p = _cache.Get();
                if (p == null)
                {
                    //all proxies are busy
                    Thread.Sleep(1000);
                    continue;
                }

                try
                {
                    var loader = ObjectCreator.Get<LoaderWithTimeout>();
                    var result = loader.LoadUrl(url, p);
                    UpdateStatistics(result, p.Uid);
                    if (result.Error != null)
                    {
                        throw result.Error;
                    }

                    _cache.Put(p, true);
                    return result.Result;
                }
                catch (Exception ex)
                {
                    _cache.Put(p, false);
                    _log.LogError(ex, url);
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
        }

        public bool Test(string url, Proxy p)
        {
            try
            {
                var loader = ObjectCreator.Get<LoaderWithTimeout>();
                var result = loader.LoadUrl(url, p);

                return !result.Cancelled && result.Error == null;
            }
            catch (Exception)
            {
                return false;
            }
        }

        private void UpdateStatistics(LoaderResult result, Guid proxyGuid)
        {
            if (result.Cancelled)
            {
                _db.UpdateProxyUsageStatistic(proxyGuid, result.Ping, timeout: true);
                return;
            }

            if (result.Error != null)
            {
                UpdateUsageSatisticIfError(result, proxyGuid);
                return;    
            }
               
            _db.UpdateProxyUsageStatistic(proxyGuid, result.Ping, success: true);
        }

        private bool IsForbidden(Exception e)
        {
            var ex = e as WebException;
            if (ex == null)
            {
                return false;
            }

            if (ex.Status != WebExceptionStatus.ProtocolError)
            {
                return false;
            }

            var response = ex.Response as HttpWebResponse;
            return response != null &&
                (response.StatusCode == HttpStatusCode.ServiceUnavailable || response.StatusCode == HttpStatusCode.Forbidden);
        }

        private void UpdateUsageSatisticIfError(LoaderResult result, Guid proxyGuid)
        {
            var forbidden = IsForbidden(result.Error);
            _db.UpdateProxyUsageStatistic(proxyGuid, result.Ping, forbidden403: forbidden, fail: !forbidden);
        }
    }
}
