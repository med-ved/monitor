namespace airbnbmonitor.Code.Definitions
{
    using System.Collections.Generic;

    public class MonitoringData
    {
        public List<FlatInfo> FlatsInfoList { get; set; }
        public IEnumerable<GridPoint> FlatsGrid { get; set; }
        public FlatsSummary Summary { get; set; }
    }
}