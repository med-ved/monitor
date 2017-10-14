namespace DataMiner.FlatParser
{
    using System;

    public class FlatStatusRequest
    {
        public int Id { get; set; }
        public DateTime Date { get; set; }
        public string Country { get; set; }
        public string City { get; set; }

        public override string ToString()
        {
            return string.Format("Date={0}, Flat Id={1}, Country={2}, City={3}", Date, Id, Country, City);
        }
    }
}
