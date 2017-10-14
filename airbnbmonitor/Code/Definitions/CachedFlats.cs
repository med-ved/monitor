namespace airbnbmonitor.Code.Definitions
{
    using System.Collections.Generic;
    using DataMiner.FlatParser;

    public class CachedFlats
    {
        public IEnumerable<FlatData> FlatsData { get; set; }
        public IDictionary<long, FlatData> FlatsDataDict { get; set; }
        public IDictionary<long, Flat> Flats { get; set; }
    }
}