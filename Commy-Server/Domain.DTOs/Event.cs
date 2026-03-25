namespace Domain.DTOs
{
    public class Event
    {
        public long Eid { get; set; }
        public long Community { get; set; }
        public string Name { get; set; } = string.Empty;
        public string Location { get; set; } = string.Empty;
        public long EventTime { get; set; }
        public InterestedProfilesData Interested { get; set; } = null!;
        public string ImageURI { get; set; } = string.Empty;
    }
}
