using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Web.Hosting;

namespace DataMiner
{
    public class Program //: IRegisteredObject
    {
        private CityReader reader = null;
        private bool _logToConsole = false;

        public bool LogToConsole
        {
            get
            {
                return _logToConsole;
            }
            set
            {
                _logToConsole = value;
                Logger.LogToConsole = value;
            }
        }

        public Program()
        {
            //HostingEnvironment.RegisterObject(this);
        }

        private void MainLoop()
        {
            Loader.Init();
            Thread.Sleep(10 * 60 * 1000);
            while(true)
            {
                try
                {
                    var date = Helpers.GetSpbCurrentTime().Date;
                    if (CanStartReadingFlats(date))
                    {
                        FlatProcessedChecker.Init(date);
                        Logger.Log("Start time: " + Helpers.GetSpbCurrentTime());

                        reader = new CityReader();
                        var request = new CityRequest()
                        {
                            Name = "Saint-Petersburg",
                            Country = "Russia",
                            Date = date,
                            StartLatitude = 59.79219567461983,
                            EndLatitude = 60.04839774295754,
                            StartLongitude = 29.818058044888858,
                            EndLongitude = 30.817813904263858,
                        };

                        Database.SetLatestProcessedDate(date, false);
                        reader.CheckCityStatus(request);
                        Database.SetLatestProcessedDate(date, true);

                        Logger.Log("DONE.");
                    }

                    Thread.Sleep(60000);
                }
                catch(Exception e)
                {
                    Logger.LogError(e);
                }
                
            }
        }

        private bool CanStartReadingFlats(DateTime date)
        {
            var info = Database.GetLatestProcessedDateInfo();
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

        /*public void Stop(bool immediate)
        {
            reader.Stop.Value = true;
            HostingEnvironment.UnregisterObject(this);
        }*/

        public void Run()
        {
            Task.Run(() => MainLoop());
        }
    }
}
