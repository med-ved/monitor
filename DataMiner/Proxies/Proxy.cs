namespace DataMiner.Proxies
{
    using System;

    public class Proxy
    {
        public Guid Uid { get; set; }
        public string Url { get; set; }
        public int Port { get; set; }
        public DateTime? LastUsageTime { get; set; }
        public ProxyStatus Status { get; set; }
        public int Ping { get; set; }

        public string GetUrl()
        {
            return string.Format("{0}:{1}", Url, Port);
        }

        public void UpdateLastUsageTime()
        {
            LastUsageTime = Helpers.GetSpbCurrentTime();
        }
    }
}
