namespace airbnbmonitor.Code.Definitions
{
    using DataMiner.FlatParser;

    public class FlatInfo
    {
        public long Id { get; set; }
        public double? Latitude { get; set; }
        public double? Longitude { get; set; }
        public double Occupacy { get; set; }
        public double Revenue { get; set; }

        public FlatInfo(FlatData fd)
        {
            Id = fd.FlatId;
            Latitude = fd.Latitude;
            Longitude = fd.Longitude;
            Occupacy = fd.OccupacyPercent;
            Revenue = fd.EstimatedRevenue;
        }
    }
}