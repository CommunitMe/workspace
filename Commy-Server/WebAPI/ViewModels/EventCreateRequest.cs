using Mapster;

namespace WebAPI.ViewModels
{
    [AdaptTo(typeof(Domain.Models.Event))]
    public class EventCreateRequest
    {
        [AdaptMember(nameof(Domain.Models.Event.RelevantCommunity))]
        public string Community { get; set; } = string.Empty;
        public string Name { get; set; } = string.Empty;
        public string Location { get; set; } = string.Empty;
        public long EventTime { get; set; }
        public string ImageURI { get; set; } = string.Empty;
    }
}
