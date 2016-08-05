using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DataMiner;

namespace tester
{
    class Program
    {
        static void Main(string[] args)
        {
            Console.WriteLine("Start time: " + DateTime.Now);
            /*var r = new CityReader();
            var request = new CityRequest() { Name = "Saint-Petersburg", Country = "Russia",
                Date = DateTime.Now
                //Date = DateTime.Parse("2016-08-03 23:05:25.670")
            };
            try
            {
                r.CheckCityStatus(request);
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
                throw;
            }*/

            var reader = new FlatReader();
            var request = new FlatStatusRequest()
            {
                Id = 10849485,
                Date = DateTime.Parse("2016-08-04 11:04:28.903"),
                Country = "Russia",
                City = "Saint-Petersburg"
            };

            /*var flat = null;
            using (var context = new MonitorEntities())
            {
                flat = context.Flats.Where(f => f.Id == status.Flat.Id).FirstOrDefault<Flats>();
                if (flat == null)
                {
                    flat = Database.AddNewFlat(context, status);
                }
            }

            var processed = Database.IsFlatProcessed(request.Id, request.Date);*/

            var flat = new Flat();
            flat.Id = 456;
            var date = DateTime.Now;

            var status = new FlatStatus()
            {
                Flat = flat,
                Date = date,
                Price = 123,
                Available = true
            };
            Database.Save(status);
            var processed = Database.IsFlatProcessed(flat.Id, date);
            Database.Save(status);
            processed = Database.IsFlatProcessed(flat.Id, date);
            //var result = reader.CheckFlatStatus(request);*/

            Console.WriteLine("DONE. Time: " + DateTime.Now);
            Console.ReadLine();
        }
    }
}
