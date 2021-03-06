﻿namespace DataMiner.Database
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Data.SqlClient;
    using DataMiner.Logger;
    using DataMiner.FlatParser;
    using DataMiner.Proxies;
    using System.Configuration;

    public class LatestDateInfo
    {
        public DateTime? Date { get; set; }
        public bool? Finished { get; set; }
    }

    public static class NullableExtentios
    {
        public static T? Get<T>(this T? t) where T : struct
        {
            if (t.HasValue)
            {
                return t.Value;
            }

            return null;
        }
    }

    public class Database : IDatabase
    {
        private readonly Lazy<ILogger> _log;

        public Database(Lazy<ILogger> log)
        {
            _log = log;
        }

        public void Save(FlatStatus status)
        {
            using (var context = new MonitorEntities())
            {
                var flat = context.Flats.Where(f => f.Id == status.Flat.Id).FirstOrDefault<Flats>();
                if (flat == null)
                {
                    flat = AddNewFlat(context, status);
                }

                var newStatus = new FlatStatuses()
                {
                    Available = status.Available,
                    Date = status.Date,
                    Price = (short?)status.Price.Get(),
                    FlatId = flat.Id,
                    Flats = flat
                };
                context.FlatStatuses.Add(newStatus);

                context.SaveChanges();
            }
        }

        public Flats AddNewFlat(MonitorEntities context, FlatStatus status)
        {
            var newFlat = new Flats()
            {
                Id = status.Flat.Id,
                Latitude = (float?)status.Flat.Latitude.Get(),
                Longitude = (float?)status.Flat.Longitude.Get(),
                Country = status.Flat.Country,
                City = status.Flat.City,
                Rating = (float?)status.Flat.Rating.Get(),
                MaxGuests = (int?)status.Flat.MaxGuests.Get(),
                BedsCount = (int?)status.Flat.BedsCount.Get(),
                BathroomsCount = (int?)status.Flat.BathroomsCount.Get(),
                BedroomsCount = (int?)status.Flat.BedroomsCount.Get(),
                RoomType = status.Flat.RoomType,

                MatchDescription = (float?)status.Flat.ShortDescription.MatchDescription.Get(),
                Communication = (float?)status.Flat.ShortDescription.Communication.Get(),
                Cleanly = (float?)status.Flat.ShortDescription.Cleanly.Get(),
                Location = (float?)status.Flat.ShortDescription.Location.Get(),
                Settlement = (float?)status.Flat.ShortDescription.Settlement.Get(),
                PriceQualityRation = (float?)status.Flat.ShortDescription.PriceQualityRation.Get(),

                Description = GetFlatDescription(status),
                Facilities = GetFlatFacilities(status)
            };
            context.Flats.Add(newFlat);

            return newFlat;
        }

        public Description GetFlatDescription(FlatStatus status)
        {
            var description = new Description();
            description.Access = status.Flat.Description.Access;
            description.Description1 = status.Flat.Description.Description;
            description.HouseRules = status.Flat.Description.HouseRules;
            description.Interaction = status.Flat.Description.Interaction;
            description.Locale = status.Flat.Description.Locale;
            description.Name = status.Flat.Description.Name;
            description.NeighborhoodOverview = status.Flat.Description.NeighborhoodOverview;
            description.Notes = status.Flat.Description.Notes;
            description.Space = status.Flat.Description.Space;
            description.Summary = status.Flat.Description.Summary;
            description.Transit = status.Flat.Description.Transit;

            return description;
        }

        public Facilities GetFlatFacilities(FlatStatus status)
        {
            var facilities = new Facilities();
            facilities.AirConditioner = status.Flat.Facilities.AirConditioner;
            facilities.Breakfast = status.Flat.Facilities.Breakfast;
            facilities.CableTv = status.Flat.Facilities.CableTv;
            facilities.Dryer = status.Flat.Facilities.Dryer;
            facilities.EventsFriendly = status.Flat.Facilities.EventsFriendly;
            facilities.FamilyFriendly = status.Flat.Facilities.FamilyFriendly;
            facilities.Fireplace = status.Flat.Facilities.Fireplace;
            facilities.FreeParking = status.Flat.Facilities.FreeParking;
            facilities.Gym = status.Flat.Facilities.Gym;
            facilities.Hairdryer = status.Flat.Facilities.Hairdryer;
            facilities.Heating = status.Flat.Facilities.Heating;
            facilities.Intercom = status.Flat.Facilities.Intercom;
            facilities.Internet = status.Flat.Facilities.Internet;
            facilities.Iron = status.Flat.Facilities.Iron;
            facilities.Jacuzzi = status.Flat.Facilities.Jacuzzi;
            facilities.Kitchen = status.Flat.Facilities.Kitchen;
            facilities.Lift = status.Flat.Facilities.Lift;
            facilities.NotebookWorkingPlace = status.Flat.Facilities.NotebookWorkingPlace;
            facilities.PeopleWithLimitedAbilities = status.Flat.Facilities.PeopleWithLimitedAbilities;
            facilities.PetsAllowed = status.Flat.Facilities.PetsAllowed;
            facilities.Porter = status.Flat.Facilities.Porter;
            facilities.Shoulder = status.Flat.Facilities.Shoulder;
            facilities.SmokingAllowed = status.Flat.Facilities.SmokingAllowed;
            facilities.SwimingPoll = status.Flat.Facilities.SwimingPoll;
            facilities.ToiletAccessories = status.Flat.Facilities.ToiletAccessories;
            facilities.TV = status.Flat.Facilities.TV;
            facilities.WashingMashine = status.Flat.Facilities.WashingMashine;
            facilities.WirelessInternet = status.Flat.Facilities.WirelessInternet;

            return facilities;
        }
        
        public LatestDateInfo GetLatestProcessedDateInfo()
        {
            using (var context = new MonitorEntities())
            {
                var settings = context.Settings.FirstOrDefault();
                if (settings == null)
                {
                    return null;
                }

                var info = new LatestDateInfo
                {
                    Date = settings.LatestDay,
                    Finished = settings.Finished
                };

                return info;
            }
        }

        public void SetLatestProcessedDate(DateTime date, bool finished)
        {
            using (var context = new MonitorEntities())
            {
                var settings = context.Settings.FirstOrDefault();
                if (settings == null)
                {
                    var newSettings = new Settings()
                    {
                        Id = 1,
                        LatestDay = date,
                        Finished = finished
                    };

                    context.Settings.Add(newSettings);
                }
                else
                {
                    settings.Finished = finished;
                    settings.LatestDay = date;
                }
                
                context.SaveChanges();
            }
        }

        public void WriteLog(string log, string message)
        {
            using (var context = new MonitorEntities())
            {
                var logEntry = new Log()
                {
                    Type = log,
                    Timestamp = Helpers.GetSpbCurrentTime(),
                    Message = message
                };

                context.Log.Add(logEntry);
                context.SaveChanges();
            }
        }

        public IEnumerable<Proxy> GetProxies()
        {
            using (var context = new MonitorEntities())
            {
                var proxyes = context.Proxyes.Where(p => p.Active).ToList();
                var result = new List<Proxy>();
                foreach (var p in proxyes)
                {
                    var pr = new Proxy();
                    pr.Uid = p.Id;
                    pr.Url = p.Url;
                    pr.Port = p.Port.HasValue ? p.Port.Value : 80;
                    pr.Ping = p.Ping.HasValue ? p.Ping.Value : -1;
                    pr.LastUsageTime = p.LastUsageTime;
                    pr.Status = (ProxyStatus)p.Status;

                    result.Add(pr);
                }

                return result;
            }
        }

        private Proxy GetDummyProxy()
        {
            var p = new Proxy();
            p.Status = ProxyStatus.DontUpdate;
            p.Ping = -1;
            p.LastUsageTime = null;
 
            return p;
        }

        public void SetProxyStatus(Guid uid, ProxyStatus status = ProxyStatus.DontUpdate, DateTime? lastUsageTime = null)
        {
            var p = GetDummyProxy();
            p.Uid = uid;
            p.Status = status;
            p.LastUsageTime = lastUsageTime;

            UpdateProxy(p);
        }

        private void UpdateProxy(Proxy p)
        {
            using (var context = new MonitorEntities())
            {
                var proxy = context.Proxyes.Where(r => r.Id == p.Uid).FirstOrDefault();
                if (proxy == null)
                {
                    _log.Value.LogError(string.Format("Proxye {0}:{1} ({2}) not found in db", p.Url, p.Port, p.Uid));
                    return;
                }

                if (p.Status != ProxyStatus.DontUpdate)
                {
                    proxy.Status = (int)p.Status;
                }

                if (p.Ping != -1)
                {
                    proxy.Ping = p.Ping;
                }

                if (p.LastUsageTime != null)
                {
                    proxy.LastUsageTime = p.LastUsageTime;
                }

                context.SaveChanges();
            }
        }

        public void UpdateProxyUsageStatistic(Guid uid, int ping, bool timeout = false, bool fail = false, 
            bool success = false, bool forbidden403 = false)
        {
            using (var context = new MonitorEntities())
            {
                var proxy = context.Proxyes.Where(r => r.Id == uid).FirstOrDefault();
                if (proxy == null)
                {
                    _log.Value.LogError(string.Format("Proxye {0} not found in db", uid));
                    return;
                }

                proxy.UseCount++;
                proxy.LastUsageTime = Helpers.GetSpbCurrentTime();
                proxy.Ping = ping;
                proxy.TotalPing += ping;
                if (timeout)
                {
                    proxy.Timeouts++;
                }

                if (fail)
                {
                    proxy.Fails++;
                }

                if (success)
                {
                    proxy.Successes++;
                }

                if (forbidden403)
                {
                    proxy.Forbidden403++;
                }

                context.SaveChanges();
            }
        }

        public IEnumerable<long> GetProcessedFlatsIds(DateTime date)
        {
            using (var context = new MonitorEntities())
            {
                return context.FlatStatuses.Where(fs => fs.Date.Value == date && fs.FlatId.HasValue)
                    .Select(fs => fs.FlatId.Value).ToList();
            }
        }

        public IDictionary<long, Flat> GetFlats()
        {
            using (var context = new MonitorEntities())
            {
                return context.Flats.AsNoTracking()
                    .ToList().Select(f => new Flat(f)).ToDictionary(f => f.Id, f => f);
            }
        }

        public Flat GetFullFlatInfo(long id)
        {
            using (var context = new MonitorEntities())
            {
                var flat = context.Flats
                    .Include("Description")
                    .Include("Facilities")
                    .Where(f => f.Id == id).FirstOrDefault();

                if (flat == null)
                {
                    return null;
                }

                return new Flat(flat)
                {
                    ShortDescription = new FlatShortDescription(flat),
                    Description = new FlatDescription(flat)
                };
            }
        }

        //Entity Framework way to slow in this case. Fallback to ado.net
        //TODO: rewrite with EF
        public IEnumerable<FlatMonthlyData> GetFlatsMonthlyData()
        {
            string connectionString = ConfigurationManager.AppSettings["connectionStr"];

            string queryString = @"SELECT FlatId, YearNum, MonthNum, COUNT(*) Count,
                  SUM(CASE WHEN Available <> 1 THEN 1 ELSE 0 END) Occupancy,
                  SUM(CASE WHEN Available <> 1 THEN Price ELSE 0 END) Revenue
                  FROM [dbo].[FlatStatuses]
                  GROUP BY FlatId, YearNum, MonthNum
                  HAVING COUNT(*) > 0";

            using (var connection = new SqlConnection(connectionString))
            {
                var result = new List<FlatMonthlyData>();
                using (var command = new SqlCommand(queryString, connection))
                {
                    connection.Open();
                    using (var reader = command.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            result.Add(new FlatMonthlyData(reader));
                        }
                    }
                }

                return result;
            }
        }
    }
}
