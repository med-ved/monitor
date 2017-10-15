namespace tester
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using airbnbmonitor.Code;
    using NUnit.Framework;
    using airbnbmonitor.Code.Definitions;
    using DataMiner.FlatParser;

    class FlatDataTests
    {
        [TestFixture]
        public class MonitiringTest
        {
            [Test]
            public void GetDataForFlatsEmpty()
            {
                var analytics = new Analytics();
                var startOfThePeriod = new DateTime(2016, 10, 1);
                var endOfThePeriod = DateTime.Now;
                var data = analytics._GetDataForFlats(new FlatData[] { }, startOfThePeriod, endOfThePeriod);
                Assert.AreEqual(data.AvgEstimatedMonthlyRevenue, 0);
                Assert.AreEqual(data.AvgOccupacyPercent, 0);
                Assert.AreEqual(data.AvgRevenuePerDay, 0);
                Assert.AreEqual(data.FlatsCount, 0);
                Assert.IsNull(data.GraphData);
            }

            [Test]
            public void GetDataForFlatsOneFlat()
            {
                var analytics = new Analytics();
                var startOfThePeriod = new DateTime(2016, 10, 1);
                var endOfThePeriod = DateTime.Now;

                var flat = new FlatData()
                {
                    MonthlyData = new List<FlatMonthlyData>()
                    {
                        AddFlatMonthlyData(2017, 1, 25, 20, 30000),
                    },

                    OccupacyPercent = 0.8,
                    RevenuePerDay = 1500,
                    EstimatedRevenue = 37200
                };

                var cache = new ApplicationCache(null, null);
                flat.MonthlyDataDict = cache._CreateMonthlyDataDictionary(flat.MonthlyData);

                var data = analytics._GetDataForFlats(new FlatData[] { flat }, startOfThePeriod, endOfThePeriod);
                Assert.AreEqual(data.AvgEstimatedMonthlyRevenue, 37200);
                Assert.AreEqual(data.AvgOccupacyPercent, 80.0d);
                Assert.AreEqual(data.AvgRevenuePerDay, 1500.0d);
                Assert.AreEqual(data.FlatsCount, 1);
                Assert.IsNotNull(data.GraphData);

                var lineChart = data.GraphData.LineChartData.ToArray();
                Assert.AreEqual(lineChart.Length, MonthCount(startOfThePeriod, endOfThePeriod));
                var current = startOfThePeriod;
                int i = 0;
                while (current <= endOfThePeriod)
                {
                    if (current != new DateTime(2017, 1, 1))
                    {
                         TestFlatMonthlySummary(lineChart[i], current, 0, 0, 0);
                    }
                   
                    current = current.AddMonths(1);
                    i++;
                }

                TestFlatMonthlySummary(lineChart[3], new DateTime(2017, 1, 1), 37200, 80.0d, 1500.0d);
                TestDistributionGraph(data.GraphData.DistributionByRevenue, 1, new double[] { 0, 1, 0, 0, 0, 0, 0, 0, 0, 0 });
                TestDistributionGraph(data.GraphData.DistributionByOccupacy, 1, new double[] { 0, 0, 0, 0, 0, 0, 0, 0, 1, 0 });
            }

            [Test]
            public void GetDataForFlatsOneFlatTwoMonth()
            {
                var monitoring = new Analytics();
                var startOfThePeriod = new DateTime(2016, 10, 1);
                var endOfThePeriod = DateTime.Now;

                var flat = new FlatData()
                {
                    MonthlyData = new List<FlatMonthlyData>()
                    {
                        AddFlatMonthlyData(2017, 1, 25, 20, 30000),
                        AddFlatMonthlyData(2017, 3, 20, 15, 40000),
                    },

                    OccupacyPercent = 0.775,
                    RevenuePerDay = 2083,
                    EstimatedRevenue = 49600
                };

                var cache = new ApplicationCache(null, null);
                flat.MonthlyDataDict = cache._CreateMonthlyDataDictionary(flat.MonthlyData);

                var data = monitoring._GetDataForFlats(new FlatData[] { flat }, startOfThePeriod, endOfThePeriod);
                Assert.AreEqual(data.AvgEstimatedMonthlyRevenue, 49600);
                Assert.AreEqual(data.AvgOccupacyPercent, 77.5d);
                Assert.AreEqual((int)data.AvgRevenuePerDay, 2083);
                Assert.AreEqual(data.FlatsCount, 1);
                Assert.IsNotNull(data.GraphData);

                var lineChart = data.GraphData.LineChartData.ToArray();
                Assert.AreEqual(lineChart.Length, MonthCount(startOfThePeriod, endOfThePeriod));
                var current = startOfThePeriod;
                int i = 0;
                while (current <= endOfThePeriod)
                {
                    if (current != new DateTime(2017, 1, 1) && current != new DateTime(2017, 3, 1))
                    {
                        TestFlatMonthlySummary(lineChart[i], current, 0, 0, 0);
                    }

                    current = current.AddMonths(1);
                    i++;
                }

                TestFlatMonthlySummary(lineChart[3], new DateTime(2017, 1, 1), 37200, 80.0d, 1500.0d);
                TestDistributionGraph(data.GraphData.DistributionByRevenue, 1, new double[] { 0, 0, 1, 0, 0, 0, 0, 0, 0, 0 });
                TestDistributionGraph(data.GraphData.DistributionByOccupacy, 1, new double[] { 0, 0, 0, 0, 0, 0, 0, 1, 0, 0 });
            }

            [Test]
            public void GetDataForFlatsTwoFlats()
            {
                var monitoring = new Analytics();
                var startOfThePeriod = new DateTime(2016, 10, 1);
                var endOfThePeriod = DateTime.Now;

                var flat1 = new FlatData()
                {
                    MonthlyData = new List<FlatMonthlyData>()
                    {
                        AddFlatMonthlyData(2017, 1, 25, 20, 30000),
                        AddFlatMonthlyData(2017, 3, 20, 15, 40000),
                    },

                    OccupacyPercent = 0.775,
                    RevenuePerDay = 2083,
                    EstimatedRevenue = 49600
                };

                var flat2 = new FlatData()
                {
                    MonthlyData = new List<FlatMonthlyData>()
                    {
                        AddFlatMonthlyData(2017, 1, 28, 25, 40000),
                    },

                    OccupacyPercent = 0.893,
                    RevenuePerDay = 1600,
                    EstimatedRevenue = 44285
                };

                var cache = new ApplicationCache(null, null);
                flat1.MonthlyDataDict = cache._CreateMonthlyDataDictionary(flat1.MonthlyData);
                flat2.MonthlyDataDict = cache._CreateMonthlyDataDictionary(flat2.MonthlyData);

                var data = monitoring._GetDataForFlats(new FlatData[] { flat1, flat2 }, startOfThePeriod, endOfThePeriod);
                Assert.AreEqual((int)data.AvgEstimatedMonthlyRevenue, 47828);
                Assert.AreEqual(Math.Round(data.AvgOccupacyPercent, 1), 81.4d);
                Assert.AreEqual((int)data.AvgRevenuePerDay, 1922);
                Assert.AreEqual(data.FlatsCount, 2);
                Assert.IsNotNull(data.GraphData);

                var lineChart = data.GraphData.LineChartData.ToArray();
                Assert.AreEqual(lineChart.Length, MonthCount(startOfThePeriod, endOfThePeriod));
                var current = startOfThePeriod;
                int i = 0;
                while (current <= endOfThePeriod)
                {
                    if (current != new DateTime(2017, 1, 1) && current != new DateTime(2017, 3, 1))
                    {
                        TestFlatMonthlySummary(lineChart[i], current, 0, 0, 0);
                    }

                    current = current.AddMonths(1);
                    i++;
                }

                TestFlatMonthlySummary(lineChart[3], new DateTime(2017, 1, 1), 40742, 84.6d, 1550);
                TestFlatMonthlySummary(lineChart[5], new DateTime(2017, 3, 1), 62000, 75.0d, 2666);
                TestDistributionGraph(data.GraphData.DistributionByRevenue, 2, new double[] { 0, 0, 1, 0, 0, 0, 0, 0, 0, 0 });
                TestDistributionGraph(data.GraphData.DistributionByOccupacy, 2, new double[] { 0, 0, 0, 0, 0, 0, 0, 0.5, 0.5, 0 });
            }

            private FlatMonthlyData AddFlatMonthlyData(int year, int month, int count, int occupacy, int revenue)
            {
                var result = new FlatMonthlyData()
                {
                    Year = year,
                    Month = month,
                    Count = count, //total count of records for this flat for given month 
                    Occupacy = occupacy, //count of day flat was rented
                    Revenue = revenue, //total revenue
                };

                result.OccupacyPercent = result.Occupacy / (double)result.Count;
                result.RevenuePerDay = revenue / (double)occupacy;
                result.EstimatedMonthlyRevenue = result.Count > 0 ? result.Revenue * (DateTime.DaysInMonth(result.Year, result.Month) / (double)result.Count) : 0; //extrapoltaion of existing data to whole month

                return result; 
            }

            private void TestFlatMonthlySummary(FlatMonthlySummary summary, DateTime date, double avgEstimatedMonthlyRevenue,
                                                double avgOccupacyPercent, double avgRevenuePerDay)
            {
                Assert.AreEqual(summary.Date, date);
                Assert.AreEqual((int)summary.AvgEstimatedMonthlyRevenue, avgEstimatedMonthlyRevenue);
                Assert.AreEqual(Math.Round(summary.AvgOccupacyPercent, 1), avgOccupacyPercent);
                Assert.AreEqual((int)summary.AvgRevenuePerDay, avgRevenuePerDay);
            }

            private void TestDistributionGraph(DistributionGraph dg, int flatsCount, double[] Percentage)
            {
                Assert.AreEqual(dg.FlatsCount, flatsCount);
                for (int i = 0; i < dg.Data.Length; i++)
                {
                    Assert.AreEqual(dg.Data[i].Percentage, Percentage[i]);
                }
            }

            private int MonthCount(DateTime a, DateTime b)
            {
                return ((b.Year - a.Year) * 12) + b.Month - a.Month + 1;
            }
        }
    }
}
