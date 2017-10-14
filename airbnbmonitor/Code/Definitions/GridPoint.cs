namespace airbnbmonitor.Code.Definitions
{
    public class GridPoint
    {
        public double? Latitude { get; set; }
        public double? Longitude { get; set; }
        public double? Occupancy { get; set; }
        public int Revenue { get; set; }

        public GridPoint(HeatMapGridPoint pt)
        {
            this.Latitude = pt.Latitude;
            this.Longitude = pt.Longitude;
            this.Revenue = (int)(pt.Sum / pt.Count);
            this.Occupancy = pt.OccupacySum / pt.Count;
        }
    }
}
