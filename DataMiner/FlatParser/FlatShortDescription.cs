namespace DataMiner.FlatParser
{
    using DataMiner.Database;

    public class FlatShortDescription
    {
        public FlatShortDescription()
        {
        }

        public FlatShortDescription(Flats efFlat)
        {
            MatchDescription = efFlat.MatchDescription;
            Communication = efFlat.Communication;
            Cleanly = efFlat.Cleanly;
            Location = efFlat.Location;
            Settlement = efFlat.Settlement;
            PriceQualityRation = efFlat.PriceQualityRation;
        }

        public double? MatchDescription { get; set; }
        public double? Communication { get; set; }
        public double? Cleanly { get; set; }
        public double? Location { get; set; }
        public double? Settlement { get; set; }
        public double? PriceQualityRation { get; set; }
    }
}
