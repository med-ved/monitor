namespace DataMiner.Database
{
    using System;
    using System.Collections.Generic;
    using DataMiner.FlatParser;
    using DataMiner.Proxies;

    public interface IDatabase
    {
        void Save(FlatStatus status);
        IEnumerable<long> GetProcessedFlatsIds(DateTime date);
        IDictionary<long, Flat> GetFlats();
        Flat GetFullFlatInfo(long id);

        IEnumerable<FlatMonthlyData> GetFlatsMonthlyData();
        Flats AddNewFlat(MonitorEntities context, FlatStatus status);
        Description GetFlatDescription(FlatStatus status);
        Facilities GetFlatFacilities(FlatStatus status);
        LatestDateInfo GetLatestProcessedDateInfo();

        void SetLatestProcessedDate(DateTime date, bool finished);
        void WriteLog(string log, string message);

        IEnumerable<Proxy> GetProxies();
        void SetProxyStatus(Guid uid, ProxyStatus status = ProxyStatus.DontUpdate, DateTime? lastUsageTime = null);
        void UpdateProxyUsageStatistic(Guid uid, int ping, bool timeout = false, bool fail = false,
                                       bool success = false, bool forbidden403 = false);
    }
}
