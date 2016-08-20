using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DataMiner
{
    public class LatestDateInfo
    {
        public DateTime? Date { get; set; }
        public bool? Finished { get; set; }
    }

    public static class Database
    {
        public static void Save(FlatStatus status)
        {
            using (var context = new MonitorEntities())
            {
                var flat = context.Flats.Where(f => f.Id == status.Flat.Id).FirstOrDefault<Flats>();
                if (flat == null)
                {
                    flat = AddNewFlat(context, status);
                }

                var newStatus = new FlatStatuses()
                {
                    Available = status.Available,
                    Date = status.Date,
                    Price = (short?)status.Price.Get(),
                    FlatId = flat.Id,
                    Flats = flat
                };
                context.FlatStatuses.Add(newStatus);

                context.SaveChanges();
            }
        }

        public static Flats AddNewFlat(MonitorEntities context, FlatStatus status)
        {
            var newFlat = new Flats()
            {
                Id = status.Flat.Id,
                Latitude = (float?)status.Flat.Latitude.Get(),
                Longitude = (float?)status.Flat.Longitude.Get(),
                Country = status.Flat.Country,
                City = status.Flat.City,
                Rating = (float?)status.Flat.Rating.Get(),
                MaxGuests = (int?)status.Flat.MaxGuests.Get(),
                BedsCount = (int?)status.Flat.BedsCount.Get(),
                BathroomsCount = (int?)status.Flat.BathroomsCount.Get(),
                BedroomsCount = (int?)status.Flat.BedroomsCount.Get(),
                RoomType = status.Flat.RoomType,

                MatchDescription = (float?)status.Flat.ShortDescription.MatchDescription.Get(),
                Communication = (float?)status.Flat.ShortDescription.Communication.Get(),
                Cleanly = (float?)status.Flat.ShortDescription.Cleanly.Get(),
                Location = (float?)status.Flat.ShortDescription.Location.Get(),
                Settlement = (float?)status.Flat.ShortDescription.Settlement.Get(),
                PriceQualityRation = (float?)status.Flat.ShortDescription.PriceQualityRation.Get(),

                Description = GetFlatDescription(status),
                Facilities = GetFlatFacilities(status)
            };
            context.Flats.Add(newFlat);

            return newFlat;
        }

        public static Description GetFlatDescription(FlatStatus status)
        {
            var description = new Description();
            description.Access = status.Flat.Description.Access;
            description.Description1 = status.Flat.Description.Description;
            description.HouseRules = status.Flat.Description.HouseRules;
            description.Interaction = status.Flat.Description.Interaction;
            description.Locale = status.Flat.Description.Locale;
            description.Name = status.Flat.Description.Name;
            description.NeighborhoodOverview = status.Flat.Description.NeighborhoodOverview;
            description.Notes = status.Flat.Description.Notes;
            description.Space = status.Flat.Description.Space;
            description.Summary = status.Flat.Description.Summary;
            description.Transit = status.Flat.Description.Transit;

            return description;
        }

        public static Facilities GetFlatFacilities(FlatStatus status)
        {
            var facilities = new Facilities();
            facilities.AirConditioner = status.Flat.Facilities.AirConditioner;
            facilities.Breakfast = status.Flat.Facilities.Breakfast;
            facilities.CableTv = status.Flat.Facilities.CableTv;
            facilities.Dryer = status.Flat.Facilities.Dryer;
            facilities.EventsFriendly = status.Flat.Facilities.EventsFriendly;
            facilities.FamilyFriendly = status.Flat.Facilities.FamilyFriendly;
            facilities.Fireplace = status.Flat.Facilities.Fireplace;
            facilities.FreeParking = status.Flat.Facilities.FreeParking;
            facilities.Gym = status.Flat.Facilities.Gym;
            facilities.Hairdryer = status.Flat.Facilities.Hairdryer;
            facilities.Heating = status.Flat.Facilities.Heating;
            facilities.Intercom = status.Flat.Facilities.Intercom;
            facilities.Internet = status.Flat.Facilities.Internet;
            facilities.Iron = status.Flat.Facilities.Iron;
            facilities.Jacuzzi = status.Flat.Facilities.Jacuzzi;
            facilities.Kitchen = status.Flat.Facilities.Kitchen;
            facilities.Lift = status.Flat.Facilities.Lift;
            facilities.NotebookWorkingPlace = status.Flat.Facilities.NotebookWorkingPlace;
            facilities.PeopleWithLimitedAbilities = status.Flat.Facilities.PeopleWithLimitedAbilities;
            facilities.PetsAllowed = status.Flat.Facilities.PetsAllowed;
            facilities.Porter = status.Flat.Facilities.Porter;
            facilities.Shoulder = status.Flat.Facilities.Shoulder;
            facilities.SmokingAllowed = status.Flat.Facilities.SmokingAllowed;
            facilities.SwimingPoll = status.Flat.Facilities.SwimingPoll;
            facilities.ToiletAccessories = status.Flat.Facilities.ToiletAccessories;
            facilities.TV = status.Flat.Facilities.TV;
            facilities.WashingMashine = status.Flat.Facilities.WashingMashine;
            facilities.WirelessInternet = status.Flat.Facilities.WirelessInternet;

            return facilities;
        }

        public static bool IsFlatProcessed(long flatId, DateTime date)
        {
            using (var context = new MonitorEntities())
            {
                return context.FlatStatuses.Any(s => s.FlatId.Value == flatId && s.Date == date); 
            }
        }

        public static T? Get<T>(this Nullable<T> t) where T : struct
        {
            if (t.HasValue)
            {
                return t.Value;
            }

            return null;
        }

        public static LatestDateInfo GetLatestProcessedDateInfo()
        {
            using (var context = new MonitorEntities())
            {
                var settings = context.Settings.FirstOrDefault();
                if (settings == null)
                {
                    return null;
                }

                var info = new LatestDateInfo
                {
                    Date = settings.LatestDay,
                    Finished = settings.Finished
                };

                return info;
            }
        }

        public static void SetLatestProcessedDate(DateTime date, bool finished)
        {
            using (var context = new MonitorEntities())
            {
                var settings = context.Settings.FirstOrDefault();
                if (settings == null)
                {
                    var newSettings = new Settings()
                    {
                        Id = 1,
                        LatestDay = date,
                        Finished = finished
                    };

                    context.Settings.Add(newSettings);
                }
                else
                {
                    settings.Finished = finished;
                    settings.LatestDay = date;
                }
                
                context.SaveChanges();
            }
        }
    }
}
