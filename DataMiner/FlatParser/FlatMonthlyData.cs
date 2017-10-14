namespace DataMiner.FlatParser
{
    using System;
    using System.Data.SqlClient;

    public class FlatMonthlyData
    {
        public long FlatId { get; set; }
        public int Year { get; set; }
        public int Month { get; set; }
        public int Count { get; set; }
        public long Occupacy { get; set; }
        public int Revenue { get; set; }

        public double OccupacyPercent { get; set; }
        public double RevenuePerDay { get; set; }
        public double EstimatedMonthlyRevenue { get; set; }

        public DateTime Date { get; set; }

        public FlatMonthlyData()
        {
        }

        public FlatMonthlyData(SqlDataReader reader)
        {
            FlatId = reader.GetInt64(0);
            Year = reader.GetInt32(1);
            Month = reader.GetInt32(2);
            Count = reader.GetInt32(3);
            Occupacy = reader.IsDBNull(4) ? 0 : reader.GetInt32(4);
            Revenue = reader.IsDBNull(5) ? 0 : reader.GetInt32(5);

            Date = new DateTime(Year, Month, 1);
            OccupacyPercent = Count > 0 ? Occupacy / (double)Count : 0;
            RevenuePerDay = Occupacy > 0 ? Revenue / Occupacy : 0;
            EstimatedMonthlyRevenue = Count > 0 ? Revenue * (DateTime.DaysInMonth(Year, Month) / (double)Count) : 0;
        }
    }
}
