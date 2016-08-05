using System;
using System.Collections.Generic;
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

        public override string ToString()
        {
            return String.Format("Date={0}, Country={1}, City={2}, FlatType={3}", Date, Country, Name, FlatType);
        }
    }

    public class CityReader
    {
        private int FlatNumber = 0;
        private int DuplicateChecks = 0;
        private static int MaxFlats = 300;
        private static string[] FlatTypes = new string[] { "Entire+home%2Fapt", "Private+room", "Shared+room" };

        public void CheckCityStatus(CityRequest request)
        {
            Console.WriteLine("Reading city: " + request);
            string url = String.Format("https://www.airbnb.ru/s/{0}--{1}", request.Name, request.Country); //Saint-Petersburg--Russia
            var html = Loader.Load(url);

            CQ dom = html;
            var minPrice = dom[".price-range-slider"].Attr("data-min-price-daily");
            var maxPrice = dom[".price-range-slider"].Attr("data-max-price-daily");

            for(int i = 0; i < FlatTypes.Length; i++)
            {
                request.FlatType = FlatTypes[i];
                Console.WriteLine("REading flat type: " + request.FlatType);
                CheckFlats(request, Convert.ToInt32(minPrice), Convert.ToInt32(maxPrice));
            }
        }

        private void CheckFlats(CityRequest request, int min, int max)
        {
            if (min >= max)
            {
                return;
            }

            var result = ReadFlats(request, min, max);
            var totalCount = result["visible_results_count"];

            Console.WriteLine(String.Format("min: {0}, max: {1}, total_apartments: {2} ", min, max, totalCount));
            if (totalCount >= MaxFlats)
            {
                int half = (max - min) / 2;
                CheckFlats(request, min, min + half);
                CheckFlats(request, min + half + 1, max);
            }
            else
            {
                ProcessFlats(request, min, max, result);
            }
        }

        private dynamic ReadFlats(CityRequest request, int min, int max, int? page = null)
        {
            string url = String.Format("https://www.airbnb.ru/search/search_results?source=filters&location={0}%2C%20{1}&price_min={2}&price_max={3}&room_types%5B%5D={4}",
                request.Name, request.Country, min, max, request.FlatType); //Saint-Petersburg--Russia

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
            dynamic metadata = data["results_json"]["metadata"];

            int total = Convert.ToInt32(metadata["listings_count"]);
            int flatsPerPage = Convert.ToInt32(metadata["pagination"]["result_count"]);
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
                ProcessPage(request, min, max, i);
            }
        }

        private void ProcessPage(CityRequest request, int min, int max, int page)
        {
            var data = ReadFlats(request, min, max, page);
            ProcessPage(request, data);
        }

        private void ProcessPage(CityRequest request, dynamic data)
        {
            var flatsIds = data["property_ids"];
            var tasks = new Task<FlatStatus>[flatsIds.Length];
            for (var i = 0; i < flatsIds.Length; i++)
            {
                var flatReader = new FlatReader();

                var flatRequest = new FlatStatusRequest()
                {
                    Id = flatsIds[i],
                    Date = request.Date,
                    Country = request.Country,
                    City = request.Name
                };
                Console.WriteLine("Reading flat: " + FlatNumber++ + " FlatType: " + request.FlatType + " Time: " + DateTime.Now);

                if (!Database.IsFlatProcessed(flatRequest.Id, flatRequest.Date))
                {
                    var status = flatReader.CheckFlatStatus(flatRequest);
                    Database.Save(status);
                    Thread.Sleep(10000);
                }
                else
                {
                    Console.WriteLine("Flat already processed. Duplicate checks count: " + DuplicateChecks++);
                }
            }
            
            //task.Wait();
            //FlatStatus result = task.Result;
            //Console.WriteLine("Starting thred for request: " + i);
            //tasks[i] = Task<FlatStatus>.Factory.StartNew(() => flatReader.CheckFlatStatus(flatRequest));
            /*var results = new FlatStatus[flatsIds.Length];
            for (var i = 0; i < flatsIds.Length; i++)
            {
                tasks[i].Wait();
                results[i] = tasks[i].Result;
            }*/

        }
    }
}
