namespace airbnbmonitor.Code.Definitions
{
    using System.Collections.Generic;

    public class FlatGraphData
    {
        public IEnumerable<FlatMonthlySummary> LineChartData { get; set; }
        public FlatMonthlySummary Summary { get; set; }
        public DistributionGraph DistributionByRevenue { get; set; }
        public DistributionGraph DistributionByOccupacy { get; set; }
    }
}