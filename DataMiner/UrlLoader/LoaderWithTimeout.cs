namespace DataMiner.UrlLoader
{
    using System;
    using System.Net;
    using System.Text;
    using System.Threading;
    using DataMiner.Proxies;

    public class LoaderWithTimeout
    {
        private static readonly int Timeout = 25000;
        private DateTime _requestStartTime = DateTime.Now;

        private volatile Exception _exception;
        private LoaderResult _result { get; set; }

        private readonly ManualResetEvent mre = new ManualResetEvent(false);

        private void OnLoadComplete(object sender, DownloadStringCompletedEventArgs e)
        {
            var span = DateTime.Now - _requestStartTime;

            try
            {
                _result = new LoaderResult()
                {
                    Ping = Convert.ToInt32(span.TotalMilliseconds),
                    Error = e.Error,
                    Cancelled = e.Cancelled,
                    Result = e.Error == null ? e.Result : null
                };
            }
            catch (Exception ex)
            {
                _exception = ex;
            }
            finally
            {
                mre.Set();
            }
        }

        public LoaderResult LoadUrl(string url, Proxy p)
        {
            _requestStartTime = DateTime.Now;

            using (var client = new WebClient())
            {
                var proxy = new WebProxy(p.Url, p.Port);
                client.Proxy = proxy;
                client.Encoding = Encoding.UTF8;
                client.Headers.Add("User-Agent: Other");
                client.DownloadStringCompleted += OnLoadComplete;
                client.DownloadStringAsync(new Uri(url));

                if (!mre.WaitOne(Timeout))
                {
                    client.CancelAsync();
                    return new LoaderResult()
                    {
                        Ping = Timeout,
                        Cancelled = true,
                        Error = new Exception("Loader: TIMEOUT")
                    };
                }

                if (_exception != null)
                {
                    throw _exception;
                }

                return _result;
            }
        }
    }
}
