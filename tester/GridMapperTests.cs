namespace tester
{
    using System.Linq;
    using NUnit.Framework;
    using airbnbmonitor.Code.Definitions;
    using airbnbmonitor.Code;
    using DataMiner.FlatParser;

    [TestFixture]
    public class GridMapperTests
    {
        [Test]
        public void GridMapperNoData()
        {
            var settings = new GridMapperSettings()
            {
                StartLatitude = 0, //niz
                EndLatitude = 2, //verh
                StartLongitude = 0, //levo
                EndLongitude = 2,  //pravo

                ColumnsCount = 3,
                RowsCount = 3,
            };

            var gridMapper = new GridMapper();
            var grid = gridMapper.MakeGrid(new FlatData[] { }, settings);
            Assert.AreEqual(grid.Count(), 0);
        }

        [Test]
        public void GridMapperOneFlat()
        {
            /*
             000
             010
             000
             */
            var settings = new GridMapperSettings()
            {
                StartLatitude = 0, //niz
                EndLatitude = 3, //verh
                StartLongitude = 0, //levo
                EndLongitude = 3,  //pravo

                ColumnsCount = 3,
                RowsCount = 3,
            };

            var gridMapper = new GridMapper();
            var flat1 = new FlatData
            {
                Latitude = 1.2,
                Longitude = 1.2,
                EstimatedRevenue = 10000,
                OccupacyPercent = 50,
            };

            var grid = gridMapper.MakeGrid(new FlatData[] { flat1 }, settings).ToArray();
            Assert.AreEqual(grid.Count(), 1);
            Assert.AreEqual(grid[0].Latitude, 1.5);
            Assert.AreEqual(grid[0].Longitude, 1.5);
            Assert.AreEqual(grid[0].Occupancy, 50);
            Assert.AreEqual(grid[0].Revenue, 10000);
        }

        [Test]
        public void GridMapperFourFlatsTwoInCenter()
        {
            /*
             100
             020
             001
             */

            var settings = new GridMapperSettings()
            {
                StartLatitude = 0, //niz
                EndLatitude = 3, //verh
                StartLongitude = 0, //levo
                EndLongitude = 3,  //pravo

                ColumnsCount = 3,
                RowsCount = 3,
            };

            var gridMapper = new GridMapper();
            var flat1 = new FlatData
            {
                Latitude = 0.2,
                Longitude = 0.2,
                EstimatedRevenue = 10000,
                OccupacyPercent = 50,
            };

            var flat2 = new FlatData
            {
                Latitude = 1.2,
                Longitude = 1.2,
                EstimatedRevenue = 20000,
                OccupacyPercent = 70,
            };

            var flat3 = new FlatData
            {
                Latitude = 1.4,
                Longitude = 1.4,
                EstimatedRevenue = 40000,
                OccupacyPercent = 80,
            };

            var flat4 = new FlatData
            {
                Latitude = 2.2,
                Longitude = 2.2,
                EstimatedRevenue = 15000,
                OccupacyPercent = 60,
            };

            var grid = gridMapper.MakeGrid(new FlatData[] { flat1, flat2, flat3, flat4 }, settings).ToArray();
            Assert.AreEqual(grid.Count(), 3);

            // up left
            Assert.AreEqual(grid[0].Latitude, 0.5);
            Assert.AreEqual(grid[0].Longitude, 0.5);
            Assert.AreEqual(grid[0].Occupancy, 50);
            Assert.AreEqual(grid[0].Revenue, 10000);

            // center
            Assert.AreEqual(grid[1].Latitude, 1.5);
            Assert.AreEqual(grid[1].Longitude, 1.5);
            Assert.AreEqual(grid[1].Occupancy, 75);
            Assert.AreEqual(grid[1].Revenue, 30000);

            // bottom right
            Assert.AreEqual(grid[2].Latitude, 2.5);
            Assert.AreEqual(grid[2].Longitude, 2.5);
            Assert.AreEqual(grid[2].Occupancy, 60);
            Assert.AreEqual(grid[2].Revenue, 15000);
        }
    }
}
