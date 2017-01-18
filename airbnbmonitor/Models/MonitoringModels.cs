using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using DataMiner;

namespace airbnbmonitor.Models
{
    public class MonitoringDataModel
    {
        public double? Latitude { get; set; }
        public double? Longitude { get; set; }
        public double? FillFactor { get; set; }
        public int Revenue { get; set; }
    }
}