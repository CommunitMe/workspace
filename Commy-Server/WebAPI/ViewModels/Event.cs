using Mapster;

namespace WebAPI.ViewModels
{
    [AdaptTwoWays(typeof(Domain.Models.Event))]
    public class Event
    {
        [AdaptMember(nameof(Domain.Models.Event.Id))]
        public string Eid { get; set; } = string.Empty;
        [AdaptMember(nameof(Domain.Models.Event.RelevantCommunity))]
        public string Community { get; set; } = string.Empty;
        public string Name { get; set; } = string.Empty;
        public string Location { get; set; } = string.Empty;
        public long EventTime { get; set; }
        public InterestedProfilesData Interested { get; set; } = null!;
        public string ImageURI { get; set; } = string.Empty;
    }
}
