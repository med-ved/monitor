namespace DataMiner.FlatParser
{
    using System;

    public class FlatStatus
    {
        public Flat Flat { get; set; }

        public DateTime Date { get; set; }
        public int? Price { get; set; }
        public bool? Available { get; set; }
    }
}
