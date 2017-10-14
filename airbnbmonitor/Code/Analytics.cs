namespace airbnbmonitor.Code
{
    using System;
    using System.Data;
    using System.Collections.Generic;
    using System.Linq;
    using airbnbmonitor.Code.Definitions;
    using DataMiner.FlatParser;

    public class Analytics
    {
        public enum MonitoringDataType { All, TopOccupacy, TopRevenue }
        private readonly CachedFlats _cachedFlats;

        public Analytics(CachedFlats cachedFlats = null)
        {
            _cachedFlats = cachedFlats;
        }

        private void AddMonthlyData(FlatMonthlySummary total, FlatMonthlyData dataToAdd)
        {
            total.AvgEstimatedMonthlyRevenue += dataToAdd.EstimatedMonthlyRevenue;
            total.AvgOccupacyPercent += dataToAdd.OccupacyPercent;
            total.AvgRevenuePerDay += dataToAdd.RevenuePerDay;
        }

        private double SafeDiv(double val, double divisor)
        {
            if (divisor == 0)
            {
                return val;
            }

            return val / divisor;
        }

        private void CalcAvgValue(FlatMonthlySummary summary, int count)
        {
            summary.AvgEstimatedMonthlyRevenue = SafeDiv(summary.AvgEstimatedMonthlyRevenue, count);
            summary.AvgOccupacyPercent = SafeDiv(summary.AvgOccupacyPercent, count) * 100;
            summary.AvgRevenuePerDay = SafeDiv(summary.AvgRevenuePerDay, count);
        }

        private void AddFlatToDistributionCategory(FlatMonthlySummary summary, DistributionGraph graph, DistributionGraphSettings settings)
        {
            var pos = (int)((settings.GetDistributionField(summary) - settings.Min) / settings.StepValue);
            if (pos < 0)
            {
                pos = 0;
            }

            if (pos > settings.Steps - 1)
            {
                pos = settings.Steps - 1;
            }

            graph.Data[pos].Count++;
        }

        private void SetDistributionPercentage(int flatsCount, DistributionGraph graph)
        {
            graph.FlatsCount = flatsCount;
            foreach (var d in graph.Data)
            {
                d.Percentage = flatsCount  != 0? (double)d.Count / flatsCount : 0;
            }
        }

        private DistributionGraph CreateDistributionGraphData(IEnumerable<FlatData> flatsList, DistributionGraphSettings settings)
        {
            var result = new DistributionGraph(settings);
            int flatsCount = 0;
            foreach (var flat in flatsList)
            {
                var currentDate = new DateTime(settings.StartTime.Year, settings.StartTime.Month, 1);
                var summary = new FlatMonthlySummary();
                int count = 0;
                while (currentDate <= settings.EndTime)
                {
                    var key = new DateTime(currentDate.Year, currentDate.Month, 1);
                    if (flat.MonthlyDataDict.ContainsKey(key))
                    {
                        count++;
                        var data = flat.MonthlyDataDict[key];
                        AddMonthlyData(summary, data);
                    }

                    currentDate = currentDate.AddMonths(1);
                }

                if (count == 0)
                {
                    continue;
                }

                flatsCount++;
                CalcAvgValue(summary, count);
                AddFlatToDistributionCategory(summary, result, settings);
            }

            SetDistributionPercentage(flatsCount, result);
            return result;
        }

        private FlatGraphData CreateFlatsGraphData(IEnumerable<FlatData> flatsList, DateTime startTime, DateTime endTime)
        {
            var graphData = new List<FlatMonthlySummary>();
            var summary = new FlatMonthlySummary();
            var totalCount = 0;

            var currentDate = new DateTime(startTime.Year, startTime.Month, 1);  
            while (currentDate <= endTime)
            {
                var key = new DateTime(currentDate.Year, currentDate.Month, 1);
                var monthData = new FlatMonthlySummary();
                monthData.Date = key;

                int count = 0;
                foreach (var flat in flatsList)
                {
                    if (flat.MonthlyDataDict.ContainsKey(key))
                    {
                        count++;
                        totalCount++;
                        var data = flat.MonthlyDataDict[key];
                        AddMonthlyData(monthData, data);
                        AddMonthlyData(summary, data);
                    }
                }

                CalcAvgValue(monthData, count);
                graphData.Add(monthData);

                currentDate = currentDate.AddMonths(1);
            }

            CalcAvgValue(summary, totalCount);

            var occupacyGraphSettings = new DistributionGraphSettings()
            {
                StartTime = startTime,
                EndTime = endTime,
                Min = 0,
                Max = 100,
                Steps = 10,
                GetDistributionField = (s) => s.AvgOccupacyPercent
            };

            var revenueGraphSettings = new DistributionGraphSettings()
            {
                StartTime = startTime,
                EndTime = endTime,
                Min = 0,
                Max = 200000,
                Steps = 10,
                GetDistributionField = (s) => s.AvgEstimatedMonthlyRevenue
            };

            return new FlatGraphData()
            {
                LineChartData = graphData,
                Summary = summary,
                DistributionByRevenue = CreateDistributionGraphData(flatsList, revenueGraphSettings),
                DistributionByOccupacy = CreateDistributionGraphData(flatsList, occupacyGraphSettings)
            };
        }

        private IEnumerable<FlatData> CreateFlatsList(CachedFlats cachedFlats, IEnumerable<long> flatIds)
        {
            var flatsList = new List<FlatData>();
            foreach (long id in flatIds)
            {
                if (cachedFlats.FlatsDataDict.ContainsKey(id))
                {
                    flatsList.Add(cachedFlats.FlatsDataDict[id]);
                }
            }

            return flatsList;
        }

        public FlatsSummary _GetDataForFlats(IEnumerable<FlatData> flatsList, DateTime startTime, DateTime endTime)
        {
            var result = new FlatsSummary();
            if (!flatsList.Any())
            {
                return result;
            }

            result.FlatsCount = flatsList.Count();
            result.GraphData = CreateFlatsGraphData(flatsList, startTime, endTime);
            result.AvgEstimatedMonthlyRevenue = result.GraphData.Summary.AvgEstimatedMonthlyRevenue;
            result.AvgOccupacyPercent = result.GraphData.Summary.AvgOccupacyPercent;
            result.AvgRevenuePerDay = result.GraphData.Summary.AvgRevenuePerDay;

            return result;
        }

        public MonitoringData GetData(MonitoringDataType type)
        {
            /*
                нужны следующие данные
                1) средний доход за период
                2) средняя заполняемость за период
                3) средний доход и средняя заполняемость по месяцам

            1 и 2 можно вычислить на основе 3
            Если в не сезон (октябрь-апрель) у квартиры 100% заполняемость, значит она сдана в долгосрок

            Нужна 3 по отдельным квартирам
                */

            var startOfThePeriod = new DateTime(2016, 10, 1);
            var endOfThePeriod = DateTime.Now;

            var flatsList = _cachedFlats.FlatsData;
            if (type == MonitoringDataType.TopOccupacy)
            {
                flatsList = flatsList.OrderByDescending(f => f.OccupacyPercent).Take(flatsList.Count() / 10).ToList();
            }

            if (type == MonitoringDataType.TopRevenue)
            {
                flatsList = flatsList.OrderByDescending(f => f.EstimatedRevenue).Take(flatsList.Count() / 10).ToList();
            }

            var settings = new GridMapperSettings()
            {
                StartLatitude = 59.79219567461983, //niz
                EndLatitude = 60.04839774295754, //verh
                StartLongitude = 29.818058044888858, //levo
                EndLongitude = 30.817813904263858,  //pravo

                ColumnsCount = 120,
                RowsCount = 80,
            };

            var gridMapper = new GridMapper();
            var grid = gridMapper.MakeGrid(flatsList, settings);

            var flatsInfoList = flatsList.Select(f => new FlatInfo(f)).ToList();
            var result = new MonitoringData
            {
                FlatsInfoList = flatsInfoList,
                FlatsGrid = grid,
                Summary = _GetDataForFlats(flatsList, startOfThePeriod, endOfThePeriod)
            };

            return result;
        }

        public FlatsSummary GetDataForFlats(IEnumerable<long> flatIds, DateTime startTime, DateTime endTime)
        {
            var flatsList = CreateFlatsList(_cachedFlats, flatIds);
            return _GetDataForFlats(flatsList, startTime, endTime);
        }

        public SingleFlatSummary GetFlat(Flat flat, DateTime startTime, DateTime endTime)
        {
            return new SingleFlatSummary()
            {
                Flat = flat,
                Summary = GetDataForFlats(new long[] { flat.Id }, startTime, endTime)
            };
        }
    }
}