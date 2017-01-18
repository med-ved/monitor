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
    public enum CityReaderResult { Exception, Stopped, DateChanged, Ok }

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

        public CityRequest Copy()
        {
            var copy = new CityRequest()
            {
                Name = this.Name,
                Country = this.Country,
                Date = this.Date,
                FlatType = this.FlatType
            };

            return copy;
        }

        public override string ToString()
        {
            return String.Format("Date={0}, Country={1}, City={2}, FlatType={3}", Date, Country, Name, FlatType);
        }
    }

    public class ThreadSafeValue<T>
    {
        private object _lock = new object();
        private T _val = default(T);

        public ThreadSafeValue(T initialValue = default(T))
        {
            Value = initialValue;
        }   

        public T Value
        {
            get
            {
                lock (_lock)
                {
                    return _val;
                }
            }

            set
            {
                lock (_lock)
                {
                    _val = value;
                }
            }
        }
    }

    public class CityReader
    {
        private ThreadSafeValue<int> FlatNumber = new ThreadSafeValue<int>();
        private static ThreadSafeValue<int> QadrantNumber = new ThreadSafeValue<int>();
        private ThreadSafeValue<int> DuplicateChecks = new ThreadSafeValue<int>();
        private static ThreadSafeValue<int> MaxFlats = new ThreadSafeValue<int>(300);

        //public ThreadSafeValue<bool> Stop = new ThreadSafeValue<bool>();
        public ThreadSafeValue<bool> DateChanged = new ThreadSafeValue<bool>();

        /*public bool CheckCityQadrandThread(CityRequest request, string minPrice, string maxPrice)
        {
            Logger.Log("Reading map qadrant {0}, startLatitude={1}, startLongitude={2}, endLatitude={3}, endLongitude={4}",
                            QadrantNumber.Value++, request.StartLatitude, request.StartLongitude, request.EndLatitude, request.EndLongitude);

            while(true)
            {
                try
                {
                    CheckFlats(request, Convert.ToInt32(minPrice), Convert.ToInt32(maxPrice));
                    return true; //CityReaderResult.Ok;
                }
                catch (Exception e)
                {
                    Logger.LogError(e);
                    //return CityReaderResult.Exception;
                }
            }
        }*/

        public bool CheckCityStatus(CityRequest r)
        {
            Logger.Log("Reading city: " + r);

            var minPrice = "640";
            var maxPrice = "50000";

            int latDiv = 2;
            int longDiv = 2;

            double latPart = (r.EndLatitude - r.StartLatitude) / (double)latDiv;
            double longPart = (r.EndLongitude - r.StartLongitude) / (double)longDiv;

            //var threads = new List<Task<bool>>();
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
                            QadrantNumber.Value++, request.StartLatitude, request.StartLongitude, request.EndLatitude, request.EndLongitude);

                    try
                    {
                        if (DateChanged.Value)
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
                    //var t = Task.Factory.StartNew(() => CheckCityQadrandThread(request, minPrice, maxPrice));
                    //threads.Add(t);
                }
            }

            /*for(int i = 0; i < threads.Count; i++)
            {
                threads[i].Wait();
            }*/

            return true;
        }

        private void CheckFlats(CityRequest request, int min, int max)
        {
            if (min >= max)
            {
                return;
            }

            var result = ReadFlats(request, min, max);
            var totalCount = Helpers.GetIfExists(result, new string[] { "results_json", "metadata", "listings_count" });

            Logger.Log(String.Format("min: {0}, max: {1}, total_apartments: {2} ", min, max, totalCount));
            if (totalCount >= MaxFlats.Value)
            {
                if (DateChanged.Value)
                {
                    Logger.Log("Stopped CheckFlats 1");
                    return;
                }

                int half = (max - min) / 2;
                CheckFlats(request, min, min + half);

                if (DateChanged.Value)
                {
                    Logger.Log("Stopped CheckFlats 2");
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
                if (DateChanged.Value)
                {
                    Logger.Log("Stopped ProcessFlats 1");
                    return;
                }
                ProcessPage(request, min, max, i);
            }
        }

        private void ProcessPage(CityRequest request, int min, int max, int page)
        {
            if (DateChanged.Value)
            {
                Logger.Log("Stopped ProcessPage 1");
                return;
            }

            var data = ReadFlats(request, min, max, page);
            ProcessPage(request, data);
        }

        private void ProcessPage(CityRequest request, dynamic data)
        {
            List<int> flatsIds = GetFlatsIds(data);
            var threads = new List<Task<bool>>();
            for (var i = 0; i < flatsIds.Count; i++)
            {
                var n = i;
                var t = Task<bool>.Factory.StartNew(() => FlatReadThread(flatsIds[n], request.Copy()));
                threads.Add(t);
            }

            for(int j = 0; j < threads.Count; j++)
            {
                threads[j].Wait();
            }
        }

        private bool FlatReadThread(int flatId, CityRequest request)
        { 
            if (Helpers.GetSpbCurrentTime().Date > request.Date)
            {
                Logger.Log("DATE CHANGEND. DONE");
                DateChanged.Value = true;
            }

            if (DateChanged.Value)
            {
                Logger.Log("Stopped ProcessPage 2");
                return true;
            }

            var flatRequest = new FlatStatusRequest()
            {
                Id = flatId,//flatsIds[i],
                Date = request.Date,
                Country = request.Country,
                City = request.Name
            };
            
            //Logger.Log(">>Reading flat: " + FlatNumber.Value + " Time: " + Helpers.GetSpbCurrentTime() 
            //    + " Duplicate checks count" + DuplicateChecks.Value);

            if (!FlatProcessedChecker.Check(flatRequest.Id)) //!Database.IsFlatProcessed(flatRequest.Id, flatRequest.Date))
            {
                FlatStatus status = null;
                try
                {
                    var flatReader = new FlatReader();
                    status = flatReader.CheckFlatStatus(flatRequest);
                    FlatNumber.Value++;

                    Database.Save(status);
                    FlatProcessedChecker.Add(flatRequest.Id);
                }
                catch (Exception e)
                {
                    Logger.LogError(e);
                }
            }
            else
            {
                DuplicateChecks.Value++;
            }

            return true;
        }

        private List<int> GetFlatsIds(dynamic data)
        {
            if (data == null)
            {
                return new List<int>();
            }

            var list = Helpers.GetIfExists(data, new string[] { "results_json", "search_results" });
            if (list == null)
            {
                return new List<int>();
            }

            var result = new List<int>();
            for(var i=0; i < list.Length; i++)
            {
                var id = Helpers.GetIfExists(list[i], new string[] { "listing", "id" });
                if (id != null)
                {
                    result.Add(id);                     
                }
            }

            return result;
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
