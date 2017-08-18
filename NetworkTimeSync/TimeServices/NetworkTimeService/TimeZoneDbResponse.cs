namespace NetworkTimeSync.TimeServices.NetworkTimeService
{
    public class TimeZoneDbResponse
    {
        public string Status { get; set; }
        public string Message { get; set; }
        public string CountryCode { get; set; }
        public string CountryName { get; set; }
        public string ZoneName { get; set; }
        public string Abbreviation { get; set; }
        public long GmtOffset { get; set; }
        public string Dst { get; set; }
        public long DstStart { get; set; }
        public long DstEnd { get; set; }
        public string NextAbbreviation { get; set; }
        public long TimeStamp { get; set; }
        public string Formatted { get; set; }
    }
}