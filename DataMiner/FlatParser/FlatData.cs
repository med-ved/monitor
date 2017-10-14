namespace DataMiner.FlatParser
{
    using System;
    using System.Collections.Generic;

    public class FlatData
    {
        public long FlatId { get; set; }
        public Flat Flat { get; set; }
        public double? Latitude { get; set; }
        public double? Longitude { get; set; }

        public List<FlatMonthlyData> MonthlyData { get; set; }
        public IDictionary<DateTime, FlatMonthlyData> MonthlyDataDict { get; set; }
        public double OccupacyPercent { get; set; }
        public double RevenuePerDay { get; set; }
        public double EstimatedRevenue { get; set; }
    }
}
