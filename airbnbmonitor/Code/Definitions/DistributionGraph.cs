namespace airbnbmonitor.Code.Definitions
{
    public class DistributionGraph
    {
        public DistributionGraph(DistributionGraphSettings settings)
        {
            Data = new DistributionGraphItem[settings.Steps];

            var step = (settings.Max - settings.Min) / settings.Steps;
            for (var i = 0; i < settings.Steps; i++)
            {
                Data[i] = new DistributionGraphItem();
                Data[i].From = settings.Min + step * i;
                Data[i].To = settings.Min + step * (i + 1);
            }
        }

        public int FlatsCount { get; set; }
        public DistributionGraphItem[] Data { get; set; }
    }
}