using System;
using System.Collections.Generic;
using System.Dynamic;
using System.Globalization;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using System.Web.Script.Serialization;
using CsQuery;

namespace DataMiner
{
    public class FlatStatusRequest
    {
        public int Id { get; set; }
        public DateTime Date { get; set; }
        public string Country { get; set; }
        public string City { get; set; }

        public override string ToString()
        {
            return String.Format("Date={0}, Flat Id={1}, Country={2}, City={3}", Date, Id, Country, City);
        }
    }

    public class FlatReader
    {
        public FlatReader()
        {
        }

        public FlatStatus CheckFlatStatus(FlatStatusRequest request)
        {
            Logger.Log("Reading: " + request);
            var result = FlatStatusThread(request);
            return result;
        }

        private FlatStatus FlatStatusThread(FlatStatusRequest request)
        {
            string url = String.Format("https://www.airbnb.ru/rooms/{0}?check_in={1:yyyy-MM-dd}&guests=1&check_out={2:yyyy-MM-dd}",
                request.Id, request.Date, request.Date.AddDays(1));

            int n = 0;
            while(true)
            {
                string html = null;

                html = Loader.Load(url);
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
                        Logger.WriteLine("errors.txt", string.Format("Cant read AVAILABILITY of flat {0}", request.Id));
                        return null;
                    }
                    continue;
                }

                var serializer = new JavaScriptSerializer();
                dynamic apiSettingsObj = serializer.Deserialize<object>(apiSettings);
                string key = Helpers.GetIfExists(apiSettingsObj, new string[] { "api_config", "key" });

