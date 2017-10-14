namespace DataMiner.FlatParser
{
    using DataMiner.Database;

    public class FlatDescription
    {
        public FlatDescription()
        {
        }

        public FlatDescription(Flats efFlat)
        {
            Access = efFlat.Description.Access;
            Description = efFlat.Description.Description1;
            HouseRules = efFlat.Description.HouseRules;
            Interaction = efFlat.Description.Interaction;
            Locale = efFlat.Description.Locale;
            Name = efFlat.Description.Name;
            NeighborhoodOverview = efFlat.Description.NeighborhoodOverview;
            Notes = efFlat.Description.Notes;
            Space = efFlat.Description.Space;
            Summary = efFlat.Description.Summary;
            Transit = efFlat.Description.Transit;
        }

        public string Access { get; set; }
        public string Description { get; set; }
        public string HouseRules { get; set; }
        public string Interaction { get; set; }
        public string Locale { get; set; }
        public string Name { get; set; }
        public string NeighborhoodOverview { get; set; }
        public string Notes { get; set; }
        public string Space { get; set; }
        public string Summary { get; set; }
        public string Transit { get; set; }
    }
}
