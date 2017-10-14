namespace airbnbmonitor.Code.Definitions
{
    public class HeatMapGridPoint
    {
        public double Latitude { get; set; }
        public double Longitude { get; set; }
        
        public double Sum { get; set; }
        public int Count { get; set; }
        public int Revenue { get; set; }
        public double OccupacySum { get; set; }
    }
}