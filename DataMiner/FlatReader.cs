﻿using System;
using System.Collections.Generic;
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
            //Task.Run(() => MethodWithParameter(param));
            Console.WriteLine("Reading: " + request);
            /*Task<FlatStatus> task = Task<FlatStatus>.Factory.StartNew(() => this.FlatStatusThread(request));
            task.Wait();
            FlatStatus result = task.Result;*/
            var result = this.FlatStatusThread(request);
            return result;
        }

        private FlatStatus FlatStatusThread(FlatStatusRequest request)
        {
            string url = String.Format("https://www.airbnb.ru/rooms/{0}?check_in={1:yyyy-MM-dd}&guests=1&check_out={2:yyyy-MM-dd}",
                request.Id, request.Date, request.Date.AddDays(1));
            string html = null;

            html = Loader.Load(url);
            if (html == null)
            {
                return null;
            }

            CQ dom = html;
            var result = ParseResult(dom, request);

            
            var apiSettings = dom["#_bootstrap-layout-init"].Attr("content");
            var serializer = new JavaScriptSerializer();
            dynamic apiSettingsObj = serializer.Deserialize<object>(apiSettings);
            string key = apiSettingsObj["api_config"]["key"];

            result.Available = GetFlatAvailability(request, dom, key);
            return result;
        }

        private bool? GetFlatAvailability(FlatStatusRequest request, CQ dom, string key)
        {
            string url = String.Format("https://www.airbnb.ru/api/v2/calendar_months?currency=RUB&locale=ru&listing_id={0}&month={1}&year={2}&count=3&_format=with_conditions&key={3}",
                request.Id, request.Date.Month, request.Date.Year, key);

            string json = Loader.Load(url);
            if (json == null)
            {
                return null;
            }

            var serializer = new JavaScriptSerializer();
            dynamic availbility = serializer.Deserialize<object>(json);
            var monthes = availbility["calendar_months"];
            foreach (var month in monthes)
            {
                if (month["month"] == request.Date.Month)
                {
                    foreach (var day in month["days"])
                    {
                        DateTime date = DateTime.Parse(day["date"]);
                        if (date.Date == request.Date.Date)
                        {
                            return day["available"];
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

            string descriptionBrief = dom["span:contains('Об этом жилье')"].Parent().Next().Text();
            string descriptionFull = GetString(dom, "#description");
            result.Flat.Description = descriptionBrief + "  \n  " + descriptionFull;
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
            var serializer = new JavaScriptSerializer();
            dynamic settings = serializer.Deserialize<object>(json);

            result.Flat.ShortDescription = GetShortDescription(dom, settings);
            result.Flat.Facilities = GetFacilities(dom, settings);

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

            return settings["pricing_quote"]["rate"]["amount"];
        }

        private FlatFacilities GetFacilities(CQ dom, dynamic settings)
        {
            var result = new FlatFacilities();
            var items = settings["listing"]["listing_amenities"];

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
            var items = settings["listing"]["review_details_interface"]["review_summary"];

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

            str = str.Replace(".", ",");
            double result;
            if (double.TryParse(str, out result))
            {
                return result;
            }

            return null;
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
