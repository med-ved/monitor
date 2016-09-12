using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using DataMiner;

namespace tester
{
    class MyWebClient : WebClient
    {
        protected override WebRequest GetWebRequest(Uri address)
        {
            WebRequest request = base.GetWebRequest(address);
            if (request is HttpWebRequest)
            {
                (request as HttpWebRequest).KeepAlive = true;
            }
            return request;
        }
    }

    class Program
    {
        static void Main(string[] args)
        {
            /*Console.WriteLine("Start time: " + DateTime.Now);

            var r = new CityReader();
            var request = new CityRequest()
            {
                Name = "Saint-Petersburg",
                Country = "Russia",
                Date = DateTime.Now,
                StartLatitude = 59.79219567461983,
                EndLatitude = 60.04839774295754,
                StartLongitude = 29.818058044888858,
                EndLongitude = 30.817813904263858,
            };

            r.CheckCityStatus(request);

            Console.WriteLine("DONE. Time: " + DateTime.Now);*/

            //var pm = new DataMiner.Program();
            //pm.Run();


            try
            {
                using (var client = new WebClient())
                {
                    var proxy = new WebProxy("80.80.160.251", 8080);
                    client.Proxy = proxy;

                    var html = client.DownloadString("http://www.google.com");
                    //var html = client.DownloadString("https://www.airbnb.ru/rooms/10206147");
                    Console.WriteLine(html);
                }

            }
            catch (WebException e)
            {
                Console.WriteLine(e);
            }

            Console.ReadLine();
        }
    }
}
