namespace DataMiner.FlatParser
{
    using DataMiner.Database;

    public class Flat
    {
        public long Id { get; set; }
        public double? Latitude { get; set; }
        public double? Longitude { get; set; }
        public string Country { get; set; }
        public string City { get; set; }
        public double? Rating { get; set; }

        public int? MaxGuests { get; set; }
        public int? BedsCount { get; set; }
        public int? BathroomsCount { get; set; }
        public int? BedroomsCount { get; set; }
        public string RoomType { get; set; }

        public FlatDescription Description { get; set; }
        public FlatShortDescription ShortDescription { get; set; }
        public FlatFacilities Facilities { get; set; }

        public Flat()
        {
        }

        public Flat(Flats efFlat)
        {
            Id = efFlat.Id;
            Latitude = efFlat.Latitude; 
            Longitude = efFlat.Longitude;
            Country = efFlat.Country;
            City = efFlat.City;
            Rating = efFlat.Rating;

            MaxGuests = efFlat.MaxGuests;
            BedsCount = efFlat.BedsCount;
            BathroomsCount = efFlat.BathroomsCount;
            BedroomsCount = efFlat.BedroomsCount;
            RoomType = efFlat.RoomType;

            ShortDescription = null; 
            Description = null;
            Facilities = null;
        }
    }
}
