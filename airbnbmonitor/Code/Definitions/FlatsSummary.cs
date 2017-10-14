namespace airbnbmonitor.Code.Definitions
{
    using System;
    using DataMiner.FlatParser;

    public class FlatMonthlySummary
    {
        public DateTime Date { get; set; }
        public double AvgOccupacyPercent { get; set; }
        public double AvgRevenuePerDay { get; set; }
        public double AvgEstimatedMonthlyRevenue { get; set; }
    }

    public class FlatsSummary
    {
        public int FlatsCount { get; set; }
        public double AvgOccupacyPercent { get; set; }
        public double AvgRevenuePerDay { get; set; }
        public double AvgEstimatedMonthlyRevenue { get; set; }
        public FlatGraphData GraphData { get; set; }
    }
    
    public class SingleFlatSummary
    {
        public Flat Flat { get; set; }
        public FlatsSummary Summary { get; set; }
    }
}