                result.Available = GetFlatAvailability(request, dom, key);
                return result;
            } 
            
        }

        private bool? GetFlatAvailability(FlatStatusRequest request, CQ dom, string key)
        {
            if (string.IsNullOrWhiteSpace(key))
            {
                return null;
            }

            string url = String.Format("https://www.airbnb.ru/api/v2/calendar_months?currency=RUB&locale=ru&listing_id={0}&month={1}&year={2}&count=3&_format=with_conditions&key={3}",
                request.Id, request.Date.Month, request.Date.Year, key);

            string json = Loader.Load(url);
            if (json == null)
            {
                return null;
            }

            var serializer = new JavaScriptSerializer();
            dynamic availbility = serializer.Deserialize<object>(json);
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

                if (monthNumber == request.Date.Month)
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

                        DateTime date = DateTime.Parse(dateStr);
                        if (date.Date == request.Date.Date)
                        {
                            return Helpers.GetIfExists(day, "available");
                        }
                    }
                }
            }

            return null;
        }

        private FlatStatus ParseResult(CQ dom, FlatStatusRequest request)
        {
            var result = new FlatStatus();

            result.Price = GetPrice(dom); 
            result.Date = request.Date;
            result.Available = null;

            result.Flat = new Flat();
            result.Flat.Id = request.Id;
            result.Flat.Latitude = GetDouble(dom, "meta[property*=latitude]", "content");
            result.Flat.Longitude = GetDouble(dom, "meta[property*=longitude]", "content");

            result.Flat.Country = request.Country;
            result.Flat.City = request.City;
            result.Flat.Rating = GetDouble(dom, "#display-address .star-rating", "content");

            result.Flat.MaxGuests = GetInt(dom,"strong[data-reactid*='$Вмещает гостей=2.2']");
            result.Flat.BedsCount = GetInt(dom, "strong[data-reactid*='Кровать=2.2']");
            if (result.Flat.BedsCount == null)
            {
                result.Flat.BedsCount = GetInt(dom, "strong[data-reactid*='$Кроватей=2.2']");
            }

            result.Flat.BathroomsCount = GetInt(dom, "strong[data-reactid*='$Количество ванных комнат=2.2']");
            result.Flat.BedroomsCount = GetInt(dom, "strong[data-reactid*='$Количество спален=2.2']");
            result.Flat.RoomType = GetString(dom, "strong[data-reactid*='$Тип размещения=2.0.2']");

            var json = dom["#_bootstrap-listing"].Attr("content");
            if (json == null)
            {
                return result;
            }

            var serializer = new JavaScriptSerializer();
            dynamic settings = serializer.Deserialize<object>(json);

            result.Flat.ShortDescription = GetShortDescription(dom, settings);
            result.Flat.Facilities = GetFacilities(dom, settings);
            result.Flat.Description = GetFullDescription(settings);
            return result;
        }

        private FlatDescription GetFullDescription(dynamic settings)
        {
            var result = new FlatDescription();
            var listing = Helpers.GetIfExists(settings, "listing");
            var description = Helpers.GetIfExists(listing, "localized_sectioned_description");
            if (description == null)
            {
                result.HouseRules = Helpers.GetIfExists(listing, "localized_additional_house_rules");
                result.Description = Helpers.GetIfExists(listing, "localized_description");
                return result;
            }

            result.Access = Helpers.GetIfExists(description, "access");
            result.Description = Helpers.GetIfExists(description, "description");
            result.HouseRules = Helpers.GetIfExists(description, "house_rules");
            result.Interaction = Helpers.GetIfExists(description, "interaction");
            result.Locale = Helpers.GetIfExists(description, "locale");
            result.Name = Helpers.GetIfExists(description, "name");
            result.NeighborhoodOverview = Helpers.GetIfExists(description, "neighborhood_overview");
            result.Notes = Helpers.GetIfExists(description, "notes");
            result.Space = Helpers.GetIfExists(description, "space");
            result.Summary = Helpers.GetIfExists(description, "summary");
            result.Transit = Helpers.GetIfExists(description, "transit");

            return result;
        }

        private int? GetPrice(CQ dom)
        {
            var json = dom["script[data-hypernova-key='p3book_itbundlejs']"].Text();
            if (json.Length < 8)
            {
                return null;
            }

            json = json.Substring(4, json.Length - 7);
            var serializer = new JavaScriptSerializer();
            dynamic settings = serializer.Deserialize<object>(json);
            return Helpers.GetIfExists(settings, new[] { "pricing_quote", "rate", "amount" });
        }

        private FlatFacilities GetFacilities(CQ dom, dynamic settings)
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

        private FlatShortDescription GetShortDescription(CQ dom, dynamic settings)
        {
            var result = new FlatShortDescription();
            var items = Helpers.GetIfExists(settings, new[] { "listing", "review_details_interface", "review_summary" });

            result.Cleanly = GetShortDescriptionItem(items, "Чистота");
            result.Communication = GetShortDescriptionItem(items, "Общение");
            result.Location = GetShortDescriptionItem(items, "Расположение");
            result.MatchDescription = GetShortDescriptionItem(items, "Соответствие описанию");
            result.PriceQualityRation = GetShortDescriptionItem(items, "Общение");
            result.Settlement = GetShortDescriptionItem(items, "Цена/качество");

            return result;
        }

        private double? GetShortDescriptionItem(dynamic items, string name)
        {
            if (items == null)
            {
                return null;
            }

            foreach(var item in items)
            {
                if (item["label"] == name)
                {
                    return item["value"];
                }
            }

            return null;
        }

        private double? GetDouble(CQ dom, string selector, string attr = null)
        {
            var str = GetString(dom, selector, attr);
            if (string.IsNullOrWhiteSpace(str))
            {
                return null;
            }

            return GetDouble(str, 0);
        }

        public static double GetDouble(string value, double defaultValue)
        {
            double result;

            //Try parsing in the current culture
            if (!double.TryParse(value, NumberStyles.Any, CultureInfo.CurrentCulture, out result) &&
                //Then try in US english
                !double.TryParse(value, NumberStyles.Any, CultureInfo.GetCultureInfo("en-US"), out result) &&
                //Then in neutral language
                !double.TryParse(value, NumberStyles.Any, CultureInfo.InvariantCulture, out result))
            {
                result = defaultValue;
            }

            return result;
        }

        private int? GetInt(CQ dom, string selector, string attr = null)
        {
            var str = GetString(dom, selector, attr);
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

        private string GetString(CQ dom, string selector, string attr=null)
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
    }
}
