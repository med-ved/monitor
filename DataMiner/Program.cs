namespace DataMiner
{
    using System;
    using System.Threading;
    using System.Threading.Tasks;
    using DataMiner.CityParser;
    using DataMiner.Database;
    using DataMiner.FlatParser;
    using DataMiner.Logger;
    using DataMiner.UrlLoader;

    public class Program
    {
        private readonly IDatabase _db;
        private readonly ILogger _log;

        public Program(IDatabase db, ILogger log)
        {
            _db = db;
            _log = log;
        }

        private void MainLoop()
        {
            Loader.Init(_db, _log);
            Thread.Sleep(5 * 60 * 1000);
            while (true)
            {
                try
                {
                    var date = Helpers.GetSpbCurrentTime().Date;
                    if (CanStartReadingFlats(date))
                    {
                        FlatProcessedChecker.Init(date, _db);
                        _log.Log("Start time: " + Helpers.GetSpbCurrentTime());

                        var reader = ObjectCreator.Get<CityReader>();
                        var request = new CityRequest()
                        {
                            Name = "Saint-Petersburg",
                            Country = "Russia",
                            Date = date,
                            StartLatitude = 59.79219567461983, //niz
                            EndLatitude = 60.04839774295754, //verh
                            StartLongitude = 29.818058044888858, //levo
                            EndLongitude = 30.817813904263858, //pravo

                            CenterStartLatitude = 59.914969,
                            CenterEndLatitude = 59.969745,
                            CenterStartLongitude = 30.249778,
                            CenterEndLongitude = 30.399810
                        };

                        _db.SetLatestProcessedDate(date, false);
                        reader.ReadCityFlats(request);
                        _db.SetLatestProcessedDate(date, true);

                        _log.Log("DONE.");
                    }

                    Thread.Sleep(60000);
                }
                catch (Exception e)
                {
                    _log.LogError(e);
                }
            }
        }

        private bool CanStartReadingFlats(DateTime date)
        {
            var info = _db.GetLatestProcessedDateInfo();
            if (info == null || info.Date == null)
            {
                return true;
            }

            if (date > info.Date)
            {
                return true;
            }

            if (date <= info.Date && (
                (info.Finished.HasValue && !info.Finished.Value) || !info.Finished.HasValue))
            {
                return true;
            }

            return false;
        }

        public void Run()
        {
            Task.Run(() => MainLoop());
        }
    }
}
