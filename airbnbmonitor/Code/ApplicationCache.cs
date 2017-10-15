namespace airbnbmonitor.Code
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using airbnbmonitor.Code.Definitions;
    using airbnbmonitor.Code.Interfaces;
    using DataMiner.Database;
    using DataMiner.FlatParser;

    public class ApplicationCache
    {
        private static readonly string FlatsKey = "flatsList";
        private static readonly string MonthlyDataKey = "monthlyData";

        private readonly ICacheService _cacheService;
        private readonly IDatabase _db;

        public ApplicationCache(ICacheService cacheService, IDatabase db)
        {
            _cacheService = cacheService;
            _db = db;
        }

        public CachedFlats GetCachedFlats()
        {
            var flats = _cacheService.Get(FlatsKey, _db.GetFlats);
            var result = _cacheService.Get(MonthlyDataKey, () => this.GetFlatsMonthlyData(flats));
            result.Flats = flats;

            return result;
        }

        public IDictionary<DateTime, FlatMonthlyData> _CreateMonthlyDataDictionary(List<FlatMonthlyData> monthlyData)
        {
            var result = new Dictionary<DateTime, FlatMonthlyData>();
            foreach (var d in monthlyData)
            {
                var date = new DateTime(d.Year, d.Month, 1);
                result.Add(date, d);
            }

            return result;
        }

        private CachedFlats GetFlatsMonthlyData(IDictionary<long, Flat> flats)
        {
            var data = _db.GetFlatsMonthlyData();
            var groupedByFlats = data.GroupBy(m => m.FlatId);

            var flatsList = new List<FlatData>();
            var flatsDict = new Dictionary<long, FlatData>();
            foreach (var g in groupedByFlats)
            {
                var flatData = new FlatData();
                flatData.MonthlyData = g.ToList();
                flatData.FlatId = g.Key;

                if (flatData.MonthlyData.Count == 0 || !flats.ContainsKey(flatData.FlatId) || FlatNeverRented(flatData.MonthlyData))
                {
                    continue;
                }

                var flat = flats[flatData.FlatId];
                flatData.MonthlyDataDict = _CreateMonthlyDataDictionary(flatData.MonthlyData);
                RemoveNonSeasonDataIfLongRent(flatData.MonthlyData);

                flatData.Latitude = flat.Latitude;
                flatData.Longitude = flat.Longitude;
                flatData.OccupacyPercent = flatData.MonthlyData.Sum(f => f.OccupacyPercent) / flatData.MonthlyData.Count;
                flatData.RevenuePerDay = flatData.MonthlyData.Sum(f => f.RevenuePerDay) / flatData.MonthlyData.Count;
                flatData.EstimatedRevenue = flatData.MonthlyData.Sum(f => f.EstimatedMonthlyRevenue) / flatData.MonthlyData.Count;

                
                flatsList.Add(flatData);
                flatsDict.Add(flatData.FlatId, flatData);
            }

            return new CachedFlats()
            {
                FlatsDataDict = flatsDict,
                FlatsData = flatsList
            };
        }

        private bool FlatNeverRented(IEnumerable<FlatMonthlyData> monthlyData)
        {
            return !monthlyData.Any(f => f.Occupacy > 0);
        }

        private void RemoveNonSeasonDataIfLongRent(IEnumerable<FlatMonthlyData> monthlyData)
        {
            int checks = 0;
            int full = 0;
            foreach (var m in monthlyData)
            {
                if (m.Month > 9 || m.Month < 5)
                {
                    checks++;
                    if (m.Occupacy == m.Count)
                    {
                        full++;
                    }
                }
            }

            //if flat has 100% occupacy during non-season almost 100% chanсe that its rented out for long term
            if (checks == full)
            {
                foreach (var m in monthlyData)
                {
                    if (m.Month > 9 || m.Month < 5)
                    {
                        m.Count = 0;
                        m.Occupacy = 0;
                        m.EstimatedMonthlyRevenue = 0;
                        m.OccupacyPercent = 0;
                        m.Revenue = 0;
                        m.RevenuePerDay = 0;
                    }
                }
            }
        }
    }
}