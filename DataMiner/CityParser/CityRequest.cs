namespace DataMiner.CityParser
{
    using System;

    public class CityRequest
    {
        public string Name { get; set; }
        public string Country { get; set; }
        public DateTime Date { get; set; }
        public string FlatType { get; set; }

        public double StartLongitude { get; set; }
        public double EndLongitude { get; set; }
        public double StartLatitude { get; set; }
        public double EndLatitude { get; set; }

        public double CenterStartLongitude { get; set; }
        public double CenterEndLongitude { get; set; }
        public double CenterStartLatitude { get; set; }
        public double CenterEndLatitude { get; set; }

        public CityRequest Copy()
        {
            var copy = new CityRequest()
            {
                Name = this.Name,
                Country = this.Country,
                Date = this.Date,
                FlatType = this.FlatType,
                StartLongitude = this.StartLongitude,
                EndLongitude = this.EndLongitude,
                StartLatitude = this.StartLatitude,
                EndLatitude = this.EndLatitude
            };

            return copy;
        }

        public override string ToString()
        {
            return string.Format("Date={0}, Country={1}, City={2}, FlatType={3}", Date, Country, Name, FlatType);
        }
    }
}
