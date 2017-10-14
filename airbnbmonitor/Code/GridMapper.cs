namespace airbnbmonitor.Code
{
    using System.Collections.Generic;
    using System.Linq;
    using airbnbmonitor.Code.Definitions;
    using DataMiner.FlatParser;

    public class GridMapper
    {
        public IEnumerable<GridPoint> MakeGrid(IEnumerable<FlatData> data, GridMapperSettings settings)
        {
            if (settings.RowsCount == 0 || settings.ColumnsCount == 0)
            {
                return new List<GridPoint>();
            }

            //Latitude = y, Longtitude = x
            double rowSize = (settings.EndLatitude - settings.StartLatitude) / settings.RowsCount;
            double colSize = (settings.EndLongitude - settings.StartLongitude) / settings.ColumnsCount;
            var result = InitGridPoints(settings, rowSize, colSize);

            foreach (var item in data)
            {
                var col = (int)((item.Longitude - settings.StartLongitude) / colSize);
                var row = (int)((item.Latitude - settings.StartLatitude) / rowSize);
                var pos = row * settings.ColumnsCount + col;
                if (pos < 0 || pos >= result.Length)
                {
                    continue;
                }

                result[pos].Count++;
                result[pos].Sum += item.EstimatedRevenue;
                result[pos].OccupacySum += item.OccupacyPercent;
            }

            return result.Where(p => p.Count > 0).Select(p => new GridPoint(p));
        }

        private HeatMapGridPoint[] InitGridPoints(GridMapperSettings settings, double rowSize, double colSize)
        {
            var result = new HeatMapGridPoint[settings.RowsCount * settings.ColumnsCount];
            double halfRowSize = rowSize / 2;
            double halfColSize = colSize / 2;
            for (var i = 0; i < settings.RowsCount; i++)
            {
                for (var j = 0; j < settings.ColumnsCount; j++)
                {
                    var pos = i * settings.ColumnsCount + j;
                    result[pos] = new HeatMapGridPoint();
                    result[pos].Latitude = settings.StartLatitude + i * rowSize + halfRowSize;
                    result[pos].Longitude = settings.StartLongitude + j * colSize + halfColSize;
                    result[pos].Count = 0;
                    result[pos].Sum = 0;
                    result[pos].OccupacySum = 0;
                }
            }

            return result;
        }
    }
}