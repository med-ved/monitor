using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Script.Serialization;
using airbnbmonitor.Models;
using DataMiner;

namespace airbnbmonitor.Code
{
    public class Monitoring
    {
        public string GetData()
        {
            using (var context = new MonitorEntities())
            {
                var statuses = context.Flats.Take(500);
                if (statuses == null)
                {
                    return "";
                }

                var result = new List<MonitoringDataModel>();
                Random rnd = new Random();
                foreach (var s in statuses)
                {
                    var newStatus = new MonitoringDataModel()
                    {
                        Latitude = s.Latitude,
                        Longitude = s.Longitude,
                        FillFactor = rnd.Next(0, 100),
                        Revenue = rnd.Next(5000, 100000)

                    };
                    result.Add(newStatus);
                }

                var serializer = new JavaScriptSerializer();
                var serializedResult = serializer.Serialize(result);
                return serializedResult;
            }
        }
    }
}