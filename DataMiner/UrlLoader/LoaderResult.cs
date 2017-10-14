namespace DataMiner.UrlLoader
{
    using System;

    public class LoaderResult
    {
        public Exception Error { get; set; }
        public string Result { get; set; }
        public bool Cancelled { get; set; }
        public int Ping { get; set; }
    }
}
