namespace DataMiner.CityParser
{
    using System;
    using System.Collections.Generic;
    using System.Globalization;
    using System.Threading;
    using System.Web.Script.Serialization;
    using DataMiner.Database;
    using DataMiner.FlatParser;
    using DataMiner.Logger;
    using DataMiner.UrlLoader;

    public enum CityReaderResult { Exception, Stopped, DateChanged, Ok }

    public class ThreadSafeWrapper<T>
    {
        private readonly object _lock = new object();
        private T _val = default(T);

        public ThreadSafeWrapper(T initialValue = default(T))
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
        private static readonly int MaxFlats = 300;
        private static readonly string SearchFlatsUrl = "https://www.airbnb.ru/search/search_results?zoom=10&sw_lat={0}&sw_lng={1}&ne_lat={2}&ne_lng={3}&price_min={4}&price_max={5}&search_by_map=true&location={6}%2C+{7}";
        private static int QadrantNumber;

        private readonly ThreadSafeWrapper<bool> _dateChanged = new ThreadSafeWrapper<bool>();

        private int _flatNumber;
        private int _duplicateChecks;

        public bool DateChanged
        {
            get
            {
                return _dateChanged.Value;
            }

            set
            {
                _dateChanged.Value = value;
            }
        }

        private readonly IDatabase _db;
        private readonly ILogger _log;

        public CityReader(IDatabase db, ILogger log)
        {
            _db = db;
            _log = log;
        }

        private void CheckCityQadrantThread(CityRequest request, string minPrice, string maxPrice)
        {
            _log.Log("Reading map qadrant {0}, startLatitude={1}, startLongitude={2}, endLatitude={3}, endLongitude={4}",
                            QadrantNumber, request.StartLatitude, request.StartLongitude, request.EndLatitude, request.EndLongitude);

            Interlocked.Increment(ref QadrantNumber);
            while (true)
            {
                try
                {
                    CheckFlats(request, Convert.ToInt32(minPrice), Convert.ToInt32(maxPrice));
                }
                catch (Exception e)
                {
                    _log.LogError(e);
                }
            }
        }

        public void ReadCityFlats(CityRequest r)
        {
            _log.Log("Reading city: " + r);

            var minPrice = "640";
            var maxPrice = "50000";

            int latDiv = 2;
            int longDiv = 2;

            double latPart = (r.EndLatitude - r.StartLatitude) / (double)latDiv;
            double longPart = (r.EndLongitude - r.StartLongitude) / (double)longDiv;

            var threads = new List<Thread>();
            for (int i = 0; i < latDiv; i++)
            {
                for (int j = 0; j < longDiv; j++)
                {

                    var request = new CityRequest()
                    {
                        Name = r.Name,
                        Country = r.Country,
                        Date = r.Date,
                        StartLatitude = r.StartLatitude + latPart * (double)i,
                        EndLatitude = r.StartLatitude + latPart * (double)(i + 1),
                        StartLongitude = r.StartLongitude + longPart * (double)j,
                        EndLongitude = r.StartLongitude + longPart * (double)(j + 1),
                    };

                    var t = new Thread(() => CheckCityQadrantThread(request.Copy(), minPrice, maxPrice));
                    t.Start();
                    threads.Add(t);
                }
            }

            for (int i = 0; i < threads.Count; i++)
            {
                int n = i;
                threads[n].Join();
            }
        }

        private void CheckFlats(CityRequest request, int min, int max)
        {
            if (min >= max)
            {
                return;
            }

            var result = ReadFlats(request, min, max);
            var totalCount = Helpers.GetIfExists(result, new string[] { "results_json", "metadata", "listings_count" });

            _log.Log(string.Format("min: {0}, max: {1}, total_apartments: {2} ", min, max, totalCount));
            if (totalCount >= MaxFlats)
            {
                if (DateChanged)
                {
                    return;
                }

                int half = (max - min) / 2;
                CheckFlats(request, min, min + half);

                if (DateChanged)
                {
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
            string url = string.Format(SearchFlatsUrl, r.StartLatitude.ToGBString(), r.StartLongitude.ToGBString(), r.EndLatitude.ToGBString(), r.EndLongitude.ToGBString(), 
                                min, max, r.Name, r.Country); //Saint-Petersburg--Russia

            if (page != null)
            {
                url += string.Format("&page={0}", page.Value);
            }

            var json = ObjectCreator.Get<Loader>().Load(url);

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

            for (int i = 2; i < pages + 1; i++)
            {
                if (DateChanged)
                {
                    _log.Log("Stopped ProcessFlats 1");
                    return;
                }

                ProcessPage(request, min, max, i);
            }
        }

        private void ProcessPage(CityRequest request, int min, int max, int page)
        {
            if (DateChanged)
            {
                _log.Log("Stopped ProcessPage 1");
                return;
            }

            var data = ReadFlats(request, min, max, page);
            ProcessPage(request, data);
        }

        private void ProcessPage(CityRequest request, dynamic data)
        {
            List<int> flatsIds = GetFlatsIds(data);
            for (var i = 0; i < flatsIds.Count; i++)
            {
                ReadFlat(flatsIds[i], request);
            }
        }

        private void ReadFlat(int flatId, CityRequest request)
        { 
            if (Helpers.GetSpbCurrentTime().Date > request.Date)
            {
                _log.Log("DATE CHANGEND. DONE");
                DateChanged = true;
            }

            if (DateChanged)
            {
                _log.Log("Stopped ProcessPage 2");
                return;
            }

            var flatRequest = new FlatStatusRequest()
            {
                Id = flatId,
                Date = request.Date,
                Country = request.Country,
                City = request.Name
            };
           
            if (!FlatProcessedChecker.Check(flatRequest.Id)) 
            {
                _log.Log(">>Reading flat: " + _flatNumber + " Time: " + Helpers.GetSpbCurrentTime() 
                            + " Duplicate checks count" + _duplicateChecks);

                FlatStatus status = null;
                try
                {
                    var flatReader = ObjectCreator.Get<FlatReader>();
                    status = flatReader.CheckFlatStatus(flatRequest);
                    Interlocked.Increment(ref _flatNumber);

                    _db.Save(status);
                    FlatProcessedChecker.Add(flatRequest.Id);
                }
                catch (Exception e)
                {
                    _log.LogError(e);
                }
            }
            else
            {
                Interlocked.Increment(ref _duplicateChecks);
            }
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
            for (var i = 0; i < list.Length; i++)
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
