using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Web.Script.Serialization;
using CsQuery;

namespace DataMiner
{
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

        public override string ToString()
        {
            return String.Format("Date={0}, Country={1}, City={2}, FlatType={3}", Date, Country, Name, FlatType);
        }
    }

    public class CityReader
    {
        private int FlatNumber = 0;
        private int QadrantNumber = 0;
        private int DuplicateChecks = 0;
        private static int MaxFlats = 300;
        private static string[] FlatTypes = new string[] { "Entire+home%2Fapt", "Private+room", "Shared+room" };

        private object locker = new object();
        private bool _stop = false;
        public bool Stop
        {
            get
            {
                lock (locker)
                {
                    return _stop;
                }
                
            }
            set
            {
                lock (locker)
                {
                    _stop = value;
                }
            }
        }

        public bool CheckCityStatus(CityRequest r)
        {
            Logger.Log("Reading city: " + r);
            string url = String.Format("https://www.airbnb.ru/s/{0}--{1}?zoom=10&sw_lat={2}&sw_lng={3}&ne_lat={4}&ne_lng={5}", 
                r.Name, r.Country, r.StartLatitude.ToGBString(), r.StartLongitude.ToGBString(), r.EndLatitude.ToGBString(), r.EndLongitude.ToGBString()); //Saint-Petersburg--Russia
            var html = Loader.Load(url);

            CQ dom = html;
            //var minPrice = dom[".price-range-slider"].Attr("data-min-price-daily");
            //var maxPrice = dom[".price-range-slider"].Attr("data-max-price-daily");



            var json = dom["#site-content .map-search"].Attr("data-bootstrap-data");
            var serializer = new JavaScriptSerializer();
            dynamic settings = serializer.Deserialize<object>(json);
            var prices = Helpers.GetIfExists(settings, new[] { "results_json", "metadata", "search" });
            var minPrice = Helpers.GetIfExists(prices, "price_range_min_native");
            var maxPrice = Helpers.GetIfExists(prices, "price_range_max_native");

            int latDiv = 2;
            int longDiv = 2;

            double latPart = (r.EndLatitude - r.StartLatitude) / (double)latDiv;
            double longPart = (r.EndLongitude - r.StartLongitude) / (double)longDiv;
            for(int i = 0; i < latDiv; i++)
            {
                for (int j = 0; j < longDiv; j++)
                {

                    var request = new CityRequest()
                    {
                        Name = r.Name,
                        Country = r.Country,
                        Date = r.Date,
                        StartLatitude = r.StartLatitude + latPart * (double)i,
                        EndLatitude = r.StartLatitude + latPart * (double)(i+1),
                        StartLongitude = r.StartLongitude + longPart * (double)j,
                        EndLongitude = r.StartLongitude + longPart * (double)(j+1),
                    };

                    Logger.Log("Reading map qadrant {0}, startLatitude={1}, startLongitude={2}, endLatitude={3}, endLongitude={4}",
                            QadrantNumber++, request.StartLatitude, request.StartLongitude, request.EndLatitude, request.EndLongitude);

                    try
                    {
                        if (Stop)
                        {
                            Logger.Log("Stopped CheckCityStatus");
                            return false;
                        }

                        CheckFlats(request, Convert.ToInt32(minPrice), Convert.ToInt32(maxPrice));
                    }
                    catch (Exception e)
                    {
                        Logger.LogError(e);
                    }
                    
                }
            }

            return true;
        }

        private void CheckFlats(CityRequest request, int min, int max)
        {
            if (min >= max)
            {
                return;
            }

            Thread.Sleep(2000);
            var result = ReadFlats(request, min, max);
            var totalCount = Helpers.GetIfExists(result, "visible_results_count");

            Logger.Log(String.Format("min: {0}, max: {1}, total_apartments: {2} ", min, max, totalCount));
            if (totalCount >= MaxFlats)
            {
                if (Stop)
                {
                    Logger.Log("Stopped CheckFlats");
                    return;
                }

                int half = (max - min) / 2;
                CheckFlats(request, min, min + half);

                if (Stop)
                {
                    Logger.Log("Stopped CheckFlats");
                    return;
                }
                CheckFlats(request, min + half + 1, max);
            }
            else
            {
                ProcessFlats(request, min, max, result);
            }
        }

        private dynamic ReadFlats(CityRequest r, int min, int max, int? page = null)
        {
            string url = String.Format("https://www.airbnb.ru/search/search_results?zoom=10&sw_lat={0}&sw_lng={1}&ne_lat={2}&ne_lng={3}&price_min={4}&price_max={5}&search_by_map=true&location={6}%2C+{7}",
                r.StartLatitude.ToGBString(), r.StartLongitude.ToGBString(), r.EndLatitude.ToGBString(), r.EndLongitude.ToGBString(), 
                min, max, r.Name, r.Country); //Saint-Petersburg--Russia

            if (page != null)
            {
                url += String.Format("&page={0}", page.Value);
            }

            var json = Loader.Load(url);

            var serializer = new JavaScriptSerializer();
            return serializer.Deserialize<object>(json);
        }

        private void ProcessFlats(CityRequest request, int min, int max, dynamic data)
        {
            var metadata = Helpers.GetIfExists(data, new string[] { "results_json", "metadata" });
            int total = Convert.ToInt32(Helpers.GetIfExists(metadata, "listings_count"));
            int flatsPerPage = Convert.ToInt32(Helpers.GetIfExists(metadata, new string[] { "pagination", "result_count" }));
                
            if (flatsPerPage == 0)
            {
                return;
            }

            int pages = total / flatsPerPage;
            if (total % flatsPerPage > 0)
            {
                pages++;
            }

            ProcessPage(request, data);
            for (int i = 2; i < pages + 1; i++)
            {
                if (Stop)
                {
                    Logger.Log("Stopped ProcessFlats");
                    return;
                }
                ProcessPage(request, min, max, i);
            }
        }

        private void ProcessPage(CityRequest request, int min, int max, int page)
        {
            if (Stop)
            {
                Logger.Log("Stopped ProcessPage 1");
                return;
            }

            var data = ReadFlats(request, min, max, page);
            ProcessPage(request, data);
        }

        private void ProcessPage(CityRequest request, dynamic data)
        {
            var flatsIds = Helpers.GetIfExists(data, "property_ids");
            if (flatsIds == null)
            {
                return;
            }

            var tasks = new Task<FlatStatus>[flatsIds.Length];
            for (var i = 0; i < flatsIds.Length; i++)
            {
                if (Stop)
                {
                    Logger.Log("Stopped ProcessPage 2");
                    return;
                }

                var flatReader = new FlatReader();

                var flatRequest = new FlatStatusRequest()
                {
                    Id = flatsIds[i],
                    Date = request.Date,
                    Country = request.Country,
                    City = request.Name
                };
                Logger.Log("Reading flat: " + FlatNumber++ + " FlatType: " + request.FlatType + " Time: " + DateTime.Now);

                if (!Database.IsFlatProcessed(flatRequest.Id, flatRequest.Date))
                {
                    FlatStatus status = null;
                    try
                    {
                        status = flatReader.CheckFlatStatus(flatRequest);
                    }
                    catch (Exception e)
                    {
                        Logger.LogError(e);
                    }

                    Database.Save(status);
                    Thread.Sleep(7500);
                }
                else
                {
                    Logger.Log("Flat already processed. Duplicate checks count: " + DuplicateChecks++);
                }
            }
        }
    }

    public static class DoubleExtensions
    {
        public static string ToGBString(this double value)
        {
            return value.ToString(CultureInfo.GetCultureInfo("en-GB"));
        }
    }
}
