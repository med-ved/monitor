using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using DataMiner;

namespace tester
{
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

            var pm = new DataMiner.Program();
            pm.Run();
            Thread.Sleep(20000);
            pm.Stop(false);

            Console.ReadLine();
        }
    }
}
