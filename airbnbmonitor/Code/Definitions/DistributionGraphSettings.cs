namespace airbnbmonitor.Code.Definitions
{
    using System;

    public class DistributionGraphSettings
    {
        public DateTime StartTime { get; set; }
        public DateTime EndTime { get; set; }
        public double Min { get; set; }
        public double Max { get; set; }
        public int Steps { get; set; }
        public double StepValue { get { return (Max - Min) / Steps; } }

        public Func<FlatMonthlySummary, double> GetDistributionField { get; set; }
    }
}