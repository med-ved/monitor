namespace DataMiner.FlatParser
{
    using System;
    using System.Globalization;
    using System.Web.Script.Serialization;
    using CsQuery;
    using DataMiner.Logger;
    using DataMiner.UrlLoader;

    public class FlatReader
    {
        private static readonly string FlatUrl = "https://www.airbnb.ru/rooms/{0}?check_in={1:yyyy-MM-dd}&guests=1&check_out={2:yyyy-MM-dd}";
        private static readonly string FlatAvailabilityUrl = "https://www.airbnb.ru/api/v2/calendar_months?currency=RUB&locale=ru&listing_id={0}&month={1}&year={2}&count=3&_format=with_conditions&key={3}";
        private readonly ILogger _log;

        public FlatReader(ILogger log)
        {
            _log = log;
        }

        public FlatStatus CheckFlatStatus(FlatStatusRequest request)
        {
            string url = string.Format(FlatUrl, request.Id, request.Date, request.Date.AddDays(1));

            int n = 0;
            while (true)
            {
                string html = ObjectCreator.Get<Loader>().Load(url);
                if (html == null)
                {
                    return null;
                }

                CQ dom = html;
                var result = ParseResult(dom, request);
                var apiSettings = dom["#_bootstrap-layout-init"].Attr("content");
                if (apiSettings == null)
                {
                    n++;
                    if (n > 3)
                    {
                        _log.WriteLine("errors.txt", string.Format("Cant read AVAILABILITY of flat {0}", request.Id));
                        return null;
                    }

                    continue;
                }

                var serializer = new JavaScriptSerializer();
                dynamic apiSettingsObj = serializer.Deserialize<object>(apiSettings);
                string key = Helpers.GetIfExists(apiSettingsObj, new string[] { "api_config", "key" });

                var calendarData = GetCalendarMonthAvailability(request, key);
                result.Available = GetFlatAvailability(calendarData, request.Date);
                result.Price = GetFlatPrice(calendarData, request.Date);

                return result;
            } 
        }

        private dynamic GetCalendarMonthAvailability(FlatStatusRequest request, string key)
        {
            if (string.IsNullOrWhiteSpace(key))
            {
                return null;
            }

            string url = string.Format(FlatAvailabilityUrl, request.Id, request.Date.Month, request.Date.Year, key);

            string json = ObjectCreator.Get<Loader>().Load(url);
            if (json == null)
            {
                return null;
            }

            var serializer = new JavaScriptSerializer();
            return serializer.Deserialize<object>(json);
        }

        private int? GetFlatPrice(dynamic availbility, DateTime date)
        {
            var dateData = GetCalendarDateData(availbility, date);
            return Helpers.GetIfExists(dateData, new string[] { "price", "local_price" });
        }

        private bool? GetFlatAvailability(dynamic availbility, DateTime date)
        {
            var dateData = GetCalendarDateData(availbility, date);
            return Helpers.GetIfExists(dateData, "available");
        }

        private dynamic GetCalendarDateData(dynamic availbility, DateTime date)
        {
            var monthes = Helpers.GetIfExists(availbility, "calendar_months");
            if (monthes == null)
            {
                return null;
            }

            foreach (var month in monthes)
            {
                var monthNumber = Helpers.GetIfExists(month, "month");
                if (monthNumber == null)
                {
                    continue;
                }

                if (monthNumber == date.Month)
                {
                    var days = Helpers.GetIfExists(month, "days");
                    if (days == null)
                    {
                        continue;
                    }

                    foreach (var day in days)
                    {
                        var dateStr = Helpers.GetIfExists(day, "date");
                        if (dateStr == null)
                        {
                            continue;
                        }

                        var dt = DateTime.Parse(dateStr);
                        if (dt.Date == date.Date)
                        {
                            return day;
                        }
                    }
                }
            }

            return null;
        }

        private FlatStatus ParseResult(CQ dom, FlatStatusRequest request)
        {
            var result = new FlatStatus();

            var json = GetStringFromDom(dom, "script[data-hypernova-key*='p3show_marketplacebundlejs']");
            json = json.Substring(4, json.Length - 7);
            var serializer = new JavaScriptSerializer();
            dynamic settings = serializer.Deserialize<object>(json);
            var items = Helpers.GetIfExists(settings, new[] { "bootstrapData", "reduxData", "marketplacePdp", "listingInfo", "listing" });

            result.Date = request.Date;
            result.Available = null;

            result.Flat = new Flat();
            result.Flat.Id = request.Id;
            result.Flat.Latitude = GetDouble(items, "lat");
            result.Flat.Longitude = GetDouble(items, "lng");

            result.Flat.Country = request.Country;
            result.Flat.City = request.City;
            result.Flat.Rating = GetInt(items, "star_rating");

            result.Flat.MaxGuests = GetInt(items, "person_capacity");
            result.Flat.BedsCount = GetInt(items, "beds");
            result.Flat.BathroomsCount = GetBathroomsCount(items);
            result.Flat.BedroomsCount = GetInt(items, "bedrooms");
            result.Flat.RoomType = GetString(items, "room_and_property_type");

            result.Flat.ShortDescription = GetShortDescription(items);
            result.Flat.Description = GetFullDescription(items);
            result.Flat.Facilities = new FlatFacilities();

            return result;
        }

        private int? GetBathroomsCount(dynamic obj)
        {
            string str = GetString(obj, "bathroom_label");
            if (string.IsNullOrWhiteSpace(str))
            {
                return null;
            }

            var parts = str.Split(' ');
            if (parts.Length < 1)
            {
                return null;
            }

            int result;
            if (int.TryParse(parts[0], out result))
            {
                return result;
            }

            return null;
        }

        private FlatDescription GetFullDescription(dynamic settings)
        {
            var result = new FlatDescription();
            var description = Helpers.GetIfExists(settings, "sectioned_description");
            if (description == null)
            {
                result.HouseRules = GetString(settings, "house_rules");
                result.Description = GetString(settings, "description");
                return result;
            }

            result.Access = GetString(description, "access");
            result.Description = GetString(description, "description");
            result.HouseRules = GetString(description, "house_rules");
            result.Interaction = GetString(description, "interaction");
            result.Locale = GetString(description, "locale");
            result.Name = GetString(description, "name");
            result.NeighborhoodOverview = GetString(description, "neighborhood_overview");
            result.Notes = GetString(description, "notes");
            result.Space = GetString(description, "space");
            result.Summary = GetString(description, "summary");
            result.Transit = GetString(description, "transit");
            return result;
        }

        private FlatFacilities GetFacilities(dynamic settings)
        {
            var result = new FlatFacilities();
            var items = Helpers.GetIfExists(settings, new[] { "listing", "listing_amenities" });

            result.Kitchen = GetFacilitieesItem(items, "kitchen");
            result.Internet = GetFacilitieesItem(items, "internet");
            result.TV = GetFacilitieesItem(items, "tv");
            result.ToiletAccessories = GetFacilitieesItem(items, "essentials");
            result.Heating = GetFacilitieesItem(items, "heating");
            result.AirConditioner = GetFacilitieesItem(items, "ac");
            result.WashingMashine = GetFacilitieesItem(items, "washer");
            result.Dryer = GetFacilitieesItem(items, "dryer");
            result.FreeParking = GetFacilitieesItem(items, "free_parking");
            result.WirelessInternet = GetFacilitieesItem(items, "wireless_internet");
            result.CableTv = GetFacilitieesItem(items, "cable");
            result.Breakfast = GetFacilitieesItem(items, "breakfast");
            result.PetsAllowed = GetFacilitieesItem(items, "allows_pets");
            result.FamilyFriendly = GetFacilitieesItem(items, "family_friendly");
            result.EventsFriendly = GetFacilitieesItem(items, "event_friendly");
            result.SmokingAllowed = GetFacilitieesItem(items, "allows_smoking");
            result.PeopleWithLimitedAbilities = GetFacilitieesItem(items, "wheelchair_accessible");
            result.Fireplace = GetFacilitieesItem(items, "fireplace");
            result.Intercom = GetFacilitieesItem(items, "buzzer");
            result.Porter = GetFacilitieesItem(items, "doorman");
            result.SwimingPoll = GetFacilitieesItem(items, "pool");
            result.Jacuzzi = GetFacilitieesItem(items, "jacuzzi");
            result.Gym = GetFacilitieesItem(items, "gym");
            result.Shoulder = GetFacilitieesItem(items, "hangers");
            result.Iron = GetFacilitieesItem(items, "iron");
            result.Hairdryer = GetFacilitieesItem(items, "hair-dryer");
            result.NotebookWorkingPlace = GetFacilitieesItem(items, "laptop-friendly");
            result.Lift = GetFacilitieesItem(items, "elevator");

            return result;
        }

        private bool? GetFacilitieesItem(dynamic items, string name)
        {
            if (items == null)
            {
                return null;
            }

            foreach (var item in items)
            {
                if (item["tag"] == name)
                {
                    return item["is_present"];
                }
            }

            return null;
        }

        private FlatShortDescription GetShortDescription(dynamic settings)
        {
            var result = new FlatShortDescription();
            var items = Helpers.GetIfExists(settings, new[] { "p3_event_data_logging" });

            result.Cleanly = GetDouble(items, "cleanliness_rating");
            result.Communication = GetDouble(items, "communication_rating");
            result.Location = GetDouble(items, "location_rating");
            result.MatchDescription = GetDouble(items, "accuracy_rating");
            result.PriceQualityRation = GetDouble(items, "value_rating");
            result.Settlement = GetDouble(items, "checkin_rating");
            return result;
        }

        private double? GetDouble(dynamic obj, string field)
        {
            var str = GetString(obj, field);
            if (string.IsNullOrWhiteSpace(str))
            {
                return null;
            }

            return GetDouble(str, 0);
        }

        private int? GetInt(dynamic obj, string field)
        {
            var str = GetString(obj, field);
            if (string.IsNullOrWhiteSpace(str))
            {
                return null;
            }

            int result;
            if (int.TryParse(str, out result))
            {
                return result;
            }

            return null;
        }

        public static double GetDouble(string value, double defaultValue)
        {
            double result;

            if (!double.TryParse(value, NumberStyles.Any, CultureInfo.CurrentCulture, out result) &&
                !double.TryParse(value, NumberStyles.Any, CultureInfo.GetCultureInfo("en-US"), out result) &&
                !double.TryParse(value, NumberStyles.Any, CultureInfo.InvariantCulture, out result))
            {
                result = defaultValue;
            }

            return result;
        }

        private string GetStringFromDom(CQ dom, string selector, string attr = null)
        {
            var el = dom[selector];
            if (!string.IsNullOrWhiteSpace(attr))
            {
                return el.Attr(attr);
            }
            else
            {
                return el.Text();
            }
        }

        private string GetString(dynamic obj, string field)
        {
            var el = Helpers.GetIfExists(obj, field);
            if (el == null)
            {
                return null;
            }

            return Convert.ToString(el);
        }
    }
}
