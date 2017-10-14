namespace DataMiner.FlatParser
{
    using System;
    using System.Collections.Generic;
    using DataMiner.Database;

    public static class FlatProcessedChecker
    {
        private static object _locker = new object();
        private static HashSet<long> _flats = new HashSet<long>();

        public static void Init(DateTime date, IDatabase db)
        {
            lock (_locker)
            {
                var onlyDate = new DateTime(date.Year, date.Month, date.Day);
                Clear();
                var flatIds = db.GetProcessedFlatsIds(onlyDate);
                foreach (var id in flatIds)
                {
                    _flats.Add(id);
                }
            }
        }

        public static bool Check(int flatId)
        {
            lock (_locker)
            {
                return _flats.Contains(flatId);
            }
        }

        public static void Add(int flatId)
        {
            lock (_locker)
            {
                if (!Check(flatId))
                {
                    _flats.Add(flatId);
                }
            }
        }

        private static void Clear()
        {
            _flats.Clear();
        }
    }
}
