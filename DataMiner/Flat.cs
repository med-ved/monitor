using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DataMiner
{
    public class Flat
    {
        public int Id { get; set; }
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
    }

    public class FlatFacilities
    {
        public bool? Kitchen { get; set; }
        public bool? Internet { get; set; }
        public bool? TV { get; set; }
        public bool? ToiletAccessories { get; set; }
        public bool? Heating { get; set; }
        public bool? AirConditioner { get; set; }
        public bool? WashingMashine { get; set; }
        public bool? Dryer { get; set; }
        public bool? FreeParking { get; set; }
        public bool? WirelessInternet { get; set; }
        public bool? CableTv { get; set; }
        public bool? Breakfast { get; set; }
        public bool? PetsAllowed { get; set; }
        public bool? FamilyFriendly { get; set; }
        public bool? EventsFriendly { get; set; }
        public bool? SmokingAllowed { get; set; }
        public bool? PeopleWithLimitedAbilities { get; set; }
        public bool? Fireplace { get; set; }
        public bool? Intercom { get; set; }
        public bool? Porter { get; set; }
        public bool? SwimingPoll { get; set; }
        public bool? Jacuzzi { get; set; }
        public bool? Gym { get; set; }
        public bool? Shoulder { get; set; }
        public bool? Iron { get; set; }
        public bool? Hairdryer { get; set; }
        public bool? NotebookWorkingPlace { get; set; }
        public bool? Lift { get; set; }
    }

    public class FlatShortDescription
    {
        public double? MatchDescription { get; set; }
        public double? Communication { get; set; }
        public double? Cleanly { get; set; }
        public double? Location { get; set; }
        public double? Settlement { get; set; }
        public double? PriceQualityRation { get; set; }
    }

    public class FlatDescription
    {
        public string Access { get; set; }
        public string Description { get; set; }
        public string HouseRules { get; set; }
        public string Interaction { get; set; }
        public string Locale { get; set; }
        public string Name { get; set; }
        public string NeighborhoodOverview { get; set; }
        public string Notes { get; set; }
        public string Space { get; set; }
        public string Summary { get; set; }
        public string Transit { get; set; }
    }

    public class FlatStatus
    {
        public Flat Flat { get; set; }
        
        public DateTime Date { get; set; }
        public int? Price { get; set; }
        public bool? Available { get; set; }
    }
}
